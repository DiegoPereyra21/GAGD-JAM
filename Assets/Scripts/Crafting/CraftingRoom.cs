using System;
using UnityEngine;
using UnityEngine.InputSystem;

// Entrar y salir de la vista de crafteo.
public class CraftingRoom : MonoBehaviour
{
    [Header("Player")]
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private Transform playerTransform;
    [Tooltip("Raíz del player: se ocultan sus renderers mientras se craftea.")]
    [SerializeField] private GameObject playerRoot;
    [SerializeField] private Light playerSpotlight;

    [Header("Cámara")]
    [SerializeField] private CameraTransition cameraTransition;
    [SerializeField] private Transform houseViewAnchor;
    [SerializeField] private CraftingCameraRail cameraRail;

    [Header("Interacción")]
    [SerializeField] private CraftingPointer pointer;
    [SerializeField] private InteractableOutline outline;
    [SerializeField] private float interactRadius = 2f;

    public event Action OnEntered;
    public event Action OnExited;

    public bool IsInside { get; private set; }
    public bool IsBusy { get; private set; }

    private InputAction interactAction;
    private bool playerInRange;

    private void Awake() => interactAction = playerInput.actions["Interact"];
    private void OnEnable() => interactAction.performed += OnInteract;
    private void OnDisable() => interactAction.performed -= OnInteract;

    private void Update()
    {
        if (IsInside) return;

        bool inRange = Vector3.Distance(transform.position, playerTransform.position) <= interactRadius;
        if (inRange == playerInRange) return;

        playerInRange = inRange;
        SetOutline(playerInRange);
    }

    public void SetBusy(bool busy) => IsBusy = busy;

    private void OnInteract(InputAction.CallbackContext ctx)
    {
        if (IsBusy) return;

        if (IsInside) Exit();
        else if (playerInRange) Enter();
    }

    private void Enter()
    {
        IsInside = true;
        playerMovement.SetFrozen(true);
        SetPlayerVisible(false);
        if (playerSpotlight != null) playerSpotlight.enabled = false;
        SetOutline(false);

        cameraRail.Activate();
        OnEntered?.Invoke();
    }

    private void Exit()
    {
        pointer.CancelDrag();
        cameraRail.Deactivate();

        IsInside = false;
        playerMovement.SetFrozen(false);
        SetPlayerVisible(true);
        if (playerSpotlight != null) playerSpotlight.enabled = true;

        cameraTransition.TransitionTo(houseViewAnchor);
        playerInRange = false; // el outline se recalcula en el próximo Update

        OnExited?.Invoke();
    }

    private void SetPlayerVisible(bool visible)
    {
        foreach (Renderer r in playerRoot.GetComponentsInChildren<Renderer>(true))
            r.enabled = visible;
    }

    private void SetOutline(bool highlighted)
    {
        if (outline != null)
            outline.SetHighlighted(highlighted);
    }
}