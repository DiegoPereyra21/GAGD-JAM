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
    [SerializeField] private string gameSceneName = "Level 1";

    private VisualElement root;
    private Button newGameButton;
    private Button continueButton;
    private Button optionsButton;
    private Button backButton;
    private VisualElement optionsPanel;
    private VisualElement content;

    private VisualElement confirmNewGamePanel;
    private Button confirmNewGameButton;
    private Button cancelNewGameButton;

    private void OnEnable()
    {
        root = GetComponent<UIDocument>().rootVisualElement;

        root.Query<Button>().ForEach(button =>
        {
            button.RegisterCallback<MouseEnterEvent>(OnButtonHover);
        });

        musicEvent.Post(gameObject);

        content = root.Q<VisualElement>("Content");
        newGameButton = root.Q<Button>("NewGameButton");
        continueButton = root.Q<Button>("ContinueButton");
        optionsButton = root.Q<Button>("OptionsButton");
        backButton = root.Q<Button>("BackButton");
        optionsPanel = root.Q<VisualElement>("OptionsPanel");

        confirmNewGamePanel = root.Q<VisualElement>("ConfirmNewGamePanel");
        confirmNewGameButton = root.Q<Button>("ConfirmNewGameButton");
        cancelNewGameButton = root.Q<Button>("CancelNewGameButton");

        if (newGameButton != null)
            newGameButton.clicked += OpenConfirmNewGame;

        if (confirmNewGameButton != null)
            confirmNewGameButton.clicked += StartNewGame;

        if (cancelNewGameButton != null)
            cancelNewGameButton.clicked += CloseConfirmNewGame;

        if (continueButton != null)
        {
            continueButton.clicked += ContinueGame;
            continueButton.SetEnabled(GameProgressManager.HasSaveData);
        }

        if (optionsButton != null)
            optionsButton.clicked += OpenOptions;

        if (backButton != null)
            backButton.clicked += CloseOptions;

        optionsPanel.style.display = DisplayStyle.None;
        confirmNewGamePanel.style.display = DisplayStyle.None;
    }

    private void OnDisable()
    {
        if (root == null)
            return;

        root.Query<Button>().ForEach(button =>
        {
            button.UnregisterCallback<MouseEnterEvent>(OnButtonHover);
        });

        if (newGameButton != null)
            newGameButton.clicked -= OpenConfirmNewGame;

        if (confirmNewGameButton != null)
            confirmNewGameButton.clicked -= StartNewGame;

        if (cancelNewGameButton != null)
            cancelNewGameButton.clicked -= CloseConfirmNewGame;

        if (continueButton != null)
            continueButton.clicked -= ContinueGame;

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

    private void OpenConfirmNewGame()
    {
        content.style.display = DisplayStyle.None;
        confirmNewGamePanel.style.display = DisplayStyle.Flex;
    }

    private void CloseConfirmNewGame()
    {
        confirmNewGamePanel.style.display = DisplayStyle.None;
        content.style.display = DisplayStyle.Flex;
    }

    private void StartNewGame()
    {
        stopMusicEvent.Post(gameObject);

        PlayerPrefs.DeleteAll();
        HomeStorage.Instance.Load();
        GameProgressManager.Instance.ResetForNewGame();

        SceneManager.LoadScene(gameSceneName);
    }

    private void ContinueGame()
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