using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Collider))]
public class Mailbox : MonoBehaviour
{
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private QuestManager questManager;
    [SerializeField] private List<QuestData> pendingLetters = new List<QuestData>();

    public int RemainingCount => pendingLetters.Count;

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
            playerInRange = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            playerInRange = false;
    }

    private void OnInteract(InputAction.CallbackContext ctx)
    {
        if (!playerInRange || pendingLetters.Count == 0) return;

        QuestData letter = pendingLetters[0];
        pendingLetters.RemoveAt(0);
        questManager.AddQuest(letter);
    }
}