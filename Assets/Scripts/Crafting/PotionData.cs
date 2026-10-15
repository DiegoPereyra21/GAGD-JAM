using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PotionIngredient
{
    [SerializeField] private ItemData item;
    [SerializeField] private ItemState state;
    [SerializeField, Min(1)] private int amount = 1;

    public ItemData Item => item;
    public ItemState State => state;
    public int Amount => amount;
    public ItemKey Key => new ItemKey(item, state);
}

[CreateAssetMenu(fileName = "NewPotion", menuName = "Game Data/Potion")]
public class PotionData : GameDataAsset
{
    [TextArea(2, 4)]
    [SerializeField] private string description;
    [SerializeField] private Sprite image;
    [Tooltip("Modelo para el showcase, la caja y el área de entregas.")]
    [SerializeField] private GameObject model;
    [SerializeField] private List<PotionIngredient> ingredients = new List<PotionIngredient>();

    public string Description => description;
    public Sprite Image => image;
    public GameObject Model => model;
    public IReadOnlyList<PotionIngredient> Ingredients => ingredients;

    // Suma entradas repetidas del mismo ítem+estado.
    public Dictionary<ItemKey, int> GetRequirements()
    {
        Dictionary<ItemKey, int> result = new Dictionary<ItemKey, int>();

        foreach (PotionIngredient ingredient in ingredients)
        {
            if (ingredient == null || ingredient.Item == null || ingredient.Amount <= 0) continue;

            result.TryGetValue(ingredient.Key, out int current);
            result[ingredient.Key] = current + ingredient.Amount;
        }

        return result;
    }

    // Coincidencia exacta: mismos ítems, mismos estados, mismas cantidades, nada de más.
    public bool Matches(IReadOnlyDictionary<ItemKey, int> contents)
    {
        Dictionary<ItemKey, int> required = GetRequirements();
        if (required.Count == 0) return false;

        int presentKeys = 0;

        foreach (KeyValuePair<ItemKey, int> pair in contents)
        {
            if (pair.Value <= 0) continue;
            presentKeys++;

            if (!required.TryGetValue(pair.Key, out int needed) || needed != pair.Value)
                return false;
        }

        return presentKeys == required.Count;
    }
}