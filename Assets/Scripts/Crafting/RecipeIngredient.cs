using System;
using Game.Collectibles;

[Serializable]
public class RecipeIngredient
{
    public CollectibleType type;
    public int amount = 1;
}