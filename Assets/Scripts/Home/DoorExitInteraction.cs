using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Collider))]
public class DoorExitInteraction : MonoBehaviour
{
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private CameraTransition cameraTransition;
    [SerializeField] private InteractableOutline outline;
    //activa desactiva el canasto
    [SerializeField] private BasketDisplay basketDisplay;

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

        if (!GameProgressManager.Instance.IsNightActive)
        {
            Debug.Log("[DoorExitInteraction] No podés salir sin dormir primero.");
            return;
        }

        basketDisplay.SetAvailable(true);
        GameProgressManager.Instance.MarkWentOutside();
        cameraTransition.TransitionToPlayer();
    }

}