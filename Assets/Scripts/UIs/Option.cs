using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Components;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

public class Option : MonoBehaviour
{
    [SerializeField] GameObject option;
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

    [Header("Language")]
    [SerializeField] GameObject languageWindow;

    [Header("Encyclopedia")]
    [SerializeField] GameObject encyclopedia;
    [SerializeField] GameObject itemTable;
    [SerializeField] TMP_Dropdown sortBy;
    List<GameObject> itemBoxes = new();

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

        LocalizationSettings.SelectedLocaleChanged += OnLocaleChanged;
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
        encyclopedia.SetActive(true);
        for(int i = 1; i < Enum.GetValues(typeof(ItemManager.Items)).Length; i++)
        {
            string itemName = ((ItemManager.Items)i).ToString();
            if (itemName.Contains("Enchanted")) continue;
            GameObject itemImageBox = PoolManager.Spawn(ResourceEnum.Prefab.ImageBox, itemTable.transform);
            itemBoxes.Add(itemImageBox);
            if (Enum.TryParse(itemName, out ResourceEnum.Sprite sprite))
            {
                Sprite sprt = ResourceManager.Get(sprite);
                itemImageBox.GetComponentsInChildren<Image>()[1].sprite = sprt;
                itemImageBox.GetComponent<Help>().SetDescription((ItemManager.Items)i);
                itemImageBox.GetComponentInChildren<AspectRatioFitter>().aspectRatio = sprt.rect.width / sprt.rect.height;
                ItemManager.Craftable craftable = ItemManager.craftables.Find(x => x.itemType == (ItemManager.Items)i);
                int knowledgeRequired = craftable != null ? craftable.requiredKnowledge : 255;
                itemImageBox.AddComponent<ItemDataForSort>().Set((ItemManager.Items)i, knowledgeRequired);
            }
        }
        sortBy.options[0].text = new LocalizedString("Basic", "Item Type").GetLocalizedString();
        sortBy.options[1].text = new LocalizedString("Item", "Required Knowledge").GetLocalizedString();
        encyclopedia.SetActive(false);
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
        option.SetActive(false);
        if(GameManager.Instance.openedWindows.Count == 0) Time.timeScale = 1;
    }

    public void OpenLanguageWindow()
    {
        languageWindow.SetActive(true);
        GameManager.Instance.openedWindows.Push(languageWindow);
    }

    public void SetLanguage(string localeCode)
    {
        Locale locale = LocalizationSettings.AvailableLocales.Locales.Find(x => x.Identifier.Code == localeCode);
        if (locale != null) LocalizationSettings.SelectedLocale = locale;
        else Debug.LogWarning($"Wrong locale code : {localeCode}");
    }

    public void OpenEncyclopedia()
    {
        encyclopedia.SetActive(true);
        GameManager.Instance.openedWindows.Push(encyclopedia);
    }

    public void OpenSaveSlot(bool save)
    {
        //saveOrLoadText.text = save ? "Save" : "Load";
        saveOrLoadText.GetComponent<LocalizeStringEvent>().StringReference 
            = save ? new("Basic", "Save") : new("Basic", "Load");
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
                string info = $"\n<i><size=12>{new LocalizedString("Basic", "Saved Time:").GetLocalizedString()} {saveData.savedTime}</size></i>";
                if (i == 0) info += $"\n<i>{new LocalizedString("Basic", "Autosaved").GetLocalizedString()}</i>";
                saveSlots[i].isEmpty = false;
                saveSlots[i].SetInfo(info, i, saveData.ingameDate);
                if(i != 0) saveSlots[i].deleteButton.SetActive(true);
            }
            else
            {
                saveSlots[i].isEmpty = true;
                saveSlots[i].SetInfo($"<i>[{i}] {new LocalizedString("Basic", "Empty Slot").GetLocalizedString()}</i>", i);
                if (i != 0) saveSlots[i].deleteButton.SetActive(false);
            }
        }
    }

    public void Save(int slot)
    {
        if(!saveSlots[slot].isEmpty)
        {
            GameManager.Instance.OutGameUIManager.OpenConfirmWindow("Confirm:Overwrite", () =>
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
        if (PlayerPrefs.GetString($"SaveDataInfo{slot}", "{}") == "{}") return;
        else
        {
            saveSlotsObject.SetActive(false);
            StartCoroutine(GameManager.Instance.Load(slot));
        }
    }

    public void DeleteSaveData(int slot)
    {
        GameManager.Instance.OutGameUIManager.OpenConfirmWindow("Confirm:Delete Save Data", () =>
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
            GameManager.Instance.OutGameUIManager.OpenConfirmWindow("Confirm:Return to title", () =>
            {
                GoingTitle();
            });
        }
        else
        {
            GoingTitle();
        }
    }

    void GoingTitle()
    {
        if(GameManager.Instance.BattleRoyaleManager != null) GameManager.Instance.GetComponent<GameResult>().ExitBattle(true);
        resume.SetActive(false);
        saveButton.gameObject.SetActive(false);
        goTitle.SetActive(false);
        GameManager.Instance.optionCanvas.SetActive(false);
        GameManager.Instance.Title.title.SetActive(true);

    }

    void OnLocaleChanged(Locale newLocale)
    {
        foreach (var itemBox in itemBoxes)
        {
            itemBox.GetComponent<Help>().SetDescription(itemBox.GetComponent<ItemDataForSort>().itemType);
        }
        ReloadSavedata();
        sortBy.options[0].text = new LocalizedString("Basic", "Item Type").GetLocalizedString();
        sortBy.options[1].text = new LocalizedString("Item", "Required Knowledge").GetLocalizedString();
        sortBy.captionText.text = sortBy.options[sortBy.value].text;
    }
}
