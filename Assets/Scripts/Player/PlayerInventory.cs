using System;
using System.Collections.Generic;
using UnityEngine;
using Game.Collectibles;
//inventario del player donde POR AHORA, va guardando los items en un inventario, la idea es luego hacer la UI interactiva
public class PlayerInventory : MonoBehaviour
{
    public event Action OnInventoryChanged;
    private readonly Dictionary<CollectibleType, int> items = new Dictionary<CollectibleType, int>();
    public IReadOnlyDictionary<CollectibleType, int> Items => items;
    public void AddItem(CollectibleType type, int amount = 1)
    {
        if (!items.ContainsKey(type))
            items[type] = 0;

        items[type] += amount;
        OnInventoryChanged?.Invoke();
    }
    public int GetCount(CollectibleType type)
    {
        return items.TryGetValue(type, out int count) ? count : 0;
    }
    public void Clear()
    {
        items.Clear();
        OnInventoryChanged?.Invoke();
    }
}