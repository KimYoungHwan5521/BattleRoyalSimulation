using Steamworks;
using System;
using System.Collections;
using System.Collections.Generic;
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
    [SerializeField] Image itemTabBtn;
    [SerializeField] GameObject tabItems;
    [SerializeField] GameObject itemTable;
    [SerializeField] TMP_Dropdown sortBy;
    List<GameObject> itemBoxes = new();

    [SerializeField] Image charTabBtn;
    [SerializeField] GameObject tabCharacteristics;
    [SerializeField] GameObject characteristicTable;
    [SerializeField] TMP_Dropdown sortBy_Characteristic;
    [SerializeField] AutoNewLineLayoutGroup characteristicAutoNewlineLG;
    List<GameObject> characteristicBoxes = new();

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
        public LocalizedString localizeName;

        public void Set(ItemManager.Items itemType, int knowledgeRequiredForCrafting)
        {
            this.itemType = itemType;
            this.knowledgeRequiredForCrafting = knowledgeRequiredForCrafting;
            localizeName = new LocalizedString("Item", itemType.ToString());
        }
    }

    class CharacteristicDataForSort : MonoBehaviour
    {
        public LocalizedString localizeName;
        public CharacteristicRarity rarity;

        public void Set(LocalizedString localizeName, CharacteristicRarity rarity)
        {
            this.localizeName = localizeName;
            this.rarity = rarity;
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
        // Items
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
        sortBy.options[2].text = new LocalizedString("Basic", "Name").GetLocalizedString();

        // Characteristics
        for (int i = 0; i < CharacteristicManager.Characteristics.Count; i++)
        {
            GameObject characteristicBox = PoolManager.Spawn(ResourceEnum.Prefab.Characteristic, characteristicTable.transform);
            characteristicBoxes.Add(characteristicBox);
            characteristicBox.GetComponent<Image>().color = CharacteristicManager.Characteristics[i].rarity switch
            {
                CharacteristicRarity.Common => new Color(0.2984f, 0.8483f, 0.9471f),
                CharacteristicRarity.Uncommon => new Color(0.8050f, 0.2980f, 0.9490f),
                CharacteristicRarity.Rare => new Color(0.9490f, 0.8036f, 0.2980f),
                _ => new Color(1, 1, 1)
            };
            characteristicBox.AddComponent<CharacteristicDataForSort>().Set(CharacteristicManager.Characteristics[i].characteristicName, CharacteristicManager.Characteristics[i].rarity);
        }
        characteristicAutoNewlineLG.characteristicsBox = characteristicBoxes.ToArray();
        sortBy_Characteristic.options[0].text = new LocalizedString("Basic", "Rarity").GetLocalizedString();
        sortBy_Characteristic.options[1].text = new LocalizedString("Basic", "Name").GetLocalizedString();

        ChangeTab(0);
        encyclopedia.SetActive(false);
    }

    public void ChangeTab(int index)
    {
        if(index == 0)
        {
            tabCharacteristics.SetActive(false);
            tabItems.SetActive(true);
            itemTabBtn.color = new Color(0.55f, 1, 1);
            charTabBtn.color = new Color(1, 1, 1);
        }
        else
        {
            tabItems.SetActive(false);
            tabCharacteristics.SetActive(true);
            itemTabBtn.color = new Color(1, 1, 1);
            charTabBtn.color = new Color(0.55f, 1, 1);
        }
        characteristicAutoNewlineLG.ArrangeCharacteristics();
    }

    void SortTable(GameObject wantTable, int sortBy)
    {
        var children = wantTable.transform.Cast<Transform>().ToList();
        List<Transform> sorted = null;
        // sort by - 0 : Item type, 1 : Knowledge required for crafting, 2 : Name, 3 : Rarity
        switch (sortBy)
        {
            case 0:
                sorted = children.OrderBy(x => x.GetComponent<ItemDataForSort>().itemType).ToList();
                break;
            case 1:
                sorted = children.OrderBy(x => x.GetComponent<ItemDataForSort>().knowledgeRequiredForCrafting).ToList();
                break;
            case 2:
                if (wantTable == itemTable) sorted = children.OrderBy(x => x.GetComponent<ItemDataForSort>().localizeName.GetLocalizedString()).ToList();
                else sorted = children.OrderBy(x => x.GetComponent<CharacteristicDataForSort>().localizeName.GetLocalizedString()).ToList();
                break;
            case 3:
                sorted = children.OrderBy(x => x.GetComponent<CharacteristicDataForSort>().rarity).ToList();
                break;
        }
        for (int i = 0; i < sorted.Count; i++)
        {
            sorted[i].SetSiblingIndex(i);
        }
        characteristicAutoNewlineLG.ArrangeCharacteristics();
    }

    public void Sort(int table)
    {
        GameObject wantTable;
        int sortingOrder = 0;
        if (table == 0)
        {
            wantTable = itemTable;
            sortingOrder = sortBy.value;
        }
        else
        {
            wantTable = characteristicTable;
            sortingOrder = sortBy_Characteristic.value == 1 ? 2 : 3;
        }
        SortTable(wantTable, sortingOrder);
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
        characteristicAutoNewlineLG.ArrangeCharacteristics();
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
                string info = $"\n<i><size=12>{new LocalizedString("Basic", "Saved Time:").GetLocalizedString()} {saveData.savedTime}\nVersion {saveData.gameVersion}</size></i>";
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

            DeleteSteamCloudData($"SaveDataInfo{slot}");
            DeleteSteamCloudData($"MySurvivorList{slot}");
            DeleteSteamCloudData($"LeagueReserveData{slot}");
            DeleteSteamCloudData($"ETCData{slot}");

            ReloadSavedata();
        });
    }

    void DeleteSteamCloudData(string fileName)
    {
        if (SteamRemoteStorage.FileExists(fileName))
        {
            bool success = SteamRemoteStorage.FileDelete(fileName);
            if (success)
                Debug.Log($"{fileName} 삭제 성공 (Steam Cloud)");
            else
                Debug.LogWarning($"{fileName} 삭제 실패");
        }
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
        sortBy.options[2].text = new LocalizedString("Basic", "Name").GetLocalizedString();
        sortBy.captionText.text = sortBy.options[sortBy.value].text;
        sortBy_Characteristic.options[0].text = new LocalizedString("Basic", "Rarity").GetLocalizedString();
        sortBy_Characteristic.options[1].text = new LocalizedString("Basic", "Name").GetLocalizedString();
        sortBy_Characteristic.captionText.text = sortBy_Characteristic.options[sortBy_Characteristic.value].text;
        characteristicAutoNewlineLG.ArrangeCharacteristics();
    }
}
