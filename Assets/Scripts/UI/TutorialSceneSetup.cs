using UnityEngine;

public class TutorialSceneSetup : MonoBehaviour
{
    private void Awake()
    {
        TutorialModeFlag.IsActive = true;

        GameProgressManager.Instance.ResetForNewGame();
        HomeStorage.Instance.ResetInMemory();
    }
}