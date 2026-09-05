using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Collider))]
public class DeliveryInteraction : MonoBehaviour
{
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private QuestManager questManager;
    [SerializeField] private PotionDisplayArea potionDisplayArea;
    [SerializeField] private InteractableOutline outline;

    private InputAction interactAction;
    private bool playerInRange;

    private void Awake()
    {
        interactAction = playerInput.actions["Interact"];
    }

    private void Start()
    {
        foreach (QuestData quest in questManager.PendingDeliveries)
            potionDisplayArea.AddOne(quest.requiredPotion, quest.requiredPotion.visualPrefab);
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

        DeliverMatchingQuests();
    }

    private void DeliverMatchingQuests()
    {
        List<QuestData> delivered = new List<QuestData>();

        foreach (QuestData quest in questManager.ActiveQuests)
        {
            if (quest.requiredPotion == null) continue;

            if (HomeStorage.Instance.RemoveCraftedPotion(quest.requiredPotion))
            {
                questManager.MarkPendingDelivery(quest);
                delivered.Add(quest);
                potionDisplayArea.AddOne(quest.requiredPotion, quest.requiredPotion.visualPrefab);
            }
        }

        if (delivered.Count > 0)
        {
            HomeStorage.Instance.Save();
            questManager.Save();
            DialogueUI.Instance.ShowMessage("Ofelia", $"Entregadas {delivered.Count} misión(es), pendientes de venta hasta dormir.");
        }
        else
        {
            DialogueUI.Instance.ShowMessage("Ofelia", "No tengo ninguna poción que coincida con un pedido.");
        }
    }
}