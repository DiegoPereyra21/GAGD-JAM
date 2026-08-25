using System;
using System.Collections.Generic;
using UnityEngine;

public class QuestManager : MonoBehaviour
{
    public event Action OnQuestsChanged;

    [SerializeField] private List<QuestData> activeQuests = new List<QuestData>();

    public IReadOnlyList<QuestData> ActiveQuests => activeQuests;

    public void AddQuest(QuestData quest)
    {
        if (activeQuests.Contains(quest)) return;

        activeQuests.Add(quest);
        OnQuestsChanged?.Invoke();
    }
}