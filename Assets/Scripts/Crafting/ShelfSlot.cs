using UnityEngine;
using Game.Collectibles;

public class ShelfSlot : MonoBehaviour
{
    [SerializeField] private CollectibleType type;
    public CollectibleType Type => type;
}