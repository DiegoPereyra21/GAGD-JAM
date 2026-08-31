using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using Game.Collectibles;

[RequireComponent(typeof(Collider))]
public class SleepPotionBox : MonoBehaviour
{
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private InteractableOutline outline;
    [SerializeField] private TextMeshPro label;
    [SerializeField] private Camera billboardCamera;
    [SerializeField] private int potionCost = 100;
    [SerializeField] private IngredientType specialIngredient;
    [SerializeField] private GameObject basketVisualPrefab;
    [SerializeField] private PlayerInventory inventory;
    [SerializeField] private BasketDisplay basketDisplay;
    [SerializeField] private float confirmationDuration = 2f;

    private InputAction interactAction;
    private bool playerInRange;
    private bool showingConfirmation;

    private void Awake()
    {
        interactAction = playerInput.actions["Interact"];
        label.gameObject.SetActive(true);
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

    private void LateUpdate()
    {
        if (!showingConfirmation)
            RefreshLabel();

        if (billboardCamera != null)
            label.transform.rotation = Quaternion.LookRotation(label.transform.position - billboardCamera.transform.position);
    }

    private bool CanDeliverToday(GameProgressManager progress)
    {
        return progress.SleepIngredientPurchased && progress.CurrentDay > progress.SleepIngredientPurchaseDay;
    }

    private void RefreshLabel()
    {
        GameProgressManager progress = GameProgressManager.Instance;

        if (progress.SleepIngredientObtained)
        {
            label.text = "";
            return;
        }

        if (progress.SleepIngredientPurchased)
        {
            label.text = CanDeliverToday(progress) ? "ENTREGA DE INGREDIENTE ESPECIAL" : "VUELVE MAÑANA";
            return;
        }

        int missing = Mathf.Max(0, potionCost - progress.Money);
        label.text = $"Faltan {missing} de oro";
    }

    private void OnInteract(InputAction.CallbackContext ctx)
    {
        if (!playerInRange) return;

        GameProgressManager progress = GameProgressManager.Instance;

        if (progress.SleepIngredientObtained) return;

        if (progress.SleepIngredientPurchased)
        {
            if (!CanDeliverToday(progress)) return;
            if (inventory.IsFull) return;

            inventory.AddItem(specialIngredient, 1);
            basketDisplay.Drop(specialIngredient, basketVisualPrefab);

            progress.MarkSleepIngredientObtained();
            progress.Save();
            return;
        }

        if (progress.Money < potionCost) return;

        progress.TrySpendMoney(potionCost);
        progress.MarkSleepIngredientPurchased();
        progress.Save();
    }

    private IEnumerator ShowConfirmation(string message)
    {
        showingConfirmation = true;
        label.text = message;
        yield return new WaitForSeconds(confirmationDuration);
        showingConfirmation = false;
    }
}