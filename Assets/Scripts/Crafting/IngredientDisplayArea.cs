using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Game.Collectibles;

public class IngredientDisplayArea : MonoBehaviour
{
    [SerializeField] private Transform areaOrigin;
    [SerializeField] private float areaWidth = 3f;
    [SerializeField] private float areaDepth = 2f;
    [SerializeField] private int itemsPerRow = 3;
    [SerializeField] private float itemSpacing = 0.3f;
    [SerializeField] private float labelHeight = 0.5f;
    [SerializeField] private int maxVisualsPerGroup = 6;

    private class TypeGroup
    {
        public IngredientType type;
        public Transform anchor;
        public TextMeshPro label;
        public List<GameObject> visuals = new List<GameObject>();
        public int trueCount;
    }

    private readonly Dictionary<IngredientType, TypeGroup> groups = new Dictionary<IngredientType, TypeGroup>();
    private readonly List<TypeGroup> groupOrder = new List<TypeGroup>();

    public void AddOne(IngredientType type, GameObject visualPrefab)
    {
        TypeGroup group = GetOrCreateGroup(type);
        group.trueCount++;

        if (group.visuals.Count < maxVisualsPerGroup)
            SpawnVisual(group, visualPrefab);

        group.label.text = group.trueCount.ToString();
    }

    public void RemoveOne(IngredientType type, GameObject visualPrefab = null)
    {
        if (!groups.TryGetValue(type, out TypeGroup group) || group.trueCount <= 0) return;

        group.trueCount--;

        if (group.visuals.Count > 0)
        {
            int lastIndex = group.visuals.Count - 1;
            Destroy(group.visuals[lastIndex]);
            group.visuals.RemoveAt(lastIndex);
        }

        if (visualPrefab != null && group.trueCount >= maxVisualsPerGroup && group.visuals.Count < maxVisualsPerGroup)
            SpawnVisual(group, visualPrefab);

        group.label.text = group.trueCount.ToString();
    }

    public void SetCount(IngredientType type, int count, GameObject visualPrefab)
    {
        TypeGroup group = GetOrCreateGroup(type);
        group.trueCount = count;

        int visibleTarget = Mathf.Min(count, maxVisualsPerGroup);

        if (visualPrefab == null)
        {
            Debug.LogWarning($"[IngredientDisplayArea] Falta el prefab visual para {type.displayName}, no se puede mostrar.");
        }
        else
        {
            while (group.visuals.Count < visibleTarget)
                SpawnVisual(group, visualPrefab);
        }

        while (group.visuals.Count > visibleTarget)
        {
            int lastIndex = group.visuals.Count - 1;
            Destroy(group.visuals[lastIndex]);
            group.visuals.RemoveAt(lastIndex);
        }

        group.label.text = count.ToString();
    }

    private void SpawnVisual(TypeGroup group, GameObject visualPrefab)
    {
        if (visualPrefab == null) return;

        GameObject visual = Instantiate(visualPrefab, group.anchor);
        visual.AddComponent<ProcessedItemVisual>().Init(group.type);

        if (visual.TryGetComponent(out Rigidbody rb))
            rb.isKinematic = true;

        group.visuals.Add(visual);
        RepositionAll(group);
    }

    public bool TryPickUp(IngredientType type, GameObject visual)
    {
        if (!groups.TryGetValue(type, out TypeGroup group)) return false;
        if (!group.visuals.Remove(visual)) return false;

        group.trueCount = Mathf.Max(0, group.trueCount - 1);
        RepositionAll(group);
        group.label.text = group.trueCount.ToString();
        return true;
    }

    private void RepositionAll(TypeGroup group)
    {
        for (int i = 0; i < group.visuals.Count; i++)
        {
            Vector3 localOffset = new Vector3((i % itemsPerRow) * itemSpacing, 0f, (i / itemsPerRow) * itemSpacing);
            group.visuals[i].transform.localPosition = localOffset;
            group.visuals[i].transform.localRotation = Quaternion.identity;
        }
    }

    private TypeGroup GetOrCreateGroup(IngredientType type)
    {
        if (groups.TryGetValue(type, out TypeGroup existing))
            return existing;

        GameObject anchorObject = new GameObject($"Group_{type.displayName}");
        anchorObject.transform.SetParent(areaOrigin, false);

        GameObject labelObject = new GameObject($"Label_{type.displayName}");
        labelObject.transform.SetParent(anchorObject.transform, false);
        labelObject.transform.localPosition = new Vector3(0f, labelHeight, 0f);

        TextMeshPro label = labelObject.AddComponent<TextMeshPro>();
        label.fontSize = 3f;
        label.alignment = TextAlignmentOptions.Center;
        label.text = "0";

        TypeGroup group = new TypeGroup { type = type, anchor = anchorObject.transform, label = label };
        groups[type] = group;
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