using UnityEngine;
using TMPro;
using Game.Collectibles;

public class ProcessedSlot : MonoBehaviour
{
    [SerializeField] private IngredientType type;
    [SerializeField] private Transform visualAnchor;
    [SerializeField] private TextMeshPro countLabel;

    public IngredientType Type => type;

    private GameObject currentVisual;

    public void Refresh(GameObject visualPrefab, int count)
    {
        if (currentVisual == null && visualPrefab != null)
            currentVisual = Instantiate(visualPrefab, visualAnchor.position, visualAnchor.rotation, visualAnchor);

        if (currentVisual != null)
            currentVisual.SetActive(count > 0);

        countLabel.text = count.ToString();
    }
}