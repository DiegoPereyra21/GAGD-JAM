using System.Collections.Generic;
using UnityEngine;

// Caja de pociones hechas. Se redibuja sola a partir de HomeStorage.CraftedPotions.
public class PotionBoxDisplay : MonoBehaviour
{
    [SerializeField] private Transform[] slots;
    [Tooltip("Renderer de la caja. Las pociones nuevas copian si está visible o no (por ejemplo, cuando el player está oculto en el crafteo).")]
    [SerializeField] private Renderer visibilityReference;

    private readonly List<GameObject> visuals = new List<GameObject>();
    private bool subscribed;

    private void Start()
    {
        HomeStorage.Instance.OnPotionsChanged += Rebuild;
        subscribed = true;
        Rebuild();
    }

    private void OnDestroy()
    {
        if (subscribed && HomeStorage.Instance != null)
            HomeStorage.Instance.OnPotionsChanged -= Rebuild;
    }

    private void Rebuild()
    {
        foreach (GameObject visual in visuals)
        {
            if (visual != null)
                Destroy(visual);
        }
        visuals.Clear();

        IReadOnlyList<PotionData> potions = HomeStorage.Instance.CraftedPotions;

        if (potions.Count > slots.Length)
            Debug.LogWarning($"[PotionBoxDisplay] Hay {potions.Count} pociones pero solo {slots.Length} slots en la caja.", this);

        bool visible = visibilityReference == null || visibilityReference.enabled;
        int count = Mathf.Min(potions.Count, slots.Length);

        for (int i = 0; i < count; i++)
        {
            PotionData potion = potions[i];
            if (potion == null || potion.Model == null) continue;

            GameObject visual = Instantiate(potion.Model, slots[i].position, slots[i].rotation, slots[i]);

            if (visual.TryGetComponent(out Rigidbody rb))
                rb.isKinematic = true;

            foreach (Renderer r in visual.GetComponentsInChildren<Renderer>(true))
                r.enabled = visible;

            visuals.Add(visual);
        }
    }
}
