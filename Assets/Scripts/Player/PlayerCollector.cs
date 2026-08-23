using UnityEngine;
using UnityEngine.InputSystem;
using Game.Collectibles;
//para q el player al apretar "E" agfgarre directamente el hongo, hada o baya o lo que sea
public class PlayerCollector : MonoBehaviour
{
    [SerializeField] private float collectRadius = 1.5f;
    [SerializeField] private LayerMask collectibleLayer;
    [SerializeField] private PlayerInventory inventory;

    private InputAction interactAction;
    private void Awake()
    {
        interactAction = GetComponent<PlayerInput>().actions["Interact"];
    }
    private void OnEnable() => interactAction.performed += OnInteract;
    private void OnDisable() => interactAction.performed -= OnInteract;
    private void OnInteract(InputAction.CallbackContext ctx)
    {
        Collectible target = FindNearestCollectible();
        if (target == null) return;

        CollectibleType type = target.Type;
        int value = target.Value;

        target.Collect(() => inventory.AddItem(type, value));
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