using System;
using System.Collections.Generic;
using UnityEngine;

public class QuestManager : MonoBehaviour
{
    private const string SaveKey = "QuestManager_Data";

    [SerializeField] private QuestDatabase questDatabase;
    [SerializeField] private List<QuestData> activeQuests = new List<QuestData>();

    public event Action OnQuestsChanged;

    public IReadOnlyList<QuestData> ActiveQuests => activeQuests;

    private readonly List<QuestData> pendingDeliveries = new List<QuestData>();
    public IReadOnlyList<QuestData> PendingDeliveries => pendingDeliveries;

    private void Awake()
    {
        Load();
    }

    public void AddQuest(QuestData quest)
    {
        if (activeQuests.Contains(quest)) return;

        activeQuests.Add(quest);
        OnQuestsChanged?.Invoke();
    }

    public void MarkPendingDelivery(QuestData quest)
    {
        if (pendingDeliveries.Contains(quest)) return;
        pendingDeliveries.Add(quest);
    }

    public void ProcessPendingDeliveries()
    {
        foreach (QuestData quest in pendingDeliveries)
        {
            GameProgressManager.Instance.AddMoney(quest.moneyReward);
            CompleteQuest(quest);
        }

        pendingDeliveries.Clear();
    }

    public void CompleteQuest(QuestData quest)
    {
        activeQuests.Remove(quest);
        OnQuestsChanged?.Invoke();
    }

    public void Save()
    {
        SaveData data = new SaveData();

        foreach (QuestData quest in activeQuests)
            data.questIds.Add(quest.questId);

        foreach (QuestData quest in pendingDeliveries)
            data.pendingDeliveryIds.Add(quest.questId);

        string json = JsonUtility.ToJson(data);
        PlayerPrefs.SetString(SaveKey, json);
        PlayerPrefs.Save();

        Debug.Log($"[QuestManager] Guardado: {json}");
    }

    public void Load()
    {
        activeQuests.Clear();
        pendingDeliveries.Clear();

        if (!PlayerPrefs.HasKey(SaveKey))
        {
            Debug.Log("[QuestManager] No hay datos guardados, arranca sin misiones activas.");
            return;
        }

        string json = PlayerPrefs.GetString(SaveKey);
        SaveData data = JsonUtility.FromJson<SaveData>(json);

        foreach (string id in data.questIds)
        {
            QuestData quest = questDatabase.FindById(id);
            if (quest != null)
                activeQuests.Add(quest);
        }

        foreach (string id in data.pendingDeliveryIds)
        {
            QuestData quest = questDatabase.FindById(id);
            if (quest != null)
                pendingDeliveries.Add(quest);
        }

        OnQuestsChanged?.Invoke();
        Debug.Log($"[QuestManager] Cargado: {json}");
    }

    [Serializable]
    private class SaveData
    {
        public List<string> questIds = new List<string>();
        public List<string> pendingDeliveryIds = new List<string>();
    }
}