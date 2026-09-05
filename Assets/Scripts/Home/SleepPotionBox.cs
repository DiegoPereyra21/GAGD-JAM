using UnityEngine;
using UnityEngine.InputSystem;
using Game.Collectibles;

[RequireComponent(typeof(Collider))]
public class SleepPotionBox : MonoBehaviour
{
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private InteractableOutline outline;
    [SerializeField] private int potionCost = 100;
    [SerializeField] private IngredientType specialIngredient;
    [SerializeField] private GameObject basketVisualPrefab;
    [SerializeField] private PlayerInventory inventory;
    [SerializeField] private BasketDisplay basketDisplay;

    private InputAction interactAction;
    private bool playerInRange;

    private void Awake()
    {
        interactAction = playerInput.actions["Interact"];
    }

    private void OnEnable() => interactAction.performed += OnInteract;
    private void OnDisable() => interactAction.performed -= OnInteract;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            outline?.SetHighlighted(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            outline?.SetHighlighted(false);
        }
    }

    private bool CanDeliverToday(GameProgressManager progress)
    {
        return progress.SleepIngredientPurchased && progress.CurrentDay > progress.SleepIngredientPurchaseDay;
    }

    private void OnInteract(InputAction.CallbackContext ctx)
    {
        if (!playerInRange) return;

        GameProgressManager progress = GameProgressManager.Instance;

        if (progress.SleepIngredientObtained) return;

        if (progress.SleepIngredientPurchased)
        {
            if (!CanDeliverToday(progress))
            {
                DialogueUI.Instance.ShowMessage("Ofelia", "Todavía no llegó, tengo que volver mañana");
                return;
            }

            if (inventory.IsFull)
            {
                DialogueUI.Instance.ShowMessage("Ofelia", "No tengo lugar en el canasto para el ingrediente especial");
                return;
            }

            inventory.AddItem(specialIngredient, 1);
            basketDisplay.Drop(specialIngredient, basketVisualPrefab);

            progress.MarkSleepIngredientObtained();
            progress.Save();

            DialogueUI.Instance.ShowMessage("Ofelia", "¡Llegó el ingrediente especial!");
            return;
        }

        if (progress.Money < potionCost)
        {
            int missing = potionCost - progress.Money;
            DialogueUI.Instance.ShowMessage("Ofelia", $"Todavía me faltan {missing} monedas para el ingrediente especial");
            return;
        }

        progress.TrySpendMoney(potionCost);
        progress.MarkSleepIngredientPurchased();
        progress.Save();

        DialogueUI.Instance.ShowMessage("Ofelia", "Ya pagué. Tengo que volver mañana a buscarlo");
    }
}