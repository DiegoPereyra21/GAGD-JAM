using System;

// Identifica un ítem en un estado concreto. Es la clave de todos los contenedores y recetas.
public readonly struct ItemKey : IEquatable<ItemKey>
{
    public readonly ItemData Item;
    public readonly ItemState State;

    public ItemKey(ItemData item, ItemState state)
    {
        Item = item;
        State = state;
    }

    public static ItemKey Raw(ItemData item) => new ItemKey(item, ItemState.Raw);
    public static ItemKey Processed(ItemData item) => new ItemKey(item, ItemState.Processed);

    public bool IsValid => Item != null && Item.HasState(State);

    public string DisplayName
    {
        get
        {
            if (Item == null) return "(sin ítem)";
            return Item.GetName(State) ?? Item.DisplayName;
        }
    }

    public bool Equals(ItemKey other) => ReferenceEquals(Item, other.Item) && State == other.State;
    public override bool Equals(object obj) => obj is ItemKey other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(Item is null ? 0 : Item.GetHashCode(), (int)State);

    public static bool operator ==(ItemKey a, ItemKey b) => a.Equals(b);
    public static bool operator !=(ItemKey a, ItemKey b) => !a.Equals(b);

    public override string ToString() => $"{(Item != null ? Item.Id : "null")}:{State}";
}