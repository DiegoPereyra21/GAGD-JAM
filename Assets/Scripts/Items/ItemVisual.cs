using UnityEngine;

// Marca un modelo instanciado como representación de un ítem (estante, caldero, canasto).
public class ItemVisual : MonoBehaviour
{
    public ItemKey Key { get; private set; }
    public Component Owner { get; private set; }

    public void Init(ItemKey key, Component owner)
    {
        Key = key;
        Owner = owner;
    }
}