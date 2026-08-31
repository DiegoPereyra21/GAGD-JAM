using System;
using System.Collections.Generic;
using UnityEngine;
using Game.Collectibles;

public class HomeStorage : MonoBehaviour
{
    private const string SaveKey = "HomeStorage_Data";

    public static HomeStorage Instance { get; private set; }

    public event Action OnStorageChanged;

    [SerializeField] private IngredientDatabase ingredientDatabase;
    [SerializeField] private PotionRecipeDatabase potionDatabase;

    private readonly Dictionary<IngredientType, int> totals = new Dictionary<IngredientType, int>();
    public IReadOnlyDictionary<IngredientType, int> Totals => totals;

    private readonly List<PotionRecipe> craftedPotions = new List<PotionRecipe>();
    public IReadOnlyList<PotionRecipe> CraftedPotions => craftedPotions;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        Load();
    }

    public void Deposit(PlayerInventory inventory)
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

    public void Save()
    {
        SaveData data = new SaveData();

        foreach (var pair in totals)
            data.entries.Add(new SerializableEntry { ingredientId = pair.Key.ingredientId, count = pair.Value });

        foreach (PotionRecipe potion in craftedPotions)
            data.potionIds.Add(potion.potionId);

        string json = JsonUtility.ToJson(data);
        PlayerPrefs.SetString(SaveKey, json);
        PlayerPrefs.Save();

        Debug.Log($"[HomeStorage] Guardado: {json}");
    }

    public void Load()
    {
        totals.Clear();
        craftedPotions.Clear();

        if (!PlayerPrefs.HasKey(SaveKey))
        {
            Debug.Log("[HomeStorage] No hay datos guardados, arranca vacío.");
            return;
        }

        string json = PlayerPrefs.GetString(SaveKey);
        SaveData data = JsonUtility.FromJson<SaveData>(json);

        foreach (var entry in data.entries)
        {
            IngredientType ingredient = ingredientDatabase.FindById(entry.ingredientId);
            if (ingredient != null)
                totals[ingredient] = entry.count;
        }

        foreach (string potionId in data.potionIds)
        {
            PotionRecipe recipe = potionDatabase.FindById(potionId);
            if (recipe != null)
                craftedPotions.Add(recipe);
        }

        OnStorageChanged?.Invoke();
        Debug.Log($"[HomeStorage] Cargado: {json}");
    }

    [Serializable]
    private class SerializableEntry
    {
        public string ingredientId;
        public int count;
    }

    [Serializable]
    private class SaveData
    {
        public List<SerializableEntry> entries = new List<SerializableEntry>();
        public List<string> potionIds = new List<string>();
    }

    public bool HasEnough(IngredientType type, int amount)
    {
        return totals.TryGetValue(type, out int count) && count >= amount;
    }

    public bool RemoveOne(IngredientType type)
    {
        if (!totals.TryGetValue(type, out int count) || count <= 0)
            return false;

        totals[type] = count - 1;
        OnStorageChanged?.Invoke();
        return true;
    }

    public void AddPotion(PotionRecipe recipe)
    {
        craftedPotions.Add(recipe);
        OnStorageChanged?.Invoke();
    }

    public bool RemoveCraftedPotion(PotionRecipe recipe)
    {
        int index = craftedPotions.IndexOf(recipe);
        if (index < 0) return false;

        craftedPotions.RemoveAt(index);
        OnStorageChanged?.Invoke();
        return true;
    }
    public void AddIngredient(IngredientType type, int amount = 1)
    {
        if (!totals.ContainsKey(type))
            totals[type] = 0;

        totals[type] += amount;
        OnStorageChanged?.Invoke();
    }
}