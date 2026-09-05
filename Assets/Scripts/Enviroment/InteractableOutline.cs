using UnityEngine;
using UnityURP.Outline;

public class InteractableOutline : MonoBehaviour
{
    [SerializeField] private OutlineRenderer outlineRenderer;

    private void Awake()
    {
        SetHighlighted(false);
    }

    public void SetHighlighted(bool highlighted)
    {
        if (outlineRenderer == null) return;
        outlineRenderer.enabled = highlighted;
    }
}