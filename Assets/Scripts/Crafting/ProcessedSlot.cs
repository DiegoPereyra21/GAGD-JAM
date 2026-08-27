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

    public void Refresh(GameObject visualPrefab, int count)
    {
        foreach (GameObject visual in currentVisuals)
            Destroy(visual);
        currentVisuals.Clear();

        if (visualPrefab != null)
        {
            for (int i = 0; i < count; i++)
            {
                int row = i / itemsPerRow;
                int col = i % itemsPerRow;

                Vector3 offset = visualAnchor.right * (col * spacing) + visualAnchor.forward * (row * spacing);
                GameObject visual = Instantiate(visualPrefab, visualAnchor.position + offset, visualAnchor.rotation, visualAnchor);
                currentVisuals.Add(visual);
            }
        }

        countLabel.text = count.ToString();
    }
}