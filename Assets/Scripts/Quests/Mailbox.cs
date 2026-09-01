using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
//script para asignar tareas y de paso aplicar el outline
[RequireComponent(typeof(Collider))]
public class Mailbox : MonoBehaviour
{
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private QuestManager questManager;
    [SerializeField] private List<QuestData> pendingLetters = new List<QuestData>();
    [SerializeField] private InteractableOutline outline;

    private InputAction interactAction;
    private bool playerInRange;

    private void Awake()
    {
        interactAction = playerInput.actions["Interact"];
    }

    private void OnEnable()
    {
        interactAction.performed += OnInteract;
        GameProgressManager.Instance.OnNightStarted += DiscardExpiredLetters;
    }

    private void OnDisable()
    {
        interactAction.performed -= OnInteract;
        GameProgressManager.Instance.OnNightStarted -= DiscardExpiredLetters;
    }

    private void DiscardExpiredLetters()
    {
        int currentDay = GameProgressManager.Instance.CurrentDay;
        pendingLetters.RemoveAll(letter => letter.availableDay < currentDay);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            outline?.SetHighlighted(true);
        }
    }

    public int RemainingCount
    {
        get
        {
            int currentDay = GameProgressManager.Instance.CurrentDay;
            int count = 0;
            foreach (QuestData letter in pendingLetters)
                if (letter.availableDay <= currentDay) count++;
            return count;
        }
    }

    private void OnInteract(InputAction.CallbackContext ctx)
    {
        if (!playerInRange) return;

        int currentDay = GameProgressManager.Instance.CurrentDay;
        QuestData letter = pendingLetters.Find(q => q.availableDay <= currentDay);
        if (letter == null) return;

        pendingLetters.Remove(letter);
        questManager.AddQuest(letter);
    }

}