using System;
using UnityEngine;
using UnityEngine.Localization;

public enum CraftingQuality { NotCrafted, Poor, Common, Fine, Excellent, Masterpiece }

[Serializable]
public class Item
{
    public ItemManager.Items itemType;
    public LocalizedString itemName;
    public int amount;
    public int weight;
    public CraftingQuality quality;

    public Item(ItemManager.Items itemType, LocalizedString itemName, int weight, CraftingQuality quality = CraftingQuality.NotCrafted, int amount = 1)
    {
        this.itemType = itemType;
        this.itemName = itemName;
        this.weight = weight;
        this.quality = quality;
        this.amount = amount;
    }

}
