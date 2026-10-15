using UnityEngine;

public class HoverOutline : MonoBehaviour
{
    [SerializeField] private InteractableOutline outline;
    [SerializeField] private LayerMask hoverLayer;
    [SerializeField] private float maxDistance = 100f;

    private bool isHovering;

    private void Update()
    {
        if (Camera.main == null) return;

        Ray ray = Camera.main.ScreenPointToRay(UnityEngine.InputSystem.Mouse.current.position.ReadValue());
        bool hitThisObject = Physics.Raycast(ray, out RaycastHit hit, maxDistance, hoverLayer)
            && hit.collider.gameObject == gameObject;

        if (hitThisObject != isHovering)
        {
            isHovering = hitThisObject;
            outline.SetHighlighted(isHovering);
        }
    }
}