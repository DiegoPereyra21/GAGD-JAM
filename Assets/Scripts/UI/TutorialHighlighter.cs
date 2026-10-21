using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

// Hace titilar los outlines que señala el tutorial.
// En cada refresco se agregan los que tienen que titilar y Apply() apaga los que ya no hacen falta.
public class TutorialHighlighter
{
    private const BindingFlags FieldFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;

    private readonly HashSet<InteractableOutline> active = new HashSet<InteractableOutline>();
    private readonly HashSet<InteractableOutline> pending = new HashSet<InteractableOutline>();

    public void Add(InteractableOutline outline)
    {
        if (outline != null) pending.Add(outline);
    }

    public void Add(Component owner) => Add(OutlineOf(owner));

    public void AddObject(GameObject target) => Add(OutlineOn(target));

    public void Apply()
    {
        foreach (InteractableOutline outline in active)
        {
            if (outline != null && !pending.Contains(outline))
                outline.SetPulsing(false);
        }

        foreach (InteractableOutline outline in pending)
        {
            if (outline != null)
                outline.SetPulsing(true);
        }

        active.Clear();
        active.UnionWith(pending);
        pending.Clear();
    }

    public void Clear()
    {
        pending.Clear();
        Apply();
    }

    // El outline que usa un componente (su campo "outline"); si no tiene, el del mismo objeto o sus hijos.
    // Se lee por reflexión para no tener que agregar referencias a mano en cada script.
    public static InteractableOutline OutlineOf(Component owner)
    {
        if (owner == null) return null;

        InteractableOutline fromField = GetField<InteractableOutline>(owner, "outline");
        return fromField != null ? fromField : OutlineOn(owner.gameObject);
    }

    public static InteractableOutline OutlineOn(GameObject target)
    {
        if (target == null) return null;

        InteractableOutline outline = target.GetComponent<InteractableOutline>();
        return outline != null ? outline : target.GetComponentInChildren<InteractableOutline>(true);
    }

    // Lee un campo privado serializado (ej. "clickTarget" del Cauldron, "station" de WorkTable).
    public static T GetField<T>(object owner, string fieldName) where T : class
    {
        if (owner == null) return null;

        FieldInfo field = owner.GetType().GetField(fieldName, FieldFlags);
        return field != null ? field.GetValue(owner) as T : null;
    }
}
