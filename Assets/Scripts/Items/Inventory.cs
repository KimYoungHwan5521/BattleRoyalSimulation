using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

public class Inventory
{
    // ConcurrentDictionary: ������ ������!
    private ConcurrentDictionary<ItemManager.Items, Item> inventory = new();

    // ������ �߰�
    public void AddItem(Item item)
    {
        inventory.AddOrUpdate(
            item.itemType,
            (key) => item, // ���� ��� �� ����Ʈ
            (key, existingItem) => {
                lock (existingItem)
                {
                    existingItem.amount += item.amount;
                }
                return existingItem;
            }
        );
    }

    // ������ ����
    public bool RemoveItem(ItemManager.Items type, int amount = 1)
    {
        if (inventory.TryGetValue(type, out Item item))
        {
            lock (item)
            {
                if (item.amount >= amount)
                {
                    item.amount -= amount;
                    return true;
                }
                else
                {
                    UnityEngine.Debug.LogWarning("Wrong amount inputed");
                    return false;
                }

            }
        }
        return false;
    }

    // Ư�� ������ ���� ����
    public bool HasItem(ItemManager.Items type)
    {
        return inventory.TryGetValue(type, out Item item) && item.amount > 0;
    }

    // Ư�� ������ ��������
    public Item GetItem(ItemManager.Items type)
    {
        if (inventory.TryGetValue(type, out Item item) && item.amount > 0)
        {
            lock (item)
            {
                return item;
            }
        }
        return null;
    }

    public List<Item> FindAll(Predicate<Item> match)
    {
        List<Item> result = new List<Item>();
        foreach (Item item in inventory.Values)
        {
            if (match(item))
            {
                result.Add(item);
            }
        }
        return result;
    }
    public Item Find(Predicate<Item> match)
    {
        Item result = null;
        foreach (Item item in inventory.Values)
        {
            if (match(item))
            {
                return result;
            }
        }
        return null;
    }

    public List<Item> GetAllItems()
    {
        List<Item> items = new ();
        foreach (var kvp in inventory)
        {
            lock (kvp.Value)
            {
                // ������ ���纻 ����ų� ������ ����
                items.Add(kvp.Value);
            }
        }
        return items;
    }

    public void Clear()
    {
        inventory.Clear();
    }

}
