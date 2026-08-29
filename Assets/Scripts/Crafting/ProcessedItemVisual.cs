using UnityEngine;
using Game.Collectibles;

public class ProcessedItemVisual : MonoBehaviour
{
    public IngredientType Type { get; private set; }
    public void Init(IngredientType type) => Type = type;
}