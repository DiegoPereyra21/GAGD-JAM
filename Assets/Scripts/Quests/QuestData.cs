using System.Collections.Generic;
using UnityEngine;
//Quizas luego agreguemos mas "datos" de cada aldeano, como la prisa en dias que tiene y la "calidad"
[CreateAssetMenu(fileName = "NewQuest", menuName = "Quests/Quest Data")]
public class QuestData : ScriptableObject
{
    public string villagerName;
    public string missionName;
    public List<QuestObjective> objectives;
}