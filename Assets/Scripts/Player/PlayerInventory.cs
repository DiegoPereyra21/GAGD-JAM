using System;
using System.Collections.Generic;
using UnityEngine;
using Game.Collectibles;
//inventario del player donde POR AHORA, va guardando los items en un inventario, la idea es luego hacer la UI interactiva
public class PlayerInventory : MonoBehaviour
{
    //para q tengaq sentido el "tirar" items, q tenga limite
    [SerializeField] private int maxTotalItems = 10;
    public bool IsFull => TotalCount >= maxTotalItems;

    public event Action OnInventoryChanged;
    private readonly Dictionary<IngredientType, int> items = new Dictionary<IngredientType, int>();
    public IReadOnlyDictionary<IngredientType, int> Items => items;

    public void AddItem(IngredientType type, int amount = 1)
    {
        if (IsFull) return;
        if (!items.ContainsKey(type)) items[type] = 0;
        items[type] += amount;
        OnInventoryChanged?.Invoke();
    }

    public int GetCount(IngredientType type)
    {
        return items.TryGetValue(type, out int count) ? count : 0;
    }

    public void RemoveItem(IngredientType type, int amount = 1)
    {
        if (!items.ContainsKey(type)) return;
        items[type] = Mathf.Max(0, items[type] - amount);
        OnInventoryChanged?.Invoke();
    }
    public void Clear()
    {
        items.Clear();
        OnInventoryChanged?.Invoke();
    }

    public int TotalCount
    {
        get
        {
            int total = 0;
            foreach (var count in items.Values)
                total += count;
            return total;
        }
    }
}