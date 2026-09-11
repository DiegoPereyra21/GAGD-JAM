using UnityEngine;
using UnityEngine.InputSystem;

public class StairInteraction : MonoBehaviour
{
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private CharacterController playerController;
    [SerializeField] private CameraTransition cameraTransition;
    [SerializeField] private Transform teleportDestination;
    [SerializeField] private Transform cameraViewAnchor;
    [SerializeField] private InteractableOutline outline;
    [SerializeField] private float interactRadius = 1.5f;
    public bool EntryBlocked { get; set; }
    public string BlockedMessage { get; set; } = "Todavía tengo cosas pendientes antes de subir.";

    private InputAction interactAction;
    //Test
    private static float lastTeleportTime = -999f;
    private const float teleportCooldown = 0.3f;

    public event System.Action OnUsed;

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

        playerController.enabled = false;
        playerController.transform.position = teleportDestination.position;
        playerController.enabled = true;

        cameraTransition.TransitionTo(cameraViewAnchor);
        OnUsed?.Invoke();
    }
}