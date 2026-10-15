using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ItemEntry
{
    public string itemId;
    public ItemState state;
    public int count;
}

// Contenedor genérico de ítems (canasto, despensa, caldero). Solo guarda cantidades > 0.
public class ItemContainer
{
    public event Action OnChanged;

    private readonly Dictionary<ItemKey, int> counts = new Dictionary<ItemKey, int>();
    private readonly int capacity; // 0 = sin límite

    public ItemContainer(int capacity = 0)
    {
        this.capacity = Mathf.Max(0, capacity);
    }

    public IReadOnlyDictionary<ItemKey, int> Contents => counts;
    public int Capacity => capacity;
    public bool HasCapacityLimit => capacity > 0;
    public int TotalCount { get; private set; }
    public bool IsEmpty => TotalCount == 0;
    public bool IsFull => HasCapacityLimit && TotalCount >= capacity;
    public int FreeSpace => HasCapacityLimit ? Mathf.Max(0, capacity - TotalCount) : int.MaxValue;

    public int Count(ItemKey key) => counts.TryGetValue(key, out int count) ? count : 0;
    public int CountAllStates(ItemData item) => Count(ItemKey.Raw(item)) + Count(ItemKey.Processed(item));
    public bool Has(ItemKey key, int amount = 1) => Count(key) >= amount;

    public bool CanAdd(ItemKey key, int amount = 1) => key.IsValid && amount > 0 && amount <= FreeSpace;

    public bool TryAdd(ItemKey key, int amount = 1)
    {
        if (!key.IsValid)
        {
            Debug.LogWarning($"[ItemContainer] Se intentó agregar una clave inválida: {key}");
            return false;
        }

        if (!CanAdd(key, amount)) return false;

        AddInternal(key, amount);
        OnChanged?.Invoke();
        return true;
    }

    public bool TryRemove(ItemKey key, int amount = 1)
    {
        if (amount <= 0 || !Has(key, amount)) return false;

        RemoveInternal(key, amount);
        OnChanged?.Invoke();
        return true;
    }

    public void Clear()
    {
        if (IsEmpty) return;

        counts.Clear();
        TotalCount = 0;
        OnChanged?.Invoke();
    }

    // Mueve todo a otro contenedor. Si no entra completo, no mueve nada.
    public bool MoveAllTo(ItemContainer target)
    {
        if (target == null || target == this) return false;
        if (IsEmpty) return true;
        if (TotalCount > target.FreeSpace) return false;

        foreach (KeyValuePair<ItemKey, int> pair in counts)
            target.AddInternal(pair.Key, pair.Value);

        counts.Clear();
        TotalCount = 0;

        target.OnChanged?.Invoke();
        OnChanged?.Invoke();
        return true;
    }

    public List<ItemEntry> ToEntries()
    {
        List<ItemEntry> entries = new List<ItemEntry>(counts.Count);

        foreach (KeyValuePair<ItemKey, int> pair in counts)
            entries.Add(new ItemEntry { itemId = pair.Key.Item.Id, state = pair.Key.State, count = pair.Value });

        return entries;
    }

    // Reemplaza el contenido con lo guardado. Ignora ids que ya no existen o estados inválidos.
    public void LoadEntries(IEnumerable<ItemEntry> entries, GameDatabase database)
    {
        counts.Clear();
        TotalCount = 0;

        if (entries != null && database != null)
        {
            foreach (ItemEntry entry in entries)
            {
                if (entry == null || entry.count <= 0) continue;

                ItemData item = database.FindItem(entry.itemId);
                ItemKey key = new ItemKey(item, entry.state);

                if (!key.IsValid)
                {
                    Debug.LogWarning($"[ItemContainer] Entrada de save ignorada: '{entry.itemId}' ({entry.state}).");
                    continue;
                }

                AddInternal(key, entry.count);
            }
        }

        OnChanged?.Invoke();
    }

    private void AddInternal(ItemKey key, int amount)
    {
        counts[key] = Count(key) + amount;
        TotalCount += amount;
    }

    private void RemoveInternal(ItemKey key, int amount)
    {
        int remaining = Count(key) - amount;

        if (remaining > 0) counts[key] = remaining;
        else counts.Remove(key);

        TotalCount -= amount;
    }
}