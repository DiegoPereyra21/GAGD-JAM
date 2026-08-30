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

    //para que por ahora me tepee al otro lado de la puerta, luego agregaremos animacion de puerta y que se mueva obligadamente hacia afuera
    [SerializeField] private CharacterController playerController;
    [SerializeField] private Transform teleportDestination;
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

        if (!GameProgressManager.Instance.IsNightActive)
        {
            Debug.Log("[DoorExitInteraction] No podés salir sin dormir primero.");
            return;
        }

        lastTeleportTime = Time.time;

        playerController.enabled = false;
        playerController.transform.position = teleportDestination.position;
        playerController.enabled = true;
        playerInRange = false;

        basketDisplay.SetAvailable(true);
        GameProgressManager.Instance.MarkWentOutside();
        GameProgressManager.Instance.MarkOutside();
        cameraTransition.TransitionToPlayer();
    }

}