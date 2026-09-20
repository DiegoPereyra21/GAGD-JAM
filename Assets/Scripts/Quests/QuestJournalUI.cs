using Game.Collectibles;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.InputSystem;

[RequireComponent(typeof(UIDocument))]
public class QuestJournalUI : MonoBehaviour
{
    [SerializeField] private QuestManager questManager;
    [SerializeField] private PlayerInventory inventory;
    [SerializeField] private VisualTreeAsset questCardTemplate;
    [SerializeField] private VisualTreeAsset questRowTemplate;
    [SerializeField] private IngredientDatabase ingredientDatabase;
    [SerializeField] private PotionRecipe sleepPotionRecipe;

    private static readonly Color CraftedColor = new Color(1f, 0.5f, 0f, 0.25f);   //naranja, poca transparencia
    private static readonly Color DeliveredColor = new Color(0f, 0.8f, 0.2f, 0.25f); //verde, poca transparencia


    private VisualElement journalRoot;
    private Label titleLabel;
    private bool isCollapsed;
    private ScrollView scrollView;

    private void Awake()
    {
        VisualElement root = GetComponent<UIDocument>().rootVisualElement;
        scrollView = root.Q<ScrollView>("QuestScrollView");

        journalRoot = root.Q<VisualElement>("QuestJournalRoot");
        titleLabel = root.Q<Label>("JournalTitle");
        journalRoot.style.transformOrigin = new TransformOrigin(Length.Percent(100), Length.Percent(0));
        journalRoot.style.width = 380;
        journalRoot.style.scale = new Scale(new Vector3(1.15f, 1.15f, 1f));
    }

    private void Update()
    {
        if (Keyboard.current.qKey.wasPressedThisFrame)
        {
            isCollapsed = !isCollapsed;
            scrollView.style.display = isCollapsed ? DisplayStyle.None : DisplayStyle.Flex;
        }
    }

    private void OnFirstLayout(GeometryChangedEvent evt)
    {
        VisualElement root = GetComponent<UIDocument>().rootVisualElement;
        root.UnregisterCallback<GeometryChangedEvent>(OnFirstLayout);

        root.style.width = root.resolvedStyle.width + 60;
        root.style.scale = new Scale(new Vector3(1.15f, 1.15f, 1f));
    }
    private void OnEnable()
    {
        inventory.OnInventoryChanged += Refresh;
        questManager.OnQuestsChanged += Refresh;
        HomeStorage.Instance.OnStorageChanged += Refresh;
        GameProgressManager.Instance.OnDayStarted += Refresh;
        GameProgressManager.Instance.OnNightStarted += Refresh;
        GameProgressManager.Instance.OnWentOutside += Refresh;
        Refresh();
    }

    private void OnDisable()
    {
        inventory.OnInventoryChanged -= Refresh;
        questManager.OnQuestsChanged -= Refresh;
        HomeStorage.Instance.OnStorageChanged -= Refresh;
        GameProgressManager.Instance.OnDayStarted -= Refresh;
        GameProgressManager.Instance.OnNightStarted -= Refresh;
        GameProgressManager.Instance.OnWentOutside -= Refresh;
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

        if (GameProgressManager.Instance.SleepIngredientObtained && sleepPotionRecipe != null)
            scrollView.Add(BuildSleepPotionCard());

        int totalCount = questManager.ActiveQuests.Count +
        (GameProgressManager.Instance.SleepIngredientObtained && sleepPotionRecipe != null ? 1 : 0);

        titleLabel.text = totalCount > 0 ? $"Diario de objetivos ({totalCount})" : "Diario de objetivos";
    }

    private VisualElement BuildSleepPotionCard()
    {
        VisualElement card = questCardTemplate.Instantiate();

        card.Q<Label>("VillagerName").text = "Para mí";
        card.Q<Label>("MissionName").text = sleepPotionRecipe.potionName;

        VisualElement rowsContainer = card.Q<VisualElement>("RowsContainer");
        foreach (RecipeIngredient ingredient in sleepPotionRecipe.ingredients)
            rowsContainer.Add(BuildRecipeRow(ingredient));

        return card;
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

        ApplyQuestStatus(card, quest);
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

        ApplyQuestStatus(card, quest);
        return card;
    }

    private VisualElement BuildRecipeRow(RecipeIngredient ingredient)
    {
        VisualElement row = questRowTemplate.Instantiate();

        Label ingredientLabel = row.Q<Label>("IngredientLabel");
        ingredientLabel.text = $"{ingredient.amount} x {ingredient.type.displayName}";
        ingredientLabel.style.letterSpacing = 1;
        ingredientLabel.style.flexGrow = 1;
        ingredientLabel.style.whiteSpace = WhiteSpace.NoWrap;

        row.Q<Label>("ProgressLabel").style.display = DisplayStyle.None;

        return row;
    }

    private void ApplyQuestStatus(VisualElement card, QuestData quest)
    {
        bool delivered = questManager.PendingDeliveries.Contains(quest);
        bool crafted = !delivered && quest.requiredPotion != null
            && HomeStorage.Instance.CraftedPotions.Contains(quest.requiredPotion);

        if (!delivered && !crafted) return;

        card.style.backgroundColor = delivered ? DeliveredColor : CraftedColor;

        Label statusLabel = new Label(delivered ? "Entregada" : "Crafteada");
        statusLabel.style.position = Position.Absolute;
        statusLabel.style.top = 4;
        statusLabel.style.right = 4;
        statusLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
        statusLabel.style.color = Color.white;
        card.Add(statusLabel);
    }
}