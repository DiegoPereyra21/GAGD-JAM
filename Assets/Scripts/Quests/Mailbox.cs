using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Mailbox : MonoBehaviour
{
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private QuestManager questManager;
    [SerializeField] private List<QuestData> pendingLetters = new List<QuestData>();
    [SerializeField] private InteractableOutline outline;
    [SerializeField] private float interactRadius = 2f;

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

    private InputAction interactAction;

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
    private void Update()
    {
        outline?.SetHighlighted(IsPlayerInRange());
    }

    private bool IsPlayerInRange()
    {
        return Vector3.Distance(transform.position, playerTransform.position) <= interactRadius;
    }

    private void OnInteract(InputAction.CallbackContext ctx)
    {
        if (!IsPlayerInRange()) return;

        int currentDay = GameProgressManager.Instance.CurrentDay;
        QuestData letter = pendingLetters.Find(q => q.availableDay <= currentDay);

        if (letter == null)
        {
            DialogueUI.Instance.ShowMessage("Ofelia", "No tengo más cartas por ahora.");
            return;
        }

        if (questManager.AddQuest(letter))
            pendingLetters.Remove(letter);
        else
            DialogueUI.Instance.ShowMessage("Ofelia", "Ya tengo demasiados pedidos, no puedo aceptar más por ahora.");
    }
}