using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Game.Collectibles;

[RequireComponent(typeof(UIDocument))]
public class QuestJournalUI : MonoBehaviour
{
    [SerializeField] private QuestManager questManager;
    [SerializeField] private PlayerInventory inventory;
    [SerializeField] private VisualTreeAsset questCardTemplate;
    [SerializeField] private VisualTreeAsset questRowTemplate;
    [SerializeField] private IngredientDatabase ingredientDatabase;
    private ScrollView scrollView;

    private void Awake()
    {
        VisualElement root = GetComponent<UIDocument>().rootVisualElement;
        scrollView = root.Q<ScrollView>("QuestScrollView");
    }

    private void OnEnable()
    {
        inventory.OnInventoryChanged += Refresh;
        questManager.OnQuestsChanged += Refresh;
        HomeStorage.Instance.OnStorageChanged += Refresh;
        GameProgressManager.Instance.OnDayStarted += Refresh;
        GameProgressManager.Instance.OnNightStarted += Refresh;
        Refresh();
    }

    private void OnDisable()
    {
        inventory.OnInventoryChanged -= Refresh;
        questManager.OnQuestsChanged -= Refresh;
        HomeStorage.Instance.OnStorageChanged -= Refresh;
        GameProgressManager.Instance.OnDayStarted -= Refresh;
        GameProgressManager.Instance.OnNightStarted -= Refresh;
    }

    private void Refresh()
    {
        scrollView.Clear();

        bool isOutside = GameProgressManager.Instance.IsOutside;

        var remaining = new Dictionary<IngredientType, int>();
        foreach (IngredientType type in ingredientDatabase.AllIngredients)
            remaining[type] = inventory.GetCount(type) + HomeStorage.Instance.Totals.GetValueOrDefault(type);

        foreach (QuestData quest in questManager.ActiveQuests)
            scrollView.Add(isOutside ? BuildGatherCard(quest, remaining) : BuildRecipeCard(quest));
    }

    // Afuera: qué ir a recolectar. Convierte cualquier ingrediente procesado a su versión cruda.
    private VisualElement BuildGatherCard(QuestData quest, Dictionary<IngredientType, int> remaining)
    {
        VisualElement card = questCardTemplate.Instantiate();

        card.Q<Label>("VillagerName").text = quest.villagerName;
        card.Q<Label>("MissionName").text = quest.missionName;

        VisualElement rowsContainer = card.Q<VisualElement>("RowsContainer");
        foreach (QuestObjective objective in quest.objectives)
            rowsContainer.Add(BuildGatherRow(objective, remaining));

        return card;
    }

    private VisualElement BuildGatherRow(QuestObjective objective, Dictionary<IngredientType, int> remaining)
    {
        VisualElement row = questRowTemplate.Instantiate();

        IngredientType rawType = objective.type.rawSource != null ? objective.type.rawSource : objective.type;

        int available = remaining[rawType];
        int allocated = Mathf.Min(available, objective.targetAmount);
        remaining[rawType] -= allocated;

        row.Q<Label>("IngredientLabel").text = rawType.displayName;
        row.Q<Label>("ProgressLabel").text = $"{allocated}/{objective.targetAmount}";

        bool isComplete = allocated >= objective.targetAmount;
        row.Q<Label>("ProgressLabel").style.color = isComplete ? Color.green : Color.red;

        return row;
    }

    // Adentro: la receta real de la poción, con los ingredientes procesados tal como se usan en el caldero.
    private VisualElement BuildRecipeCard(QuestData quest)
    {
        VisualElement card = questCardTemplate.Instantiate();

        card.Q<Label>("VillagerName").text = quest.villagerName;
        card.Q<Label>("MissionName").text = quest.missionName;

        VisualElement rowsContainer = card.Q<VisualElement>("RowsContainer");

        if (quest.requiredPotion != null)
        {
            foreach (RecipeIngredient ingredient in quest.requiredPotion.ingredients)
                rowsContainer.Add(BuildRecipeRow(ingredient));
        }

        return card;
    }

    private VisualElement BuildRecipeRow(RecipeIngredient ingredient)
    {
        VisualElement row = questRowTemplate.Instantiate();

        row.Q<Label>("IngredientLabel").text = ingredient.type.displayName;
        row.Q<Label>("ProgressLabel").text = $"x{ingredient.amount}";
        row.Q<Label>("ProgressLabel").style.color = Color.white;

        return row;
    }
}