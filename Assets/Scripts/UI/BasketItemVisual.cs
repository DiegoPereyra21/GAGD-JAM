using UnityEngine;
using Game.Collectibles;

public class BasketItemVisual : MonoBehaviour
{
    public CollectibleType Type { get; private set; }

    public void Init(CollectibleType type)
    {
        Type = type;
    }
}