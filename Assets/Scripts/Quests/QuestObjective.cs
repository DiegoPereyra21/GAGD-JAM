using System;
using Game.Collectibles;

[Serializable]
public class QuestObjective
{
    public CollectibleType type;
    public int targetAmount = 1;
}