using System;
using System.Collections.Generic;
using UnityEngine;

// Todo lo que está guardado en la casa: despensa (crudos y procesados), caldero y pociones hechas.
// Las operaciones no guardan solas: quien las llama decide cuándo hacer Save() o GameSaveSystem.SaveAll().
public class HomeStorage : MonoBehaviour
{
    private const string SaveKey = "HomeStorage_v2";
    private const string LegacySaveKey = "HomeStorage_Data";

    public static HomeStorage Instance { get; private set; }

    public event Action OnStorageChanged; // cualquier cambio: despensa, caldero o pociones
    public event Action OnPotionsChanged;

    public ItemContainer Pantry { get; } = new ItemContainer();
    public ItemContainer Cauldron { get; } = new ItemContainer();

    private readonly List<PotionData> craftedPotions = new List<PotionData>();
    public IReadOnlyList<PotionData> CraftedPotions => craftedPotions;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        Pantry.OnChanged += RaiseStorageChanged;
        Cauldron.OnChanged += RaiseStorageChanged;

        Load();
    }

    private void RaiseStorageChanged() => OnStorageChanged?.Invoke();

    private void RaisePotionsChanged()
    {
        OnPotionsChanged?.Invoke();
        OnStorageChanged?.Invoke();
    }

    // ---------- Recolección ----------

    public void Deposit(PlayerInventory inventory)
    {
        inventory.Container.MoveAllTo(Pantry);
    }

    // ---------- Crafteo ----------

    public bool TryProcess(ItemData item)
    {
        if (item == null || !item.Processable) return false;
        if (!Pantry.TryRemove(ItemKey.Raw(item))) return false;

        Pantry.TryAdd(ItemKey.Processed(item));
        return true;
    }

    public bool TryMoveToCauldron(ItemKey key)
    {
        if (!Pantry.TryRemove(key)) return false;

        Cauldron.TryAdd(key);
        return true;
    }

    public bool TryReturnFromCauldron(ItemKey key)
    {
        if (!Cauldron.TryRemove(key)) return false;

        Pantry.TryAdd(key);
        return true;
    }

    public void ReturnCauldronToPantry() => Cauldron.MoveAllTo(Pantry);

    // Vacía el caldero y agrega la poción. La coincidencia con la receta se valida antes de llamar.
    public void CompleteCraft(PotionData potion)
    {
        if (potion == null) return;

        Cauldron.Clear();
        craftedPotions.Add(potion);
        RaisePotionsChanged();
    }

    // ---------- Pociones ----------

    public bool HasCraftedPotion(PotionData potion) => craftedPotions.Contains(potion);

    public bool RemoveCraftedPotion(PotionData potion)
    {
        if (!craftedPotions.Remove(potion)) return false;

        RaisePotionsChanged();
        return true;
    }

    // ---------- Save ----------

    public void Save()
    {
        if (TutorialModeFlag.IsActive) return;

        SaveData data = new SaveData
        {
            pantry = Pantry.ToEntries(),
            cauldron = Cauldron.ToEntries()
        };

        foreach (PotionData potion in craftedPotions)
            data.potionIds.Add(potion.Id);

        string json = JsonUtility.ToJson(data);
        PlayerPrefs.SetString(SaveKey, json);
        PlayerPrefs.Save();

        Debug.Log($"[HomeStorage] Guardado: {json}");
    }

    public void Load()
    {
        if (PlayerPrefs.HasKey(LegacySaveKey))
            PlayerPrefs.DeleteKey(LegacySaveKey); // formato viejo, incompatible

        SaveData data = null;
        if (PlayerPrefs.HasKey(SaveKey))
            data = JsonUtility.FromJson<SaveData>(PlayerPrefs.GetString(SaveKey));
        if (data == null)
            data = new SaveData();

        GameDatabase database = GameDatabase.Instance;

        Pantry.LoadEntries(data.pantry, database);
        Cauldron.LoadEntries(data.cauldron, database);

        craftedPotions.Clear();
        if (database != null)
        {
            foreach (string potionId in data.potionIds)
            {
                PotionData potion = database.FindPotion(potionId);
                if (potion != null) craftedPotions.Add(potion);
                else Debug.LogWarning($"[HomeStorage] Poción guardada desconocida: '{potionId}'.");
            }
        }

        RaisePotionsChanged();
    }

    public void ResetInMemory()
    {
        Pantry.Clear();
        Cauldron.Clear();
        craftedPotions.Clear();
        RaisePotionsChanged();
    }

    [Serializable]
    private class SaveData
    {
        public List<ItemEntry> pantry = new List<ItemEntry>();
        public List<ItemEntry> cauldron = new List<ItemEntry>();
        public List<string> potionIds = new List<string>();
    }
}