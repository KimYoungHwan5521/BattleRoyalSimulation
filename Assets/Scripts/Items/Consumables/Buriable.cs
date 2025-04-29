public class Buriable : Consumable
{
    bool isEnchanted;
    public bool IsEnchanted => isEnchanted;

    public void Enchant()
    {
        isEnchanted = true;
    }

    public Buriable(ItemManager.Items itemType, string itemName, float weight, int amount = 1) : base(itemType, itemName, weight, amount)
    {
    }
}
