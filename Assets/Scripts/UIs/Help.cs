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

    public void SetDescription(string description)
    {
        raw = true;
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

    public void SetDescription(ItemManager.Items item)
    {
        raw = true;
        //description = new LocalizedString("Item", $"Help:{item}").GetLocalizedString();
        description = $"<b>{new LocalizedString("Item", item.ToString()).GetLocalizedString()}</b>";
        description += item switch
        {
            ItemManager.Items.NotValid => throw new System.NotImplementedException(),
            ItemManager.Items.Knife => $"\n{new LocalizedString("Basic", "Melee weapon").GetLocalizedString()}\n{new LocalizedString("Item", "Damage").GetLocalizedString()} : 30",
            ItemManager.Items.Dagger => $"\n{new LocalizedString("Basic", "Melee weapon").GetLocalizedString()}\n{new LocalizedString("Item", "Damage").GetLocalizedString()} : 40",
            ItemManager.Items.Bat => $"\n{new LocalizedString("Basic", "Melee weapon").GetLocalizedString()}\n{new LocalizedString("Item", "Damage").GetLocalizedString()} : 35",
            ItemManager.Items.LongSword => $"\n{new LocalizedString("Basic", "Melee weapon").GetLocalizedString()}\n{new LocalizedString("Item", "Damage").GetLocalizedString()} : 50",
            ItemManager.Items.Shovel => $"\n{new LocalizedString("Basic", "Melee weapon").GetLocalizedString()}\n{new LocalizedString("Item", "Damage").GetLocalizedString()} : 45",
            ItemManager.Items.Knife_Enchanted => "",
            ItemManager.Items.Dagger_Enchanted => "",
            ItemManager.Items.LongSword_Enchanted => "",
            ItemManager.Items.Revolver => $"\n{new LocalizedString("Basic", "Ranged weapon").GetLocalizedString()}\n{new LocalizedString("Item", "Max Damage").GetLocalizedString()} : 80\n{new LocalizedString("Item", "Attack Range").GetLocalizedString()} : 20\n{new LocalizedString("Item", "Magazine Capacity").GetLocalizedString()} : 7",
            ItemManager.Items.Pistol => $"\n{new LocalizedString("Basic", "Ranged weapon").GetLocalizedString()}\n{new LocalizedString("Item", "Max Damage").GetLocalizedString()} : 40\n{new LocalizedString("Item", "Attack Range").GetLocalizedString()} : 20\n{new LocalizedString("Item", "Magazine Capacity").GetLocalizedString()} : 17",
            ItemManager.Items.AssaultRifle => $"\n{new LocalizedString("Basic", "Ranged weapon").GetLocalizedString()}\n{new LocalizedString("Item", "Max Damage").GetLocalizedString()} : 110\n{new LocalizedString("Item", "Attack Range").GetLocalizedString()} : 50\n{new LocalizedString("Item", "Magazine Capacity").GetLocalizedString()} : 30",
            ItemManager.Items.SubMachineGun => $"\n{new LocalizedString("Basic", "Ranged weapon").GetLocalizedString()}\n{new LocalizedString("Item", "Max Damage").GetLocalizedString()} : 40\n{new LocalizedString("Item", "Attack Range").GetLocalizedString()} : 25\n{new LocalizedString("Item", "Magazine Capacity").GetLocalizedString()} : 30",
            ItemManager.Items.ShotGun => $"\n{new LocalizedString("Basic", "Ranged weapon").GetLocalizedString()}\n{new LocalizedString("Item", "Max Damage").GetLocalizedString()} : 40x12\n{new LocalizedString("Item", "Attack Range").GetLocalizedString()} : 20\n{new LocalizedString("Item", "Magazine Capacity").GetLocalizedString()} : 4",
            ItemManager.Items.SniperRifle => $"\n{new LocalizedString("Basic", "Ranged weapon").GetLocalizedString()}\n{new LocalizedString("Item", "Max Damage").GetLocalizedString()} : 200\n{new LocalizedString("Item", "Attack Range").GetLocalizedString()} : 90\n{new LocalizedString("Item", "Magazine Capacity").GetLocalizedString()} : 5",
            ItemManager.Items.Bazooka => $"\n{new LocalizedString("Basic", "Ranged weapon").GetLocalizedString()}\n{new LocalizedString("Item", "Max Damage").GetLocalizedString()} : 200\n{new LocalizedString("Item", "Attack Range").GetLocalizedString()} : 40\n{new LocalizedString("Item", "Magazine Capacity").GetLocalizedString()} : 1",
            ItemManager.Items.LASER => $"\n{new LocalizedString("Basic", "Ranged weapon").GetLocalizedString()}\n{new LocalizedString("Item", "Damage").GetLocalizedString()} : 100\n{new LocalizedString("Item", "Attack Range").GetLocalizedString()} : 45\n{new LocalizedString("Item", "Magazine Capacity").GetLocalizedString()} : ¡Ä",
            ItemManager.Items.Bullet_Revolver => $"\n{new LocalizedString("Item", "Bullet").GetLocalizedString()}",
            ItemManager.Items.Bullet_Pistol => $"\n{new LocalizedString("Item", "Bullet").GetLocalizedString()}",
            ItemManager.Items.Bullet_AssaultRifle => $"\n{new LocalizedString("Item", "Bullet").GetLocalizedString()}",
            ItemManager.Items.Bullet_SubMachineGun => $"\n{new LocalizedString("Item", "Bullet").GetLocalizedString()}",
            ItemManager.Items.Bullet_ShotGun => $"\n{new LocalizedString("Item", "Bullet").GetLocalizedString()}",
            ItemManager.Items.Bullet_SniperRifle => $"\n{new LocalizedString("Item", "Bullet").GetLocalizedString()}",
            ItemManager.Items.Rocket_Bazooka => $"\n{new LocalizedString("Item", "Bullet").GetLocalizedString()}",
            ItemManager.Items.LowLevelBulletproofHelmet => $"\n{new LocalizedString("Item", "Helmet").GetLocalizedString()}\n{new LocalizedString("Item", "Armor").GetLocalizedString()} : 20",
            ItemManager.Items.MiddleLevelBulletproofHelmet => $"\n{new LocalizedString("Item", "Helmet").GetLocalizedString()}\n{new LocalizedString("Item", "Armor").GetLocalizedString()} : 40",
            ItemManager.Items.HighLevelBulletproofHelmet => $"\n{new LocalizedString("Item", "Helmet").GetLocalizedString()}\n{new LocalizedString("Item", "Armor").GetLocalizedString()} : 60",
            ItemManager.Items.LegendaryBulletproofHelmet => $"\n{new LocalizedString("Item", "Helmet").GetLocalizedString()}\n{new LocalizedString("Item", "Armor").GetLocalizedString()} : 200",
            ItemManager.Items.LowLevelBulletproofVest => $"\n{new LocalizedString("Item", "Vest").GetLocalizedString()}\n{new LocalizedString("Item", "Armor").GetLocalizedString()} : 10",
            ItemManager.Items.MiddleLevelBulletproofVest => $"\n{new LocalizedString("Item", "Vest").GetLocalizedString()}\n{new LocalizedString("Item", "Armor").GetLocalizedString()} : 30",
            ItemManager.Items.HighLevelBulletproofVest => $"\n{new LocalizedString("Item", "Vest").GetLocalizedString()}\n{new LocalizedString("Item", "Armor").GetLocalizedString()} : 50",
            ItemManager.Items.LegendaryBulletproofVest => $"\n{new LocalizedString("Item", "Vest").GetLocalizedString()}\n{new LocalizedString("Item", "Armor").GetLocalizedString()} : 70",
            ItemManager.Items.BandageRoll => $"\n{new LocalizedString("Item", "Consumable").GetLocalizedString()}\n{new LocalizedString("Item", "Bleeding Control").GetLocalizedString()} : 100",
            ItemManager.Items.HemostaticBandageRoll => $"\n{new LocalizedString("Item", "Consumable").GetLocalizedString()}\n{new LocalizedString("Item", "Bleeding Control").GetLocalizedString()} : 300",
            ItemManager.Items.Poison => $"\n{new LocalizedString("Item", "Consumable").GetLocalizedString()}\n{new LocalizedString("Item", $"Explain:{item}").GetLocalizedString()}",
            ItemManager.Items.Antidote => $"\n{new LocalizedString("Item", "Consumable").GetLocalizedString()}\n{new LocalizedString("Item", $"Explain:{item}").GetLocalizedString()}",
            ItemManager.Items.Potion => $"\n{new LocalizedString("Item", "Consumable").GetLocalizedString()}\n{new LocalizedString("Item", "Health Recovery").GetLocalizedString()} : 50",
            ItemManager.Items.AdvancedPotion => $"\n{new LocalizedString("Item", "Consumable").GetLocalizedString()}\n{new LocalizedString("Item", "Health Recovery").GetLocalizedString()} : 100",
            ItemManager.Items.Components => $"\n{new LocalizedString("Item", "Crafting Materials").GetLocalizedString()}",
            ItemManager.Items.AdvancedComponent => $"\n{new LocalizedString("Item", "Crafting Materials").GetLocalizedString()}",
            ItemManager.Items.Chemicals => $"\n{new LocalizedString("Item", "Crafting Materials").GetLocalizedString()}",
            ItemManager.Items.Gunpowder => $"\n{new LocalizedString("Item", "Crafting Materials").GetLocalizedString()}",
            ItemManager.Items.Salvages => $"\n{new LocalizedString("Item", "Crafting Materials").GetLocalizedString()}",
            ItemManager.Items.BearTrap => $"\n{new LocalizedString("Item", "Trap").GetLocalizedString()}\n{new LocalizedString("Item", "Damage").GetLocalizedString()} : 40",
            ItemManager.Items.BearTrap_Enchanted => "",
            ItemManager.Items.LandMine => $"\n{new LocalizedString("Item", "Trap").GetLocalizedString()}\n{new LocalizedString("Item", "Damage").GetLocalizedString()} : 100",
            ItemManager.Items.NoiseTrap => $"\n{new LocalizedString("Item", "Trap").GetLocalizedString()}\n{new LocalizedString("Item", $"Explain:{item}").GetLocalizedString()}",
            ItemManager.Items.ChemicalTrap => $"\n{new LocalizedString("Item", "Trap").GetLocalizedString()}\n{new LocalizedString("Item", $"Explain:{item}").GetLocalizedString()}",
            ItemManager.Items.ShrapnelTrap => $"\n{new LocalizedString("Item", "Trap").GetLocalizedString()}\n{new LocalizedString("Item", "Damage").GetLocalizedString()} : 20(x48)",
            ItemManager.Items.ExplosiveTrap => $"\n{new LocalizedString("Item", "Trap").GetLocalizedString()}\n{new LocalizedString("Item", "Damage").GetLocalizedString()} : 100",
            ItemManager.Items.WalkingAid => $"\n{new LocalizedString("Item", "ETC").GetLocalizedString()}\n{new LocalizedString("Item", $"Explain:{item}").GetLocalizedString()}",
            ItemManager.Items.TrapDetectionDevice => $"\n{new LocalizedString("Item", "ETC").GetLocalizedString()}\n{new LocalizedString("Item", $"Explain:{item}").GetLocalizedString()}",
            ItemManager.Items.BiometricRader => $"\n{new LocalizedString("Item", "ETC").GetLocalizedString()}\n{new LocalizedString("Item", $"Explain:{item}").GetLocalizedString()}",
            ItemManager.Items.EnergyBarrier => $"\n{new LocalizedString("Item", "ETC").GetLocalizedString()}\n{new LocalizedString("Item", $"Explain:{item}").GetLocalizedString()}",
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
}
