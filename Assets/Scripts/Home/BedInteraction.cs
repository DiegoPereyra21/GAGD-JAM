using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(DayTransition))]
public class BedInteraction : MonoBehaviour
{
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private InteractableOutline outline;
    [SerializeField] private QuestManager questManager;
    [SerializeField] private PotionDisplayArea potionDisplayArea;
    [SerializeField] private PotionRecipe sleepPotionRecipe;
    private DayTransition dayTransition;
    private InputAction interactAction;
    private bool playerInRange;

    private void Awake()
    {
        interactAction = playerInput.actions["Interact"];
        dayTransition = GetComponent<DayTransition>();
    }

    private void OnEnable() => interactAction.performed += OnInteract;
    private void OnDisable() => interactAction.performed -= OnInteract;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            outline?.SetHighlighted(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            outline?.SetHighlighted(false);
        }
    }
    //para que no pueda ir a dormir sin haber salido nunca

    private void OnInteract(InputAction.CallbackContext ctx)
    {
        if (!playerInRange) return;

        if (!GameProgressManager.Instance.HasBeenOutsideThisCycle)
        {
            DialogueUI.Instance.ShowMessage("Ofelia", "No puedo acostarme ahora, tengo que recolectar.");
            return;
        }

        if (HomeStorage.Instance.RemoveCraftedPotion(sleepPotionRecipe))
        {
            questManager.ProcessPendingDeliveries();
            questManager.Save();
            HomeStorage.Instance.Save();
            dayTransition.PlayWinEnding();
            return;
        }

        if (GameProgressManager.Instance.CurrentDay >= 7)
        {
            questManager.ProcessPendingDeliveries();
            questManager.Save();
            dayTransition.PlayLoseEnding();
            return;
        }

        questManager.ProcessPendingDeliveries();
        questManager.ClearAllActiveQuests();
        questManager.Save();
        potionDisplayArea.ClearAll();
        GameProgressManager.Instance.Sleep();
        dayTransition.PlayDayIntro(GameProgressManager.Instance.CurrentDay);
    }

    private void Start()
    {
        if (GameProgressManager.Instance.ShouldSpawnAtDoor) return;

        if (GameProgressManager.Instance.ConsumeWelcomeFade())
            dayTransition.PlayDayIntro(GameProgressManager.Instance.CurrentDay);
    }
}