using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

public class Inventory
{
    // ConcurrentDictionary: 스레드 세이프!
    private ConcurrentDictionary<ItemManager.Items, Item> inventory = new();

    // 아이템 추가
    public void AddItem(Item item)
    {
        inventory.AddOrUpdate(
            item.itemType,
            (key) => item, // 없는 경우 새 리스트
            (key, existingItem) => {
                lock (existingItem)
                {
                    existingItem.amount += item.amount;
                }
                return existingItem;
            }
        );
    }

    // 아이템 제거
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

    // 특정 아이템 존재 여부
    public bool HasItem(ItemManager.Items type)
    {
        return inventory.TryGetValue(type, out Item item) && item.amount > 0;
    }

    // 특정 아이템 가져오기
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
                // 아이템 복사본 만들거나 참조를 리턴
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
