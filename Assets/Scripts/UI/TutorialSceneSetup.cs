using UnityEngine;

public class TutorialSceneSetup : MonoBehaviour
{
    [SerializeField] private QuestManager questManager;
    private void Awake()
    {
        TutorialModeFlag.IsActive = true;

        GameProgressManager.Instance.ResetForNewGame();
        HomeStorage.Instance.ResetInMemory();
        questManager.ClearAllActiveQuests();
    }
}