using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Game.Collectibles;

public class ProcessedSlot : MonoBehaviour
{
    [SerializeField] private IngredientType type;
    [SerializeField] private Transform visualAnchor;
    [SerializeField] private TextMeshPro countLabel;
    [SerializeField] private int itemsPerRow = 3;
    [SerializeField] private float spacing = 0.3f;

    public IngredientType Type => type;

    private readonly List<GameObject> currentVisuals = new List<GameObject>();

    public void AddOne(GameObject visualPrefab)
    {
        if (visualPrefab == null) return;

        int index = currentVisuals.Count;
        int row = index / itemsPerRow;
        int col = index % itemsPerRow;

        Vector3 offset = visualAnchor.right * (col * spacing) + visualAnchor.forward * (row * spacing);
        GameObject visual = Instantiate(visualPrefab, visualAnchor.position + offset, visualAnchor.rotation, visualAnchor);
        currentVisuals.Add(visual);

        UpdateLabel();
    }

    public void RemoveOne()
    {
        if (currentVisuals.Count == 0) return;

        int lastIndex = currentVisuals.Count - 1;
        Destroy(currentVisuals[lastIndex]);
        currentVisuals.RemoveAt(lastIndex);

        UpdateLabel();
    }

    private void UpdateLabel()
    {
        countLabel.text = currentVisuals.Count.ToString();
    }
}