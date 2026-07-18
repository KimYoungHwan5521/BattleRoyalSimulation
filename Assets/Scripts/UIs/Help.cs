using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Localization;

public class Help : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    RectTransform rect;
    [SerializeField, TextArea] string description;
    bool raw;

    private void Start()
    {
        rect = GetComponent<RectTransform>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!string.IsNullOrWhiteSpace(description))
        {
            RectTransform descriptionRT = GameManager.Instance.description.GetComponent<RectTransform>();
            if(raw) GameManager.Instance.description.GetComponent<Description>().SetRawText(description);
            else GameManager.Instance.description.GetComponent<Description>().SetText(description);
            descriptionRT.position =
                    new(Mathf.Clamp(Input.mousePosition.x + 25, 0, Screen.width - descriptionRT.rect.width - 5),
                    Mathf.Clamp(Input.mousePosition.y - 25, descriptionRT.rect.height + 5, Screen.height));
            descriptionRT.gameObject.SetActive(true);
            GameManager.Instance.FixLayout(descriptionRT);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        GameManager.Instance.description.SetActive(false);
    }

    public void SetDescription(string description, bool raw = true)
    {
        this.raw = raw;
        this.description = description;
    }

    public void SetDescription(SurgeryType surgeryType, InjurySite site)
    {
        raw = true;
        string key = "";
        if (surgeryType == SurgeryType.RecoverySerumAdministeration) key = "Help:RecoverySerumAdministeration";
        else if(surgeryType == SurgeryType.ArtificialPartTransplant)
        {
            if (site == InjurySite.RightEye || site == InjurySite.LeftEye) key = "Help:ArtificialPartTransplant_Eye";
            else key = "Help:ArtificialPartTransplant";
        }
        else if (surgeryType == SurgeryType.AugmentedPartTransplant)
        {
            if (site == InjurySite.RightEye || site == InjurySite.LeftEye) key = "Help:AugmentedPartTransplant_Eye";
            else key = "Help:AugmentedPartTransplant";
        }
        else if (surgeryType == SurgeryType.TrancendantPartTransplant)
        {
            key = "Help:TrancendantPartTransplant";
        }
        description = new LocalizedString("Injury", key).GetLocalizedString();
    }

    public void SetDescription(LocalizedString description)
    {
        raw = true;
        this.description = description.GetLocalizedString();
    }

    public void SetDescriptionWithKey(string key, params string[] vars)
    {
        raw = true;
        var localizedString = new LocalizedString("Basic", key);
        switch (vars.Length)
        {
            case 0:
                break;
            case 1:
                localizedString.Arguments
                    = new[] { new { param0 = vars[0] } };
                break;
            case 2:
                localizedString.Arguments
                    = new[] { new { param0 = vars[0], param1 = vars[1] } };
                break;
            default:
                Debug.Log("To many params");
                break;
        }
        //LocalizeStringEvent localizeStringEvent = GameManager.Instance.description.GetComponentInChildren<LocalizeStringEvent>();
        //localizeStringEvent.StringReference = localizedString;
        //localizeStringEvent.RefreshString();
        description = localizedString.GetLocalizedString();
    }

    public void SetDescription(InjurySite injurySite)
    {
        switch (injurySite)
        {
            default:
                description = "";
                break;
        }
    }

    public void SetDescription(ItemManager.Items item, CraftingQuality quality = CraftingQuality.NotCrafted)
    {
        raw = true;
        //description = new LocalizedString("Item", $"Help:{item}").GetLocalizedString();
        description = $"<b>{new LocalizedString("Item", item.ToString()).GetLocalizedString()}</b>";
        description += quality switch
        {
            CraftingQuality.Masterpiece => $"\n<color=#F09310>{new LocalizedString("Basic", "Masterpiece").GetLocalizedString()}</color>",
            CraftingQuality.Excellent => $"\n<color=#B527C2>{new LocalizedString("Basic", "Excellent").GetLocalizedString()}</color>",
            CraftingQuality.Average => $"\n<color=#1A8EF6>{new LocalizedString("Basic", "Average").GetLocalizedString()}</color>",
            CraftingQuality.Shoddy => $"\n<color=#60CC3F>{new LocalizedString("Basic", "Shoddy").GetLocalizedString()}</color>",
            CraftingQuality.Botched => $"\n<color=#7F7F7F>{new LocalizedString("Basic", "Botched").GetLocalizedString()}</color>",
          _ => "",
        };
        description += item switch
        {
            ItemManager.Items.NotValid => throw new System.NotImplementedException(),
            ItemManager.Items.Knife or ItemManager.Items.Knife_Enchanted => $"\n{new LocalizedString("Basic", "Melee weapon").GetLocalizedString()}\n{new LocalizedString("Item", "Damage").GetLocalizedString()} : 30\n{new LocalizedString("Item", "Weight").GetLocalizedString()} : 1\n{new LocalizedString("Item", "Required Strength").GetLocalizedString()} : 0",
            ItemManager.Items.Dagger or ItemManager.Items.Dagger_Enchanted => $"\n{new LocalizedString("Basic", "Melee weapon").GetLocalizedString()}\n{new LocalizedString("Item", "Damage").GetLocalizedString()} : 40\n{new LocalizedString("Item", "Weight").GetLocalizedString()} : 1\n{new LocalizedString("Item", "Required Strength").GetLocalizedString()} : 0",
            ItemManager.Items.Bat => $"\n{new LocalizedString("Basic", "Melee weapon").GetLocalizedString()}\n{new LocalizedString("Item", "Damage").GetLocalizedString()} : 35\n{new LocalizedString("Item", "Weight").GetLocalizedString()} : 2\n{new LocalizedString("Item", "Required Strength").GetLocalizedString()} : 0",
            ItemManager.Items.LongSword or ItemManager.Items.LongSword_Enchanted => $"\n{new LocalizedString("Basic", "Melee weapon").GetLocalizedString()}\n{new LocalizedString("Item", "Damage").GetLocalizedString()} : 50\n{new LocalizedString("Item", "Weight").GetLocalizedString()} : 2\n{new LocalizedString("Item", "Required Strength").GetLocalizedString()} : 0",
            ItemManager.Items.Shovel => $"\n{new LocalizedString("Basic", "Melee weapon").GetLocalizedString()}\n{new LocalizedString("Item", "Damage").GetLocalizedString()} : 45\n{new LocalizedString("Item", "Weight").GetLocalizedString()} : 2\n{new LocalizedString("Item", "Required Strength").GetLocalizedString()} : 0",
            ItemManager.Items.Bow => $"\n{new LocalizedString("Basic", "Ranged weapon").GetLocalizedString()}\n{new LocalizedString("Item", "Max Damage").GetLocalizedString()} : {quality switch{CraftingQuality.Masterpiece => 25,CraftingQuality.Excellent => 22.5f,CraftingQuality.Shoddy => 17.5f,CraftingQuality.Botched => 15,_ => 20}}\n{new LocalizedString("Item", "Attack Range").GetLocalizedString()} : {quality switch { CraftingQuality.Masterpiece => 36, CraftingQuality.Excellent => 33, CraftingQuality.Shoddy => 27, CraftingQuality.Botched => 24, _ => 30 }}\n{new LocalizedString("Item", "Weight").GetLocalizedString()} : 1\n{new LocalizedString("Item", "Required Strength").GetLocalizedString()} : 0",
            ItemManager.Items.AdvancedBow => $"\n{new LocalizedString("Basic", "Ranged weapon").GetLocalizedString()}\n{new LocalizedString("Item", "Max Damage").GetLocalizedString()} : {quality switch { CraftingQuality.Masterpiece => 75, CraftingQuality.Excellent => 67.5f, CraftingQuality.Shoddy => 52.5f, CraftingQuality.Botched => 45, _ => 60 }}\n{new LocalizedString("Item", "Attack Range").GetLocalizedString()} : {quality switch { CraftingQuality.Masterpiece => 80, CraftingQuality.Excellent => 70, CraftingQuality.Shoddy => 50, CraftingQuality.Botched => 40, _ => 60 }}\n{new LocalizedString("Item", "Weight").GetLocalizedString()} : 3\n{new LocalizedString("Item", "Required Strength").GetLocalizedString()} : 0",
            ItemManager.Items.Revolver => $"\n{new LocalizedString("Basic", "Ranged weapon").GetLocalizedString()}\n{new LocalizedString("Item", "Max Damage").GetLocalizedString()} : {quality switch { CraftingQuality.Masterpiece => 110, CraftingQuality.Excellent => 95, CraftingQuality.Shoddy => 65, CraftingQuality.Botched => 50, _ => 80 }}\n{new LocalizedString("Item", "Attack Range").GetLocalizedString()} : {quality switch { CraftingQuality.Masterpiece => 25, CraftingQuality.Excellent => 22.5f, CraftingQuality.Shoddy => 17.5f, CraftingQuality.Botched => 15, _ => 20 }}\n{new LocalizedString("Item", "Magazine Capacity").GetLocalizedString()} : 7\n{new LocalizedString("Item", "Weight").GetLocalizedString()} : 1\n{new LocalizedString("Item", "Required Strength").GetLocalizedString()} : 0",
            ItemManager.Items.Pistol => $"\n{new LocalizedString("Basic", "Ranged weapon").GetLocalizedString()}\n{new LocalizedString("Item", "Max Damage").GetLocalizedString()} : {quality switch { CraftingQuality.Masterpiece => 60, CraftingQuality.Excellent => 50, CraftingQuality.Shoddy => 30, CraftingQuality.Botched => 20, _ => 40 }}\n{new LocalizedString("Item", "Attack Range").GetLocalizedString()} : {quality switch { CraftingQuality.Masterpiece => 25, CraftingQuality.Excellent => 22.5f, CraftingQuality.Shoddy => 17.5f, CraftingQuality.Botched => 15, _ => 20 }}\n{new LocalizedString("Item", "Magazine Capacity").GetLocalizedString()} : 17\n{new LocalizedString("Item", "Weight").GetLocalizedString()} : 1\n{new LocalizedString("Item", "Required Strength").GetLocalizedString()} : 0",
            ItemManager.Items.AssaultRifle => $"\n{new LocalizedString("Basic", "Ranged weapon").GetLocalizedString()}\n{new LocalizedString("Item", "Max Damage").GetLocalizedString()} : {quality switch { CraftingQuality.Masterpiece => 80, CraftingQuality.Excellent => 70, CraftingQuality.Shoddy => 50, CraftingQuality.Botched => 40, _ => 60 }}\n{new LocalizedString("Item", "Attack Range").GetLocalizedString()} : {quality switch { CraftingQuality.Masterpiece => 60, CraftingQuality.Excellent => 55, CraftingQuality.Shoddy => 45, CraftingQuality.Botched => 40, _ => 50 }}\n{new LocalizedString("Item", "Magazine Capacity").GetLocalizedString()} : 30\n{new LocalizedString("Item", "Weight").GetLocalizedString()} : 4\n{new LocalizedString("Item", "Required Strength").GetLocalizedString()} : 0",
            ItemManager.Items.SubMachineGun => $"\n{new LocalizedString("Basic", "Ranged weapon").GetLocalizedString()}\n{new LocalizedString("Item", "Max Damage").GetLocalizedString()} : {quality switch { CraftingQuality.Masterpiece => 60, CraftingQuality.Excellent => 50, CraftingQuality.Shoddy => 30, CraftingQuality.Botched => 20, _ => 40 }}\n{new LocalizedString("Item", "Attack Range").GetLocalizedString()} : {quality switch { CraftingQuality.Masterpiece => 31, CraftingQuality.Excellent => 28, CraftingQuality.Shoddy => 22, CraftingQuality.Botched => 19, _ => 25 }}\n{new LocalizedString("Item", "Magazine Capacity").GetLocalizedString()} : 30\n{new LocalizedString("Item", "Weight").GetLocalizedString()} : 3\n{new LocalizedString("Item", "Required Strength").GetLocalizedString()} : 0",
            ItemManager.Items.ShotGun => $"\n{new LocalizedString("Basic", "Ranged weapon").GetLocalizedString()}\n{new LocalizedString("Item", "Max Damage").GetLocalizedString()} : {quality switch { CraftingQuality.Masterpiece => 60, CraftingQuality.Excellent => 50, CraftingQuality.Shoddy => 30, CraftingQuality.Botched => 20, _ => 40 }}x12\n{new LocalizedString("Item", "Attack Range").GetLocalizedString()} : {quality switch { CraftingQuality.Masterpiece => 25, CraftingQuality.Excellent => 22.5f, CraftingQuality.Shoddy => 17.5f, CraftingQuality.Botched => 15, _ => 20 }}\n{new LocalizedString("Item", "Magazine Capacity").GetLocalizedString()} : 4\n{new LocalizedString("Item", "Weight").GetLocalizedString()} : 3\n{new LocalizedString("Item", "Required Strength").GetLocalizedString()} : 0",
            ItemManager.Items.SniperRifle => $"\n{new LocalizedString("Basic", "Ranged weapon").GetLocalizedString()}\n{new LocalizedString("Item", "Max Damage").GetLocalizedString()} : {quality switch { CraftingQuality.Masterpiece => 300, CraftingQuality.Excellent => 250, CraftingQuality.Shoddy => 150, CraftingQuality.Botched => 100, _ => 200 }}\n{new LocalizedString("Item", "Attack Range").GetLocalizedString()} : {quality switch { CraftingQuality.Masterpiece => 130, CraftingQuality.Excellent => 110, CraftingQuality.Shoddy => 70, CraftingQuality.Botched => 50, _ => 90 }}\n{new LocalizedString("Item", "Magazine Capacity").GetLocalizedString()} : 5\n{new LocalizedString("Item", "Weight").GetLocalizedString()} : 4\n{new LocalizedString("Item", "Required Strength").GetLocalizedString()} : 0",
            ItemManager.Items.Bazooka => $"\n{new LocalizedString("Basic", "Ranged weapon").GetLocalizedString()}\n{new LocalizedString("Item", "Max Damage").GetLocalizedString()} : {quality switch { CraftingQuality.Masterpiece => 600, CraftingQuality.Excellent => 500, CraftingQuality.Shoddy => 300, CraftingQuality.Botched => 200, _ => 400 }}\n{new LocalizedString("Item", "Attack Range").GetLocalizedString()} : {quality switch { CraftingQuality.Masterpiece => 50, CraftingQuality.Excellent => 45, CraftingQuality.Shoddy => 35, CraftingQuality.Botched => 30, _ => 40 }}\n{new LocalizedString("Item", "Magazine Capacity").GetLocalizedString()} : 1\n{new LocalizedString("Item", "Weight").GetLocalizedString()} : 8\n{new LocalizedString("Item", "Required Strength").GetLocalizedString()} : 40",
            ItemManager.Items.LASER => $"\n{new LocalizedString("Basic", "Ranged weapon").GetLocalizedString()}\n{new LocalizedString("Item", "Damage").GetLocalizedString()} : {quality switch { CraftingQuality.Masterpiece => 150, CraftingQuality.Excellent => 125, CraftingQuality.Shoddy => 90, CraftingQuality.Botched => 80, _ => 100 }}\n{new LocalizedString("Item", "Attack Range").GetLocalizedString()} : {quality switch { CraftingQuality.Masterpiece => 55, CraftingQuality.Excellent => 50, CraftingQuality.Shoddy => 40, CraftingQuality.Botched => 35, _ => 45 }}\n{new LocalizedString("Item", "Magazine Capacity").GetLocalizedString()} : ¡Ä\n{new LocalizedString("Item", "Weight").GetLocalizedString()} : 1\n{new LocalizedString("Item", "Required Strength").GetLocalizedString()} : 0",
            ItemManager.Items.Arrow or ItemManager.Items.Arrow_Enchanted=> $"\n{new LocalizedString("Item", "Bullet").GetLocalizedString()}",
            ItemManager.Items.Bullet_Revolver => $"\n{new LocalizedString("Item", "Bullet").GetLocalizedString()}",
            ItemManager.Items.Bullet_Pistol => $"\n{new LocalizedString("Item", "Bullet").GetLocalizedString()}",
            ItemManager.Items.Bullet_AssaultRifle => $"\n{new LocalizedString("Item", "Bullet").GetLocalizedString()}",
            ItemManager.Items.Bullet_SubMachineGun => $"\n{new LocalizedString("Item", "Bullet").GetLocalizedString()}",
            ItemManager.Items.Bullet_ShotGun => $"\n{new LocalizedString("Item", "Bullet").GetLocalizedString()}",
            ItemManager.Items.Bullet_SniperRifle => $"\n{new LocalizedString("Item", "Bullet").GetLocalizedString()}",
            ItemManager.Items.Rocket_Bazooka => $"\n{new LocalizedString("Item", "Bullet").GetLocalizedString()}",
            ItemManager.Items.LowLevelBulletproofHelmet => $"\n{new LocalizedString("Item", "Helmet").GetLocalizedString()}\n{new LocalizedString("Item", "Armor").GetLocalizedString()} : {quality switch { CraftingQuality.Masterpiece => 30, CraftingQuality.Excellent => 25, CraftingQuality.Shoddy => 15, CraftingQuality.Botched => 10, _ => 20 }}\n{new LocalizedString("Item", "Weight").GetLocalizedString()} : 1",
            ItemManager.Items.MiddleLevelBulletproofHelmet => $"\n{new LocalizedString("Item", "Helmet").GetLocalizedString()}\n{new LocalizedString("Item", "Armor").GetLocalizedString()} : {quality switch { CraftingQuality.Masterpiece => 50, CraftingQuality.Excellent => 45, CraftingQuality.Shoddy => 35, CraftingQuality.Botched => 30, _ => 40 }}\n{new LocalizedString("Item", "Weight").GetLocalizedString()} : 1",
            ItemManager.Items.HighLevelBulletproofHelmet => $"\n{new LocalizedString("Item", "Helmet").GetLocalizedString()}\n{new LocalizedString("Item", "Armor").GetLocalizedString()} : {quality switch { CraftingQuality.Masterpiece => 70, CraftingQuality.Excellent => 65, CraftingQuality.Shoddy => 55, CraftingQuality.Botched => 50, _ => 60 }}\n{new LocalizedString("Item", "Weight").GetLocalizedString()} : 1",
            ItemManager.Items.LegendaryBulletproofHelmet => $"\n{new LocalizedString("Item", "Helmet").GetLocalizedString()}\n{new LocalizedString("Item", "Armor").GetLocalizedString()} : {quality switch { CraftingQuality.Masterpiece => 300, CraftingQuality.Excellent => 250, CraftingQuality.Shoddy => 150, CraftingQuality.Botched => 100, _ => 200 }}\n{new LocalizedString("Item", "Weight").GetLocalizedString()} : 3",
            ItemManager.Items.LowLevelBulletproofVest => $"\n{new LocalizedString("Item", "Vest").GetLocalizedString()}\n{new LocalizedString("Item", "Armor").GetLocalizedString()} : {quality switch { CraftingQuality.Masterpiece => 20, CraftingQuality.Excellent => 15, CraftingQuality.Shoddy => 7.5f, CraftingQuality.Botched => 5, _ => 10 }}\n{new LocalizedString("Item", "Weight").GetLocalizedString()} : 3",
            ItemManager.Items.MiddleLevelBulletproofVest => $"\n{new LocalizedString("Item", "Vest").GetLocalizedString()}\n{new LocalizedString("Item", "Armor").GetLocalizedString()} : {quality switch { CraftingQuality.Masterpiece => 40, CraftingQuality.Excellent => 35, CraftingQuality.Shoddy => 25, CraftingQuality.Botched => 20, _ => 30 }}\n{new LocalizedString("Item", "Weight").GetLocalizedString()} : 6",
            ItemManager.Items.HighLevelBulletproofVest => $"\n{new LocalizedString("Item", "Vest").GetLocalizedString()}\n{new LocalizedString("Item", "Armor").GetLocalizedString()} : {quality switch { CraftingQuality.Masterpiece => 70, CraftingQuality.Excellent => 60, CraftingQuality.Shoddy => 45, CraftingQuality.Botched => 40, _ => 50 }}\n{new LocalizedString("Item", "Weight").GetLocalizedString()} : 9",
            ItemManager.Items.LegendaryBulletproofVest => $"\n{new LocalizedString("Item", "Vest").GetLocalizedString()}\n{new LocalizedString("Item", "Armor").GetLocalizedString()} : {quality switch { CraftingQuality.Masterpiece => 150, CraftingQuality.Excellent => 125, CraftingQuality.Shoddy => 85, CraftingQuality.Botched => 70, _ => 100 }}\n{new LocalizedString("Item", "Weight").GetLocalizedString()} : 12",
            ItemManager.Items.BandageRoll => $"\n{new LocalizedString("Item", "Consumable").GetLocalizedString()}\n{new LocalizedString("Item", "Bleeding Control").GetLocalizedString()} : 100",
            ItemManager.Items.HemostaticBandageRoll => $"\n{new LocalizedString("Item", "Consumable").GetLocalizedString()}\n{new LocalizedString("Item", "Bleeding Control").GetLocalizedString()} : 300",
            ItemManager.Items.Poison => $"\n{new LocalizedString("Item", "Consumable").GetLocalizedString()}\n{new LocalizedString("Item", $"Explain:{item}").GetLocalizedString()}",
            ItemManager.Items.Antidote => $"\n{new LocalizedString("Item", "Consumable").GetLocalizedString()}\n{new LocalizedString("Item", $"Explain:{item}").GetLocalizedString()}",
            ItemManager.Items.Potion => $"\n{new LocalizedString("Item", "Consumable").GetLocalizedString()}\n{new LocalizedString("Item", "Health Recovery").GetLocalizedString()} : {quality switch { CraftingQuality.Masterpiece => 70, CraftingQuality.Excellent => 60, CraftingQuality.Shoddy => 40, CraftingQuality.Botched => 30, _ => 50 }}",
            ItemManager.Items.AdvancedPotion => $"\n{new LocalizedString("Item", "Consumable").GetLocalizedString()}\n{new LocalizedString("Item", "Health Recovery").GetLocalizedString()} : {quality switch { CraftingQuality.Masterpiece => 150, CraftingQuality.Excellent => 125, CraftingQuality.Shoddy => 85, CraftingQuality.Botched => 70, _ => 100 }}",
            ItemManager.Items.Components => $"\n{new LocalizedString("Item", "Crafting Materials").GetLocalizedString()}",
            ItemManager.Items.AdvancedComponent => $"\n{new LocalizedString("Item", "Crafting Materials").GetLocalizedString()}",
            ItemManager.Items.Chemicals => $"\n{new LocalizedString("Item", "Crafting Materials").GetLocalizedString()}",
            ItemManager.Items.Gunpowder => $"\n{new LocalizedString("Item", "Crafting Materials").GetLocalizedString()}",
            ItemManager.Items.Salvages => $"\n{new LocalizedString("Item", "Crafting Materials").GetLocalizedString()}",
            ItemManager.Items.BearTrap or ItemManager.Items.BearTrap_Enchanted => $"\n{new LocalizedString("Item", "Trap").GetLocalizedString()}\n{new LocalizedString("Item", "Damage").GetLocalizedString()} : {quality switch { CraftingQuality.Masterpiece => 60, CraftingQuality.Excellent => 50, CraftingQuality.Shoddy => 30, CraftingQuality.Botched => 20, _ => 40 }}",
            ItemManager.Items.LandMine => $"\n{new LocalizedString("Item", "Trap").GetLocalizedString()}\n{new LocalizedString("Item", "Damage").GetLocalizedString()} : {quality switch { CraftingQuality.Masterpiece => 140, CraftingQuality.Excellent => 120, CraftingQuality.Shoddy => 80, CraftingQuality.Botched => 60, _ => 100 }}",
            ItemManager.Items.NoiseTrap => $"\n{new LocalizedString("Item", "Trap").GetLocalizedString()}\n{new LocalizedString("Item", $"Explain:{item}").GetLocalizedString()}",
            ItemManager.Items.ChemicalTrap => $"\n{new LocalizedString("Item", "Trap").GetLocalizedString()}\n{new LocalizedString("Item", $"Explain:{item}").GetLocalizedString()}",
            ItemManager.Items.ShrapnelTrap => $"\n{new LocalizedString("Item", "Trap").GetLocalizedString()}\n{new LocalizedString("Item", "Damage").GetLocalizedString()} : {quality switch { CraftingQuality.Masterpiece => 60, CraftingQuality.Excellent => 50, CraftingQuality.Shoddy => 30, CraftingQuality.Botched => 20, _ => 40 }}(x48)",
            ItemManager.Items.ExplosiveTrap => $"\n{new LocalizedString("Item", "Trap").GetLocalizedString()}\n{new LocalizedString("Item", "Damage").GetLocalizedString()} : {quality switch { CraftingQuality.Masterpiece => 140, CraftingQuality.Excellent => 120, CraftingQuality.Shoddy => 80, CraftingQuality.Botched => 60, _ => 100 }}",
            ItemManager.Items.WalkingAid => $"\n{new LocalizedString("Item", "ETC").GetLocalizedString()}\n{new LocalizedString("Item", $"Explain:{item}").GetLocalizedString()}",
            ItemManager.Items.TrapDetectionDevice => $"\n{new LocalizedString("Item", "ETC").GetLocalizedString()}\n{new LocalizedString("Basic", "Detection Range").GetLocalizedString()} : {quality switch { CraftingQuality.Masterpiece => 2.5f, CraftingQuality.Excellent => 2, CraftingQuality.Shoddy => 1.25f, CraftingQuality.Botched => 1, _ => 1.5f }}\n{new LocalizedString("Item", $"Explain:{item}").GetLocalizedString()}",
            ItemManager.Items.BiometricRader => $"\n{new LocalizedString("Item", "ETC").GetLocalizedString()}\n{new LocalizedString("Basic", "Detection Range").GetLocalizedString()} : {quality switch { CraftingQuality.Masterpiece => 20, CraftingQuality.Excellent => 17.5f, CraftingQuality.Shoddy => 12.5f, CraftingQuality.Botched => 10, _ => 15 }}\n{new LocalizedString("Item", $"Explain:{item}").GetLocalizedString()}",
            ItemManager.Items.EnergyBarrier => $"\n{new LocalizedString("Item", "ETC").GetLocalizedString()}\n{new LocalizedString("Basic", "Defense Rate").GetLocalizedString()} : {quality switch { CraftingQuality.Masterpiece => 70, CraftingQuality.Excellent => 60, CraftingQuality.Shoddy => 40, CraftingQuality.Botched => 30, _ => 50 }}%\n{new LocalizedString("Item", $"Explain:{item}").GetLocalizedString()}",
            _ => throw new System.NotImplementedException()
        };
        var craftable = ItemManager.craftables.Find(x => x.itemType == item);
        if (craftable == null)
        {
            description += $"\n{new LocalizedString("Item", "Uncraftable").GetLocalizedString()}";
        }
        else
        {
            description += $"\n{new LocalizedString("Item", "Required Knowledge").GetLocalizedString()} : {craftable.requiredKnowledge}\n{new LocalizedString("Item", "Crafting Recipe").GetLocalizedString()}";
            if (craftable.outputAmount > 1) description += $"(x{craftable.outputAmount})";
            description += " : ";
            if (craftable.needAdvancedComponentCount > 0) description += $"{new LocalizedString("Item", "AdvancedComponent").GetLocalizedString()} x{craftable.needAdvancedComponentCount} + ";
            if (craftable.needComponentsCount > 0) description += $"{new LocalizedString("Item", "Components").GetLocalizedString()} x{craftable.needComponentsCount} + ";
            if (craftable.needChemicalsCount > 0) description += $"{new LocalizedString("Item", "Chemicals").GetLocalizedString()} x{craftable.needChemicalsCount} + ";
            if (craftable.needGunpowderCount > 0) description += $"{new LocalizedString("Item", "Gunpowder").GetLocalizedString()} x{craftable.needGunpowderCount} + ";
            if (craftable.needSalvagesCount > 0) description += $"{new LocalizedString("Item", "Salvages").GetLocalizedString()} x{craftable.needSalvagesCount} + ";
            foreach(var etcMaterial in craftable.etcNeedItems)
            {
                description += $"{new LocalizedString("Item", etcMaterial.Key.ToString()).GetLocalizedString()} x{etcMaterial.Value} + ";
            }
            description = description[..^3];
        }
    }

    public void SetDescription(CharacteristicType type)
    {
        raw = true;
        if(!CharacteristicManager.UnlockCheck(type)) description = "";
        else
        {
            Characteristic charicteristic = CharacteristicManager.Characteristics.Find(x => x.type == type);
            description = $"<b>{new LocalizedString("Characteristic", charicteristic.rarity.ToString()).GetLocalizedString()}</b>\n{charicteristic.description.GetLocalizedString()}";
        }
    }
}
