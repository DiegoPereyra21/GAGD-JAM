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

    private InputAction toggleAction;
    private bool isOpen;

    private void Awake()
    {
        toggleAction = playerInput.actions["Inventory"];
    }

    private void OnEnable() => toggleAction.performed += OnToggle;
    private void OnDisable() => toggleAction.performed -= OnToggle;

    private void OnToggle(InputAction.CallbackContext ctx)
    {
        isOpen = !isOpen;
        playerMovement.SetFrozen(isOpen);

        if (isOpen)
            cameraTransition.TransitionToBasket();
        else
            cameraTransition.TransitionToPlayer();
    }
    //logica de spawneo del item en el canasto
    public void Drop(GameObject visualPrefab)
    {
        Vector3 spawnPos = dropPoint.position + new Vector3(
            Random.Range(-spawnHorizontalSpread, spawnHorizontalSpread),
            spawnHeight,
            Random.Range(-spawnHorizontalSpread, spawnHorizontalSpread));

        GameObject go = Instantiate(visualPrefab, spawnPos, Random.rotation);
        StartCoroutine(SettleThenFreeze(go));
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
        if (dropPoint == null) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(dropPoint.position + Vector3.up * spawnHeight * 0.5f,
            new Vector3(spawnHorizontalSpread * 2f, spawnHeight, spawnHorizontalSpread * 2f));
    }
}