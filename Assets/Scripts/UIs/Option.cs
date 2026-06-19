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
    [SerializeField] TMP_Dropdown craftQualityDropdown;
    List<GameObject> itemBoxes = new();

    [Space(10)]
    [SerializeField] Image charTabBtn;
    [SerializeField] GameObject tabCharacteristics;
    [SerializeField] GameObject characteristicTable;
    [SerializeField] TMP_Dropdown sortBy_Characteristic;
    [SerializeField] AutoNewLineLayoutGroup characteristicAutoNewlineLG;
    List<GameObject> characteristicBoxes = new();

    [Space(10)]
    [SerializeField] Image trainingTabBtn;
    [SerializeField] GameObject tabTrainings;
    [SerializeField] GameObject trainingTable;
    [SerializeField] TMP_Dropdown sortBy_Training;
    [SerializeField] List<GameObject> trainingBoxes = new();

    [Space(10)]
    [SerializeField] Image achievementsTabBtn;
    [SerializeField] GameObject tabAchievements;
    [SerializeField] GameObject achievementsTable;
    [SerializeField] TMP_Dropdown sortBy_Achievement;
    [SerializeField] List<GameObject> achievementsBoxes;

    [Header("Buttons")]
    [SerializeField] GameObject resume;
    //[SerializeField] Button saveButton;
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

    class TrainingDataForSort : MonoBehaviour
    {
        public TrainingInfo linkedTrainingInfo;

        public void Set(TrainingInfo training)
        {
            linkedTrainingInfo = training;
        }
    }

    class AchievementDataForSort : MonoBehaviour
    {
        public AchievementUIManager.AchievementInfo linkedAchievementInfo;

        public void Set(AchievementUIManager.AchievementInfo achievementInfo)
        {
            this.linkedAchievementInfo = achievementInfo;
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
        //saveButton.gameObject.SetActive(false);
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
                itemImageBox.GetComponentsInChildren<Image>()[^1].sprite = sprt;
                itemImageBox.GetComponentsInChildren<Image>()[0].sprite = GameManager.Instance.GetComponent<InGameUIManager>().craftingQualityOutlines[0];
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

        craftQualityDropdown.options[0].text = new LocalizedString("Basic", "Default").GetLocalizedString();
        craftQualityDropdown.options[1].text = new LocalizedString("Basic", "Poor").GetLocalizedString();
        craftQualityDropdown.options[2].text = new LocalizedString("Basic", "Common").GetLocalizedString();
        craftQualityDropdown.options[3].text = new LocalizedString("Basic", "Fine").GetLocalizedString();
        craftQualityDropdown.options[4].text = new LocalizedString("Basic", "Excellent").GetLocalizedString();
        craftQualityDropdown.options[5].text = new LocalizedString("Basic", "Masterpiece").GetLocalizedString();

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
            if (!CharacteristicManager.UnlockCheck(CharacteristicManager.Characteristics[i].type)) characteristicBox.GetComponentInChildren<Locked>(true).gameObject.SetActive(true);
            characteristicBox.AddComponent<CharacteristicDataForSort>().Set(CharacteristicManager.Characteristics[i].characteristicName, CharacteristicManager.Characteristics[i].rarity);
        }
        characteristicAutoNewlineLG.characteristicsBox = characteristicBoxes.ToArray();
        sortBy_Characteristic.options[0].text = new LocalizedString("Basic", "Name").GetLocalizedString();
        sortBy_Characteristic.options[1].text = new LocalizedString("Basic", "Rarity").GetLocalizedString();

        // Trainings
        for(int i=0; i < TrainingManager.Trainings.Count; i++)
        {
            GameObject trainingBox = PoolManager.Spawn(ResourceEnum.Prefab.Training, trainingTable.transform);
            trainingBoxes.Add(trainingBox);
            trainingBox.GetComponent<Image>().color = TrainingManager.Trainings[i].rarity switch
            {
                TrainingRarity.Common => new Color(0.2984f, 0.8483f, 0.9471f),
                TrainingRarity.Uncommon => new Color(0.8050f, 0.2980f, 0.9490f),
                TrainingRarity.Rare => new Color(0.9490f, 0.8036f, 0.2980f),
                _ => new Color(1, 1, 1)
            };
            if (!TrainingManager.UnlockCheck(TrainingManager.Trainings[i])) trainingBox.GetComponentInChildren<Locked>(true).gameObject.SetActive(true);
            trainingBox.AddComponent<TrainingDataForSort>().Set(TrainingManager.Trainings[i]);
            TrainingInfo training = TrainingManager.Trainings[i];
            trainingBox.GetComponentInChildren<LocalizeStringEvent>().StringReference = training.trainingName;
            if(Enum.TryParse(training.trainingName.TableEntryReference.Key.Replace(" ", ""), out ResourceEnum.Sprite sprite))
            {
                trainingBox.GetComponentsInChildren<Image>()[1].sprite = ResourceManager.Get(sprite);
            }
            else
            {
                trainingBox.GetComponentsInChildren<Image>()[1].sprite = ResourceManager.Get(ResourceEnum.Sprite.Unknown);
            }
            trainingBox.GetComponentInChildren<AspectRatioFitter>().aspectRatio = trainingBox.GetComponentsInChildren<Image>()[1].sprite.rect.width / trainingBox.GetComponentsInChildren<Image>()[1].sprite.rect.height;
            trainingBox.GetComponent<Help>().SetDescription(training.GetTrainingExplain(false));
        }
        sortBy_Training.options[0].text = new LocalizedString("Basic", "Name").GetLocalizedString();
        sortBy_Training.options[1].text = new LocalizedString("Basic", "Rarity").GetLocalizedString();
        sortBy_Training.options[2].text = new LocalizedString("Basic", "Strength").GetLocalizedString();
        sortBy_Training.options[3].text = new LocalizedString("Basic", "Agility").GetLocalizedString();
        sortBy_Training.options[4].text = new LocalizedString("Basic", "Fighting").GetLocalizedString();
        sortBy_Training.options[5].text = new LocalizedString("Basic", "Shooting").GetLocalizedString();
        sortBy_Training.options[6].text = new LocalizedString("Basic", "Crafting").GetLocalizedString();
        sortBy_Training.options[7].text = new LocalizedString("Basic", "Knowledge").GetLocalizedString();
        sortBy_Training.options[8].text = new LocalizedString("Basic", "Stat Total").GetLocalizedString();

        // Achievements
        for(int i=0; i<AchievementUIManager.AchievementInfos.Count; i++)
        {
            GameObject achievementBox = PoolManager.Spawn(ResourceEnum.Prefab.Achievement, achievementsTable.transform);
            achievementsBoxes.Add(achievementBox);
            achievementBox.AddComponent<AchievementDataForSort>().Set(AchievementUIManager.AchievementInfos[i]);
            AchievementUIManager.AchievementInfo achievement = AchievementUIManager.AchievementInfos[i];
            achievementBox.GetComponentInChildren<LocalizeStringEvent>().StringReference = new LocalizedString("Achievement", achievement.achievementKey);
            string parseString = achievement.achievementKey.Replace(" ", "").Replace("-", "");
            if (char.IsDigit(parseString[0])) parseString = "_" + parseString;
            if (Enum.TryParse(parseString, out ResourceEnum.Sprite spriteE))
            {
                if (achievement.Unlocked) achievementBox.GetComponentsInChildren<Image>()[2].sprite = ResourceManager.Get(spriteE);
                else if(Enum.TryParse(parseString + "_unlock", out ResourceEnum.Sprite spriteE_unlock))
                {
                    achievementBox.GetComponentsInChildren<Image>()[2].sprite = ResourceManager.Get(spriteE_unlock);
                }
                else achievementBox.GetComponentsInChildren<Image>()[2].sprite = ResourceManager.Get(ResourceEnum.Sprite.Unknown);
            }
            else achievementBox.GetComponentsInChildren<Image>()[2].sprite = ResourceManager.Get(ResourceEnum.Sprite.Unknown);
            if (!achievement.statsKey.Equals(""))
            {
                // 진척도
                if(achievement.statIsInt)
                {
                    achievementBox.GetComponentsInChildren<TextMeshProUGUI>()[1].text = $"({achievement.GetCurrentStat()} / {achievement.goalStat})";
                    achievementBox.GetComponentsInChildren<Image>()[^1].fillAmount = (float)achievement.GetCurrentStat() / achievement.goalStat;
                }
                else
                {
                    achievementBox.GetComponentsInChildren<TextMeshProUGUI>()[1].text = $"({achievement.GetCurrentStatF()} / {achievement.goalStat})";
                    achievementBox.GetComponentsInChildren<Image>()[^1].fillAmount = achievement.GetCurrentStatF() / achievement.goalStat;
                }
            }
            else
            {
                achievementBox.GetComponentsInChildren<Image>()[3].gameObject.SetActive(false);
            }
            // 해금요소
            if(!achievement.unlockElementName.Equals(""))
            {
                string unlockElement = achievement.unlockElement == AchievementUIManager.UnlockElement.Characteristic ? new LocalizedString("Basic", "Characteristic").GetLocalizedString() : new LocalizedString("Basic", "Training").GetLocalizedString();
                string unlockElementDetail = achievement.unlockElement == AchievementUIManager.UnlockElement.Characteristic ? new LocalizedString("Characteristic", achievement.unlockElementName).GetLocalizedString() : new LocalizedString("Training", achievement.unlockElementName).GetLocalizedString();
                achievementBox.GetComponentsInChildren<TextMeshProUGUI>(true)[2].text = $"{new LocalizedString("Basic", "Unlock").GetLocalizedString()} : {unlockElement} - {unlockElementDetail}";
            }
            else
            {
                achievementBox.GetComponentsInChildren<TextMeshProUGUI>(true)[2].gameObject.SetActive(false);
            }
            // 해금일
            if(SteamManager.Initialized && SteamUserStats.GetAchievementAndUnlockTime(achievement.achievementKey, out bool achived, out uint achievedTime) && achived)
            {
                DateTime date = DateTimeOffset.FromUnixTimeSeconds(achievedTime).LocalDateTime;
                achievementBox.GetComponentsInChildren<TextMeshProUGUI>(true)[3].text = $"{new LocalizedString("Basic", "Unlock Date").GetLocalizedString()} : {date:yyyy-MM-dd}";
            }
            else
            {
                achievementBox.GetComponentsInChildren<TextMeshProUGUI>(true)[3].text = $"({new LocalizedString("Basic", "Locked").GetLocalizedString()})";
            }
            achievementBox.GetComponent<Help>().SetDescription(new LocalizedString("Achievement", $"Help:{achievement.achievementKey}").GetLocalizedString());
            sortBy_Achievement.options[0].text = new LocalizedString("Basic", "Name").GetLocalizedString();
            sortBy_Achievement.options[1].text = new LocalizedString("Basic", "Unlock Date").GetLocalizedString();
        }
        
        ChangeTab(0);
        encyclopedia.SetActive(false);
    }

    public void ChangeTab(int index)
    {
        tabItems.SetActive(index == 0);
        tabCharacteristics.SetActive(index == 1);
        tabTrainings.SetActive(index == 2);
        tabAchievements.SetActive(index == 3);

        itemTabBtn.color = new Color(index == 0 ? 0.75f : 1, 1, 1);
        charTabBtn.color = new Color(index == 1 ? 0.75f : 1, 1, 1);
        trainingTabBtn.color = new Color(index == 2 ? 0.75f : 1, 1, 1);
        achievementsTabBtn.color = new Color(index == 3 ? 0.75f : 1, 1, 1);

        characteristicAutoNewlineLG.ArrangeCharacteristics();
    }

    void SortTable(GameObject wantTable, int sortBy)
    {
        var children = wantTable.transform.Cast<Transform>().ToList();
        List<Transform> sorted = null;
        // sort by - 0 : Item type, 1 : Knowledge required for crafting, 2 : Name, 3 : Rarity
        // 4~9 : Strength, Agility, Fighting, Shooting, Crafting, Knowledge, 10: Stat total
        // 11 : Unlock date
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
                else if(wantTable == characteristicTable) sorted = children.OrderBy(x => x.GetComponent<CharacteristicDataForSort>().localizeName.GetLocalizedString()).ToList();
                else if(wantTable == trainingTable) sorted = children.OrderBy(x => x.GetComponent<TrainingDataForSort>().linkedTrainingInfo.trainingName.GetLocalizedString()).ToList();
                else sorted = children.OrderBy(x => new LocalizedString("Achievement", x.GetComponent<AchievementDataForSort>().linkedAchievementInfo.achievementKey).GetLocalizedString()).ToList();
                break;
            case 3:
                if (wantTable == characteristicTable) sorted = children.OrderBy(x => x.GetComponent<CharacteristicDataForSort>().rarity).ToList();
                else sorted = children.OrderBy(x => x.GetComponent<TrainingDataForSort>().linkedTrainingInfo.rarity).ToList();
                break;
            case 4:
                sorted = children.OrderByDescending(x =>
                {
                    TrainingInfo training = x.GetComponent<TrainingDataForSort>().linkedTrainingInfo;
                    return training.increaseStats.FindIndex(y => y.Item1 == 0) == -1 ? 0 : training.increaseStats.Find(y => y.Item1 == 0).Item2;
                }).ToList();
                break;
            case 5:
                sorted = children.OrderByDescending(x =>
                {
                    TrainingInfo training = x.GetComponent<TrainingDataForSort>().linkedTrainingInfo;
                    return training.increaseStats.FindIndex(y => y.Item1 == 1) == -1 ? 0 : training.increaseStats.Find(y => y.Item1 == 1).Item2;
                }).ToList();
                break;
            case 6:
                sorted = children.OrderByDescending(x =>
                {
                    TrainingInfo training = x.GetComponent<TrainingDataForSort>().linkedTrainingInfo;
                    return training.increaseStats.FindIndex(y => y.Item1 == 2) == -1 ? 0 : training.increaseStats.Find(y => y.Item1 == 2).Item2;
                }).ToList();
                break;
            case 7:
                sorted = children.OrderByDescending(x =>
                {
                    TrainingInfo training = x.GetComponent<TrainingDataForSort>().linkedTrainingInfo;
                    return training.increaseStats.FindIndex(y => y.Item1 == 3) == -1 ? 0 : training.increaseStats.Find(y => y.Item1 == 3).Item2;
                }).ToList();
                break;
            case 8:
                sorted = children.OrderByDescending(x =>
                {
                    TrainingInfo training = x.GetComponent<TrainingDataForSort>().linkedTrainingInfo;
                    return training.increaseStats.FindIndex(y => y.Item1 == 4) == -1 ? 0 : training.increaseStats.Find(y => y.Item1 == 4).Item2;
                }).ToList();
                break;
            case 9:
                sorted = children.OrderByDescending(x =>
                {
                    TrainingInfo training = x.GetComponent<TrainingDataForSort>().linkedTrainingInfo;
                    return training.increaseStats.FindIndex(y => y.Item1 == 5) == -1 ? 0 : training.increaseStats.Find(y => y.Item1 == 5).Item2;
                }).ToList();
                break;
            case 10:
                sorted = children.OrderByDescending(x =>
                {
                    int total = 0;
                    TrainingInfo training = x.GetComponent<TrainingDataForSort>().linkedTrainingInfo;
                    foreach(var stat in training.increaseStats) total += stat.Item2;
                    return total;
                }).ToList();
                break;
            case 11:
                sorted = children.OrderByDescending(x => 
                {
                    var achievement = x.GetComponent<AchievementDataForSort>().linkedAchievementInfo;
                    if (SteamManager.Initialized && SteamUserStats.GetAchievementAndUnlockTime(achievement.achievementKey, out bool achived, out uint achievedTime) && achived)
                    {
                        return (int)achievedTime;
                    }
                    else return -1;
                }).ToList();
                break;
        }
        for (int i = 0; i < sorted.Count; i++)
        {
            sorted[i].SetSiblingIndex(i);
        }
        characteristicAutoNewlineLG.ArrangeCharacteristics();
    }

    public void ChangeViewCraftableQuality()
    {
        foreach(var itemBox in itemBoxes)
        {
            if(ItemManager.CheckUseQuality(itemBox.GetComponent<ItemDataForSort>().itemType))
            {
                itemBox.GetComponentsInChildren<Image>()[0].sprite = GameManager.Instance.GetComponent<InGameUIManager>().craftingQualityOutlines[craftQualityDropdown.value];
                itemBox.GetComponent<Help>().SetDescription(itemBox.GetComponent<ItemDataForSort>().itemType, (CraftingQuality)craftQualityDropdown.value);
            }
        }
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
        else if (table == 1)
        {
            wantTable = characteristicTable;
            sortingOrder = sortBy_Characteristic.value + 1;
        }
        else if (table == 2)
        {
            wantTable = trainingTable;
            sortingOrder = sortBy_Training.value + 2;
        }
        else
        {
            wantTable = achievementsTable;
            sortingOrder = sortBy_Achievement.value == 0 ? 2 : 11;
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
        for(int i=0; i<characteristicBoxes.Count; i++) characteristicBoxes[i].GetComponentInChildren<Locked>(true).gameObject.SetActive(!CharacteristicManager.UnlockCheck(CharacteristicManager.Characteristics[i].type));
        for(int i=0; i<trainingBoxes.Count; i++) trainingBoxes[i].GetComponentInChildren<Locked>(true).gameObject.SetActive(!TrainingManager.UnlockCheck(TrainingManager.Trainings[i]));
        // 진척도 갱신
        for(int i=0; i<achievementsBoxes.Count; i++)
        {
            var achievement = achievementsBoxes[i].GetComponent<AchievementDataForSort>().linkedAchievementInfo;
            if (!achievement.statsKey.Equals(""))
            {
                if (achievement.statIsInt)
                {
                    achievementsBoxes[i].GetComponentsInChildren<TextMeshProUGUI>()[1].text = $"({achievement.GetCurrentStat()} / {achievement.goalStat})";
                    achievementsBoxes[i].GetComponentsInChildren<Image>()[^1].fillAmount = (float)achievement.GetCurrentStat() / achievement.goalStat;
                }
                else
                {
                    achievementsBoxes[i].GetComponentsInChildren<TextMeshProUGUI>()[1].text = $"({achievement.GetCurrentStatF()} / {achievement.goalStat})";
                    achievementsBoxes[i].GetComponentsInChildren<Image>()[^1].fillAmount = achievement.GetCurrentStatF() / achievement.goalStat;
                }
            }
        }
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
                if(i > 0)
                {
                    DeleteSaveData(i);
                    continue;
                }

                var saveData = JsonUtility.FromJson<SaveDataInfo>(json);
                if (saveData.gameVersion.Split('.')[0] == "1") DeleteSaveData(0);
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
        //GameManager.Instance.OutGameUIManager.OpenConfirmWindow("Confirm:Delete Save Data", () =>
        //{
            PlayerPrefs.DeleteKey($"SaveDataInfo{slot}");
            PlayerPrefs.DeleteKey($"MySurvivorList{slot}");
            PlayerPrefs.DeleteKey($"LeagueReserveData{slot}");
            PlayerPrefs.DeleteKey($"ETCData{slot}");

            DeleteSteamCloudData($"SaveDataInfo{slot}");
            DeleteSteamCloudData($"MySurvivorList{slot}");
            DeleteSteamCloudData($"LeagueReserveData{slot}");
            DeleteSteamCloudData($"ETCData{slot}");

            ReloadSavedata();
        //});
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
            //saveButton.gameObject.SetActive(true);
            goTitle.SetActive(true);
        }
        //saveButton.interactable = interactable;
    }

    public void GoTitle(bool ask)
    {
        if(ask)
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
        //saveButton.gameObject.SetActive(false);
        goTitle.SetActive(false);
        GameManager.Instance.CheckSaveData();
        GameManager.Instance.optionCanvas.SetActive(false);
        GameManager.Instance.Title.title.SetActive(true);
    }

    public void GiveUp()
    {
        GameManager.Instance.OutGameUIManager.OpenConfirmWindow("Confirm:Give Up Challenge", () =>
        {
            GameManager.Instance.GetComponent<GameResult>().gameOverMessage.StringReference = new("Basic", "GameOver:Give Up");
            GameManager.Instance.GetComponent<GameResult>().GameOver();
            option.SetActive(false);
        });
    }

    void OnLocaleChanged(Locale newLocale)
    {
        foreach (var itemBox in itemBoxes)
        {
            itemBox.GetComponent<Help>().SetDescription(itemBox.GetComponent<ItemDataForSort>().itemType);
        }
        foreach (var achievementBox in achievementsBoxes)
        {
            var achievement = achievementBox.GetComponent<AchievementDataForSort>().linkedAchievementInfo;
            // 해금요소
            if (!achievement.unlockElementName.Equals(""))
            {
                string unlockElement = achievement.unlockElement == AchievementUIManager.UnlockElement.Characteristic ? new LocalizedString("Basic", "Characteristic").GetLocalizedString() : new LocalizedString("Basic", "Training").GetLocalizedString();
                string unlockElementDetail = achievement.unlockElement == AchievementUIManager.UnlockElement.Characteristic ? new LocalizedString("Characteristic", achievement.unlockElementName).GetLocalizedString() : new LocalizedString("Training", achievement.unlockElementName).GetLocalizedString();
                achievementBox.GetComponentsInChildren<TextMeshProUGUI>(true)[2].text = $"{new LocalizedString("Basic", "Unlock").GetLocalizedString()} : {unlockElement} - {unlockElementDetail}";
            }
            else
            {
                achievementBox.GetComponentsInChildren<TextMeshProUGUI>(true)[2].gameObject.SetActive(false);
            }
            // 해금일
            if (SteamManager.Initialized && SteamUserStats.GetAchievementAndUnlockTime(achievement.achievementKey, out bool achived, out uint achivedTime) && achived)
            {
                DateTime date = DateTimeOffset.FromUnixTimeSeconds(achivedTime).LocalDateTime;
                achievementBox.GetComponentsInChildren<TextMeshProUGUI>(true)[3].text = $"{new LocalizedString("Basic", "Unlock Date").GetLocalizedString()} : {date:yyyy-mm-dd}";

            }
            else
            {
                achievementBox.GetComponentsInChildren<TextMeshProUGUI>(true)[3].text = $"({new LocalizedString("Basic", "Locked").GetLocalizedString()})";
            }
            achievementBox.GetComponent<Help>().SetDescription(new LocalizedString("Achievement", $"Help:{achievement.achievementKey}").GetLocalizedString());
        }
        ReloadSavedata();
        sortBy.options[0].text = new LocalizedString("Basic", "Item Type").GetLocalizedString();
        sortBy.options[1].text = new LocalizedString("Item", "Required Knowledge").GetLocalizedString();
        sortBy.options[2].text = new LocalizedString("Basic", "Name").GetLocalizedString();
        sortBy.captionText.text = sortBy.options[sortBy.value].text;
        sortBy_Characteristic.options[0].text = new LocalizedString("Basic", "Name").GetLocalizedString();
        sortBy_Characteristic.options[1].text = new LocalizedString("Basic", "Rarity").GetLocalizedString();
        sortBy_Characteristic.captionText.text = sortBy_Characteristic.options[sortBy_Characteristic.value].text;
        sortBy_Training.options[0].text = new LocalizedString("Basic", "Name").GetLocalizedString();
        sortBy_Training.options[1].text = new LocalizedString("Basic", "Rarity").GetLocalizedString();
        sortBy_Training.options[2].text = new LocalizedString("Basic", "Strength").GetLocalizedString();
        sortBy_Training.options[3].text = new LocalizedString("Basic", "Agility").GetLocalizedString();
        sortBy_Training.options[4].text = new LocalizedString("Basic", "Fighting").GetLocalizedString();
        sortBy_Training.options[5].text = new LocalizedString("Basic", "Shooting").GetLocalizedString();
        sortBy_Training.options[6].text = new LocalizedString("Basic", "Crafting").GetLocalizedString();
        sortBy_Training.options[7].text = new LocalizedString("Basic", "Knowledge").GetLocalizedString();
        sortBy_Training.options[8].text = new LocalizedString("Basic", "Stat Total").GetLocalizedString();
        sortBy_Training.captionText.text = sortBy_Training.options[sortBy_Training.value].text;
        sortBy_Achievement.options[0].text = new LocalizedString("Basic", "Name").GetLocalizedString();
        sortBy_Achievement.options[1].text = new LocalizedString("Basic", "Unlock Date").GetLocalizedString();
        sortBy_Achievement.captionText.text = sortBy_Achievement.options[sortBy_Achievement.value].text;
        characteristicAutoNewlineLG.ArrangeCharacteristics();
    }
}
