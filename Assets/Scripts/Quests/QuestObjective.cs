using System;
using Game.Collectibles;

[Serializable]
public class QuestObjective
{
    public IngredientType type;
    public int targetAmount = 1;
}