using UnityEngine;
using Game.Collectibles;

public class CauldronItemVisual : MonoBehaviour
{
    public IngredientType Type { get; private set; }
    public void Init(IngredientType type) => Type = type;
}