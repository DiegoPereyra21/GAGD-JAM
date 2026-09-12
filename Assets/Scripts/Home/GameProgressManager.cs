using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;


public class GameProgressManager : MonoBehaviour
{
    private const string SaveKey = "GameProgress_Data";
    private const string DayKey = "GameProgress_Day";

    public static GameProgressManager Instance { get; private set; }

    [SerializeField] private float nightDuration = 300f;
    [SerializeField] private float dayDuration = 300f; // duración real en segundos de las 6 horas de crafteo (6am a 12pm)

    //para que sea mas entendible la noche y el dia con el nuevo skybox
    [SerializeField] private float dawnFraction = 0.25f;

    public event Action OnNightStarted;
    public event Action OnDayStarted;

    [Header("Audio")]
    [SerializeField] private AK.Wwise.Event timeOfDayMusic;

    [Header("Compass")]
    [SerializeField] private UIDocument compassUIDocument;


    public int CurrentDay { get; private set; } = 1;
    public int Money { get; private set; }
    public float NightTimeRemaining { get; private set; }
    public bool IsNightActive { get; private set; }
    public bool IsCraftingTimeActive { get; private set; }
    public float DayTimeRemaining { get; private set; }
    public bool HasBeenOutsideThisCycle { get; private set; }
    public bool IsInsane { get; private set; } = true; // para el indicador de sanidad en las transiciones
    public int InventoryCount { get; private set; } = 0; // para tener un conteo de items que se quedan en la casa
    //public static bool HasSaveData => PlayerPrefs.HasKey(SaveKey);
    public static bool HasSaveData => PlayerPrefs.HasKey(DayKey);

    public event Action OnWentOutside;
    public event Action OnNightTimeExpired;

    public bool ShouldSpawnAtDoor { get; private set; }

    public void MarkEnteredHouse()
    {
        ShouldSpawnAtDoor = true;
    }


    public bool SleepIngredientPurchased { get; private set; }
    public bool SleepIngredientObtained { get; private set; }

    public bool TrySpendMoney(int amount)
    {
        if (Money < amount) return false;
        Money -= amount;
        return true;
    }

    public int SleepIngredientPurchaseDay { get; private set; }

    public void MarkSleepIngredientPurchased()
    {
        SleepIngredientPurchased = true;
        SleepIngredientPurchaseDay = CurrentDay;
    }

    public void MarkSleepIngredientObtained()
    {
        SleepIngredientPurchased = false;
        SleepIngredientObtained = true;
    }

    //para q solamente empiece el ciclo cuanod salga
    public bool IsOutside { get; private set; }

    public void MarkOutside()
    {
        IsOutside = true;

        SetCompassVisible(true);

        OnWentOutside?.Invoke();
    }
    public void MarkWentOutside()
    {
        HasBeenOutsideThisCycle = true;
    }
    //arregla lo de que no haga el fade si empiezo de 0
    private bool pendingWelcomeFade;

    public void RequestWelcomeFade()
    {
        pendingWelcomeFade = true;
    }

    public bool ConsumeWelcomeFade()
    {
        if (!pendingWelcomeFade) return false;
        pendingWelcomeFade = false;
        return true;
    }
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
        OnDayStarted += SetDayMusic;
        OnNightStarted += SetNightMusic;
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "MainMenu")
        {
            AkSoundEngine.StopAll(gameObject);
            return;
        }

        AkSoundEngine.SetSwitch("TimeOfDay", IsNightActive ? "Night" : "Day", gameObject);
        timeOfDayMusic.Post(gameObject);
    }

    private void Start()
    {
        StartNight();

        SetCompassVisible(false);
    }

    private void Update()
    {
        if (IsNightActive && IsOutside && !TutorialModeFlag.IsActive)
        {
            NightTimeRemaining -= Time.deltaTime;
            if (NightTimeRemaining <= 0f)
            {
                NightTimeRemaining = 0f;
                IsNightActive = false;
                OnNightTimeExpired?.Invoke();
            }
        }

        if (IsCraftingTimeActive && !TutorialModeFlag.IsActive)
        {
            DayTimeRemaining -= Time.deltaTime;
            if (DayTimeRemaining <= 0f)
            {
                DayTimeRemaining = 0f;
                IsCraftingTimeActive = false;
            }
        }
    }
    

    public float LinearNightProgress
    {
        get
        {
            if (!IsNightActive) return 1f;
            return 1f - Mathf.Clamp01(NightTimeRemaining / nightDuration);
        }
    }

    public float DayProgress
    {
        get
        {
            if (!IsCraftingTimeActive) return 1f;
            return 1f - Mathf.Clamp01(DayTimeRemaining / dayDuration);
        }
    }

    public void StartNight()
    {
        NightTimeRemaining = nightDuration;
        IsNightActive = true;
        IsOutside = false;
        OnNightStarted?.Invoke();

        SetCompassVisible(false);
    }
    
    public float NightProgress
    {
        get
        {
            if (!IsNightActive) return 1f;

            float rawProgress = 1f - Mathf.Clamp01(NightTimeRemaining / nightDuration);
            return Mathf.Clamp01((rawProgress - (1f - dawnFraction)) / dawnFraction);
        }
    }
    public void ResetForNewGame()
    {
        CurrentDay = 1;
        Money = 0;
        HasBeenOutsideThisCycle = false;
        ShouldSpawnAtDoor = false;
        IsNightActive = false;
        NightTimeRemaining = 0f;
        SleepIngredientPurchased = false;
        SleepIngredientObtained = false;
        SleepIngredientPurchaseDay = 0;
        IsCraftingTimeActive = true;
        DayTimeRemaining = dayDuration;
        RequestWelcomeFade();
        Save();
        StartNight();
    }

    public void EnterHouse()
    {
        IsNightActive = false;
        IsOutside = false;
        IsCraftingTimeActive = true;
        DayTimeRemaining = dayDuration;
        OnDayStarted?.Invoke();

        SetCompassVisible(false);
    }

    public void Sleep()
    {
        CurrentDay++;
        HasBeenOutsideThisCycle = false;
        ShouldSpawnAtDoor = false;
        IsCraftingTimeActive = false;

        Save();

        StartNight();
    }

    public float FullCycleProgress
    {
        get
        {
            if (IsOutside)
                return LinearNightProgress * 0.5f;

            if (IsCraftingTimeActive)
                return 0.5f + DayProgress * 0.25f;

            if (!IsNightActive)
                return 0.75f; // crafteo cerrado, todavía no dormiste: se queda acá

            return 0f; // recién arrancando el ciclo (justo después de dormir, antes de salir)
        }
    }

    public void AddMoney(int amount)
    {
        Money += amount;
    }

    public void Save()
    {
        if (TutorialModeFlag.IsActive) return;

        SaveData data = new SaveData
        {
            day = CurrentDay,
            money = Money,
            shouldSpawnAtDoor = ShouldSpawnAtDoor,
            sleepIngredientPurchased = SleepIngredientPurchased,
            sleepIngredientObtained = SleepIngredientObtained,
            sleepIngredientPurchaseDay = SleepIngredientPurchaseDay
        };
        string json = JsonUtility.ToJson(data);

        PlayerPrefs.SetString(SaveKey, json);
        PlayerPrefs.SetInt(DayKey, CurrentDay);

        PlayerPrefs.Save();

        Debug.Log($"[GameProgressManager] Guardado: {json}");
    }

    public void Load()
    {
        if (!PlayerPrefs.HasKey(SaveKey))
        {
            CurrentDay = 1;
            Money = 0;
            Debug.Log("[GameProgressManager] No hay datos guardados, arranca en día 1.");
            return;
        }

        string json = PlayerPrefs.GetString(SaveKey);
        SaveData data = JsonUtility.FromJson<SaveData>(json);

        CurrentDay = data.day;
        Money = data.money;
        ShouldSpawnAtDoor = data.shouldSpawnAtDoor;
        SleepIngredientPurchased = data.sleepIngredientPurchased;
        SleepIngredientObtained = data.sleepIngredientObtained;
        SleepIngredientPurchaseDay = data.sleepIngredientPurchaseDay;

        Debug.Log($"[GameProgressManager] Cargado: {json}");
    }

    // funciones para cambiar musica con wwise
    private void SetDayMusic()
    {
        if (SceneManager.GetActiveScene().name == "MainMenu") return;
        AkSoundEngine.SetSwitch("TimeOfDay", "Day", gameObject);
    }

    private void SetNightMusic()
    {
        if (SceneManager.GetActiveScene().name == "MainMenu") return;
        AkSoundEngine.SetSwitch("TimeOfDay", "Night", gameObject);
    }



    [Serializable]
    private class SaveData
    {
        public int day;
        public int money;
        public bool shouldSpawnAtDoor;
        public bool sleepIngredientPurchased;
        public bool sleepIngredientObtained;
        public int sleepIngredientPurchaseDay;
    }
    //fix no dejaba recolectar objetos 
    public void ResumeNight()
    {
        if (NightTimeRemaining > 0f)
            IsNightActive = true;
    }

    // manejo de compas
    private void SetCompassVisible(bool visible)
    {
        if (compassUIDocument == null)
        {
            Debug.LogError("Compass UIDocument no está asignado.");
            return;
        }

        VisualElement root = compassUIDocument.rootVisualElement;

        if (root == null)
        {
            Debug.LogError("Compass rootVisualElement es null.");
            return;
        }

        root.style.display = visible
            ? DisplayStyle.Flex
            : DisplayStyle.None;

        Debug.Log($"🧭 Compass visible: {visible}");
    }
}