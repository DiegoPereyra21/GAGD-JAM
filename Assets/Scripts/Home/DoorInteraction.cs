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
        if (!playerInRange) return;

        HomeStorage.Instance.Deposit(inventory);
        HomeStorage.Instance.Save();
        basketDisplay.ClearAll();
        basketDisplay.SetAvailable(false);

        GameProgressManager.Instance.EnterHouse();
        cameraTransition.TransitionTo(houseViewAnchor);
    }
}