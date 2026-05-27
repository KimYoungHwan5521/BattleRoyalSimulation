using UnityEngine.Localization;

public class Consumable : Item
{
    public Consumable(ItemManager.Items itemType, LocalizedString itemName, float weight, CraftingQuality quality = CraftingQuality.NotCrafted, int amount = 1) 
        : base(itemType, itemName, weight, quality, amount)
    {

    }
}
