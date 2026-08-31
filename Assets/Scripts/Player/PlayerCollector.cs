using UnityEngine;
using UnityEngine.InputSystem;
using Game.Collectibles;
//para q el player al apretar "E" agfgarre directamente el hongo, hada o baya o lo que sea
public class PlayerCollector : MonoBehaviour
{
    [SerializeField] private float collectRadius = 1.5f;
    [SerializeField] private LayerMask collectibleLayer;
    [SerializeField] private PlayerInventory inventory;
    //para cesto interactivo
    [SerializeField] private BasketDisplay basketDisplay;
    //para avisar q tiene q pausarse el movimiento del player cuando recolecta algo, usar oninteract
    [SerializeField] private PlayerMovement playerMovement;

    // Sonido de pickup
    [SerializeField] private AK.Wwise.Event pickupEvent;

    private InputAction interactAction;
    private void Awake()
    {
        interactAction = GetComponent<PlayerInput>().actions["Interact"];
    }

    private Collectible currentHighlighted;

    private void Update()
    {
        Collectible nearest = FindNearestCollectible();

        if (nearest != currentHighlighted)
        {
            if (currentHighlighted != null)
                currentHighlighted.SetHighlighted(false);

            if (nearest != null)
                nearest.SetHighlighted(true);

            currentHighlighted = nearest;
        }
    }

    private void OnEnable() => interactAction.performed += OnInteract;
    private void OnDisable() => interactAction.performed -= OnInteract;
    private void OnInteract(InputAction.CallbackContext ctx)
    {
        if (!GameProgressManager.Instance.IsNightActive) return;
        if (inventory.IsFull) return;

        Collectible target = FindNearestCollectible();
        if (target == null || target.IsCollected) return;

        IngredientType type = target.Type;
        int value = target.Value;
        GameObject visualPrefab = target.BasketVisualPrefab;

        playerMovement.FreezeMovement(target.FreezeDuration);

        target.Collect(() =>
        {
            pickupEvent.Post(gameObject);
            inventory.AddItem(type, value);
            if (visualPrefab != null)
                basketDisplay.Drop(type, visualPrefab);
        });
    }
    //hace como un collider frente al player para que "agarre" lo que tenga al frente suyo(luego tengo q hacer un inventario en el player para q los "guarde")
    private Collectible FindNearestCollectible()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, collectRadius, collectibleLayer);
        Collectible nearest = null;
        float minDist = float.MaxValue;

        foreach (var hit in hits)
        {
            if (hit.TryGetComponent(out Collectible c))
            {
                float dist = Vector3.Distance(transform.position, hit.transform.position);
                if (dist < minDist) { minDist = dist; nearest = c; }
            }
        }

        return nearest;
    }
}