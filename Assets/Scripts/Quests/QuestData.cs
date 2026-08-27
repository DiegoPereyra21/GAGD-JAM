using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewQuest", menuName = "Quests/Quest Data")]
public class QuestData : ScriptableObject
{
    public string questId;
    public string villagerName;
    public string missionName;
    public List<QuestObjective> objectives;
    public int moneyReward;
    public PotionRecipe requiredPotion;
}