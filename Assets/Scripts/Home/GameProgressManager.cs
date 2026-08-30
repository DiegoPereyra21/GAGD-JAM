using System;
using UnityEngine;

public class GameProgressManager : MonoBehaviour
{
    private const string SaveKey = "GameProgress_Data";

    public static GameProgressManager Instance { get; private set; }

    [SerializeField] private float nightDuration = 300f;

    public event Action OnNightStarted;
    public event Action OnDayStarted;

    public int CurrentDay { get; private set; } = 1;
    public int Money { get; private set; }
    public float NightTimeRemaining { get; private set; }
    public bool IsNightActive { get; private set; }
    public bool HasBeenOutsideThisCycle { get; private set; }
    public bool IsInsane { get; private set; } = true; // para el indicador de sanidad en las transiciones
    public int InventoryCount { get; private set; } = 0; // para tener un conteo de items que se quedan en la casa
    public static bool HasSaveData => PlayerPrefs.HasKey(SaveKey);
    //para q solamente empiece el ciclo cuanod salga
    public bool IsOutside { get; private set; }

    public void MarkOutside()
    {
        IsOutside = true;
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
    }

    private void Start()
    {
        StartNight();
    }

    private void Update()
    {
        if (!IsNightActive || !IsOutside) return;

        NightTimeRemaining -= Time.deltaTime;
        if (NightTimeRemaining <= 0f)
        {
            NightTimeRemaining = 0f;
            IsNightActive = false;
        }
    }
    public void StartNight()
    {
        NightTimeRemaining = nightDuration;
        IsNightActive = true;
        IsOutside = false;
        OnNightStarted?.Invoke();
    }
    
    public float NightProgress
    {
        get
        {
            if (!IsNightActive) return 1f;
            return 1f - Mathf.Clamp01(NightTimeRemaining / nightDuration);
        }
    }
    public void ResetForNewGame()
    {
        CurrentDay = 1;
        Money = 0;
        HasBeenOutsideThisCycle = false;
        IsNightActive = false;
        NightTimeRemaining = 0f;
        RequestWelcomeFade();
        Save();
        StartNight();
    }

    public void EnterHouse()
    {
        IsNightActive = false;
        IsOutside = false;
        OnDayStarted?.Invoke();
    }

    public void Sleep()
    {
        CurrentDay++;
        HasBeenOutsideThisCycle = false;
        Save();
        StartNight();
    }
    public void AddMoney(int amount)
    {
        Money += amount;
    }

    public void Save()
    {
        SaveData data = new SaveData { day = CurrentDay, money = Money };
        string json = JsonUtility.ToJson(data);
        PlayerPrefs.SetString(SaveKey, json);
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

        Debug.Log($"[GameProgressManager] Cargado: {json}");
    }



    [Serializable]
    private class SaveData
    {
        public int day;
        public int money;
    }
    //fix no dejaba recolectar objetos 
    public void ResumeNight()
    {
        if (NightTimeRemaining > 0f)
            IsNightActive = true;
    }
}