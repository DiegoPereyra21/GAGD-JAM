using System.Collections.Generic;
using System.Linq;
using Game.Collectibles;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TutorialManager : MonoBehaviour
{
    [SerializeField] private StairInteraction downstairs;
    [SerializeField] private DoorExitInteraction doorExit;
    [SerializeField] private QuestManager questManager;
    [SerializeField] private CauldronCraftingSystem cauldronCraftingSystem;
    [SerializeField] private DeliveryInteraction deliveryInteraction;
    [SerializeField] private PlayerCollector playerCollector;
    [SerializeField] private DoorInteraction doorInteraction;
    [SerializeField] private PlayerInventory inventory;
    [SerializeField] private StairInteraction upstairs; 
    [SerializeField] private int questsNeededForNextStep = 3;
    [SerializeField] private string mainMenuSceneName = "MainMenu";
    [SerializeField] private BasketDisplay basketDisplay;
    private bool inventoryOpenedOnce;
    private bool inventoryClosedMessageShown;
    private bool collectedMessageShown;

    private enum Step
    {
        Movement, GoOutside, CheckMailbox, CollectItems,
        CraftingIntro, CraftPotion, Deliver, WaitingToSleep, Done
    }

    private Step currentStep = Step.Movement;
    private bool nightPhaseDone;

    private void Start()
    {
        TutorialUI.Instance.Show("Tutorial", "Usa WASD para moverte y E para interactuar con los objetos del ambiente.");
    }

    private void Update()
    {
        if (currentStep == Step.CollectItems && doorInteraction != null)
        {
            bool hasAll = HasAllQuestMaterials();
            doorInteraction.EntryBlocked = !hasAll;
            doorInteraction.BlockedMessage = "Todavía me falta recolectar ingredientes para mis pedidos.";

            if (hasAll && !collectedMessageShown)
            {
                collectedMessageShown = true;
                TutorialUI.Instance.Show("Tutorial", "Ya tengo todo lo que necesito, vuelve a casa para terminar las pociones.");
            }
        }

        if (upstairs != null)
        {
            bool inCraftingPhase = currentStep == Step.CraftingIntro
                || currentStep == Step.CraftPotion
                || currentStep == Step.Deliver;

            if (inCraftingPhase && !AllQuestsDelivered())
            {
                upstairs.EntryBlocked = true;
                upstairs.BlockedMessage = HomeStorage.Instance.CraftedPotions.Count > 0
                    ? "Entrega las pociones antes de acostarte."
                    : "Crea las pociones antes de acostarte.";
            }
            else
            {
                upstairs.EntryBlocked = false;
            }
        }
    }

    private bool HasAllQuestMaterials()
    {
        var remaining = new System.Collections.Generic.Dictionary<IngredientType, int>();

        foreach (QuestData quest in questManager.ActiveQuests)
        {
            foreach (QuestObjective objective in quest.objectives)
            {
                IngredientType rawType = objective.type.rawSource != null ? objective.type.rawSource : objective.type;

                if (!remaining.ContainsKey(rawType))
                    remaining[rawType] = inventory.GetCount(rawType) + HomeStorage.Instance.Totals.GetValueOrDefault(rawType);

                if (remaining[rawType] < objective.targetAmount) return false;

                remaining[rawType] -= objective.targetAmount;
            }
        }

        return true;
    }

    private bool AllQuestsDelivered()
    {
        foreach (QuestData quest in questManager.ActiveQuests)
        {
            if (!questManager.PendingDeliveries.Contains(quest))
                return false;
        }

        return true;
    }

    private void OnEnable()
    {
        if (downstairs != null) downstairs.OnUsed += HandleWentDownstairs;
        if (doorExit != null) doorExit.OnUsed += HandleWentOutside;
        if (questManager != null) questManager.OnQuestsChanged += HandleQuestsChanged;
        if (basketDisplay != null) basketDisplay.OnOpened += HandleInventoryOpened;
        if (basketDisplay != null) basketDisplay.OnClosed += HandleInventoryClosed;

        if (cauldronCraftingSystem != null)
        {
            cauldronCraftingSystem.OnEnteredCrafting += HandleEnteredCrafting;
            cauldronCraftingSystem.OnPotionCrafted += HandlePotionCrafted;
        }

        if (deliveryInteraction != null)
            deliveryInteraction.OnInteracted += HandleDelivered;

        GameProgressManager.Instance.OnDayStarted += HandleWentInside;
        GameProgressManager.Instance.OnNightTimeExpired += HandleWentInside;
        GameProgressManager.Instance.OnNightStarted += HandleSlept;
    }

    private void OnDisable()
    {
        if (downstairs != null) downstairs.OnUsed -= HandleWentDownstairs;
        if (doorExit != null) doorExit.OnUsed -= HandleWentOutside;
        if (questManager != null) questManager.OnQuestsChanged -= HandleQuestsChanged;
        if (basketDisplay != null) basketDisplay.OnOpened -= HandleInventoryOpened;
        if (basketDisplay != null) basketDisplay.OnClosed -= HandleInventoryClosed;

        if (cauldronCraftingSystem != null)
        {
            cauldronCraftingSystem.OnEnteredCrafting -= HandleEnteredCrafting;
            cauldronCraftingSystem.OnPotionCrafted -= HandlePotionCrafted;
        }

        if (deliveryInteraction != null)
            deliveryInteraction.OnInteracted -= HandleDelivered;

        GameProgressManager.Instance.OnDayStarted -= HandleWentInside;
        GameProgressManager.Instance.OnNightTimeExpired -= HandleWentInside;
        GameProgressManager.Instance.OnNightStarted -= HandleSlept;
    }

    private void HandleWentDownstairs()
    {
        if (currentStep != Step.Movement) return;

        currentStep = Step.GoOutside;
        TutorialUI.Instance.Show("Tutorial", "Dirígete hacia la puerta para salir a buscar ingredientes para los pedidos, no olvides revisar el buzón para verlos.");
    }

    private void HandleWentOutside()
    {
        if (currentStep != Step.GoOutside) return;

        currentStep = Step.CheckMailbox;
        TutorialUI.Instance.Show("Tutorial", "Recoge las misiones diarias del buzón.");

        if (playerCollector != null) playerCollector.CollectionBlocked = true;

        if (doorInteraction != null)
        {
            doorInteraction.EntryBlocked = true;
            doorInteraction.BlockedMessage = "Todavía no acepté todos los pedidos del buzón, no puedo entrar sin eso.";
        }
    }

    private void HandleInventoryClosed()
    {
        if (currentStep != Step.CollectItems || !inventoryOpenedOnce || inventoryClosedMessageShown) return;
        inventoryClosedMessageShown = true;

        TutorialUI.Instance.Show("Tutorial", "Recolecta todos los ingredientes necesarios antes de que sea de dia, luego no voy a poder salir.");
    }

    private void HandleQuestsChanged()
    {
        if (currentStep != Step.CheckMailbox) return;
        if (questManager.ActiveQuests.Count < questsNeededForNextStep) return;

        currentStep = Step.CollectItems;
        TutorialUI.Instance.Show("Tutorial", "¡Ya tengo los pedidos! Ahora a juntar los ingredientes: con E los recojo, y con TAB puedo revisar en cualquier momento qué llevo encima.");

        if (playerCollector != null) playerCollector.CollectionBlocked = false;
        if (doorInteraction != null) doorInteraction.EntryBlocked = false;
    }

    private void HandleInventoryOpened()
    {
        if (currentStep != Step.CollectItems || inventoryOpenedOnce) return;
        inventoryOpenedOnce = true;

        TutorialUI.Instance.Show("Tutorial",
            "Acá veo todo lo que fui juntando. Si algo no me sirve, con un click lo devuelvo.");
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
        TutorialUI.Instance.ShowSequence("Tutorial",
            "Pulsa A y D para desplazarte entre las mesas de trabajo.",
            "Arrastra los ingredientes de las respectivas mesadas hacia el mortero o la tabla de picar para moler o cortarlos. Una vez hecho eso, haz click izquierdo en el material conseguido para meterlo en el caldero, algunos ingredientes no requerirán molerse ni cortarse, y podrás colocarlos solo haciendo click izquierdo en ellos.",
            "Si ves un ingrediente que no te gusta en el caldero, haz click izquierdo sobre él para quitarlo. Si ya tienes todos los ingredientes listos, haz click sobre la cuchara para preparar la poción.");
    }

    private void HandlePotionCrafted()
    {
        if (currentStep != Step.CraftPotion) return;

        currentStep = Step.Deliver;
        TutorialUI.Instance.Show("Tutorial", "Cuando termines la poción, ponla en el mostrador para entregarla y recibir el pago, puedes salir de la mesa de fabricación con el botón de interacción.");
    }

    private void HandleDelivered()
    {
        if (currentStep != Step.Deliver) return;

        currentStep = Step.WaitingToSleep;
        TutorialUI.Instance.Show("Tutorial", "Asegurate de juntar el dinero suficiente para comprar el ingrediente especial para tu propia poción del sueño pesado lo antes posible, de lo contrario… bueno, ya lo verás. Ahora podés subir a dormir.");
    }

    private void HandleSlept()
    {
        if (currentStep != Step.WaitingToSleep) return;
        FinishTutorial();
    }

    private void FinishTutorial()
    {
        TutorialUI.Instance.Hide();
        TutorialModeFlag.IsActive = false;
        Time.timeScale = 1f;

        SceneManager.LoadScene(mainMenuSceneName);
    }
}