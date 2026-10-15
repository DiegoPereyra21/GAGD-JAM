using System.Collections.Generic;
using TMPro;
using UnityEngine;

// Dibuja un grupo por ItemKey (modelos + cantidad + nombre) a partir de un ItemContainer.
public class ItemShelfView : MonoBehaviour
{
    [SerializeField] private Transform areaOrigin;
    [SerializeField] private float areaWidth = 3f;
    [SerializeField] private float areaDepth = 2f;
    [SerializeField, Min(1)] private int itemsPerRow = 2;
    [SerializeField] private float itemSpacing = 0.3f;
    [SerializeField, Min(1)] private int maxVisualsPerGroup = 4;

    [Header("Etiquetas")]
    [SerializeField] private float countLabelHeight = 0.5f;
    [SerializeField] private float nameLabelHeight = 0.82f;
    [SerializeField] private float countFontSize = 1.5f;
    [SerializeField] private float nameFontSize = 1f;

    private class Group
    {
        public ItemKey key;
        public Transform anchor;
        public TextMeshPro countLabel;
        public readonly List<GameObject> visuals = new List<GameObject>();
        public bool missingModelWarned;
    }

    private readonly List<Group> groups = new List<Group>();
    private ItemContainer source;

    public void Bind(ItemContainer container, IReadOnlyList<ItemKey> keys)
    {
        Unbind();
        ClearGroups();

        foreach (ItemKey key in keys)
            groups.Add(CreateGroup(key));

        LayoutGroups();

        source = container;
        if (source != null)
            source.OnChanged += Refresh;

        Refresh();
    }

    public void Refresh()
    {
        if (source == null) return;

        foreach (Group group in groups)
        {
            int count = source.Count(group.key);
            int target = Mathf.Min(count, maxVisualsPerGroup);

            group.visuals.RemoveAll(v => v == null);

            while (group.visuals.Count > target)
            {
                int last = group.visuals.Count - 1;
                Destroy(group.visuals[last]);
                group.visuals.RemoveAt(last);
            }

            while (group.visuals.Count < target)
            {
                if (!SpawnVisual(group)) break;
            }

            PositionVisuals(group);
            group.countLabel.text = count.ToString();
        }
    }

    // Saca un visual de su grupo (por ejemplo, cuando se empieza a arrastrar).
    public void Detach(GameObject visual)
    {
        foreach (Group group in groups)
        {
            if (!group.visuals.Remove(visual)) continue;

            PositionVisuals(group);
            return;
        }
    }

    private void OnDestroy() => Unbind();

    private void Unbind()
    {
        if (source != null)
            source.OnChanged -= Refresh;

        source = null;
    }

    private bool SpawnVisual(Group group)
    {
        GameObject model = group.key.Item.GetModel(group.key.State);

        if (model == null)
        {
            if (!group.missingModelWarned)
            {
                Debug.LogWarning($"[ItemShelfView] '{group.key}' no tiene modelo asignado.", this);
                group.missingModelWarned = true;
            }
            return false;
        }

        GameObject visual = Instantiate(model, group.anchor);

        if (visual.TryGetComponent(out Rigidbody rb))
            rb.isKinematic = true;

        visual.AddComponent<ItemVisual>().Init(group.key, this);
        group.visuals.Add(visual);
        return true;
    }

    private void PositionVisuals(Group group)
    {
        for (int i = 0; i < group.visuals.Count; i++)
        {
            Transform t = group.visuals[i].transform;
            t.localPosition = new Vector3((i % itemsPerRow) * itemSpacing, 0f, (i / itemsPerRow) * itemSpacing);
            t.localRotation = Quaternion.identity;
        }
    }

    private Group CreateGroup(ItemKey key)
    {
        string label = key.DisplayName;

        GameObject anchorObject = new GameObject($"Group_{label}");
        anchorObject.transform.SetParent(areaOrigin, false);

        Group group = new Group
        {
            key = key,
            anchor = anchorObject.transform,
            countLabel = CreateLabel(anchorObject.transform, $"Count_{label}", countLabelHeight, countFontSize, "0")
        };

        CreateLabel(anchorObject.transform, $"Name_{label}", nameLabelHeight, nameFontSize, label.Replace(" ", "\n"));
        return group;
    }

    private static TextMeshPro CreateLabel(Transform parent, string objectName, float height, float fontSize, string text)
    {
        GameObject labelObject = new GameObject(objectName);
        labelObject.transform.SetParent(parent, false);
        labelObject.transform.localPosition = new Vector3(0f, height, 0f);

        TextMeshPro tmp = labelObject.AddComponent<TextMeshPro>();
        tmp.fontSize = fontSize;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.text = text;

        labelObject.AddComponent<Billboard>();
        return tmp;
    }

    private void LayoutGroups()
    {
        int total = groups.Count;
        if (total == 0) return;

        int columns = Mathf.CeilToInt(Mathf.Sqrt(total));
        int rows = Mathf.CeilToInt(total / (float)columns);

        float cellWidth = areaWidth / columns;
        float cellDepth = areaDepth / rows;

        for (int i = 0; i < total; i++)
        {
            int col = i % columns;
            int row = i / columns;
            groups[i].anchor.localPosition = new Vector3((col + 0.5f) * cellWidth, 0f, (row + 0.5f) * cellDepth);
        }
    }

    private void ClearGroups()
    {
        foreach (Group group in groups)
        {
            if (group.anchor != null)
                Destroy(group.anchor.gameObject);
        }

        groups.Clear();
    }

    private void OnDrawGizmosSelected()
    {
        if (areaOrigin == null) return;

        Gizmos.color = Color.cyan;
        Gizmos.matrix = areaOrigin.localToWorldMatrix;
        Gizmos.DrawWireCube(new Vector3(areaWidth * 0.5f, 0f, areaDepth * 0.5f), new Vector3(areaWidth, 0.01f, areaDepth));
        Gizmos.matrix = Matrix4x4.identity;
    }
}