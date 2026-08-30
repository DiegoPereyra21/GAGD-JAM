using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(DayTransition))]
public class BedInteraction : MonoBehaviour
{
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private InteractableOutline outline;
    [SerializeField] private QuestManager questManager;

    private DayTransition dayTransition;
    private InputAction interactAction;
    private bool playerInRange;

    private void Awake()
    {
        interactAction = playerInput.actions["Interact"];
        dayTransition = GetComponent<DayTransition>();
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
    //para que no pueda ir a dormir sin haber salido nunca
    private void OnInteract(InputAction.CallbackContext ctx)
    {
        if (!playerInRange) return;

        if (!GameProgressManager.Instance.HasBeenOutsideThisCycle)
        {
            Debug.Log("[BedInteraction] Todavía no saliste a recolectar, no podés dormir.");
            return;
        }

        questManager.Save();
        GameProgressManager.Instance.Sleep();
        dayTransition.PlayDayIntro(GameProgressManager.Instance.CurrentDay);
    }

    private void Start()
    {
        if (GameProgressManager.Instance.ConsumeWelcomeFade())
            dayTransition.PlayDayIntro(GameProgressManager.Instance.CurrentDay);
    }
}