using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Localization;
using UnityEngine.Localization.Components;

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

    public void SetDescription(LocalizedString description)
    {
        raw = true;
        this.description = description.GetLocalizedString();
    }

    public void SetDescriptionWithKey(string key, params string[] vars)
    {
        raw = true;
        var localizedString = new LocalizedString("Table", key);
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
        description = item switch
        {
            ItemManager.Items.NotValid => throw new System.NotImplementedException(),
            ItemManager.Items.Knife => "<b>Knife\n</b>Melee weapon\nDamage : 30\nNot craftable",
            ItemManager.Items.Dagger => "<b>Dagger\n</b>\nMelee weapon\nDamage : 40\nNot craftable",
            ItemManager.Items.Bat => "<b>Bat\n</b>\nMelee weapon\nDamage : 35\nNot craftable",
            ItemManager.Items.LongSword => "<b>Long Sowrd\n</b>\nMelee weapon\nDamage : 50\nNot craftable",
            ItemManager.Items.Shovel => "<b>Shovel\n</b>\nMelee weapon\nDamage : 45\nNot craftable",
            ItemManager.Items.Knife_Enchanted => "",
            ItemManager.Items.Dagger_Enchanted => "",
            ItemManager.Items.LongSword_Enchanted => "",
            ItemManager.Items.Revolver => "<b>Revolver\n</b>Ranged weapon\nMax damage : 80\nAttack range : 20\nMagazine capacity : 7\nKnowledge require : 15\nCraft : Components x2 + Salvages x4",
            ItemManager.Items.Pistol => "<b>Pistol\n</b>Ranged weapon\nMax damage : 40\nAttack range : 20\nMagazine capacity : 17\nKnowledge require : 25\nCraft : Components x2 + Salvages x4",
            ItemManager.Items.AssaultRifle => "<b>Assault Rifle\n</b>Ranged weapon\nMax damage : 110\nAttack range : 50\nMagazine capacity : 30\nKnowledge require : 60\nCraft : Components x6 + Salvages x4",
            ItemManager.Items.SubMachineGun => "<b>Sub Machine Gun\n</b>Ranged weapon\nMax damage : 40\nAttack range : 25\nMagazine capacity : 30\nKnowledge require : 35\nCraft : Components x4 + Salvages x4",
            ItemManager.Items.ShotGun => "<b>Shotgun\n</b>Ranged weapon\nMax damage : 40x12\nAttack range : 20\nMagazine capacity : 4\nKnowledge require : 45\nCraft : Components x4 + Salvages x4",
            ItemManager.Items.SniperRifle => "<b>Sniper Rifle\n</b>Ranged weapon\nMax damage : 200\nAttack range : 90\nMagazine capacity : 5\nKnowledge require : 70\nCraft : Components x6 + Salvages x4",
            ItemManager.Items.Bazooka => "<b>Bazooka\n</b>Ranged weapon\nMax damage : 200\nAttack range : 40\nMagazine capacity : 1\nKnowledge require : 80\nCraft : Components x8 + Salvages x4",
            ItemManager.Items.LASER => "<b>LASER\n</b>Ranged weapon\nDamage : 100\nAttack range : 45\nMagazine capacity : ¡Ä\nKnowledge require : 120\nCraft : Advanced component x4 + Components x8 + Salvages x8 + Chemicals x2",
            ItemManager.Items.Bullet_Revolver => "<b>Bullet(Revolver)\n</b>Bullet\nKnowledge require : 40\nCraft(x4) : Salvages x1 + Gunpowder x1",
            ItemManager.Items.Bullet_Pistol => "<b>Bullet(Pistol)\n</b>Bullet\nKnowledge require : 40\nCraft(x2) : Salvages x1 + Gunpowder x1",
            ItemManager.Items.Bullet_AssaultRifle => "<b>Bullet(Assault Rifle)\n</b>Bullet\nKnowledge require : 40\nCraft : Salvages x2 + Gunpowder x2",
            ItemManager.Items.Bullet_SubMachineGun => "<b>Bullet(Sub Machine Gun)\n</b>Bullet\nKnowledge require : 40\nCraft : Salvages x1 + Gunpowder x1",
            ItemManager.Items.Bullet_ShotGun => "<b>Bullet(Shotgun)\n</b>Bullet\nKnowledge require : 40\nCraft : Salvages x3 + Gunpowder x1",
            ItemManager.Items.Bullet_SniperRifle => "<b>Bullet(Sniper Rifle)\n</b>Bullet\nKnowledge require : 40\nCraft : Salvages x1 + Gunpowder x2",
            ItemManager.Items.Rocket_Bazooka => "<b>Rocket(Bazooka)\n</b>Bullet\nKnowledge require : 55\nCraft : Component x1 + Gunpowder x3",
            ItemManager.Items.LowLevelBulletproofHelmet => "<b>Low Level Bulletproof Helmet\n</b>Helmet\nArmor : 20\nKnowledge require : 48\nCraft : Salvages x7",
            ItemManager.Items.MiddleLevelBulletproofHelmet => "<b>Middle Level Bulletproof Helmet\n</b>Helmet\nArmor : 40\nKnowledge require : 68\nCraft : Components x3 + Salvages x8",
            ItemManager.Items.HighLevelBulletproofHelmet => "<b>High Level Bulletproof Helmet\n</b>Helmet\nArmor : 60\nKnowledge require : 88\nCraft : Components x6 Salvages x9",
            ItemManager.Items.LegendaryBulletproofHelmet => "<b>Legendary Bulletproof Helmet\n</b>Helmet\nArmor : 200\nKnowledge require : 108\nCraft : Components x6 + High level bulletproof helmet x1 + Middle level bulletproof helmet x2 + Low level bulletproof helmet x4",
            ItemManager.Items.LowLevelBulletproofVest => "<b>Low Level Bulletproof Vest\n</b>Vest\nArmor : 10\nKnowledge require : 52\nCraft : Salvages x10",
            ItemManager.Items.MiddleLevelBulletproofVest => "<b>Middle Level Bulletproof Vest\n</b>Vest\nArmor : 30\nKnowledge require : 72\nCraft : Components x3 + Salvages x11",
            ItemManager.Items.HighLevelBulletproofVest => "<b>High Level Bulletproof Vest\n</b>Vest\nArmor : 50\nKnowledge require : 92\nCraft : Components x6 + Salvages x12",
            ItemManager.Items.LegendaryBulletproofVest => "<b>Legendary Bulletproof Vest\n</b>Vest\nArmor : 70\nKnowledge require : 112\nCraft : Components x6 + High level bulletproof vest x1 + Middle level bulletproof vest x2 + Low level bulletproof vest x4",
            ItemManager.Items.BandageRoll => "<b>Bandage Roll\n</b>Consumable\nHemostatic amount : 100\nNot craftable",
            ItemManager.Items.HemostaticBandageRoll => "<b>Hemostatic Bandage Roll\n</b>Consumable\nHemostatic amount : 300\nKnowledge require : 30\nCraft : Chemicals x2 + Bandage roll x1",
            ItemManager.Items.Poison => "<b>Poison\n</b>Consumable\nCan enchant melee weapon, etc.\nKnowledge require : 10\nCraft(x3) : Salvages x3 + Chemicals x2",
            ItemManager.Items.Antidote => "<b>Anidote\n</b>Consumable\nCure poison.\nKnowledge require : 20\nCraft(x2) : Chemicals x2",
            ItemManager.Items.Potion => "<b>Potion\n</b>Consumable\nHP recovery amount : 50\nKnowledge require : 50\nCraft : Salvages x1 + Chemicals x3",
            ItemManager.Items.AdvancedPotion => "<b>Advanced Potion\n</b>Consumable\nHP recovery amount : 100\nKnowledge require : 104\nCraft : Chemicals x2 + Potion x1",
            ItemManager.Items.Components => "<b>Components\n</b>Crafting material\nNot craftable",
            ItemManager.Items.AdvancedComponent => "<b>Advanced Components\n</b>Crafting material\nKnowledge require : 110\nCraft : Components x4",
            ItemManager.Items.Chemicals => "<b>Chemicals\n</b>Crafting material\nNot craftable",
            ItemManager.Items.Gunpowder => "<b>Gunpowder\n</b>Crafting material\nNot craftable",
            ItemManager.Items.Salvages => "<b>Salvages\n</b>Crafting material\nNot craftable",
            ItemManager.Items.BearTrap => "<b>Bear Trap\n</b>Trap\nDamage : 40\nKnowledge require : 53\nCraft(x3) : Components x2 + Salvages x3",
            ItemManager.Items.BearTrap_Enchanted => "",
            ItemManager.Items.LandMine => "<b>Land Mine\n</b>Trap\nDamage : 100\nKnowledge require : 73\nCraft(x3) : Advanced component x1 + Components x1 + Salvages x1 + Gunpowder x3",
            ItemManager.Items.NoiseTrap => "<b>Noise Trap\n</b>Trap\nMake noise.\nKnowledge require : 65\nCraft : Advanced component x1 + Salvages x3",
            ItemManager.Items.ChemicalTrap => "<b>Chemical Trap\n</b>Trap\nPoisons nearby survivors.\nKnowledge require : 77\nCraft : Components x1 + Poison x3",
            ItemManager.Items.ShrapnelTrap => "<b>Shrapnel Trap\n</b>Trap\nDamage : 20(x48)\nKnowledge require : 81\nCraft : Components x1 + Salvages x6 + Gunpowder x1",
            ItemManager.Items.ExplosiveTrap => "<b>Explosion Trap\n</b>Trap\nDamage : 100\nKnowledge require : 85\nCraft : Advanced component x1 + Chemicals x1 + Gunpowder x2",
            ItemManager.Items.WalkingAid => "<b>Walking Aid\n</b>ETC\nReduces move speed penalty when leg is injured.\nKnowledge require : 5\nCraft : Components x1 + Salvages x2",
            ItemManager.Items.TrapDetectionDevice => "<b>Trap Detection Device\n</b>ETC\nDetect nearby traps.\nKnowledge require : 100\nCraft : Advanced component x3 + Components x5 + Salvages x2",
            ItemManager.Items.BiometricRader => "<b>Biometric Rader\n</b>ETC\nDetect mid-range survivors.\nKnowledge require : 96\nCraft : Advanced component x2 + Components x8 + Salvages x2",
            ItemManager.Items.EnergyBarrier => "<b>Energy Barrier\n</b>ETC\nBlocks projectiles with a 75% chance.\nKnowledge require : 116\nCraft : Advanced component x4 + Components x8 + Salvages x4",
            _ => throw new System.NotImplementedException()
        };
    }
}
