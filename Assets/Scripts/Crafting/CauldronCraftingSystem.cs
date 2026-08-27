using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Game.Collectibles;

[Serializable]
public class TypeVisualPrefab
{
    public CollectibleType type;
    public GameObject prefab;
}

[RequireComponent(typeof(Collider))]
public class CauldronCraftingSystem : MonoBehaviour
{
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private CameraTransition cameraTransition;

    [SerializeField] private Transform houseViewAnchor; // cámara fija del piso 1, a donde volvés al salir
    [SerializeField] private Transform leftZoneAnchor;
    [SerializeField] private Transform cauldronViewAnchor;
    [SerializeField] private Transform rightZoneAnchor;

    [SerializeField] private GameObject cauldronClickObject;
    [SerializeField] private GameObject leftClickZone;
    [SerializeField] private GameObject rightClickZone;

    [SerializeField] private LayerMask craftingLayer;
    [SerializeField] private Transform cauldronDropPoint;
    [SerializeField] private List<TypeVisualPrefab> visualPrefabs;
    [SerializeField] private PotionRecipeDatabase recipeDatabase;

    [SerializeField] private ShelfSlot[] allShelfSlots;
    [SerializeField] private InteractableOutline outline;

    private InputAction interactAction;
    private bool playerInRange;
    private bool isInside;
    private int zoneIndex; // 0 = izquierda, 1 = caldero, 2 = derecha

    private readonly List<GameObject> cauldronContents = new List<GameObject>();
    private readonly Dictionary<CollectibleType, int> cauldronIngredients = new Dictionary<CollectibleType, int>();
    public IReadOnlyDictionary<CollectibleType, int> CauldronIngredients => cauldronIngredients;

    private void Awake()
    {
        interactAction = playerInput.actions["Interact"];
    }

    private void OnEnable()
    {
        interactAction.performed += OnInteractPressed;
        HomeStorage.Instance.OnStorageChanged += RefreshShelves;
        RefreshShelves();
    }

    private void OnDisable()
    {
        interactAction.performed -= OnInteractPressed;
        HomeStorage.Instance.OnStorageChanged -= RefreshShelves;
    }

    private void RefreshShelves()
    {
        foreach (ShelfSlot slot in allShelfSlots)
        {
            int count = HomeStorage.Instance.Totals.TryGetValue(slot.Type, out int c) ? c : 0;
            slot.Refresh(GetVisualPrefab(slot.Type), count);
        }
    }

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

    private void Update()
    {
        if (!isInside) return;

        if (Keyboard.current.aKey.wasPressedThisFrame) MoveZone(-1);
        if (Keyboard.current.dKey.wasPressedThisFrame) MoveZone(1);

        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            HandleClick();
    }

    private void OnInteractPressed(InputAction.CallbackContext ctx)
    {
        if (!isInside)
        {
            if (playerInRange) EnterCauldron();
            return;
        }

        ExitToHouse();
    }

    private void EnterCauldron()
    {
        isInside = true;
        zoneIndex = 1;
        playerMovement.SetFrozen(true);
        cameraTransition.TransitionTo(cauldronViewAnchor);
    }

    private void ExitToHouse()
    {
        isInside = false;
        playerMovement.SetFrozen(false);
        cameraTransition.TransitionTo(houseViewAnchor);
    }

    private void MoveZone(int direction)
    {
        int newIndex = Mathf.Clamp(zoneIndex + direction, 0, 2);
        if (newIndex == zoneIndex) return;

        zoneIndex = newIndex;
        cameraTransition.TransitionTo(GetAnchorForZone(zoneIndex));
    }

    private Transform GetAnchorForZone(int index)
    {
        return index switch
        {
            0 => leftZoneAnchor,
            2 => rightZoneAnchor,
            _ => cauldronViewAnchor,
        };
    }

    private void HandleClick()
    {
        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (!Physics.Raycast(ray, out RaycastHit hit, 100f, craftingLayer)) return;

        GameObject hitObject = hit.collider.gameObject;

        if (hitObject == leftClickZone) { MoveZone(-1); return; }
        if (hitObject == rightClickZone) { MoveZone(1); return; }

        if (zoneIndex == 1 && hitObject == cauldronClickObject)
        {
            Craft();
            return;
        }

        if (zoneIndex != 1 && hit.collider.TryGetComponent(out ShelfSlot slot))
            TryAddIngredient(slot.Type);
    }

    private void TryAddIngredient(CollectibleType type)
    {
        if (!HomeStorage.Instance.RemoveOne(type)) return;

        HomeStorage.Instance.Save();

        GameObject prefab = GetVisualPrefab(type);
        if (prefab == null)
        {
            Debug.LogWarning($"[CauldronCraftingSystem] Falta asignar el prefab visual para {type}");
            return;
        }

        Vector3 spawnPos = cauldronDropPoint.position + new Vector3(UnityEngine.Random.Range(-0.2f, 0.2f), 0.3f, UnityEngine.Random.Range(-0.2f, 0.2f));
        GameObject visual = Instantiate(prefab, spawnPos, UnityEngine.Random.rotation);
        cauldronContents.Add(visual);

        if (!cauldronIngredients.ContainsKey(type))
            cauldronIngredients[type] = 0;
        cauldronIngredients[type]++;
    }

    private GameObject GetVisualPrefab(CollectibleType type)
    {
        foreach (TypeVisualPrefab entry in visualPrefabs)
            if (entry.type == type) return entry.prefab;
        return null;
    }

    private void Craft()
    {
        PotionRecipe matchedRecipe = FindMatchingRecipe();

        if (matchedRecipe != null)
        {
            HomeStorage.Instance.AddPotion(matchedRecipe);
            HomeStorage.Instance.Save();
            Debug.Log($"[CauldronCraftingSystem] ¡Receta encontrada! Creaste: {matchedRecipe.potionName}");
        }
        else
        {
            Debug.Log("[CauldronCraftingSystem] Ninguna receta coincide con esta combinación.");
        }

        foreach (GameObject item in cauldronContents)
            Destroy(item);

        cauldronContents.Clear();
        cauldronIngredients.Clear();
    }

    private PotionRecipe FindMatchingRecipe()
    {
        foreach (PotionRecipe recipe in recipeDatabase.Recipes)
        {
            if (Matches(recipe)) return recipe;
        }

        return null;
    }

    private bool Matches(PotionRecipe recipe)
    {
        if (recipe.ingredients.Count != cauldronIngredients.Count) return false;

        foreach (RecipeIngredient required in recipe.ingredients)
        {
            if (!cauldronIngredients.TryGetValue(required.type, out int have)) return false;
            if (have != required.amount) return false;
        }

        return true;
    }
}