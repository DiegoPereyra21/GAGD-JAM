using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using Unity.Cinemachine;

[RequireComponent(typeof(UIDocument))]
public class PauseMenu : MonoBehaviour
{
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private string mainMenuSceneName = "MainMenu";
    [SerializeField] private CinemachineBrain cinemachineBrain;

    private VisualElement root;
    private VisualElement content;
    private VisualElement optionsPanel;

    private InputAction pauseAction;
    private bool isPaused;
    private float previousTimeScale;

    private void Awake()
    {
        pauseAction = playerInput.actions["Pause"];

        VisualElement uiRoot = GetComponent<UIDocument>().rootVisualElement;

        root = uiRoot.Q<VisualElement>("PauseMenuRoot");
        content = uiRoot.Q<VisualElement>("Content");
        optionsPanel = uiRoot.Q<VisualElement>("OptionsPanel");

        uiRoot.Q<Button>("ResumeButton").clicked += Resume;
        uiRoot.Q<Button>("OptionsButton").clicked += OpenOptions;
        uiRoot.Q<Button>("BackButton").clicked += CloseOptions;
        uiRoot.Q<Button>("MainMenuButton").clicked += GoToMainMenu;
        uiRoot.Q<Button>("ExitButton").clicked += ExitGame;

        root.style.display = DisplayStyle.None;
        optionsPanel.style.display = DisplayStyle.None;
        //para q quite el boton de salir del game en caso de q este en itchio
        Button exitButton = uiRoot.Q<Button>("ExitButton");
        exitButton.clicked += ExitGame;

        #if UNITY_WEBGL && !UNITY_EDITOR
                exitButton.SetEnabled(false);
        #endif
    }

    private void OnEnable() => pauseAction.performed += OnPausePressed;
    private void OnDisable() => pauseAction.performed -= OnPausePressed;

    private void OnPausePressed(InputAction.CallbackContext ctx)
    {
        if (isPaused) Resume();
        else Pause();
    }
    
    private void Pause()
    {
        isPaused = true;
        previousTimeScale = Time.timeScale;
        Time.timeScale = 0f;

        UnityEngine.Cursor.lockState = CursorLockMode.None;
        UnityEngine.Cursor.visible = true;

        root.style.display = DisplayStyle.Flex;
        content.style.display = DisplayStyle.Flex;
        optionsPanel.style.display = DisplayStyle.None;
    }

    private void Resume()
    {
        isPaused = false;
        Time.timeScale = previousTimeScale;
        root.style.display = DisplayStyle.None;

        if (cinemachineBrain != null && cinemachineBrain.enabled)
        {
            UnityEngine.Cursor.lockState = CursorLockMode.Locked;
            UnityEngine.Cursor.visible = false;
        }
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

    private void GoToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
    }

    private void ExitGame()
    {
        Application.Quit();
    }
}