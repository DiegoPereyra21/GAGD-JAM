using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Mailbox : MonoBehaviour
{
    private const string SaveKey = "Mailbox_Data";

    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private QuestManager questManager;
    [SerializeField] private List<QuestData> allLetters = new List<QuestData>(); //el catálogo completo, se configura en el Inspector
    [SerializeField] private InteractableOutline outline;
    [SerializeField] private float interactRadius = 2f;

    private List<QuestData> pendingLetters = new List<QuestData>();

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
        Load();
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
        int before = pendingLetters.Count;
        pendingLetters.RemoveAll(letter => letter.availableDay < currentDay);
        if (pendingLetters.Count != before) Save();
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

        // Cartas que ya están activas (tomadas en una sesión anterior) no tienen que seguir bloqueando la lista
        pendingLetters.RemoveAll(q => questManager.IsQuestActive(q));

        int currentDay = GameProgressManager.Instance.CurrentDay;
        QuestData letter = pendingLetters.Find(q => q.availableDay <= currentDay);

        if (letter == null)
        {
            DialogueUI.Instance.ShowMessage("Ofelia", "No tengo más cartas por ahora.");
            return;
        }

        if (questManager.AddQuest(letter))
        {
            pendingLetters.Remove(letter);
            Save();
        }
        else
        {
            DialogueUI.Instance.ShowMessage("Ofelia", "Ya tengo demasiados pedidos, no puedo aceptar más por ahora.");
        }
    }

    private void Save()
    {
        if (TutorialModeFlag.IsActive) return;

        SaveData data = new SaveData();
        foreach (QuestData letter in pendingLetters)
            data.letterIds.Add(letter.questId);

        PlayerPrefs.SetString(SaveKey, JsonUtility.ToJson(data));
        PlayerPrefs.Save();
    }

    private void Load()
    {
        if (TutorialModeFlag.IsActive || !PlayerPrefs.HasKey(SaveKey))
        {
            pendingLetters = new List<QuestData>(allLetters);
            return;
        }

        SaveData data = JsonUtility.FromJson<SaveData>(PlayerPrefs.GetString(SaveKey));
        pendingLetters = new List<QuestData>();
        foreach (string id in data.letterIds)
        {
            QuestData letter = allLetters.Find(q => q.questId == id);
            if (letter != null) pendingLetters.Add(letter);
        }
    }

    [System.Serializable]
    private class SaveData
    {
        public List<string> letterIds = new List<string>();
    }
}