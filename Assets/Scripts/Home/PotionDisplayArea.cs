using System.Collections.Generic;
using UnityEngine;

public class PotionDisplayArea : MonoBehaviour
{
    [SerializeField] private Transform areaOrigin;
    [SerializeField] private float itemSpacing = 0.3f;
    [SerializeField] private float groupSpacing = 0.5f;

    [Header("Preview (solo editor, no afecta el juego)")]
    [SerializeField] private int previewGroupCount = 4;
    [SerializeField] private int previewItemsPerGroup = 3;

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
        Vector3 offset = group.anchor.right * (index * itemSpacing);

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
        for (int i = 0; i < groupOrder.Count; i++)
            groupOrder[i].anchor.localPosition = new Vector3(i * groupSpacing, 0f, 0f);
    }

    private void OnDrawGizmosSelected()
    {
        if (areaOrigin == null) return;

        Gizmos.matrix = areaOrigin.localToWorldMatrix;

        for (int g = 0; g < previewGroupCount; g++)
        {
            Vector3 groupStart = new Vector3(g * groupSpacing, 0f, 0f);

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(groupStart, new Vector3(groupSpacing * 0.9f, 0.05f, 0.1f));

            for (int i = 0; i < previewItemsPerGroup; i++)
            {
                Vector3 itemPos = groupStart + Vector3.right * (i * itemSpacing);
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(itemPos, 0.05f);
            }
        }

        Gizmos.matrix = Matrix4x4.identity;
    }
}