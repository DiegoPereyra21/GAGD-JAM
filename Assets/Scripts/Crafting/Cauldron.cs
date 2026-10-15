using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Muestra lo que hay en HomeStorage.Cauldron, agrega/saca ítems y craftea.
public class Cauldron : MonoBehaviour
{
    [SerializeField] private CraftingRoom room;
    [Tooltip("Objeto que se clickea para craftear (la cuchara).")]
    [SerializeField] private GameObject clickTarget;
    [Tooltip("Objeto donde se pueden soltar ítems arrastrados (el caldero, incluye sus hijos).")]
    [SerializeField] private GameObject dropTarget;
    [SerializeField] private Transform dropPoint;
    [SerializeField] private float dropSpread = 0.2f;
    [SerializeField] private float dropHeight = 0.3f;

    [Header("Showcase de la poción")]
    [SerializeField] private Transform showcaseAnchor;
    [SerializeField] private float showcaseDuration = 3f;
    [SerializeField] private float swayAngle = 12f;
    [SerializeField] private float swaySpeed = 3f;
    [SerializeField] private float shrinkDuration = 0.4f;

    public event Action<PotionData> OnPotionCrafted;

    private readonly Dictionary<ItemKey, List<GameObject>> visuals = new Dictionary<ItemKey, List<GameObject>>();
    private ItemContainer contents;

    private void Start()
    {
        contents = HomeStorage.Instance.Cauldron;
        contents.OnChanged += Sync;
        Sync();
    }

    private void OnDestroy()
    {
        if (contents != null)
            contents.OnChanged -= Sync;
    }

    public bool IsClickTarget(GameObject target)
    {
        if (clickTarget == null || target == null) return false;
        return target == clickTarget || target.transform.IsChildOf(clickTarget.transform);
    }

    public bool IsDropTarget(GameObject target)
    {
        if (target == null) return false;
        if (IsClickTarget(target)) return true;
        if (dropTarget != null && (target == dropTarget || target.transform.IsChildOf(dropTarget.transform))) return true;
        return target.TryGetComponent(out ItemVisual visual) && visual.Owner == this;
    }

    public bool TryAddFromPantry(ItemKey key)
    {
        if (room.IsBusy) return false;
        if (!HomeStorage.Instance.TryMoveToCauldron(key)) return false;

        HomeStorage.Instance.Save();
        return true;
    }

    public bool TryReturnToPantry(ItemVisual visual)
    {
        if (room.IsBusy || visual == null) return false;

        ItemKey key = visual.Key;
        if (visuals.TryGetValue(key, out List<GameObject> list))
            list.Remove(visual.gameObject);
        Destroy(visual.gameObject);

        if (!HomeStorage.Instance.TryReturnFromCauldron(key))
        {
            Sync();
            return false;
        }

        HomeStorage.Instance.Save();
        return true;
    }

    public void TryCraft()
    {
        if (room.IsBusy) return;

        HomeStorage storage = HomeStorage.Instance;

        if (storage.Cauldron.IsEmpty)
        {
            Say("El caldero está vacío.");
            return;
        }

        PotionData match = FindMatchingPotion();

        if (match == null)
        {
            storage.ReturnCauldronToPantry();
            storage.Save();
            Say("Ninguna receta coincide con los ingredientes en el caldero.");
            return;
        }

        storage.CompleteCraft(match);
        storage.Save();
        StartCoroutine(ShowcaseRoutine(match));
    }

    private PotionData FindMatchingPotion()
    {
        foreach (PotionData potion in GameDatabase.Instance.Potions)
        {
            if (potion != null && potion.Matches(contents.Contents))
                return potion;
        }

        return null;
    }

    // Ajusta los modelos dentro del caldero a lo que dice el contenedor.
    private void Sync()
    {
        foreach (ItemKey key in new List<ItemKey>(visuals.Keys))
        {
            List<GameObject> list = visuals[key];
            list.RemoveAll(v => v == null);

            int target = contents.Count(key);
            while (list.Count > target)
            {
                int last = list.Count - 1;
                Destroy(list[last]);
                list.RemoveAt(last);
            }

            if (list.Count == 0)
                visuals.Remove(key);
        }

        foreach (KeyValuePair<ItemKey, int> pair in contents.Contents)
        {
            if (!visuals.TryGetValue(pair.Key, out List<GameObject> list))
            {
                list = new List<GameObject>();
                visuals[pair.Key] = list;
            }

            while (list.Count < pair.Value)
            {
                GameObject visual = SpawnVisual(pair.Key);
                if (visual == null) break;
                list.Add(visual);
            }
        }
    }

    private GameObject SpawnVisual(ItemKey key)
    {
        GameObject model = key.Item.GetModel(key.State);
        if (model == null)
        {
            Debug.LogWarning($"[Cauldron] '{key}' no tiene modelo asignado.", this);
            return null;
        }

        Vector3 offset = new Vector3(UnityEngine.Random.Range(-dropSpread, dropSpread), dropHeight, UnityEngine.Random.Range(-dropSpread, dropSpread));
        GameObject visual = Instantiate(model, dropPoint.position + offset, UnityEngine.Random.rotation);
        visual.AddComponent<ItemVisual>().Init(key, this);
        return visual;
    }

    private IEnumerator ShowcaseRoutine(PotionData potion)
    {
        room.SetBusy(true);
        Say($"Creaste: {potion.DisplayName}");

        GameObject showcase = null;
        Quaternion baseRotation = showcaseAnchor != null ? showcaseAnchor.rotation : Quaternion.identity;

        if (potion.Model != null && showcaseAnchor != null)
        {
            showcase = Instantiate(potion.Model, showcaseAnchor.position, baseRotation);
            if (showcase.TryGetComponent(out Rigidbody rb))
                rb.isKinematic = true;
        }

        float elapsed = 0f;
        while (elapsed < showcaseDuration)
        {
            elapsed += Time.deltaTime;
            if (showcase != null)
                showcase.transform.rotation = baseRotation * Quaternion.Euler(0f, 0f, Mathf.Sin(elapsed * swaySpeed) * swayAngle);
            yield return null;
        }

        if (showcase != null)
        {
            Vector3 startScale = showcase.transform.localScale;
            float shrink = 0f;

            while (shrink < shrinkDuration)
            {
                shrink += Time.deltaTime;
                showcase.transform.localScale = Vector3.Lerp(startScale, Vector3.zero, shrink / shrinkDuration);
                yield return null;
            }

            Destroy(showcase);
        }

        room.SetBusy(false);
        OnPotionCrafted?.Invoke(potion);
    }

    private static void Say(string message) => DialogueUI.Instance.ShowMessage("Ofelia", message);
}