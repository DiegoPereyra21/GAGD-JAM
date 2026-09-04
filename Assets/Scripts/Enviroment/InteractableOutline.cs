using UnityEngine;

public class InteractableOutline : MonoBehaviour
{
    [SerializeField] private Renderer targetRenderer;
    [SerializeField] private int outlineMaterialSlot = 1;
    [SerializeField] private string outlineProperty = "_Offset";
    [SerializeField] private float normalValue = 0f;
    [SerializeField] private float highlightedValue = 0.5f;

    private int outlinePropertyID;
    private MaterialPropertyBlock propertyBlock;

    private void Awake()
    {
        outlinePropertyID = Shader.PropertyToID(outlineProperty);
        propertyBlock = new MaterialPropertyBlock();
        SetHighlighted(false);
    }

    public void SetHighlighted(bool highlighted)
    {
        targetRenderer.GetPropertyBlock(propertyBlock, outlineMaterialSlot);
        propertyBlock.SetFloat(outlinePropertyID, highlighted ? highlightedValue : normalValue);
        targetRenderer.SetPropertyBlock(propertyBlock, outlineMaterialSlot);
    }

    public void SetTargetRenderer(Renderer renderer)
    {
        targetRenderer = renderer;
    }
}