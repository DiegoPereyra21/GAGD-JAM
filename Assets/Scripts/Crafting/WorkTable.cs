using System.Collections.Generic;
using UnityEngine;

// Una mesa (mortero o tabla): estante de crudos, área de procesados y estación para procesar.
public class WorkTable : MonoBehaviour
{
    [SerializeField] private TableType table;
    [SerializeField] private ItemShelfView rawShelf;
    [SerializeField] private ItemShelfView processedShelf;
    [Tooltip("Objeto donde se suelta un ítem crudo para procesarlo (mortero / tabla).")]
    [SerializeField] private GameObject station;
    [SerializeField] private AK.Wwise.Event processSound;

    public TableType Table => table;

    private void Start()
    {
        List<ItemKey> rawKeys = new List<ItemKey>();
        List<ItemKey> processedKeys = new List<ItemKey>();

        foreach (ItemData item in GameDatabase.Instance.Items)
        {
            if (item == null || item.Table != table) continue;

            rawKeys.Add(ItemKey.Raw(item));
            if (item.Processable)
                processedKeys.Add(ItemKey.Processed(item));
        }

        ItemContainer pantry = HomeStorage.Instance.Pantry;
        rawShelf.Bind(pantry, rawKeys);

        if (processedShelf != null)
            processedShelf.Bind(pantry, processedKeys);
    }

    public bool IsStation(GameObject target)
    {
        if (station == null || target == null) return false;
        return target == station || target.transform.IsChildOf(station.transform);
    }

    public bool TryProcess(ItemKey key)
    {
        if (!key.IsValid || key.State != ItemState.Raw) return false;

        if (key.Item.Table != table)
        {
            Say("Esto no va en esta mesa.");
            return false;
        }

        if (!key.Item.Processable)
        {
            Say(table == TableType.Mortar ? "Esto no se puede moler." : "Esto no se puede cortar.");
            return false;
        }

        if (!HomeStorage.Instance.TryProcess(key.Item)) return false;

        HomeStorage.Instance.Save();

        if (processSound != null && processSound.IsValid())
            processSound.Post(gameObject);

        return true;
    }

    private static void Say(string message) => DialogueUI.Instance.ShowMessage("Ofelia", message);
}