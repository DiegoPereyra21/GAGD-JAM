using System;
using UnityEngine;
using UnityEngine.InputSystem;

public enum CraftingZone
{
    Left,
    Center,
    Right
}

// Las 3 vistas de cámara del crafteo. Se navega con la acción Move (A/D, flechas, stick) o con las zonas de click.
public class CraftingCameraRail : MonoBehaviour
{
    [SerializeField] private CraftingRoom room;
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private CameraTransition cameraTransition;

    [Header("Anclas de cámara")]
    [SerializeField] private Transform leftAnchor;
    [SerializeField] private Transform centerAnchor;
    [SerializeField] private Transform rightAnchor;

    [Header("Zonas de click laterales")]
    [SerializeField] private GameObject leftClickZone;
    [SerializeField] private GameObject rightClickZone;

    [Tooltip("Cuánto hay que inclinar Move para cambiar de zona.")]
    [SerializeField, Range(0.1f, 1f)] private float inputThreshold = 0.5f;

    public event Action<CraftingZone> OnZoneChanged;

    public CraftingZone CurrentZone { get; private set; } = CraftingZone.Center;
    public bool IsActive { get; private set; }

    private InputAction moveAction;
    private int lastDirection;

    private void Awake() => moveAction = playerInput.actions["Move"];

    public void Activate()
    {
        IsActive = true;
        lastDirection = ReadDirection();
        SetZone(CraftingZone.Center);
    }

    public void Deactivate() => IsActive = false;

    private void Update()
    {
        if (!IsActive) return;

        int direction = ReadDirection();
        if (direction != 0 && direction != lastDirection && !room.IsBusy)
            Move(direction);

        lastDirection = direction;
    }

    public bool TryHandleClick(GameObject target)
    {
        if (!IsActive || target == null) return false;

        if (target == leftClickZone)
        {
            Move(CurrentZone == CraftingZone.Left ? 1 : -1);
            return true;
        }

        if (target == rightClickZone)
        {
            Move(CurrentZone == CraftingZone.Right ? -1 : 1);
            return true;
        }

        return false;
    }

    private int ReadDirection()
    {
        float x = moveAction.ReadValue<Vector2>().x;
        if (x > inputThreshold) return 1;
        if (x < -inputThreshold) return -1;
        return 0;
    }

    private void Move(int direction)
    {
        CraftingZone next = (CraftingZone)Mathf.Clamp((int)CurrentZone + direction, 0, 2);
        if (next != CurrentZone)
            SetZone(next);
    }

    private void SetZone(CraftingZone zone)
    {
        CurrentZone = zone;
        cameraTransition.TransitionTo(GetAnchor(zone));
        OnZoneChanged?.Invoke(zone);
    }

    private Transform GetAnchor(CraftingZone zone)
    {
        switch (zone)
        {
            case CraftingZone.Left: return leftAnchor;
            case CraftingZone.Right: return rightAnchor;
            default: return centerAnchor;
        }
    }
}