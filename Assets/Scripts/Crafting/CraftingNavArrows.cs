using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

// Flechas en pantalla para cambiar de mesa en el crafteo. Las crea sola CraftingCameraRail.
public class CraftingNavArrows : MonoBehaviour
{
    // Mientras el mouse está sobre una flecha, CraftingPointer ignora el click del mundo.
    public static bool PointerOverUI { get; private set; }

    private static readonly Color Normal = new Color(0.08f, 0.05f, 0.12f, 0.75f);
    private static readonly Color Hover = new Color(0.25f, 0.17f, 0.35f, 0.92f);
    private static readonly Color Gold = new Color(1f, 0.84f, 0.29f, 1f);

    private CraftingCameraRail rail;
    private WorkTable[] tables;
    private UIDocument document;
    private VisualElement container;
    private Button leftButton;
    private Button rightButton;
    private CraftingZone? hintTarget;
    private bool failed;

    private void Awake() => rail = GetComponent<CraftingCameraRail>();

    private void OnDisable() => PointerOverUI = false;

    // El tutorial lo usa para hacer titilar la flecha que lleva a esa mesa (null = nada).
    public void Hint(CraftingZone? target) => hintTarget = target;

    public CraftingZone ZoneOf(TableType table)
    {
        if (TableAt(CraftingZone.Left) != null && TableAt(CraftingZone.Left).Table == table)
            return CraftingZone.Left;

        return CraftingZone.Right;
    }

    public string LabelFor(CraftingZone zone)
    {
        if (zone == CraftingZone.Center) return "Caldero";

        WorkTable table = TableAt(zone);
        if (table == null) return zone == CraftingZone.Left ? "Izquierda" : "Derecha";

        return table.Table == TableType.Mortar ? "Mortero" : "Tabla de picar";
    }

    // La mesa más a la izquierda (según la cámara central) es la zona Left, la otra la Right.
    private WorkTable TableAt(CraftingZone zone)
    {
        if (tables == null)
            tables = FindObjectsByType<WorkTable>(FindObjectsSortMode.None);

        Transform reference = rail.GetAnchor(CraftingZone.Center);
        var sorted = tables
            .Where(t => t != null)
            .OrderBy(t => reference.InverseTransformPoint(t.transform.position).x)
            .ToList();

        if (sorted.Count == 0) return null;
        return zone == CraftingZone.Left ? sorted.First() : sorted.Last();
    }

    private void Update()
    {
        if (rail == null || failed) return;

        if (container == null)
        {
            TryBuild();
            return;
        }

        bool show = rail.IsActive;
        container.style.display = show ? DisplayStyle.Flex : DisplayStyle.None;

        if (!show)
        {
            PointerOverUI = false;
            return;
        }

        CraftingZone zone = rail.CurrentZone;
        bool hasLeft = zone != CraftingZone.Left;
        bool hasRight = zone != CraftingZone.Right;

        leftButton.style.display = hasLeft ? DisplayStyle.Flex : DisplayStyle.None;
        rightButton.style.display = hasRight ? DisplayStyle.Flex : DisplayStyle.None;

        if (hasLeft) leftButton.text = $"<  {LabelFor((CraftingZone)((int)zone - 1))}";
        if (hasRight) rightButton.text = $"{LabelFor((CraftingZone)((int)zone + 1))}  >";

        leftButton.SetEnabled(!rail.IsBusy);
        rightButton.SetEnabled(!rail.IsBusy);

        int hintDirection = hintTarget.HasValue
            ? Mathf.Clamp((int)hintTarget.Value - (int)zone, -1, 1)
            : 0;

        Pulse(leftButton, hintDirection < 0);
        Pulse(rightButton, hintDirection > 0);
    }

    private void TryBuild()
    {
        if (document == null)
        {
            // Reutiliza el PanelSettings de cualquier UI que ya exista en la escena
            PanelSettings settings = FindObjectsByType<UIDocument>(FindObjectsSortMode.None)
                .Select(d => d.panelSettings)
                .FirstOrDefault(p => p != null);

            if (settings == null)
            {
                failed = true; // sin UI en la escena: quedan las zonas de click originales
                return;
            }

            GameObject go = new GameObject("CraftingNavArrowsUI");
            go.transform.SetParent(transform, false);
            document = go.AddComponent<UIDocument>();
            document.panelSettings = settings;
            document.sortingOrder = 50;
        }

        VisualElement root = document.rootVisualElement;
        if (root == null) return; // se reintenta el próximo frame

        root.pickingMode = PickingMode.Ignore;

        container = new VisualElement { pickingMode = PickingMode.Ignore };
        container.style.position = Position.Absolute;
        container.style.left = 0;
        container.style.right = 0;
        container.style.top = 0;
        container.style.bottom = 0;
        container.style.display = DisplayStyle.None;

        leftButton = CreateButton(-1);
        rightButton = CreateButton(1);
        container.Add(leftButton);
        container.Add(rightButton);
        root.Add(container);

        rail.SetClickZonesActive(false); // las flechas reemplazan a las zonas invisibles
    }

    private Button CreateButton(int direction)
    {
        Button button = new Button(() => rail.Navigate(direction)) { focusable = false };

        button.style.position = Position.Absolute;
        button.style.top = Length.Percent(45);
        if (direction < 0) button.style.left = 24;
        else button.style.right = 24;

        button.style.paddingLeft = 22;
        button.style.paddingRight = 22;
        button.style.paddingTop = 14;
        button.style.paddingBottom = 14;
        button.style.fontSize = 26;
        button.style.unityFontStyleAndWeight = FontStyle.Bold;
        button.style.color = Color.white;
        button.style.backgroundColor = Normal;

        SetBorder(button, 3f, Gold);
        button.style.borderTopLeftRadius = 14;
        button.style.borderTopRightRadius = 14;
        button.style.borderBottomLeftRadius = 14;
        button.style.borderBottomRightRadius = 14;

        button.RegisterCallback<PointerEnterEvent>(_ =>
        {
            PointerOverUI = true;
            button.style.backgroundColor = Hover;
        });
        button.RegisterCallback<PointerLeaveEvent>(_ =>
        {
            PointerOverUI = false;
            button.style.backgroundColor = Normal;
        });

        return button;
    }

    private static void Pulse(Button button, bool on)
    {
        float scale = on ? 1f + 0.08f * (0.5f + 0.5f * Mathf.Sin(Time.unscaledTime * 6f)) : 1f;
        button.style.scale = new Scale(new Vector3(scale, scale, 1f));

        Color border = Gold;
        border.a = on ? 1f : 0.5f;
        SetBorder(button, on ? 4f : 3f, border);
    }

    private static void SetBorder(VisualElement element, float width, Color color)
    {
        element.style.borderTopWidth = width;
        element.style.borderBottomWidth = width;
        element.style.borderLeftWidth = width;
        element.style.borderRightWidth = width;
        element.style.borderTopColor = color;
        element.style.borderBottomColor = color;
        element.style.borderLeftColor = color;
        element.style.borderRightColor = color;
    }
}