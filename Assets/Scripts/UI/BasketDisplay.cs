using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.InputSystem;
using System.Collections;

[RequireComponent(typeof(UIDocument))]
public class BasketDisplay : MonoBehaviour
{
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private Camera basketCamera;
    [SerializeField] private Transform dropPoint;
    [SerializeField] private RenderTexture renderTexture;
    //para ajustar el area donde spawnean los visibles, aveces aparecian fuera del basket
    [SerializeField] private float spawnHorizontalSpread = 0.3f; // qué tan lejos del centro en X/Z
    [SerializeField] private float spawnHeight = 0.5f;           // altura desde la que cae


    private VisualElement panel;
    private InputAction toggleAction;
    private bool isOpen;

    private void Awake()
    {
        VisualElement root = GetComponent<UIDocument>().rootVisualElement;
        panel = root.Q<VisualElement>("BasketPanel");
        panel.style.backgroundImage = new StyleBackground(Background.FromRenderTexture(renderTexture));

        toggleAction = playerInput.actions["Inventory"];
    }

    private void OnEnable()
    {
        toggleAction.performed += OnToggle;
        SetOpen(false);
    }

    private void OnDisable()
    {
        toggleAction.performed -= OnToggle;
    }

    private void OnToggle(InputAction.CallbackContext ctx)
    {
        SetOpen(!isOpen);
    }

    private void SetOpen(bool open)
    {
        isOpen = open;
        panel.style.display = open ? DisplayStyle.Flex : DisplayStyle.None;
        basketCamera.enabled = open;
    }

    public void Drop(GameObject visualPrefab)
    {
        Vector3 spawnPos = dropPoint.position + new Vector3(
            Random.Range(-spawnHorizontalSpread, spawnHorizontalSpread),
            spawnHeight,
            Random.Range(-spawnHorizontalSpread, spawnHorizontalSpread));

        GameObject go = Instantiate(visualPrefab, spawnPos, Random.rotation);
        StartCoroutine(SettleThenFreeze(go));
    }

    private void OnDrawGizmosSelected()
    {
        if (dropPoint == null) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(dropPoint.position + Vector3.up * spawnHeight * 0.5f,
            new Vector3(spawnHorizontalSpread * 2f, spawnHeight, spawnHorizontalSpread * 2f));
    }

    private IEnumerator SettleThenFreeze(GameObject go)
    {
        yield return new WaitForSeconds(0.45f);
        if (go == null) yield break;

        if (go.TryGetComponent(out Rigidbody rb))
            rb.isKinematic = true;

        go.transform.SetParent(dropPoint, true);
    }
}