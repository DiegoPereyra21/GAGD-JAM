using System.Collections.Generic;
using System.Linq;

// Cálculos compartidos sobre qué piden las quests y qué tiene el jugador.
public static class QuestRequirements
{
    // Quests activas que todavía necesitan ingredientes:
    // no están entregadas y no hay una poción ya hecha que las cubra.
    public static List<QuestData> GetUncoveredQuests(QuestManager questManager)
    {
        List<QuestData> result = new List<QuestData>();
        List<PotionData> crafted = new List<PotionData>(HomeStorage.Instance.CraftedPotions);

        foreach (QuestData quest in questManager.ActiveQuests)
        {
            if (quest == null || quest.Potion == null) continue;
            if (questManager.PendingDeliveries.Contains(quest)) continue;
            if (crafted.Remove(quest.Potion)) continue;

            result.Add(quest);
        }

        return result;
    }

    // Total por ítem que pide una poción, sumando crudo y procesado, en el orden de la receta.
    public static List<KeyValuePair<ItemData, int>> GetItemTotals(PotionData potion)
    {
        List<ItemData> order = new List<ItemData>();
        Dictionary<ItemData, int> totals = new Dictionary<ItemData, int>();

        if (potion != null)
        {
            foreach (PotionIngredient ingredient in potion.Ingredients)
            {
                if (ingredient == null || ingredient.Item == null || ingredient.Amount <= 0) continue;

                if (!totals.ContainsKey(ingredient.Item))
                {
                    totals[ingredient.Item] = 0;
                    order.Add(ingredient.Item);
                }

                totals[ingredient.Item] += ingredient.Amount;
            }
        }

        List<KeyValuePair<ItemData, int>> result = new List<KeyValuePair<ItemData, int>>(order.Count);
        foreach (ItemData item in order)
            result.Add(new KeyValuePair<ItemData, int>(item, totals[item]));

        return result;
    }

    // Cuánto tiene el jugador de un ítem, en cualquier estado: canasto + despensa + caldero.
    public static int GetOwned(ItemData item, PlayerInventory inventory)
    {
        int total = inventory != null ? inventory.GetCount(item) : 0;

        HomeStorage storage = HomeStorage.Instance;
        if (storage != null)
            total += storage.Pantry.CountAllStates(item) + storage.Cauldron.CountAllStates(item);

        return total;
    }

    public static bool HasAllMaterials(QuestManager questManager, PlayerInventory inventory)
    {
        Dictionary<ItemData, int> remaining = new Dictionary<ItemData, int>();

        foreach (QuestData quest in GetUncoveredQuests(questManager))
        {
            foreach (KeyValuePair<ItemData, int> pair in GetItemTotals(quest.Potion))
            {
                if (!remaining.ContainsKey(pair.Key))
                    remaining[pair.Key] = GetOwned(pair.Key, inventory);

                if (remaining[pair.Key] < pair.Value) return false;

                remaining[pair.Key] -= pair.Value;
            }
        }

        return true;
    }
}
