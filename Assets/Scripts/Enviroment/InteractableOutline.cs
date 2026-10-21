using UnityEngine;
using UnityURP.Outline;

public class InteractableOutline : MonoBehaviour
{
    private const float PulsePeriod = 0.9f;
    private const float PulseVisibleRatio = 0.6f;

    [SerializeField] private OutlineRenderer outlineRenderer;

    private bool highlighted;
    private bool pulsing;

    private void Awake()
    {
        SetHighlighted(false);
    }

    public void SetHighlighted(bool value)
    {
        highlighted = value;
        Apply();
    }

    public void SetPulsing(bool value)
    {
        pulsing = value;
        Apply();
    }

    private void Update()
    {
        if (pulsing) Apply();
    }

    private void Apply()
    {
        if (outlineRenderer == null) return;

        bool visible = highlighted
            || (pulsing && Mathf.Repeat(Time.unscaledTime, PulsePeriod) < PulsePeriod * PulseVisibleRatio);

        if (outlineRenderer.enabled != visible)
            outlineRenderer.enabled = visible;
    }
}