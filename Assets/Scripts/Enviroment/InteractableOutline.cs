using UnityEngine;
//a lo que le ponga esto se va a crear un outline fachero que se ilumina al estar cerca
public class InteractableOutline : MonoBehaviour
{
    [SerializeField] private Renderer targetRenderer;
    [SerializeField] private int outlineMaterialSlot = 1;
    [SerializeField] private float highlightedSize = 1.1f;

    private static readonly int OutlineSizeID = Shader.PropertyToID("_Outline_Size");
    private MaterialPropertyBlock propertyBlock;

    private void Awake()
    {
        propertyBlock = new MaterialPropertyBlock();
        SetHighlighted(false);
    }

    public void SetHighlighted(bool highlighted)
    {
        targetRenderer.GetPropertyBlock(propertyBlock, outlineMaterialSlot);
        propertyBlock.SetFloat(OutlineSizeID, highlighted ? highlightedSize : 1f);
        targetRenderer.SetPropertyBlock(propertyBlock, outlineMaterialSlot);
    }
    public void SetTargetRenderer(Renderer renderer)
    {
        targetRenderer = renderer;
    }
}