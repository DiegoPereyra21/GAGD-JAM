// QuestJournalUI.cs
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Game.Collectibles;
//Controla el scrollview, donde une los distintas "plantillas" en la ui
[RequireComponent(typeof(UIDocument))]
public class QuestJournalUI : MonoBehaviour
{
    [SerializeField] private QuestManager questManager;//para saber el listado de misiones y asignar en cada card y row lo necesario
    [SerializeField] private PlayerInventory inventory;
    [SerializeField] private VisualTreeAsset questCardTemplate;
    [SerializeField] private VisualTreeAsset questRowTemplate;

    private ScrollView scrollView;

    private void Awake()
    {
        VisualElement root = GetComponent<UIDocument>().rootVisualElement;
        scrollView = root.Q<ScrollView>("QuestScrollView");
    }
    //fix para q tome tambien los objetos del homestorage para el seguimiento de las quests
    private void OnEnable()
    {
        inventory.OnInventoryChanged += Refresh;
        questManager.OnQuestsChanged += Refresh;
        HomeStorage.Instance.OnStorageChanged += Refresh;
        Refresh();
    }

    private void OnDisable()
    {
        inventory.OnInventoryChanged -= Refresh;
        questManager.OnQuestsChanged -= Refresh;
        HomeStorage.Instance.OnStorageChanged -= Refresh;
    }

    private void Refresh()
    {
        scrollView.Clear();

        var remaining = new Dictionary<CollectibleType, int>();
        foreach (CollectibleType type in System.Enum.GetValues(typeof(CollectibleType)))
            remaining[type] = inventory.GetCount(type) + HomeStorage.Instance.Totals.GetValueOrDefault(type);

        foreach (QuestData quest in questManager.ActiveQuests)
            scrollView.Add(BuildCard(quest, remaining));
    }
    //arma las cartas segun hagan falta
    private VisualElement BuildCard(QuestData quest, Dictionary<CollectibleType, int> remaining)
    {
        VisualElement card = questCardTemplate.Instantiate();

        card.Q<Label>("VillagerName").text = quest.villagerName;
        card.Q<Label>("MissionName").text = quest.missionName;

        VisualElement rowsContainer = card.Q<VisualElement>("RowsContainer");
        foreach (QuestObjective objective in quest.objectives)
            rowsContainer.Add(BuildRow(objective, remaining));

        return card;
    }
    //arma los objetvios en liena tambine
    private VisualElement BuildRow(QuestObjective objective, Dictionary<CollectibleType, int> remaining)
    {
        VisualElement row = questRowTemplate.Instantiate();

        int available = remaining[objective.type];
        int allocated = Mathf.Min(available, objective.targetAmount);
        remaining[objective.type] -= allocated;

        row.Q<Label>("IngredientLabel").text = objective.type.ToString();
        row.Q<Label>("ProgressLabel").text = $"{allocated}/{objective.targetAmount}";

        //para q cambie de color a verde en caso de compelto
        bool isComplete = allocated >= objective.targetAmount;
        row.Q<Label>("ProgressLabel").style.color = isComplete ? Color.green : Color.red;

        return row;
    }
}