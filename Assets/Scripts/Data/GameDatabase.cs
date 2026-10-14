using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GameDatabase", menuName = "Game Data/Game Database")]
public class GameDatabase : ScriptableObject
{
    // Tiene que estar en Assets/Resources/GameDatabase.asset
    public const string ResourcesPath = "GameDatabase";

    private static GameDatabase instance;

    public static GameDatabase Instance
    {
        get
        {
            if (instance == null)
            {
                instance = Resources.Load<GameDatabase>(ResourcesPath);
                if (instance == null)
                    Debug.LogError($"[GameDatabase] No se encontró 'Resources/{ResourcesPath}.asset'.");
            }
            return instance;
        }
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics() => instance = null;

    [Header("Contenido")]
    [SerializeField] private List<ItemData> items = new List<ItemData>();
    [SerializeField] private List<PotionData> potions = new List<PotionData>();

    [Header("Referencias especiales")]
    [SerializeField] private PotionData sleepPotion;
    [Tooltip("Ojo de Leprechaun: se consigue en la caja de la poción del sueño.")]
    [SerializeField] private ItemData sleepIngredient;

    private Dictionary<string, ItemData> itemsById;
    private Dictionary<string, PotionData> potionsById;

    public IReadOnlyList<ItemData> Items => items;
    public IReadOnlyList<PotionData> Potions => potions;
    public PotionData SleepPotion => sleepPotion;
    public ItemData SleepIngredient => sleepIngredient;

    public ItemData FindItem(string id) => Find(id, items, ref itemsById);
    public PotionData FindPotion(string id) => Find(id, potions, ref potionsById);

    private static T Find<T>(string id, List<T> source, ref Dictionary<string, T> cache) where T : GameDataAsset
    {
        if (string.IsNullOrEmpty(id)) return null;

        if (cache == null)
        {
            cache = new Dictionary<string, T>();
            foreach (T entry in source)
            {
                if (entry != null && !string.IsNullOrEmpty(entry.Id))
                    cache[entry.Id] = entry;
            }
        }

        return cache.TryGetValue(id, out T found) ? found : null;
    }

    private void OnValidate()
    {
        itemsById = null;
        potionsById = null;
    }
}