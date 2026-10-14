using System;
using UnityEngine;

// Canasto del player. Solo guarda ítems crudos.
public class PlayerInventory : MonoBehaviour
{
    [Tooltip("Capacidad del canasto.")]
    [SerializeField, Min(1)] private int maxTotalItems = 10;

    public event Action OnInventoryChanged;

    private ItemContainer container;

    // Lazy: otro componente puede pedirlo en su OnEnable antes del Awake de este.
    public ItemContainer Container
    {
        get
        {
            if (container == null)
            {
                container = new ItemContainer(maxTotalItems);
                container.OnChanged += () => OnInventoryChanged?.Invoke();
            }
            return container;
        }
    }

    public bool IsFull => Container.IsFull;
    public int TotalCount => Container.TotalCount;

    public bool TryAdd(ItemData item, int amount = 1) => Container.TryAdd(ItemKey.Raw(item), amount);
    public bool TryRemove(ItemData item, int amount = 1) => Container.TryRemove(ItemKey.Raw(item), amount);
    public int GetCount(ItemData item) => Container.Count(ItemKey.Raw(item));
    public void Clear() => Container.Clear();
}