using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Game.Collectibles;

[Serializable]
public class TypeVisualPrefab
{
    public IngredientType type;
    public GameObject prefab;
}

[RequireComponent(typeof(Collider))]
public class CauldronCraftingSystem : MonoBehaviour
{
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private Light playerSpotlight;
    [SerializeField] private GameObject player;
    private Renderer[] playerRenderers;
    [SerializeField] private CameraTransition cameraTransition;
    [SerializeField] private PotionBoxDisplay potionBoxDisplay;

    [SerializeField] private Transform houseViewAnchor;
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
    [SerializeField] private InteractableOutline outline;

    [SerializeField] private IngredientDisplayArea leftShelfDisplay;
    [SerializeField] private IngredientDisplayArea rightShelfDisplay;
    [SerializeField] private List<IngredientType> leftShelfTypes;
    [SerializeField] private List<IngredientType> rightShelfTypes;

    [SerializeField] private IngredientDisplayArea mortarProcessedDisplay;
    [SerializeField] private IngredientDisplayArea cuttingBoardProcessedDisplay;
    private readonly Dictionary<IngredientType, int> processedWaiting = new Dictionary<IngredientType, int>();

    [SerializeField] private GameObject mortarClickObject;
    [SerializeField] private GameObject cuttingBoardClickObject;
    [SerializeField] private List<ProcessingRecipe> processingRecipes;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private float interactRadius = 2f;

    //para que la pocion aparezca unos segundos luego de crafteada
    [SerializeField] private Transform potionShowcaseAnchor;
    [SerializeField] private float showcaseDuration = 3f;
    [SerializeField] private float swayAngle = 12f;
    [SerializeField] private float swaySpeed = 3f;

    private bool isShowcasing;

    //para detectar bien el click y drag
    private Vector2 dragStartScreenPos;
    [SerializeField] private float clickMovementThreshold = 15f;

    private InputAction interactAction;
    private bool playerInRange;
    private bool isInside;
    private int zoneIndex; // 0 = izquierda, 1 = caldero, 2 = derecha
    private IngredientDisplayArea draggedSourceArea;

    private readonly List<GameObject> cauldronContents = new List<GameObject>();
    private readonly Dictionary<IngredientType, int> cauldronIngredients = new Dictionary<IngredientType, int>();
    public IReadOnlyDictionary<IngredientType, int> CauldronIngredients => cauldronIngredients;

    private GameObject draggedVisual;
    private IngredientType draggedType;
    private float dragPlaneHeight;

    public event System.Action OnEnteredCrafting;
    public event System.Action OnPotionCrafted;

    public enum ProcessingStation { Mortar, CuttingBoard }

    [Serializable]
    public class ProcessingRecipe
    {
        public ProcessingStation station;
        public IngredientType rawType;
        public IngredientType processedType;
    }

    [Header("Sonidos de crafting")]
    [SerializeField] private AK.Wwise.Event playSTGCut;
    [SerializeField] private AK.Wwise.Event playSTGMort;

    private void Awake()
    {
        interactAction = playerInput.actions["Interact"];
        playerRenderers = player.GetComponentsInChildren<Renderer>();
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
        foreach (IngredientType type in leftShelfTypes)
        {
            int count = HomeStorage.Instance.Totals.TryGetValue(type, out int c) ? c : 0;
            leftShelfDisplay.SetCount(type, count, GetVisualPrefab(type));
        }

        foreach (IngredientType type in rightShelfTypes)
        {
            int count = HomeStorage.Instance.Totals.TryGetValue(type, out int c) ? c : 0;
            rightShelfDisplay.SetCount(type, count, GetVisualPrefab(type));
        }
    }


    private void Update()
    {
        if (!isInside)
        {
            playerInRange = Vector3.Distance(transform.position, playerTransform.position) <= interactRadius;
            outline?.SetHighlighted(playerInRange);
        }

        if (!isInside) return;
        if (isShowcasing) return; // bloquea A/D, click y drag mientras se muestra la poción

        if (Keyboard.current.aKey.wasPressedThisFrame) MoveZone(-1);
        if (Keyboard.current.dKey.wasPressedThisFrame) MoveZone(1);

        if (Mouse.current == null) return;

        if (Mouse.current.leftButton.wasPressedThisFrame)
            HandlePress();

        if (draggedVisual != null)
        {
            UpdateDragVisualPosition();

            if (Mouse.current.leftButton.wasReleasedThisFrame)
                EndDrag();
        }
    }

    private void OnInteractPressed(InputAction.CallbackContext ctx)
    {
        if (isShowcasing) return;

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
        SetPlayerVisible(false);
        if (playerSpotlight != null) playerSpotlight.enabled = false;
        outline?.SetHighlighted(false);
        OnEnteredCrafting?.Invoke();
    }

    private void ExitToHouse()
    {
        isInside = false;
        playerMovement.SetFrozen(false);
        cameraTransition.TransitionTo(houseViewAnchor);
        SetPlayerVisible(true);
        if (playerSpotlight != null) playerSpotlight.enabled = true;
        if (playerInRange)
            outline?.SetHighlighted(true);
    }
    private void SetPlayerVisible(bool visible)
    {
        foreach (Renderer r in player.GetComponentsInChildren<Renderer>(true))
            r.enabled = visible;
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

    private void HandlePress()
    {
        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (!Physics.Raycast(ray, out RaycastHit hit, 100f, craftingLayer)) return;

        GameObject hitObject = hit.collider.gameObject;

        if (hitObject == leftClickZone)
        {
            MoveZone(zoneIndex == 0 ? 1 : -1);
            return;
        }
        if (hitObject == rightClickZone)
        {
            MoveZone(zoneIndex == 2 ? -1 : 1);
            return;
        }

        if (zoneIndex == 1)
        {
            if (hitObject == cauldronClickObject)
            {
                Craft();
                return;
            }

            if (hit.collider.TryGetComponent(out CauldronItemVisual cauldronItem))
            {
                RemoveFromCauldron(cauldronItem);
                return;
            }
        }

        if (!hit.collider.TryGetComponent(out ProcessedItemVisual visual)) return;

        IngredientDisplayArea area = hit.collider.GetComponentInParent<IngredientDisplayArea>();

        if (area == mortarProcessedDisplay || area == cuttingBoardProcessedDisplay
            || area == leftShelfDisplay || area == rightShelfDisplay)
            TryBeginDrag(visual.Type, hitObject, area);
    }

    private void RemoveFromCauldron(CauldronItemVisual cauldronItem)
    {
        IngredientType type = cauldronItem.Type;

        if (!cauldronIngredients.TryGetValue(type, out int count) || count <= 0) return;

        cauldronIngredients[type] = count - 1;
        if (cauldronIngredients[type] <= 0)
            cauldronIngredients.Remove(type);

        cauldronContents.Remove(cauldronItem.gameObject);
        Destroy(cauldronItem.gameObject);

        ReturnIngredientToOrigin(type);
    }

    private void ReturnIngredientToOrigin(IngredientType type)
    {
        if (leftShelfTypes.Contains(type) || rightShelfTypes.Contains(type))
        {
            HomeStorage.Instance.AddIngredient(type);
            HomeStorage.Instance.Save();
            return;
        }

        ProcessingRecipe recipe = processingRecipes.Find(r => r.processedType == type);
        if (recipe != null)
        {
            if (!processedWaiting.ContainsKey(type))
                processedWaiting[type] = 0;
            processedWaiting[type]++;

            IngredientDisplayArea area = GetAreaForStation(recipe.station);
            area.AddOne(type, GetVisualPrefab(type));
        }
    }

    private void TryBeginDrag(IngredientType type, GameObject clickedVisual, IngredientDisplayArea sourceArea)
    {
        if (!sourceArea.TryPickUp(type, clickedVisual)) return;

        draggedType = type;
        draggedSourceArea = sourceArea;
        dragPlaneHeight = clickedVisual.transform.position.y;
        draggedVisual = clickedVisual;
        draggedVisual.transform.SetParent(null);
        dragStartScreenPos = Mouse.current.position.ReadValue();

        if (draggedVisual.TryGetComponent(out Collider col))
            col.enabled = false;

        if (draggedVisual.TryGetComponent(out Rigidbody rb))
            rb.isKinematic = true;
    }
    private void UpdateDragVisualPosition()
    {
        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        Plane dragPlane = new Plane(Vector3.up, new Vector3(0f, dragPlaneHeight, 0f));

        if (dragPlane.Raycast(ray, out float distance))
            draggedVisual.transform.position = ray.GetPoint(distance);
    }

    private void EndDrag()
    {
        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        bool didHit = Physics.Raycast(ray, out RaycastHit hit, 100f, craftingLayer);
        GameObject hitObject = didHit ? hit.collider.gameObject : null;

        bool isProcessedItem = draggedSourceArea == mortarProcessedDisplay || draggedSourceArea == cuttingBoardProcessedDisplay;

        ProcessingStation? station = null;
        if (hitObject == mortarClickObject) station = ProcessingStation.Mortar;
        else if (hitObject == cuttingBoardClickObject) station = ProcessingStation.CuttingBoard;

        float movedDistance = Vector2.Distance(Mouse.current.position.ReadValue(), dragStartScreenPos);
        bool wasClick = movedDistance < clickMovementThreshold;

        Destroy(draggedVisual);
        draggedVisual = null;

        if (station.HasValue && !isProcessedItem)
        {
            ProcessingRecipe recipe = processingRecipes.Find(r => r.rawType == draggedType && r.station == station.Value);

            if (recipe != null)
                ProcessIngredient(draggedType, station.Value);
            else
                ReturnToSourceShelf();

            return;
        }

        if (wasClick)
        {
            if (isProcessedItem)
                ConsumeProcessedForCauldron(draggedType);
            else
                TryAddIngredient(draggedType);
        }
        else
        {
            ReturnToSourceShelf();
        }
    }

    private void ConsumeProcessedForCauldron(IngredientType type)
    {
        if (processedWaiting.TryGetValue(type, out int count) && count > 0)
            processedWaiting[type] = count - 1;

        AddIngredientToCauldron(type);
    }

    private void ReturnToSourceShelf()
    {
        if (draggedSourceArea == null) return;
        draggedSourceArea.AddOne(draggedType, GetVisualPrefab(draggedType));
    }

    private void ProcessIngredient(IngredientType rawType, ProcessingStation station)
    {
        //AkUnitySoundEngine.PostEvent("Play_STG_Cut", gameObject);
        if (station == ProcessingStation.CuttingBoard)
        {
            playSTGCut.Post(gameObject);
        } else
        {
            playSTGMort.Post(gameObject);
        }


        ProcessingRecipe recipe = processingRecipes.Find(r => r.rawType == rawType && r.station == station);
        IngredientType resultType = recipe.processedType;

        if (!HomeStorage.Instance.RemoveOne(rawType)) return;

        HomeStorage.Instance.Save();

        if (!processedWaiting.ContainsKey(resultType))
            processedWaiting[resultType] = 0;
        processedWaiting[resultType]++;

        IngredientDisplayArea area = GetAreaForStation(station);
        area.AddOne(resultType, GetVisualPrefab(resultType));
    }

    private IngredientDisplayArea GetAreaForStation(ProcessingStation station)
    {
        return station == ProcessingStation.Mortar ? mortarProcessedDisplay : cuttingBoardProcessedDisplay;
    }

    private void TryAddIngredient(IngredientType type)
    {
        if (!HomeStorage.Instance.RemoveOne(type)) return;
        HomeStorage.Instance.Save();
        AddIngredientToCauldron(type);
    }

    private void AddIngredientToCauldron(IngredientType type)
    {
        GameObject prefab = GetVisualPrefab(type);
        if (prefab == null)
        {
            Debug.LogWarning($"[CauldronCraftingSystem] Falta asignar el prefab visual para {type.displayName}");
            return;
        }

        Vector3 spawnPos = cauldronDropPoint.position + new Vector3(UnityEngine.Random.Range(-0.2f, 0.2f), 0.3f, UnityEngine.Random.Range(-0.2f, 0.2f));
        GameObject visual = Instantiate(prefab, spawnPos, UnityEngine.Random.rotation);
        visual.AddComponent<CauldronItemVisual>().Init(type);
        cauldronContents.Add(visual);

        if (!cauldronIngredients.ContainsKey(type))
            cauldronIngredients[type] = 0;
        cauldronIngredients[type]++;
    }

    private GameObject GetVisualPrefab(IngredientType type)
    {
        if (type == null) return null;

        foreach (TypeVisualPrefab entry in visualPrefabs)
        {
            if (entry.type != null && entry.type.ingredientId == type.ingredientId)
                return entry.prefab;
        }

        return null;
    }

    private void Craft()
    {
        if (isShowcasing) return;

        PotionRecipe matchedRecipe = FindMatchingRecipe();

        foreach (GameObject item in cauldronContents)
            Destroy(item);

        cauldronContents.Clear();
        cauldronIngredients.Clear();

        if (matchedRecipe != null)
            StartCoroutine(ShowcasePotionRoutine(matchedRecipe));
        else
            DialogueUI.Instance.ShowMessage("Ofelia", "Ninguna receta coincide con los ingredientes en el caldero");
    }

    private System.Collections.IEnumerator ShowcasePotionRoutine(PotionRecipe recipe)
    {
        isShowcasing = true;

        GameObject showcaseVisual = null;
        if (recipe.visualPrefab != null && potionShowcaseAnchor != null)
            showcaseVisual = Instantiate(recipe.visualPrefab, potionShowcaseAnchor.position, potionShowcaseAnchor.rotation);

        DialogueUI.Instance.ShowMessage("Ofelia", $"Creaste: {recipe.potionName}");

        float elapsed = 0f;
        Quaternion baseRotation = potionShowcaseAnchor != null ? potionShowcaseAnchor.rotation : Quaternion.identity;

        while (elapsed < showcaseDuration)
        {
            elapsed += Time.deltaTime;

            if (showcaseVisual != null)
            {
                float angle = Mathf.Sin(elapsed * swaySpeed) * swayAngle;
                showcaseVisual.transform.rotation = baseRotation * Quaternion.Euler(0f, 0f, angle);
            }

            yield return null;
        }

        if (showcaseVisual != null)
        {
            float fadeDuration = 0.4f;
            Vector3 startScale = showcaseVisual.transform.localScale;
            float fadeElapsed = 0f;

            while (fadeElapsed < fadeDuration)
            {
                fadeElapsed += Time.deltaTime;
                showcaseVisual.transform.localScale = Vector3.Lerp(startScale, Vector3.zero, fadeElapsed / fadeDuration);
                yield return null;
            }

            Destroy(showcaseVisual);
        }

        HomeStorage.Instance.AddPotion(recipe);
        HomeStorage.Instance.Save();
        potionBoxDisplay.AddOne(recipe, recipe.visualPrefab);
        SetPlayerVisible(false);
        OnPotionCrafted?.Invoke();

        isShowcasing = false;
    }

    private PotionRecipe FindMatchingRecipe()
    {
        foreach (PotionRecipe recipe in recipeDatabase.Recipes)
            if (Matches(recipe)) return recipe;

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