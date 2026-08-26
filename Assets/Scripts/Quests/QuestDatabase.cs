using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "QuestDatabase", menuName = "Quests/Quest Database")]
public class QuestDatabase : ScriptableObject
{
    [SerializeField] private List<QuestData> allQuests = new List<QuestData>();

    public QuestData FindById(string questId)
    {
        foreach (QuestData quest in allQuests)
        {
            if (quest.questId == questId)
                return quest;
        }

        return null;
    }
}