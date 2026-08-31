using UnityEngine;

public static class GameSaveSystem
{
    public static void SaveAll(QuestManager questManager)
    {
        HomeStorage.Instance.Save();
        GameProgressManager.Instance.Save();
        questManager.Save();

        Debug.Log("[GameSaveSystem] Guardado completo (HomeStorage + GameProgressManager + QuestManager).");
    }
}