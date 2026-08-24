using UnityEngine;
using UnityEngine.UIElements;
using AK.Wwise;

public class MainMenu : MonoBehaviour
{
    [Header("Evento de musica del menu")]
    [SerializeField] private AK.Wwise.Event musicEvent;

    [Header("Evento para detener la musica")]
    [SerializeField] private AK.Wwise.Event stopMusicEvent;

    [Header("Evento para el hover")]
    [SerializeField] private AK.Wwise.Event buttonHoverEvent;

    private VisualElement root;
    private Button optionsButton;
    private Button backButton;
    private VisualElement optionsPanel;
    private VisualElement content;

    private void OnEnable()
    {
        root = GetComponent<UIDocument>().rootVisualElement;

        root.Query<Button>().ForEach(button =>
        {
            button.RegisterCallback<MouseEnterEvent>(OnButtonHover);
        });

        musicEvent.Post(gameObject);

        content = root.Q<VisualElement>("Content");
        optionsButton = root.Q<Button>("OptionsButton");
        backButton = root.Q<Button>("BackButton");
        optionsPanel = root.Q<VisualElement>("OptionsPanel");

        optionsButton.clicked += OpenOptions;
        backButton.clicked += CloseOptions;

        optionsPanel.style.display = DisplayStyle.None;
    }

    private void OnDisable()
    {
        if (root == null)
            return;

        root.Query<Button>().ForEach(button =>
        {
            button.UnregisterCallback<MouseEnterEvent>(OnButtonHover);
        });

        stopMusicEvent.Post(gameObject);

        optionsButton.clicked -= OpenOptions;
        backButton.clicked -= CloseOptions;
    }

    private void OnButtonHover(MouseEnterEvent evt)
    {
        buttonHoverEvent.Post(gameObject);
    }

    private void OpenOptions()
    {
        content.style.display = DisplayStyle.None;
        optionsPanel.style.display = DisplayStyle.Flex;
    }

    private void CloseOptions()
    {
        optionsPanel.style.display = DisplayStyle.None;
        content.style.display = DisplayStyle.Flex;
    }
}