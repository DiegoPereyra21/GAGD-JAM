using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Linq;

[RequireComponent(typeof(Collider))]
public class DeliveryInteraction : MonoBehaviour
{
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private QuestManager questManager;
    [SerializeField] private PotionDisplayArea potionDisplayArea;
    [SerializeField] private InteractableOutline outline;

    private InputAction interactAction;
    private bool playerInRange;

    public event System.Action OnInteracted;

    private void Awake()
    {
        interactAction = playerInput.actions["Interact"];
    }

    private void Start()
    {
        foreach (QuestData quest in questManager.PendingDeliveries)
            potionDisplayArea.AddOne(quest.Potion);
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

    private void OnInteract(InputAction.CallbackContext ctx)
    {
        if (!playerInRange) return;

        if (!GameProgressManager.Instance.IsCraftingTimeActive)
        {
            DialogueUI.Instance.ShowMessage("Ofelia", "Ya es demasiado tarde, ya no podes enviar pedidos a esta hora.");
            return;
        }

        DeliverMatchingQuests();
        OnInteracted?.Invoke();
    }

    private void DeliverMatchingQuests()
    {
        List<QuestData> delivered = new List<QuestData>();

        foreach (QuestData quest in questManager.ActiveQuests)
        {
            if (quest.Potion == null) continue;
            if (questManager.PendingDeliveries.Contains(quest)) continue;

            if (HomeStorage.Instance.RemoveCraftedPotion(quest.Potion))
            {
                questManager.MarkPendingDelivery(quest);
                delivered.Add(quest);
                potionDisplayArea.AddOne(quest.Potion);
            }
        }

        if (delivered.Count > 0)
        {
            GameSaveSystem.SaveAll(questManager);
            DialogueUI.Instance.ShowMessage("Ofelia", $"Entregaste {delivered.Count} pocion(es) de pedidos, ahora podes ir a acostarte.");
        }
        else
        {
            DialogueUI.Instance.ShowMessage("Ofelia", "No tengo ninguna poción que pueda entregar.");
        }
    }
}
