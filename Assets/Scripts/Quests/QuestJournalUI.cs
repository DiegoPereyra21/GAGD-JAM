using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.InputSystem;
using System.Linq;

[RequireComponent(typeof(UIDocument))]
public class QuestJournalUI : MonoBehaviour
{
    [SerializeField] private QuestManager questManager;
    [SerializeField] private PlayerInventory inventory;
    [SerializeField] private VisualTreeAsset questCardTemplate;
    [SerializeField] private VisualTreeAsset questRowTemplate;

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
        PotionData sleepPotion = GameDatabase.Instance.SleepPotion;
        bool showSleepPotion = GameProgressManager.Instance.SleepIngredientObtained && sleepPotion != null;

        List<QuestData> uncovered = QuestRequirements.GetUncoveredQuests(questManager);
        Dictionary<ItemData, int> remaining = new Dictionary<ItemData, int>();

        foreach (QuestData quest in questManager.ActiveQuests)
        {
            if (quest == null) continue;

            bool covered = !uncovered.Contains(quest);

            scrollView.Add(isOutside
                ? BuildGatherCard(quest, covered, remaining)
                : BuildRecipeCard(quest, covered));
        }

        if (showSleepPotion)
            scrollView.Add(BuildSleepPotionCard(sleepPotion));

        int totalCount = questManager.ActiveQuests.Count + (showSleepPotion ? 1 : 0);
        titleLabel.text = totalCount > 0 ? $"Diario de objetivos ({totalCount})" : "Diario de objetivos";
    }

    private VisualElement BuildSleepPotionCard(PotionData sleepPotion)
    {
        VisualElement card = questCardTemplate.Instantiate();

        card.Q<Label>("VillagerName").text = "Para mí";
        card.Q<Label>("MissionName").text = sleepPotion.DisplayName;

        VisualElement rowsContainer = card.Q<VisualElement>("RowsContainer");
        foreach (PotionIngredient ingredient in sleepPotion.Ingredients)
        {
            if (ingredient != null && ingredient.Item != null)
                rowsContainer.Add(BuildRecipeRow(ingredient));
        }

        return card;
    }

    // Afuera: qué ir a recolectar, por ítem (sin importar si después se procesa).
    // Las quests ya cubiertas (poción hecha o entregada) se muestran completas y no consumen lo recolectado.
    private VisualElement BuildGatherCard(QuestData quest, bool covered, Dictionary<ItemData, int> remaining)
    {
        VisualElement card = questCardTemplate.Instantiate();

        card.Q<Label>("VillagerName").text = quest.VillagerName;
        card.Q<Label>("MissionName").text = quest.MissionName;

        VisualElement rowsContainer = card.Q<VisualElement>("RowsContainer");
        foreach (KeyValuePair<ItemData, int> pair in QuestRequirements.GetItemTotals(quest.Potion))
            rowsContainer.Add(BuildGatherRow(pair.Key, pair.Value, covered, remaining));

        ApplyQuestStatus(card, quest, covered);
        return card;
    }

    private VisualElement BuildGatherRow(ItemData item, int needed, bool covered, Dictionary<ItemData, int> remaining)
    {
        VisualElement row = questRowTemplate.Instantiate();

        int allocated = needed;

        if (!covered)
        {
            if (!remaining.ContainsKey(item))
                remaining[item] = QuestRequirements.GetOwned(item, inventory);

            allocated = Mathf.Min(remaining[item], needed);
            remaining[item] -= allocated;
        }

        row.Q<Label>("IngredientLabel").text = item.DisplayName;
        row.Q<Label>("ProgressLabel").text = $"{allocated}/{needed}";
        row.Q<Label>("ProgressLabel").style.color = allocated >= needed ? Color.green : Color.red;

        return row;
    }

    // Adentro: la receta real de la poción, con el estado de cada ingrediente tal como va al caldero.
    private VisualElement BuildRecipeCard(QuestData quest, bool covered)
    {
        VisualElement card = questCardTemplate.Instantiate();

        card.Q<Label>("VillagerName").text = quest.VillagerName;
        card.Q<Label>("MissionName").text = quest.MissionName;

        VisualElement rowsContainer = card.Q<VisualElement>("RowsContainer");

        if (quest.Potion != null)
        {
            foreach (PotionIngredient ingredient in quest.Potion.Ingredients)
            {
                if (ingredient != null && ingredient.Item != null)
                    rowsContainer.Add(BuildRecipeRow(ingredient));
            }
        }

        ApplyQuestStatus(card, quest, covered);
        return card;
    }

    private VisualElement BuildRecipeRow(PotionIngredient ingredient)
    {
        VisualElement row = questRowTemplate.Instantiate();

        Label ingredientLabel = row.Q<Label>("IngredientLabel");
        ingredientLabel.text = $"{ingredient.Amount} x {ingredient.Key.DisplayName}";
        ingredientLabel.style.letterSpacing = 1;
        ingredientLabel.style.flexGrow = 1;
        ingredientLabel.style.whiteSpace = WhiteSpace.NoWrap;

        row.Q<Label>("ProgressLabel").style.display = DisplayStyle.None;

        return row;
    }

    // covered = la quest ya no necesita ingredientes (poción hecha o entregada).
    private void ApplyQuestStatus(VisualElement card, QuestData quest, bool covered)
    {
        bool delivered = questManager.PendingDeliveries.Contains(quest);
        bool crafted = !delivered && covered;

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
