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
        List<QuestData> completed = new List<QuestData>();

        foreach (QuestData quest in questManager.ActiveQuests)
        {
            if (quest.requiredPotion == null) continue;

            if (HomeStorage.Instance.RemoveCraftedPotion(quest.requiredPotion))
            {
                GameProgressManager.Instance.AddMoney(quest.moneyReward);
                completed.Add(quest);
                potionDisplayArea.AddOne(quest.requiredPotion, quest.requiredPotion.visualPrefab);
            }
        }

        foreach (QuestData quest in completed)
            questManager.CompleteQuest(quest);

        if (completed.Count > 0)
        {
            HomeStorage.Instance.Save();
            questManager.Save();
            Debug.Log($"[DeliveryInteraction] Entregadas {completed.Count} misión(es).");
        }
        else
        {
            Debug.Log("[DeliveryInteraction] No hay pociones que coincidan con ninguna misión activa.");
        }
    }
}