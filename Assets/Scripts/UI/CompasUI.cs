using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class CompassUI : MonoBehaviour
{
    [Header("Jugador")]
    [SerializeField] private Transform playerTransform;

    [Header("Configuración")]
    [SerializeField] private float degreesVisibleEachSide = 90f;

    // Norte definido según tu antiguo NorthReference
    private const float NORTH_ROTATION_Y = -42.68f;

    private UIDocument uiDocument;

    private VisualElement compassViewport;
    private VisualElement compassContent;

    private DirectionElement[] directions;

    private bool initialized = false;


    private class DirectionElement
    {
        public VisualElement element;
        public float angle;

        public DirectionElement(VisualElement element, float angle)
        {
            this.element = element;
            this.angle = angle;
        }
    }


    private void Awake()
    {
        // Hace que el Compass sobreviva al cambiar de escena
        DontDestroyOnLoad(gameObject);

        uiDocument = GetComponent<UIDocument>();
    }


    private void OnEnable()
    {
        StartCoroutine(InitializeCompass());
    }


    private IEnumerator InitializeCompass()
    {
        // Esperamos un frame para que UI Toolkit construya la interfaz
        yield return null;

        if (initialized)
            yield break;

        if (uiDocument == null)
        {
            Debug.LogError("CompassUI: No se encontró UIDocument.");
            yield break;
        }

        VisualElement root = uiDocument.rootVisualElement;

        if (root == null)
        {
            Debug.LogError("CompassUI: rootVisualElement es null.");
            yield break;
        }


        compassViewport = root.Q<VisualElement>("CompassViewport");

        if (compassViewport == null)
        {
            Debug.LogError("CompassUI: No encontré CompassViewport.");
            yield break;
        }


        compassContent = root.Q<VisualElement>("CompassContent");

        if (compassContent == null)
        {
            Debug.LogError("CompassUI: No encontré CompassContent.");
            yield break;
        }


        directions = new DirectionElement[]
        {
            new DirectionElement(root.Q<VisualElement>("Direction_N"), 0f),
            new DirectionElement(root.Q<VisualElement>("Direction_NE"), 45f),
            new DirectionElement(root.Q<VisualElement>("Direction_E"), 90f),
            new DirectionElement(root.Q<VisualElement>("Direction_SE"), 135f),
            new DirectionElement(root.Q<VisualElement>("Direction_S"), 180f),
            new DirectionElement(root.Q<VisualElement>("Direction_SW"), 225f),
            new DirectionElement(root.Q<VisualElement>("Direction_W"), 270f),
            new DirectionElement(root.Q<VisualElement>("Direction_NW"), 315f)
        };

        initialized = true;

        Debug.Log("🧭 Compass inicializada correctamente.");
    }


    private void Update()
    {
        if (!initialized)
            return;

        // Si todavía no existe el jugador, intentamos encontrarlo
        if (playerTransform == null)
        {
            FindPlayer();

            // Todavía no apareció el jugador
            if (playerTransform == null)
                return;
        }


        if (compassViewport == null || directions == null)
            return;


        float width = compassViewport.resolvedStyle.width;
        float height = compassViewport.resolvedStyle.height;

        if (width <= 0 || height <= 0)
            return;


        // Dirección del Norte usando la antigua rotación
        Vector3 northDirection =
            Quaternion.Euler(0f, NORTH_ROTATION_Y, 0f) * Vector3.forward;


        // Dirección horizontal del jugador
        Vector3 playerForward = playerTransform.forward;
        playerForward.y = 0f;

        if (playerForward.sqrMagnitude < 0.001f)
            return;

        playerForward.Normalize();


        // Ángulo del jugador respecto al Norte
        float playerAngle = Vector3.SignedAngle(
            northDirection,
            playerForward,
            Vector3.up
        );

        playerAngle = Mathf.Repeat(playerAngle, 360f);


        // Actualizamos cada dirección
        foreach (DirectionElement direction in directions)
        {
            if (direction.element == null)
                continue;


            float difference = Mathf.DeltaAngle(
                playerAngle,
                direction.angle
            );


            // ¿Está dentro del rango visible?
            bool visible =
                Mathf.Abs(difference) <= degreesVisibleEachSide;


            direction.element.style.display =
                visible
                    ? DisplayStyle.Flex
                    : DisplayStyle.None;


            if (!visible)
                continue;


            // Convertimos la diferencia angular
            // en posición horizontal
            float normalized =
                difference / degreesVisibleEachSide;


            float xPosition =
                width * 0.5f +
                normalized * width * 0.5f;


            direction.element.style.left = xPosition;


            // Centramos horizontalmente la letra
            direction.element.style.translate =
                new Translate(
                    new Length(-50, LengthUnit.Percent),
                    new Length(-50, LengthUnit.Percent)
                );


            // Centro vertical
            direction.element.style.top = height * 0.5f;
        }
    }


    private void FindPlayer()
    {
        GameObject player =
            GameObject.FindGameObjectWithTag("Player");

        if (player != null)
        {
            playerTransform = player.transform;

            Debug.Log("🧭 Compass: Player encontrado.");
        }
    }
}