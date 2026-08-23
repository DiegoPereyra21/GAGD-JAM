using System;
using System.Collections.Generic;
using Game.Collectibles;

//Acumula los items en momeria pero no persiste, la idea es luego depositar en la casa de alguna forma con un trigger
public static class HomeStorage
{
    public static event Action OnStorageChanged;
    private static readonly Dictionary<CollectibleType, int> totals = new Dictionary<CollectibleType, int>();
    public static IReadOnlyDictionary<CollectibleType, int> Totals => totals;
    public static void Deposit(PlayerInventory inventory)
    {
        foreach (var pair in inventory.Items)
        {
            if (!totals.ContainsKey(pair.Key))
                totals[pair.Key] = 0;

            totals[pair.Key] += pair.Value;
        }

        inventory.Clear();
        OnStorageChanged?.Invoke();
    }
}