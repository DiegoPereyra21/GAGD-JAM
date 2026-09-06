using UnityEngine;
using UnityEngine.SceneManagement;

public class TutorialManager : MonoBehaviour
{
    [SerializeField] private StairInteraction downstairs;
    [SerializeField] private DoorExitInteraction doorExit;
    [SerializeField] private QuestManager questManager;
    [SerializeField] private CauldronCraftingSystem cauldronCraftingSystem;
    [SerializeField] private DeliveryInteraction deliveryInteraction;
    [SerializeField] private int questsNeededForNextStep = 3;
    [SerializeField] private string mainMenuSceneName = "MainMenu";
    [SerializeField] private float finalMessageDuration = 4f;

    private enum Step
    {
        Movement, GoOutside, CheckMailbox, CollectItems,
        CraftingIntro, CraftPotion, Deliver, Done
    }

    private Step currentStep = Step.Movement;
    private bool nightPhaseDone;

    private void Start()
    {
        TutorialUI.Instance.Show("Tutorial", "Usa WASD para moverte y E para interactuar con los objetos del ambiente.");
    }

    private void OnEnable()
    {
        if (downstairs != null) downstairs.OnUsed += HandleWentDownstairs;
        if (doorExit != null) doorExit.OnUsed += HandleWentOutside;
        if (questManager != null) questManager.OnQuestsChanged += HandleQuestsChanged;

        if (cauldronCraftingSystem != null)
        {
            cauldronCraftingSystem.OnEnteredCrafting += HandleEnteredCrafting;
            cauldronCraftingSystem.OnPotionCrafted += HandlePotionCrafted;
        }

        if (deliveryInteraction != null)
            deliveryInteraction.OnInteracted += HandleDelivered;

        GameProgressManager.Instance.OnDayStarted += HandleWentInside;
        GameProgressManager.Instance.OnNightTimeExpired += HandleWentInside;
    }

    private void OnDisable()
    {
        if (downstairs != null) downstairs.OnUsed -= HandleWentDownstairs;
        if (doorExit != null) doorExit.OnUsed -= HandleWentOutside;
        if (questManager != null) questManager.OnQuestsChanged -= HandleQuestsChanged;

        if (cauldronCraftingSystem != null)
        {
            cauldronCraftingSystem.OnEnteredCrafting -= HandleEnteredCrafting;
            cauldronCraftingSystem.OnPotionCrafted -= HandlePotionCrafted;
        }

        if (deliveryInteraction != null)
            deliveryInteraction.OnInteracted -= HandleDelivered;

        GameProgressManager.Instance.OnDayStarted -= HandleWentInside;
        GameProgressManager.Instance.OnNightTimeExpired -= HandleWentInside;
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

    // Entrar a la casa cierra la parte de afuera pase lo que pase (te saltees el buzón o no) —
    // así nunca queda trabado esperando algo que el jugador decidió no hacer.
    private void HandleWentInside()
    {
        if (nightPhaseDone) return;
        if (currentStep == Step.Movement || currentStep == Step.GoOutside) return;

        nightPhaseDone = true;
        currentStep = Step.CraftingIntro;
        TutorialUI.Instance.Show("Tutorial", "Revisa el caldero para preparar las pociones.");
    }

    private void HandleEnteredCrafting()
    {
        if (currentStep != Step.CraftingIntro) return;

        currentStep = Step.CraftPotion;
        TutorialUI.Instance.Show("Tutorial", "Arrastra los ingredientes hacia el mortero o la tabla de picar para procesarlos, y hacé click izquierdo en el resultado para meterlo en el caldero (algunos van directo con click, sin procesar). Si algo no te gusta, hacé click sobre él para sacarlo. Cuando esté listo, hacé click en la cuchara para preparar la poción.");
    }

    private void HandlePotionCrafted()
    {
        if (currentStep != Step.CraftPotion) return;

        currentStep = Step.Deliver;
        TutorialUI.Instance.Show("Tutorial", "Cuando termines la poción, ponla en el mostrador para entregarla y recibir el pago.");
    }

    private void HandleDelivered()
    {
        if (currentStep != Step.Deliver) return;

        currentStep = Step.Done;
        TutorialUI.Instance.Show("Tutorial", "Asegurate de juntar el dinero suficiente para comprar el ingrediente especial para tu propia poción del sueño pesado, o de lo contrario... bueno, ya lo verás.");

        Invoke(nameof(FinishTutorial), finalMessageDuration);
    }

    private void FinishTutorial()
    {
        TutorialUI.Instance.Hide();
        TutorialModeFlag.IsActive = false;
        Time.timeScale = 1f;

        SceneManager.LoadScene(mainMenuSceneName);
    }
}