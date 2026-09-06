using UnityEngine;
using UnityEngine.SceneManagement;

public class TutorialManager : MonoBehaviour
{
    [SerializeField] private StairInteraction downstairs;
    [SerializeField] private DoorExitInteraction doorExit;
    [SerializeField] private QuestManager questManager;
    [SerializeField] private int questsNeededForNextStep = 3;
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    private enum Step { None, Movement, GoOutside, CheckMailbox, CollectItems, Done }
    private Step currentStep = Step.None;

    private void Start()
    {
        currentStep = Step.Movement;
        TutorialUI.Instance.Show("Tutorial", "Usa WASD para moverte y E para interactuar con los objetos del ambiente.");
    }

    private void OnEnable()
    {
        if (downstairs != null) downstairs.OnUsed += HandleWentDownstairs;
        if (doorExit != null) doorExit.OnUsed += HandleWentOutside;
        if (questManager != null) questManager.OnQuestsChanged += HandleQuestsChanged;

        GameProgressManager.Instance.OnDayStarted += FinishTutorial;
        GameProgressManager.Instance.OnNightTimeExpired += FinishTutorial;
    }

    private void OnDisable()
    {
        if (downstairs != null) downstairs.OnUsed -= HandleWentDownstairs;
        if (doorExit != null) doorExit.OnUsed -= HandleWentOutside;
        if (questManager != null) questManager.OnQuestsChanged -= HandleQuestsChanged;

        GameProgressManager.Instance.OnDayStarted -= FinishTutorial;
        GameProgressManager.Instance.OnNightTimeExpired -= FinishTutorial;
    }

    private void HandleWentDownstairs()
    {
        if (currentStep != Step.Movement) return;

        currentStep = Step.GoOutside;
        TutorialUI.Instance.Show("Tutorial", "Sal afuera a recolectar ítems antes de intentar hacer alguna poción.");
    }

    private void HandleWentOutside()
    {
        if (currentStep != Step.GoOutside) return;

        currentStep = Step.CheckMailbox;
        TutorialUI.Instance.Show("Tutorial", "Recoge las misiones diarias del buzón.");
    }

    private void HandleQuestsChanged()
    {
        if (currentStep != Step.CheckMailbox) return;
        if (questManager.ActiveQuests.Count < questsNeededForNextStep) return;

        currentStep = Step.CollectItems;
        TutorialUI.Instance.Show("Tutorial", "Recolecta los ingredientes necesarios para las misiones.");
    }

    private void FinishTutorial()
    {
        if (currentStep != Step.CollectItems) return;

        currentStep = Step.Done;
        TutorialUI.Instance.Hide();
        TutorialModeFlag.IsActive = false;
        Time.timeScale = 1f;

        SceneManager.LoadScene(mainMenuSceneName);
    }
}