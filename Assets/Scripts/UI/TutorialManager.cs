using System.Collections.Generic;
using System.Linq;
using Game.Collectibles;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TutorialManager : MonoBehaviour
{
    private const string Title = "Tutorial";

    [SerializeField] private StairInteraction downstairs;
    [SerializeField] private DoorExitInteraction doorExit;
    [SerializeField] private QuestManager questManager;
    [SerializeField] private CraftingRoom craftingRoom;
    [SerializeField] private Cauldron cauldron;
    [SerializeField] private DeliveryInteraction deliveryInteraction;
    [SerializeField] private PlayerCollector playerCollector;
    [SerializeField] private DoorInteraction doorInteraction;
    [SerializeField] private PlayerInventory inventory;
    [SerializeField] private StairInteraction upstairs;
    [SerializeField] private int questsNeededForNextStep = 3;
    [SerializeField] private string mainMenuSceneName = "MainMenu";
    [SerializeField] private BasketDisplay basketDisplay;

    [Header("Guía visual")]
    [Tooltip("Radio alrededor del player en el que titilan los ingredientes que faltan juntar.")]
    [SerializeField] private float collectibleHighlightRadius = 40f;
    [Tooltip("Cada cuánto se recalcula qué titila y qué indicación mostrar.")]
    [SerializeField] private float guideRefreshInterval = 0.2f;

    private bool inventoryOpenedOnce;
    private bool inventoryClosedMessageShown;
    private bool collectedMessageShown;

    private enum Step
    {
        Movement, GoOutside, CheckMailbox, CollectItems,
        CraftingIntro, CraftPotion, Deliver, WaitingToSleep, Done
    }

    private Step currentStep = Step.Movement;
    private bool nightPhaseDone;

    // Guía visual (se busca todo en la escena, no hace falta asignar nada)
    private readonly TutorialHighlighter highlighter = new TutorialHighlighter();
    private Mailbox mailbox;
    private BedInteraction bed;
    private WorkTable[] workTables;
    private Collectible[] collectibles;
    private float guideTimer;
    private string lastCraftingHint;

    private void Start()
    {
        mailbox = FindFirstObjectByType<Mailbox>();
        bed = FindFirstObjectByType<BedInteraction>();
        workTables = FindObjectsByType<WorkTable>(FindObjectsSortMode.None);

        Show("Usá WASD para moverte y E para interactuar con las cosas que brillan. Empezá bajando las escaleras.");
    }

    private void Update()
    {
        if (currentStep == Step.CollectItems && doorInteraction != null)
        {
            bool hasAll = HasAllQuestMaterials();
            doorInteraction.EntryBlocked = !hasAll;
            doorInteraction.BlockedMessage = "Todavía me falta recolectar ingredientes para mis pedidos.";

            if (hasAll && !collectedMessageShown)
            {
                collectedMessageShown = true;
                Show("¡Ya tenés todo lo necesario! Volvé a la casa por la puerta para preparar las pociones.");
            }
        }

        if (upstairs != null)
        {
            bool inCraftingPhase = currentStep == Step.CraftingIntro
                || currentStep == Step.CraftPotion
                || currentStep == Step.Deliver;

            if (inCraftingPhase && !AllQuestsDelivered())
            {
                upstairs.EntryBlocked = true;
                upstairs.BlockedMessage = HomeStorage.Instance.CraftedPotions.Count > 0
                    ? "Entrega las pociones antes de acostarte."
                    : "Crea las pociones antes de acostarte.";
            }
            else
            {
                upstairs.EntryBlocked = false;
            }
        }

        guideTimer -= Time.unscaledDeltaTime;
        if (guideTimer <= 0f)
        {
            guideTimer = guideRefreshInterval;
            RefreshGuide();
        }
    }

    private bool HasAllQuestMaterials() => QuestRequirements.HasAllMaterials(questManager, inventory);

    private bool AllQuestsDelivered()
    {
        foreach (QuestData quest in questManager.ActiveQuests)
        {
            if (!questManager.PendingDeliveries.Contains(quest))
                return false;
        }

        return true;
    }

    private void OnEnable()
    {
        if (downstairs != null) downstairs.OnUsed += HandleWentDownstairs;
        if (doorExit != null) doorExit.OnUsed += HandleWentOutside;
        if (questManager != null) questManager.OnQuestsChanged += HandleQuestsChanged;
        if (basketDisplay != null) basketDisplay.OnOpened += HandleInventoryOpened;
        if (basketDisplay != null) basketDisplay.OnClosed += HandleInventoryClosed;

        if (craftingRoom != null) craftingRoom.OnEntered += HandleEnteredCrafting;
        if (cauldron != null) cauldron.OnPotionCrafted += HandlePotionCrafted;

        if (deliveryInteraction != null)
            deliveryInteraction.OnInteracted += HandleDelivered;

        GameProgressManager.Instance.OnDayStarted += HandleWentInside;
        GameProgressManager.Instance.OnNightTimeExpired += HandleWentInside;
        GameProgressManager.Instance.OnNightStarted += HandleSlept;
    }

    private void OnDisable()
    {
        if (downstairs != null) downstairs.OnUsed -= HandleWentDownstairs;
        if (doorExit != null) doorExit.OnUsed -= HandleWentOutside;
        if (questManager != null) questManager.OnQuestsChanged -= HandleQuestsChanged;
        if (basketDisplay != null) basketDisplay.OnOpened -= HandleInventoryOpened;
        if (basketDisplay != null) basketDisplay.OnClosed -= HandleInventoryClosed;

        if (craftingRoom != null) craftingRoom.OnEntered -= HandleEnteredCrafting;
        if (cauldron != null) cauldron.OnPotionCrafted -= HandlePotionCrafted;

        if (deliveryInteraction != null)
            deliveryInteraction.OnInteracted -= HandleDelivered;

        GameProgressManager.Instance.OnDayStarted -= HandleWentInside;
        GameProgressManager.Instance.OnNightTimeExpired -= HandleWentInside;
        GameProgressManager.Instance.OnNightStarted -= HandleSlept;

        highlighter.Clear();
    }

    // ---------------------------------------------------------------- Pasos

    private void HandleWentDownstairs()
    {
        if (currentStep != Step.Movement) return;

        currentStep = Step.GoOutside;
        Show("¡Bien! Ahora salí por la puerta: afuera están el buzón con los pedidos y los ingredientes.");
    }

    private void HandleWentOutside()
    {
        if (currentStep != Step.GoOutside) return;

        currentStep = Step.CheckMailbox;
        Show("Acercate al buzón y pulsá E para aceptar los pedidos. Cada pedido es una poción que vas a tener que preparar.");

        if (playerCollector != null) playerCollector.CollectionBlocked = true;

        if (doorInteraction != null)
        {
            doorInteraction.EntryBlocked = true;
            doorInteraction.BlockedMessage = "Todavía no acepté todos los pedidos del buzón, no puedo entrar sin eso.";
        }
    }

    private void HandleQuestsChanged()
    {
        if (currentStep != Step.CheckMailbox) return;
        if (questManager.ActiveQuests.Count < questsNeededForNextStep) return;

        currentStep = Step.CollectItems;
        Show("¡Pedidos aceptados! En el diario de objetivos ves qué ingredientes te faltan. Los que necesitás brillan: acercate y pulsá E para juntarlos. Con TAB revisás tu canasto.");

        if (playerCollector != null) playerCollector.CollectionBlocked = false;
        if (doorInteraction != null) doorInteraction.EntryBlocked = false;
    }

    private void HandleInventoryOpened()
    {
        if (currentStep != Step.CollectItems || inventoryOpenedOnce) return;
        inventoryOpenedOnce = true;

        Show("Este es tu canasto. Si juntaste algo que no sirve, hacé click sobre él para tirarlo. Pulsá TAB para cerrarlo.");
    }

    private void HandleInventoryClosed()
    {
        if (currentStep != Step.CollectItems || !inventoryOpenedOnce || inventoryClosedMessageShown) return;
        inventoryClosedMessageShown = true;

        Show("Juntá todo antes de que amanezca: de día los ingredientes se marchitan y ya no se pueden recolectar.");
    }

    // Entrar a la casa cierra la parte de afuera pase lo que pase (te saltees el buzón o no) —
    // así nunca queda trabado esperando algo que el jugador decidió no hacer.
    private void HandleWentInside()
    {
        if (nightPhaseDone) return;
        if (currentStep == Step.Movement || currentStep == Step.GoOutside) return;

        nightPhaseDone = true;
        currentStep = Step.CraftingIntro;
        Show("Ahora toca preparar las pociones. Acercate al caldero y pulsá E.");
    }

    private void HandleEnteredCrafting()
    {
        if (currentStep != Step.CraftingIntro) return;

        currentStep = Step.CraftPotion;
        lastCraftingHint = null;
        guideTimer = 0f; // la guía del crafteo arranca en el próximo frame
    }

    private void HandlePotionCrafted(PotionData potion)
    {
        if (currentStep != Step.CraftPotion) return;

        // Si quedan pedidos sin poción, la guía sigue con la próxima receta
        if (QuestRequirements.GetUncoveredQuests(questManager).Count > 0) return;

        currentStep = Step.Deliver;
        Show("¡Pociones listas! Pulsá E para salir de la mesa y dejalas en el mostrador para entregarlas.");
    }

    private void HandleDelivered()
    {
        if (currentStep != Step.Deliver) return;
        if (!AllQuestsDelivered()) return;

        currentStep = Step.WaitingToSleep;
        Show("Los pedidos se cobran cuando dormís. Juntá dinero para comprar el ingrediente especial de tu poción del sueño pesado lo antes posible… si no, ya vas a ver. Ahora subí las escaleras y andá a la cama.");
    }

    private void HandleSlept()
    {
        if (currentStep != Step.WaitingToSleep) return;
        FinishTutorial();
    }

    private void FinishTutorial()
    {
        currentStep = Step.Done;
        highlighter.Clear();
        TutorialUI.Instance.Hide();
        TutorialModeFlag.IsActive = false;
        Time.timeScale = 1f;

        SceneManager.LoadScene(mainMenuSceneName);
    }

    private static void Show(string text) => TutorialUI.Instance.Show(Title, text);

    // ---------------------------------------------------------------- Guía visual

    // Decide qué titila según el paso actual (y en el crafteo, también qué indicación mostrar).
    private void RefreshGuide()
    {
        switch (currentStep)
        {
            case Step.Movement:
                highlighter.Add(downstairs);
                break;

            case Step.GoOutside:
                highlighter.Add(doorExit);
                break;

            case Step.CheckMailbox:
                highlighter.Add(mailbox);
                break;

            case Step.CollectItems:
                if (collectedMessageShown) highlighter.Add(doorInteraction);
                else HighlightMissingCollectibles();
                break;

            case Step.CraftingIntro:
                highlighter.Add(craftingRoom);
                break;

            case Step.CraftPotion:
                GuideCrafting();
                break;

            case Step.Deliver:
                highlighter.Add(deliveryInteraction);
                break;

            case Step.WaitingToSleep:
                highlighter.Add(upstairs);
                highlighter.Add(bed);
                break;
        }

        highlighter.Apply();
    }

    private void HighlightMissingCollectibles()
    {
        HashSet<ItemData> missing = GetMissingItems();
        if (missing.Count == 0) return;

        if (collectibles == null)
            collectibles = FindObjectsByType<Collectible>(FindObjectsSortMode.None);

        Vector3 playerPosition = playerCollector != null ? playerCollector.transform.position : Vector3.zero;

        foreach (Collectible collectible in collectibles)
        {
            if (collectible == null || collectible.IsCollected) continue;
            if (collectible.Item == null || !missing.Contains(collectible.Item)) continue;
            if (playerCollector != null && Vector3.Distance(playerPosition, collectible.transform.position) > collectibleHighlightRadius) continue;

            highlighter.Add(collectible);
        }
    }

    // Ítems que piden los pedidos todavía sin poción y que el jugador aún no juntó.
    private HashSet<ItemData> GetMissingItems()
    {
        Dictionary<ItemData, int> needed = new Dictionary<ItemData, int>();

        foreach (QuestData quest in QuestRequirements.GetUncoveredQuests(questManager))
        {
            foreach (KeyValuePair<ItemData, int> pair in QuestRequirements.GetItemTotals(quest.Potion))
            {
                needed.TryGetValue(pair.Key, out int current);
                needed[pair.Key] = current + pair.Value;
            }
        }

        HashSet<ItemData> missing = new HashSet<ItemData>();
        foreach (KeyValuePair<ItemData, int> pair in needed)
        {
            if (QuestRequirements.GetOwned(pair.Key, inventory) < pair.Value)
                missing.Add(pair.Key);
        }

        return missing;
    }

    // ---------------------------------------------------------------- Guía del crafteo

    private void GuideCrafting()
    {
        if (craftingRoom == null || cauldron == null) return;

        if (!craftingRoom.IsInside)
        {
            highlighter.Add(craftingRoom);
            ShowCraftingHint("Acercate al caldero y pulsá E para seguir preparando las pociones.");
            return;
        }

        PotionData target = GetTargetPotion();
        if (target == null)
        {
            ShowCraftingHint("¡Muy bien! Ya tenés las pociones de todos los pedidos.");
            return;
        }

        string instruction = BuildCraftingInstruction(target);
        ShowCraftingHint($"Preparando: {TutorialUI.Item(target.DisplayName)}\n{instruction}\n<size=75%>A y D: cambiar de mesa   ·   E: salir de la mesa</size>");
    }

    private PotionData GetTargetPotion()
    {
        foreach (QuestData quest in QuestRequirements.GetUncoveredQuests(questManager))
        {
            if (quest.Potion != null)
                return quest.Potion;
        }

        return null;
    }

    // Indica el próximo paso para la receta y hace titilar lo que hay que tocar.
    private string BuildCraftingInstruction(PotionData target)
    {
        HomeStorage storage = HomeStorage.Instance;
        ItemContainer pot = storage.Cauldron;
        Dictionary<ItemKey, int> required = target.GetRequirements();

        // 1) Algo en el caldero que no va (o de más)
        List<ItemKey> extra = new List<ItemKey>();
        foreach (KeyValuePair<ItemKey, int> pair in pot.Contents)
        {
            required.TryGetValue(pair.Key, out int needed);
            if (pair.Value > needed) extra.Add(pair.Key);
        }

        if (extra.Count > 0)
        {
            foreach (ItemVisual visual in FindObjectsByType<ItemVisual>(FindObjectsSortMode.None))
            {
                if (visual.Owner == cauldron && extra.Contains(visual.Key))
                    highlighter.AddObject(visual.gameObject);
            }

            return $"En el caldero hay {TutorialUI.Item(extra[0].DisplayName)} que no va en esta poción: hacé click sobre él para sacarlo.";
        }

        // 2) El primer ingrediente que falta, en el orden de la receta
        foreach (PotionIngredient ingredient in target.Ingredients)
        {
            if (ingredient == null || ingredient.Item == null) continue;

            ItemKey key = ingredient.Key;
            required.TryGetValue(key, out int total);
            int missing = total - pot.Count(key);
            if (missing <= 0) continue;

            string amount = missing > 1 ? $" (faltan {missing})" : "";
            TableType table = key.Item.Table;

            if (key.State == ItemState.Processed)
            {
                if (storage.Pantry.Count(key) > 0)
                    return ClickToCauldron(key, amount, "");

                ItemKey raw = ItemKey.Raw(key.Item);
                if (storage.Pantry.Count(raw) > 0)
                {
                    bool visible = HighlightShelfItem(raw);
                    highlighter.AddObject(GetStation(table));

                    string where = table == TableType.Mortar ? "al mortero para molerlo" : "a la tabla de picar para cortarlo";
                    return WithNavigation(visible, table, $"Arrastrá {TutorialUI.Item(raw.DisplayName)} {where}{amount}.");
                }
            }
            else if (storage.Pantry.Count(key) > 0)
            {
                return ClickToCauldron(key, amount, " No hace falta procesarlo.");
            }

            return $"Te falta {TutorialUI.Item(key.Item.DisplayName)} y no tenés más en la casa: vas a tener que salir a recolectarlo.";
        }

        // 3) Está todo: a revolver
        highlighter.AddObject(TutorialHighlighter.GetField<GameObject>(cauldron, "clickTarget"));
        return "¡Ya está todo en el caldero! Hacé click en la cuchara para preparar la poción.";
    }

    private string ClickToCauldron(ItemKey key, string amount, string extraText)
    {
        bool visible = HighlightShelfItem(key);
        return WithNavigation(visible, key.Item.Table,
            $"Hacé click en {TutorialUI.Item(key.DisplayName)} para echarlo al caldero{amount}.{extraText}");
    }

    // Hace titilar los modelos de ese ítem en los estantes. Devuelve si alguno se ve en pantalla.
    private bool HighlightShelfItem(ItemKey key)
    {
        bool anyVisible = false;

        foreach (ItemVisual visual in FindObjectsByType<ItemVisual>(FindObjectsSortMode.None))
        {
            if (!(visual.Owner is ItemShelfView) || visual.Key != key) continue;

            highlighter.AddObject(visual.gameObject);
            anyVisible |= IsOnScreen(visual.transform.position);
        }

        return anyVisible;
    }

    private static string WithNavigation(bool visible, TableType table, string text)
    {
        if (visible) return text;

        string tableName = table == TableType.Mortar ? "del mortero" : "de la tabla de picar";
        return $"{text}\nEstá en la mesa {tableName}: usá A y D para ir hasta ahí.";
    }

    private GameObject GetStation(TableType table)
    {
        if (workTables == null) return null;

        WorkTable workTable = workTables.FirstOrDefault(t => t != null && t.Table == table);
        return TutorialHighlighter.GetField<GameObject>(workTable, "station");
    }

    private static bool IsOnScreen(Vector3 worldPosition)
    {
        Camera camera = Camera.main;
        if (camera == null) return true;

        Vector3 viewport = camera.WorldToViewportPoint(worldPosition);
        return viewport.z > 0f && viewport.x >= 0f && viewport.x <= 1f && viewport.y >= 0f && viewport.y <= 1f;
    }

    private void ShowCraftingHint(string text)
    {
        if (text == lastCraftingHint) return;

        lastCraftingHint = text;
        Show(text);
    }
}
