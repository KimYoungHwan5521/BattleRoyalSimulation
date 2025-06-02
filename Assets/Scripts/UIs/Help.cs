using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class Help : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    RectTransform rect;
    [SerializeField, TextArea] string description;

    private void Start()
    {
        rect = GetComponent<RectTransform>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!string.IsNullOrWhiteSpace(description))
        {
            RectTransform descriptionRT = GameManager.Instance.description.GetComponent<RectTransform>();
            TextMeshProUGUI descriptionText = descriptionRT.GetComponentInChildren<TextMeshProUGUI>();
            descriptionText.text = description;
            descriptionRT.sizeDelta = new(descriptionText.rectTransform.rect.width, descriptionText.rectTransform.rect.height);
            descriptionRT.position = 
                new(Mathf.Clamp(rect.position.x + 25, 0, Screen.width - descriptionRT.rect.width),
                Mathf.Clamp(rect.position.y - 40, descriptionRT.rect.height, Screen.height));
            descriptionRT.gameObject.SetActive(true);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        GameManager.Instance.description.SetActive(false);
    }

    public void SetDescription(string description)
    {
        this.description = description;
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
            ItemManager.Items.Knife => throw new System.NotImplementedException(),
            ItemManager.Items.Dagger => throw new System.NotImplementedException(),
            ItemManager.Items.Bat => throw new System.NotImplementedException(),
            ItemManager.Items.LongSword => throw new System.NotImplementedException(),
            ItemManager.Items.Shovel => throw new System.NotImplementedException(),
            ItemManager.Items.Knife_Enchanted => throw new System.NotImplementedException(),
            ItemManager.Items.Dagger_Enchanted => throw new System.NotImplementedException(),
            ItemManager.Items.LongSword_Enchanted => throw new System.NotImplementedException(),
            ItemManager.Items.Revolver => throw new System.NotImplementedException(),
            ItemManager.Items.Pistol => throw new System.NotImplementedException(),
            ItemManager.Items.AssaultRifle => throw new System.NotImplementedException(),
            ItemManager.Items.SubMachineGun => throw new System.NotImplementedException(),
            ItemManager.Items.ShotGun => throw new System.NotImplementedException(),
            ItemManager.Items.SniperRifle => throw new System.NotImplementedException(),
            ItemManager.Items.Bazooka => throw new System.NotImplementedException(),
            ItemManager.Items.LASER => throw new System.NotImplementedException(),
            ItemManager.Items.Bullet_Revolver => throw new System.NotImplementedException(),
            ItemManager.Items.Bullet_Pistol => throw new System.NotImplementedException(),
            ItemManager.Items.Bullet_AssaultRifle => throw new System.NotImplementedException(),
            ItemManager.Items.Bullet_SubMachineGun => throw new System.NotImplementedException(),
            ItemManager.Items.Bullet_ShotGun => throw new System.NotImplementedException(),
            ItemManager.Items.Bullet_SniperRifle => throw new System.NotImplementedException(),
            ItemManager.Items.Rocket_Bazooka => throw new System.NotImplementedException(),
            ItemManager.Items.LowLevelBulletproofHelmet => throw new System.NotImplementedException(),
            ItemManager.Items.MiddleLevelBulletproofHelmet => throw new System.NotImplementedException(),
            ItemManager.Items.HighLevelBulletproofHelmet => throw new System.NotImplementedException(),
            ItemManager.Items.LegendaryBulletproofHelmet => throw new System.NotImplementedException(),
            ItemManager.Items.LowLevelBulletproofVest => throw new System.NotImplementedException(),
            ItemManager.Items.MiddleLevelBulletproofVest => throw new System.NotImplementedException(),
            ItemManager.Items.HighLevelBulletproofVest => throw new System.NotImplementedException(),
            ItemManager.Items.LegendaryBulletproofVest => throw new System.NotImplementedException(),
            ItemManager.Items.BandageRoll => throw new System.NotImplementedException(),
            ItemManager.Items.HemostaticBandageRoll => throw new System.NotImplementedException(),
            ItemManager.Items.Poison => throw new System.NotImplementedException(),
            ItemManager.Items.Antidote => throw new System.NotImplementedException(),
            ItemManager.Items.Potion => throw new System.NotImplementedException(),
            ItemManager.Items.AdvancedPotion => throw new System.NotImplementedException(),
            ItemManager.Items.Components => throw new System.NotImplementedException(),
            ItemManager.Items.AdvancedComponent => throw new System.NotImplementedException(),
            ItemManager.Items.Chemicals => throw new System.NotImplementedException(),
            ItemManager.Items.Gunpowder => throw new System.NotImplementedException(),
            ItemManager.Items.Salvages => throw new System.NotImplementedException(),
            ItemManager.Items.BearTrap => throw new System.NotImplementedException(),
            ItemManager.Items.BearTrap_Enchanted => throw new System.NotImplementedException(),
            ItemManager.Items.LandMine => throw new System.NotImplementedException(),
            ItemManager.Items.NoiseTrap => throw new System.NotImplementedException(),
            ItemManager.Items.ChemicalTrap => throw new System.NotImplementedException(),
            ItemManager.Items.ShrapnelTrap => throw new System.NotImplementedException(),
            ItemManager.Items.ExplosiveTrap => throw new System.NotImplementedException(),
            ItemManager.Items.WalkingAid => throw new System.NotImplementedException(),
            ItemManager.Items.TrapDetectionDevice => throw new System.NotImplementedException(),
            ItemManager.Items.BiometricRader => throw new System.NotImplementedException(),
            ItemManager.Items.EnergyBarrier => throw new System.NotImplementedException(),
        };
    }
}
