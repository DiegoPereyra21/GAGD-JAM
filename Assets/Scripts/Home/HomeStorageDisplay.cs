using System.Collections.Generic;
using UnityEngine;
using Game.Collectibles;

public class HomeStorageDisplay : MonoBehaviour
{
    [SerializeField] private IngredientDisplayArea displayArea;
    [SerializeField] private List<TypeVisualPrefab> visualPrefabs;

    private void OnEnable()
    {
        HomeStorage.Instance.OnStorageChanged += Refresh;
        Refresh();
    }

    private void OnDisable()
    {
        HomeStorage.Instance.OnStorageChanged -= Refresh;
    }

    private void Refresh()
    {
        foreach (var pair in HomeStorage.Instance.Totals)
            displayArea.SetCount(pair.Key, pair.Value, GetVisualPrefab(pair.Key));
    }

    private GameObject GetVisualPrefab(IngredientType type)
    {
        foreach (TypeVisualPrefab entry in visualPrefabs)
            if (entry.type == type) return entry.prefab;
        return null;
    }
}