using UnityEngine;
using UnityEngine.InputSystem;

// Único lector del mouse en el crafteo: clicks, drag y drop.
public class CraftingPointer : MonoBehaviour
{
    [SerializeField] private CraftingRoom room;
    [SerializeField] private CraftingCameraRail cameraRail;
    [SerializeField] private Cauldron cauldron;
    [SerializeField] private WorkTable[] tables;

    [SerializeField] private LayerMask craftingLayer;
    [SerializeField] private float maxDistance = 100f;
    [Tooltip("Píxeles que se puede mover el mouse y que siga contando como click.")]
    [SerializeField] private float clickThreshold = 15f;
    [Tooltip("Cuánto se eleva el ítem mientras se arrastra.")]
    [SerializeField] private float dragHeightOffset = 0.8f;

    private ItemVisual dragged;
    private ItemShelfView dragSource;
    private Vector2 dragStartScreenPos;
    private float dragPlaneHeight;

    public bool IsDragging => dragged != null;

    private void Update()
    {
        if (!room.IsInside || room.IsBusy) return;

        Mouse mouse = Mouse.current;
        if (mouse == null) return;

        Vector2 screenPos = mouse.position.ReadValue();

        if (IsDragging)
        {
            FollowPointer(screenPos);
            if (mouse.leftButton.wasReleasedThisFrame)
                EndDrag(screenPos);
            return;
        }

        if (mouse.leftButton.wasPressedThisFrame)
            HandlePress(screenPos);
    }

    public void CancelDrag()
    {
        if (!IsDragging) return;
        FinishDrag();
    }

    private void HandlePress(Vector2 screenPos)
    {
        if (CraftingNavArrows.PointerOverUI) return;

        if (!TryRaycast(screenPos, out RaycastHit hit)) return;

        GameObject target = hit.collider.gameObject;

        if (cameraRail.TryHandleClick(target)) return;

        ItemVisual visual = hit.collider.GetComponentInParent<ItemVisual>();
        if (visual != null)
        {
            if (visual.Owner == cauldron)
                cauldron.TryReturnToPantry(visual);
            else if (visual.Owner is ItemShelfView shelf)
                BeginDrag(visual, shelf, screenPos);
            return;
        }

        if (cauldron.IsClickTarget(target))
            cauldron.TryCraft();
    }

    private void BeginDrag(ItemVisual visual, ItemShelfView shelf, Vector2 screenPos)
    {
        shelf.Detach(visual.gameObject);

        dragged = visual;
        dragSource = shelf;
        dragStartScreenPos = screenPos;
        dragPlaneHeight = visual.transform.position.y + dragHeightOffset;

        visual.transform.SetParent(null, true);

        foreach (Collider col in visual.GetComponentsInChildren<Collider>())
            col.enabled = false;

        if (visual.TryGetComponent(out Rigidbody rb))
            rb.isKinematic = true;
    }

    private void FollowPointer(Vector2 screenPos)
    {
        Ray ray = Camera.main.ScreenPointToRay(screenPos);
        Plane plane = new Plane(Vector3.up, new Vector3(0f, dragPlaneHeight, 0f));

        if (plane.Raycast(ray, out float distance))
            dragged.transform.position = ray.GetPoint(distance);
    }

    private void EndDrag(Vector2 screenPos)
    {
        ItemKey key = dragged.Key;
        bool wasClick = Vector2.Distance(screenPos, dragStartScreenPos) < clickThreshold;
        GameObject target = TryRaycast(screenPos, out RaycastHit hit) ? hit.collider.gameObject : null;

        if (wasClick || cauldron.IsDropTarget(target))
        {
            cauldron.TryAddFromPantry(key);
        }
        else
        {
            foreach (WorkTable table in tables)
            {
                if (table != null && table.IsStation(target))
                {
                    table.TryProcess(key);
                    break;
                }
            }
        }

        FinishDrag();
    }

    // Destruye el visual arrastrado y deja que el estante se redibuje según los datos.
    private void FinishDrag()
    {
        Destroy(dragged.gameObject);
        dragged = null;

        if (dragSource != null)
            dragSource.Refresh();

        dragSource = null;
    }

    private bool TryRaycast(Vector2 screenPos, out RaycastHit hit)
    {
        Ray ray = Camera.main.ScreenPointToRay(screenPos);
        return Physics.Raycast(ray, out hit, maxDistance, craftingLayer, QueryTriggerInteraction.Collide);
    }
}