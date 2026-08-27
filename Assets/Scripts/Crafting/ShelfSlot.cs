using UnityEngine;
using TMPro;
using Game.Collectibles;

public class ShelfSlot : MonoBehaviour
{
    [SerializeField] private Transform visualAnchor;
    [SerializeField] private TextMeshPro countLabel;
    //para los objetos del mortero
    [SerializeField] private bool isProcessable;
    public bool IsProcessable => isProcessable;

    [SerializeField] private IngredientType type;
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