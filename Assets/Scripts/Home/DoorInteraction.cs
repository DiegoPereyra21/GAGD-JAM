using UnityEngine;
using UnityEngine.InputSystem;

public class DoorInteraction : MonoBehaviour
{
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private float interactRadius = 2f;
    [SerializeField] private PlayerInventory inventory;
    [SerializeField] private InteractableOutline outline;
    [SerializeField] private BasketDisplay basketDisplay;
    [SerializeField] private CameraTransition cameraTransition;
    [SerializeField] private Transform houseViewAnchor;
    [SerializeField] private QuestManager questManager;

    [SerializeField] private CharacterController playerController;
    [SerializeField] private Transform teleportDestination;
    [SerializeField] private DayTransition dayTransition;

    private static float lastTeleportTime = -999f;
    private const float teleportCooldown = 0.3f;

    public bool EntryBlocked { get; set; }
    public string BlockedMessage { get; set; } = "No puedo entrar todavía.";

    private InputAction interactAction;

    private void Awake()
    {
        interactAction = playerInput.actions["Interact"];
    }

    private void OnEnable() => interactAction.performed += OnInteract;
    private void OnDisable() => interactAction.performed -= OnInteract;

    private void Update()
    {
        outline?.SetHighlighted(IsPlayerInRange());
    }

    private bool IsPlayerInRange()
    {
        return Vector3.Distance(transform.position, playerTransform.position) <= interactRadius;
    }

    private void OnInteract(InputAction.CallbackContext ctx)
    {
        if (Time.time - lastTeleportTime < teleportCooldown) return;
        if (!IsPlayerInRange()) return;

        if (EntryBlocked)
        {
            DialogueUI.Instance.ShowMessage("Ofelia", BlockedMessage);
            return;
        }

        lastTeleportTime = Time.time;

        int itemsCollected = 0;
        foreach (var pair in inventory.Items)
            itemsCollected += pair.Value;

        HomeStorage.Instance.Deposit(inventory);
        GameProgressManager.Instance.MarkEnteredHouse();
        HomeStorage.Instance.Save();
        GameProgressManager.Instance.Save();
        questManager.Save();
        basketDisplay.ClearAll();
        basketDisplay.SetAvailable(false);

        playerController.enabled = false;
        playerController.transform.position = teleportDestination.position;
        playerController.enabled = true;

        dayTransition.PlayEndOfNight(itemsCollected, () =>
        {
            GameProgressManager.Instance.EnterHouse();
            cameraTransition.TransitionTo(houseViewAnchor);
        });
    }
}