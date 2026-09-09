using System.Collections.Generic;
using UnityEngine;

public class PotionBoxDisplay : MonoBehaviour
{
    [SerializeField] private Transform[] slots;

    private readonly Dictionary<int, PotionRecipe> slotOccupants = new Dictionary<int, PotionRecipe>();
    private readonly Dictionary<int, GameObject> slotVisuals = new Dictionary<int, GameObject>();

    public bool AddOne(PotionRecipe recipe, GameObject visualPrefab)
    {
        if (visualPrefab == null) return false;

        int freeSlot = FindFreeSlot();
        if (freeSlot == -1)
        {
            Debug.LogWarning("[PotionBoxDisplay] No hay slots libres en la caja de pociones.");
            return false;
        }

        GameObject visual = Instantiate(visualPrefab, slots[freeSlot].position, slots[freeSlot].rotation, slots[freeSlot]);
        slotOccupants[freeSlot] = recipe;
        slotVisuals[freeSlot] = visual;
        return true;
    }

    public void RemoveOne(PotionRecipe recipe)
    {
        foreach (int slotIndex in new List<int>(slotOccupants.Keys))
        {
            if (slotOccupants[slotIndex] != recipe) continue;

            Destroy(slotVisuals[slotIndex]);
            slotOccupants.Remove(slotIndex);
            slotVisuals.Remove(slotIndex);
            return;
        }
    }

    public void ClearAll()
    {
        foreach (GameObject visual in slotVisuals.Values)
            Destroy(visual);

        slotOccupants.Clear();
        slotVisuals.Clear();
    }

    private int FindFreeSlot()
    {
        for (int i = 0; i < slots.Length; i++)
            if (!slotOccupants.ContainsKey(i)) return i;

        return -1;
    }
}