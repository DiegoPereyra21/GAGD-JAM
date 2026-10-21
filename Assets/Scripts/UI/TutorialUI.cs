using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class TutorialUI : MonoBehaviour
{
    public static TutorialUI Instance { get; private set; }

    // Colores del resaltado dentro del texto
    private const string PlaceColor = "#FFD54A"; // objetos y lugares (caldero, buzón...)
    private const string KeyColor = "#8FE3FF";   // teclas y acciones (E, TAB, click...)
    private const string ItemColor = "#9CF07A";  // ingredientes y pociones

    // Palabras que se resaltan solas en cualquier mensaje
    private static readonly Regex PlaceRegex = new Regex(
        @"\b(tabla de picar|diario de objetivos|caldero|cuchara|mortero|buzón|puerta|mostrador|canasto|escaleras?|cama|pedidos?)\b",
        RegexOptions.IgnoreCase);

    private static readonly Regex KeyRegex = new Regex(@"\b(WASD|TAB|A y D|E|click)\b");

    [Header("Animación al cambiar de mensaje")]
    [SerializeField] private float pulseDuration = 0.5f;
    [SerializeField] private float pulseScale = 1.06f;
    [SerializeField] private Color pulseBorderColor = new Color(1f, 0.84f, 0.29f, 1f);
    [SerializeField] private float borderWidth = 4f;

    private VisualElement root;
    private Label nameLabel;
    private Label textLabel;
    private Button nextButton;

    private readonly Queue<string> messageQueue = new Queue<string>();
    private float pulseElapsed = -1f;

    private void Awake()
    {
        Instance = this;

        VisualElement uiRoot = GetComponent<UIDocument>().rootVisualElement;
        root = uiRoot.Q<VisualElement>("TutorialPanel");
        nameLabel = root.Q<Label>("TutorialName");
        textLabel = root.Q<Label>("TutorialText");
        nextButton = root.Q<Button>("NextButton");

        textLabel.enableRichText = true;
        SetBorder(borderWidth, Color.clear);

        if (nextButton != null)
            nextButton.clicked += AdvanceQueue;

        Hide();
    }

    private void Update()
    {
        if (pulseElapsed < 0f) return;

        pulseElapsed += Time.unscaledDeltaTime;
        float t = Mathf.Clamp01(pulseElapsed / pulseDuration);

        float scale = Mathf.SmoothStep(pulseScale, 1f, t);
        root.style.scale = new Scale(new Vector3(scale, scale, 1f));

        Color border = pulseBorderColor;
        border.a = 1f - t;
        SetBorder(borderWidth, border);

        if (t >= 1f)
            pulseElapsed = -1f;
    }

    // Un solo mensaje fijo, sin botón de avanzar
    public void Show(string title, string text)
    {
        messageQueue.Clear();
        SetNextButtonVisible(false);

        nameLabel.text = title;
        SetText(text);
        root.style.display = DisplayStyle.Flex;
    }

    // Varios mensajes seguidos, avanzando con el botón "Siguiente"
    public void ShowSequence(string title, params string[] messages)
    {
        messageQueue.Clear();
        foreach (string msg in messages)
            messageQueue.Enqueue(msg);

        nameLabel.text = title;
        root.style.display = DisplayStyle.Flex;
        ShowNextInQueue();
    }

    public void Hide()
    {
        root.style.display = DisplayStyle.None;
    }

    // Para resaltar a mano el nombre de un ingrediente o poción dentro de un mensaje
    public static string Item(string name) => $"<color={ItemColor}><b>{name}</b></color>";

    public static string Highlight(string text)
    {
        if (string.IsNullOrEmpty(text)) return text;

        text = KeyRegex.Replace(text, m => $"<color={KeyColor}><b>{m.Value}</b></color>");
        text = PlaceRegex.Replace(text, m => $"<color={PlaceColor}><b>{m.Value}</b></color>");
        return text;
    }

    private void SetText(string text)
    {
        string formatted = Highlight(text);
        if (textLabel.text == formatted) return;

        textLabel.text = formatted;
        pulseElapsed = 0f;
    }

    private void ShowNextInQueue()
    {
        SetText(messageQueue.Dequeue());
        SetNextButtonVisible(messageQueue.Count > 0);
    }

    private void AdvanceQueue()
    {
        if (messageQueue.Count > 0)
            ShowNextInQueue();
    }

    private void SetNextButtonVisible(bool visible)
    {
        if (nextButton != null)
            nextButton.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
    }

    private void SetBorder(float width, Color color)
    {
        root.style.borderTopWidth = width;
        root.style.borderBottomWidth = width;
        root.style.borderLeftWidth = width;
        root.style.borderRightWidth = width;

        root.style.borderTopColor = color;
        root.style.borderBottomColor = color;
        root.style.borderLeftColor = color;
        root.style.borderRightColor = color;
    }
}
