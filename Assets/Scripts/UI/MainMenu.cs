using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using AK.Wwise;

public class MainMenu : MonoBehaviour
{
    [Header("Evento de musica del menu")]
    [SerializeField] private AK.Wwise.Event musicEvent;

    [Header("Evento para detener la musica")]
    [SerializeField] private AK.Wwise.Event stopMusicEvent;

    [Header("Evento para el hover")]
    [SerializeField] private AK.Wwise.Event buttonHoverEvent;

    [Header("Primer Nivel")]
    [SerializeField] private string gameSceneName = "Game";

    private VisualElement root;
    private Button playButton;
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
        playButton = root.Q<Button>("PlayButton");
        optionsButton = root.Q<Button>("OptionsButton");
        backButton = root.Q<Button>("BackButton");
        optionsPanel = root.Q<VisualElement>("OptionsPanel");

        if (playButton != null)
            playButton.clicked += PlayGame;

        if (optionsButton != null)
            optionsButton.clicked += OpenOptions;

        if (backButton != null)
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

        if (playButton != null)
            playButton.clicked -= PlayGame;

        if (optionsButton != null)
            optionsButton.clicked -= OpenOptions;

        if (backButton != null)
            backButton.clicked -= CloseOptions;

        stopMusicEvent.Post(gameObject);
    }

    private void OnButtonHover(MouseEnterEvent evt)
    {
        buttonHoverEvent.Post(gameObject);
    }

    private void PlayGame()
    {
        stopMusicEvent.Post(gameObject);
        SceneManager.LoadScene(gameSceneName);
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