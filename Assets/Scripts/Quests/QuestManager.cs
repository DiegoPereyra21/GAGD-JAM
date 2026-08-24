using System.Collections.Generic;
using UnityEngine;
//POR AHORA, se deben cargar las misiones a mano desdel el inpector. luego hay que implementar el BUZON el cual nos daria misiones "random", USANDO ADDQUEST()
public class QuestManager : MonoBehaviour
{
    [SerializeField] private List<QuestData> activeQuests = new List<QuestData>();

    public IReadOnlyList<QuestData> ActiveQuests => activeQuests;

    public void AddQuest(QuestData quest)
    {
        if (!activeQuests.Contains(quest))
            activeQuests.Add(quest);
    }
}