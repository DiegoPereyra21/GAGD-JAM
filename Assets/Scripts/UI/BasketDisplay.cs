using Game.Collectibles;
using UnityEngine;
using UnityEngine.InputSystem;
//administra todo lo referido al "spawnear" el item dentro del canasto
public class BasketDisplay : MonoBehaviour
{
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private CameraTransition cameraTransition;
    [SerializeField] private Transform dropPoint;
    [SerializeField] private float spawnHorizontalSpread = 0.3f;
    [SerializeField] private float spawnHeight = 0.5f;
    [SerializeField] private float settleDelay = 0.5f;
    [SerializeField] private PlayerInventory inventory;
    [SerializeField] private LayerMask basketItemLayer;
    //para cambiar la llamda q tenia
    [SerializeField] private Transform basketViewAnchor;
    //para activar desactivar segun si esta dentro o fuera de la casa
    [SerializeField] private GameObject basketRoot;
    [SerializeField] private Transform[] dropSlots;
    private GameObject[] slotOccupants;

    [SerializeField] private Renderer[] playerRenderers; //renderers del personaje a ocultar mientras se ve el canasto
    private bool isAvailable;
    
    public event System.Action OnOpened;
    public event System.Action OnClosed;

    private InputAction toggleAction;
    private bool isOpen;

    public bool IsOpen => isOpen;

    private void Awake()
    {
        toggleAction = playerInput.actions["Inventory"];
        slotOccupants = new GameObject[dropSlots.Length];
    }
    private void Update()
    {
        if (!isOpen) return;
        if (Mouse.current == null || !Mouse.current.leftButton.wasPressedThisFrame) return;

        TryRemoveClickedItem();
    }

    private void TryRemoveClickedItem()
    {
        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (Physics.Raycast(ray, out RaycastHit hit, 100f, basketItemLayer))
        {
            if (hit.collider.TryGetComponent(out BasketItemVisual visual))
            {
                inventory.RemoveItem(visual.Type, 1);
                Destroy(visual.gameObject);
            }
        }
    }

    private void OnEnable() => toggleAction.performed += OnToggle;
    private void OnDisable() => toggleAction.performed -= OnToggle;

    private void OnToggle(InputAction.CallbackContext ctx)
    {
        if (!isAvailable) return;
        if (!isOpen && playerMovement.IsPickupInProgress) return;

        isOpen = !isOpen;
        playerMovement.SetFrozen(isOpen);

        if (isOpen)
        {
            cameraTransition.TransitionTo(basketViewAnchor, cameraTransition.BasketViewFov, () => SetPlayerVisible(false));
            OnOpened?.Invoke();
        }
        else
        {
            SetPlayerVisible(true);
            cameraTransition.TransitionToPlayer();
            OnClosed?.Invoke();
        }
    }
    private void SetPlayerVisible(bool visible)
    {
        foreach (Renderer r in playerRenderers)
            if (r != null) r.enabled = visible;
    }
    
    public void SetAvailable(bool available)
    {
        isAvailable = available;
        basketRoot?.SetActive(available);

        if (!available && isOpen)
        {
            isOpen = false;
            playerMovement.SetFrozen(false);
            cameraTransition.TransitionToPlayer();
        }
    }
    //logica de spawneo del item en el canasto
    public void Drop(IngredientType type, GameObject visualPrefab)
    {
        if (dropSlots == null || dropSlots.Length == 0)
        {
            Debug.LogWarning("BasketDisplay: no hay Drop Slots asignados.", this);
            return;
        }

        int slotIndex = FindFreeSlot();
        Transform slot = dropSlots[slotIndex];

        Vector3 spawnPos = slot.position + new Vector3(
            Random.Range(-spawnHorizontalSpread, spawnHorizontalSpread),
            spawnHeight,
            Random.Range(-spawnHorizontalSpread, spawnHorizontalSpread));

        GameObject go = Instantiate(visualPrefab, spawnPos, Random.rotation);
        go.AddComponent<BasketItemVisual>().Init(type);
        slotOccupants[slotIndex] = go;
        StartCoroutine(SettleThenFreeze(go));
    }

    private int FindFreeSlot()
    {
        for (int i = 0; i < dropSlots.Length; i++)
            if (slotOccupants[i] == null) return i;

        return Random.Range(0, dropSlots.Length); //canasto lleno
    }
    //FIX BUG, se salia todo el rato los objetos de dentro del canasto
    //con esto quedan inmoviles luego de 0.5f
    private System.Collections.IEnumerator SettleThenFreeze(GameObject go)
    {
        yield return new WaitForSeconds(settleDelay);
        if (go == null) yield break;

        if (go.TryGetComponent(out Rigidbody rb))
            rb.isKinematic = true;

        go.transform.SetParent(dropPoint, true);
    }
    //para visualizar el "area" donde pueden aparecer los objetos en el canasto, sigue sin convencerme
    private void OnDrawGizmosSelected()
    {
        if (dropSlots == null) return;

        Gizmos.color = Color.yellow;
        foreach (Transform slot in dropSlots)
        {
            if (slot == null) continue;
            Gizmos.DrawWireCube(slot.position + Vector3.up * spawnHeight * 0.5f,
                new Vector3(spawnHorizontalSpread * 2f, spawnHeight, spawnHorizontalSpread * 2f));
        }
    }
    //para que al guardar en el homestorage se borre todo lo que este en el canasto
    public void ClearAll()
    {
        for (int i = dropPoint.childCount - 1; i >= 0; i--)
        {
            Transform child = dropPoint.GetChild(i);
            if (child.TryGetComponent(out BasketItemVisual _))
                Destroy(child.gameObject);
        }

        for (int i = 0; i < slotOccupants.Length; i++)
            slotOccupants[i] = null;
    }
}