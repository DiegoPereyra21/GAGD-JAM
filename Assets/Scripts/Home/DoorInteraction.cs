using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Collider))]
public class DoorInteraction : MonoBehaviour
{
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private PlayerInventory inventory;
    [SerializeField] private InteractableOutline outline;
    [SerializeField] private BasketDisplay basketDisplay;
    [SerializeField] private CameraTransition cameraTransition;
    [SerializeField] private Transform houseViewAnchor;

    //para que por ahora me tepee al otro lado de la puerta, luego agregaremos animacion de puerta y que se mueva obligadamente hacia afuera
    [SerializeField] private CharacterController playerController;
    [SerializeField] private Transform teleportDestination;
    [SerializeField] private DayTransition dayTransition;
    //mismo bug que en el tp de las escaleras
    private static float lastTeleportTime = -999f;
    private const float teleportCooldown = 0.3f;


    private InputAction interactAction;
    private bool playerInRange;

    private void Awake()
    {
        interactAction = playerInput.actions["Interact"];
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

    private void OnInteract(InputAction.CallbackContext ctx)
    {
        if (Time.time - lastTeleportTime < teleportCooldown) return;
        if (!playerInRange) return;

        lastTeleportTime = Time.time;

        int itemsCollected = 0;
        foreach (var pair in inventory.Items)
            itemsCollected += pair.Value;

        HomeStorage.Instance.Deposit(inventory);
        HomeStorage.Instance.Save();
        basketDisplay.ClearAll();
        basketDisplay.SetAvailable(false);

        playerController.enabled = false;
        playerController.transform.position = teleportDestination.position;
        playerController.enabled = true;
        playerInRange = false;

        dayTransition.PlayEndOfNight(itemsCollected, () =>
        {
            GameProgressManager.Instance.EnterHouse();
            cameraTransition.TransitionTo(houseViewAnchor);
        });
    }
}