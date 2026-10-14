using UnityEngine;

// El "Display Name" es el nombre de la misión. Lo que hay que recolectar sale de la receta de la poción.
[CreateAssetMenu(fileName = "NewQuest", menuName = "Game Data/Quest")]
public class QuestData : GameDataAsset
{
    [Header("Pedido")]
    [SerializeField] private string villagerName;
    [SerializeField] private PotionData potion;
    [SerializeField, Min(0)] private int moneyReward;
    [SerializeField, Min(1)] private int availableDay = 1;

    public string VillagerName => villagerName;
    public string MissionName => DisplayName;
    public PotionData Potion => potion;
    public int MoneyReward => moneyReward;
    public int AvailableDay => availableDay;
}