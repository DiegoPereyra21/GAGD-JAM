using System.Collections.Generic;
using UnityEngine;

public class PotionDisplayArea : MonoBehaviour
{
    [SerializeField] private Transform areaOrigin;
    [SerializeField] private float areaWidth = 3f;
    [SerializeField] private float areaDepth = 2f;
    [SerializeField] private int itemsPerRow = 3;
    [SerializeField] private float itemSpacing = 0.3f;

    private class TypeGroup
    {
        public Transform anchor;
        public List<GameObject> visuals = new List<GameObject>();
    }

    private readonly Dictionary<PotionRecipe, TypeGroup> groups = new Dictionary<PotionRecipe, TypeGroup>();
    private readonly List<TypeGroup> groupOrder = new List<TypeGroup>();

    public void AddOne(PotionRecipe recipe, GameObject visualPrefab)
    {
        TypeGroup group = GetOrCreateGroup(recipe);
        SpawnVisual(group, visualPrefab);
    }

    public void ClearAll()
    {
        foreach (TypeGroup group in groupOrder)
        {
            foreach (GameObject visual in group.visuals)
                Destroy(visual);
            group.visuals.Clear();
        }
    }

    private void SpawnVisual(TypeGroup group, GameObject visualPrefab)
    {
        if (visualPrefab == null) return;

        int index = group.visuals.Count;
        Vector3 offset = group.anchor.right * ((index % itemsPerRow) * itemSpacing)
                        + group.anchor.forward * ((index / itemsPerRow) * itemSpacing);

        GameObject visual = Instantiate(visualPrefab, group.anchor.position + offset, group.anchor.rotation, group.anchor);
        group.visuals.Add(visual);
    }

    private TypeGroup GetOrCreateGroup(PotionRecipe recipe)
    {
        if (groups.TryGetValue(recipe, out TypeGroup existing))
            return existing;

        GameObject anchorObject = new GameObject($"Group_{recipe.potionName}");
        anchorObject.transform.SetParent(areaOrigin, false);

        TypeGroup group = new TypeGroup { anchor = anchorObject.transform };
        groups[recipe] = group;
        groupOrder.Add(group);

        RelayoutGroups();
        return group;
    }

    private void RelayoutGroups()
    {
        int total = groupOrder.Count;
        int columns = Mathf.CeilToInt(Mathf.Sqrt(total));
        int rows = Mathf.CeilToInt(total / (float)columns);

        float cellWidth = areaWidth / columns;
        float cellDepth = areaDepth / rows;

        for (int i = 0; i < groupOrder.Count; i++)
        {
            int col = i % columns;
            int row = i / columns;
            groupOrder[i].anchor.localPosition = new Vector3((col + 0.5f) * cellWidth, 0f, (row + 0.5f) * cellDepth);
        }
    }
}