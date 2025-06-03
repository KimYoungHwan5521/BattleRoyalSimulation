using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Option : MonoBehaviour
{
    float lastSaveTime;
    [Header("Sound")]
    [SerializeField] Image bgmImage;
    [SerializeField] Image sfxImage;
    [SerializeField] Sprite on;
    [SerializeField] Sprite off;
    [SerializeField] Slider bgmSlider;
    [SerializeField] Slider sfxSlider;
    bool bgmOn = true;
    bool sfxOn = true;

    [Header("Encyclopedia")]
    [SerializeField] GameObject encyclopedia;
    [SerializeField] GameObject itemTable;
    [SerializeField] TMP_Dropdown sortBy;

    [Header("Buttons")]
    [SerializeField] GameObject resume;
    [SerializeField] Button saveButton;
    [SerializeField] GameObject goTitle;

    [Header("Save")]
    [SerializeField] TextMeshProUGUI saveOrLoadText;
    [SerializeField] GameObject saveSlotsObject;
    [SerializeField] SaveSlot[] saveSlots;

    class ItemDataForSort : MonoBehaviour
    {
        public ItemManager.Items itemType;
        public int knowledgeRequiredForCrafting;

        public void Set(ItemManager.Items itemType, int knowledgeRequiredForCrafting)
        {
            this.itemType = itemType;
            this.knowledgeRequiredForCrafting = knowledgeRequiredForCrafting;
        }
    }

    private void Start()
    {
        GameManager.Instance.ObjectStart += OptionSetting;
        GameManager.Instance.ObjectStart += ReloadSavedata;

    }

    private void Update()
    {
        lastSaveTime += Time.unscaledDeltaTime;
    }

    void OptionSetting()
    {
        resume.SetActive(false);
        saveButton.gameObject.SetActive(false);
        goTitle.SetActive(false);
        for(int i = 1; i < Enum.GetValues(typeof(ItemManager.Items)).Length; i++)
        {
            string itemName = ((ItemManager.Items)i).ToString();
            if (itemName.Contains("Enchanted")) continue;
            GameObject itemImageBox = PoolManager.Spawn(ResourceEnum.Prefab.ImageBox, itemTable.transform);
            if (Enum.TryParse(itemName, out ResourceEnum.Sprite sprite))
            {
                Sprite sprt = ResourceManager.Get(sprite);
                itemImageBox.GetComponentsInChildren<Image>()[1].sprite = sprt;
                itemImageBox.GetComponent<Help>().SetDescription((ItemManager.Items)i);
                itemImageBox.GetComponentInChildren<AspectRatioFitter>().aspectRatio = sprt.textureRect.width / sprt.textureRect.height;
                ItemManager.Craftable craftable = ItemManager.craftables.Find(x => x.itemType == (ItemManager.Items)i);
                int knowledgeRequired = craftable != null ? craftable.requiredKnowledge : 255;
                itemImageBox.AddComponent<ItemDataForSort>().Set((ItemManager.Items)i, knowledgeRequired);
            }
        }
    }

    void Sorting(int sortBy)
    {
        var children = itemTable.transform.Cast<Transform>().ToList();
        List<Transform> sorted = null;
        // sort by - 0 : Item type, 1 : Knowledge required for crafting
        switch (sortBy)
        {
            case 0:
                sorted = children.OrderBy(x => x.GetComponent<ItemDataForSort>().itemType).ToList();
                break;
            case 1:
                sorted = children.OrderBy(x => x.GetComponent<ItemDataForSort>().knowledgeRequiredForCrafting).ToList();
                break;
        }
        for (int i = 0; i < sorted.Count; i++)
        {
            sorted[i].SetSiblingIndex(i);
        }
    }

    public void Sort()
    {
        Sorting(sortBy.value);
    }

    public void ToggleBGM()
    {
        bgmOn = !bgmOn;
        bgmImage.sprite = bgmOn ? on : off;
        bgmSlider.interactable = bgmOn;
        GameManager.Instance.SoundManager.ToggleAudioMixerGroup(SoundManager.AudioMixerGroupType.BGM, bgmOn, bgmSlider.value);
    }

    public void ToggleSFX()
    {
        sfxOn = !sfxOn;
        sfxImage.sprite = sfxOn ? on : off;
        sfxSlider.interactable = sfxOn;
        GameManager.Instance.SoundManager.ToggleAudioMixerGroup(SoundManager.AudioMixerGroupType.SFX, sfxOn, sfxSlider.value);
    }

    public void SlideBGM()
    {
        GameManager.Instance.SoundManager.ToggleAudioMixerGroup(SoundManager.AudioMixerGroupType.BGM, true, bgmSlider.value);
    }

    public void SlideSFX()
    {
        GameManager.Instance.SoundManager.ToggleAudioMixerGroup(SoundManager.AudioMixerGroupType.SFX, true, sfxSlider.value);
    }

    public void Resume()
    {
        GameManager.Instance.openedWindows.Pop().SetActive(false);
        if(GameManager.Instance.openedWindows.Count == 0) Time.timeScale = 1;
    }

    public void OpenEncyclopedia()
    {
        encyclopedia.SetActive(true);
        GameManager.Instance.openedWindows.Push(encyclopedia);
    }

    public void OpenSaveSlot(bool save)
    {
        saveOrLoadText.text = save ? "Save" : "Load";
        for(int i=1; i<saveSlots.Length; i++)
        {
            saveSlots[i].saveButton.SetActive(save);
            saveSlots[i].GetComponent<Button>().enabled = !save;
        }
        saveSlotsObject.SetActive(true);
        GameManager.Instance.openedWindows.Push(saveSlotsObject);
    }

    public void ReloadSavedata()
    {
        for (int i = 0; i < saveSlots.Length; i++)
        {
            string json = PlayerPrefs.GetString($"SaveDataInfo{i}", "{}");
            if (json != "{}")
            {
                var saveData = JsonUtility.FromJson<SaveDataInfo>(json);
                string info = $"{saveData.ingameDate}\n<i>Saved at{saveData.savedTime}</i>";
                if (i == 0) info += "<i>Auto-saved</i>";
                saveSlots[i].SetInfo(info);
                if(i != 0) saveSlots[i].deleteButton.SetActive(true);
                saveSlots[i].isEmpty = false;
            }
            else
            {
                saveSlots[i].SetInfo("<i>Empty slot</i>");
                if (i != 0) saveSlots[i].deleteButton.SetActive(false);
                saveSlots[i].isEmpty = true;
            }
        }
    }

    public void Save(int slot)
    {
        if(!saveSlots[slot].isEmpty)
        {
            GameManager.Instance.OutGameUIManager.OpenConfirmWindow("This slot already contains data. Do you want to overwrite it?", () =>
            {
                GameManager.Instance.Save(slot);
            });
        }
        else
        {
            GameManager.Instance.Save(slot);
        }
    }

    public void Load(int slot)
    {
        StartCoroutine(GameManager.Instance.Load(slot));
    }

    public void DeleteSaveData(int slot)
    {
        GameManager.Instance.OutGameUIManager.OpenConfirmWindow("Are you sure you want to delete this save data?", () =>
        {
            PlayerPrefs.DeleteKey($"SaveDataInfo{slot}");
            PlayerPrefs.DeleteKey($"MySurvivorList{slot}");
            PlayerPrefs.DeleteKey($"LeagueReserveData{slot}");
            PlayerPrefs.DeleteKey($"ETCData{slot}");
            ReloadSavedata();
        });
    }

    public void SetSaveButtonInteractable(bool interactable)
    {
        if(interactable)
        {
            resume.SetActive(true);
            saveButton.gameObject.SetActive(true);
            goTitle.SetActive(true);
        }
        saveButton.interactable = interactable;
    }

    public void GoTitle()
    {
        if(lastSaveTime > 10)
        {
            GameManager.Instance.OutGameUIManager.OpenConfirmWindow("Go title?\n<i>(Any unsaved content will be deleted.)</i>", () =>
            {
                resume.SetActive(false);
                saveButton.gameObject.SetActive(false);
                goTitle.SetActive(false);
                GameManager.Instance.optionCanvas.SetActive(false);
                GameManager.Instance.Title.title.SetActive(true);
            });
        }
        else
        {
            resume.SetActive(false);
            saveButton.gameObject.SetActive(false);
            goTitle.SetActive(false);
            GameManager.Instance.optionCanvas.SetActive(false);
            GameManager.Instance.Title.title.SetActive(true);
        }
    }
}
