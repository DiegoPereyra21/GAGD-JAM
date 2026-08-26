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

    public void MarkWentOutside()
    {
        HasBeenOutsideThisCycle = true;
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
        if (!IsNightActive) return;

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
        OnNightStarted?.Invoke();
    }

    public void EnterHouse()
    {
        IsNightActive = false;
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