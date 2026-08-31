using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

public class LanguageManager : MonoBehaviour
{
    public static LanguageManager Instance { get; private set; }

    [Header("UI")]
    [SerializeField] private UIDocument uiDocument;

    [Header("String Table")]
    [SerializeField] private string tableName = "UI";

    private Toggle spanishToggle;
    private Toggle englishToggle;

    private bool changingLanguage;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        LocalizationSettings.SelectedLocaleChanged += OnLocaleChanged;
    }

    private void Start()
    {
        StartCoroutine(InitializeLocalization());
    }

    private void OnDisable()
    {
        LocalizationSettings.SelectedLocaleChanged -= OnLocaleChanged;
    }

    private IEnumerator InitializeLocalization()
    {
        yield return LocalizationSettings.InitializationOperation;

        RegisterLanguageButtons();
        ApplyCurrentLanguage();
    }

    private void OnLocaleChanged(Locale locale)
    {
        if (uiDocument == null)
            return;

        ApplyCurrentLanguage();
    }

    public void SetSpanish()
    {
        SetLanguage("es");
    }

    public void SetEnglish()
    {
        SetLanguage("en");
    }

    public void SetLanguage(string localeCode)
    {
        Locale locale = LocalizationSettings.AvailableLocales.GetLocale(
            new LocaleIdentifier(localeCode)
        );

        if (locale == null)
        {
            Debug.LogWarning($"No se encontró el locale: {localeCode}");
            return;
        }

        LocalizationSettings.SelectedLocale = locale;
    }

    private void RegisterLanguageButtons()
    {
        if (uiDocument == null)
        {
            Debug.LogWarning("LanguageManager: No hay UIDocument asignado.");
            return;
        }

        VisualElement root = uiDocument.rootVisualElement;

        spanishToggle = root.Q<Toggle>("SpanishToggle");
        englishToggle = root.Q<Toggle>("EnglishToggle");

        if (spanishToggle != null)
        {
            spanishToggle.RegisterValueChangedCallback(evt =>
            {
                if (evt.newValue && !changingLanguage)
                {
                    SetSpanish();
                }
            });
        }

        if (englishToggle != null)
        {
            englishToggle.RegisterValueChangedCallback(evt =>
            {
                if (evt.newValue && !changingLanguage)
                {
                    SetEnglish();
                }
            });
        }
    }

    private void ApplyCurrentLanguage()
    {
        if (uiDocument == null)
            return;

        VisualElement root = uiDocument.rootVisualElement;

        ApplyText(root, "NewGameButton", "menu.play");
        ApplyText(root, "ContinueButton", "menu.continue");
        ApplyText(root, "OptionsButton", "menu.options");
        ApplyText(root, "ExitButton", "menu.exit");

        ApplyText(root, "OptionsTitle", "options.title");
        ApplyText(root, "LanguageLabel", "options.language");

        ApplyText(root, "AudioLabel", "options.audio");
        ApplyText(root, "MusicLabel", "options.music");
        ApplyText(root, "SFXLabel", "options.sfx");
        ApplyText(root, "AmbientLabel", "options.ambience");

        ApplyText(root, "BackButton", "options.back");

        ApplyText(root, "ConfirmNewGame", "menu.confirm.text");
        ApplyText(root, "ConfirmNewGameButton", "menu.confirm.yes");
        ApplyText(root, "CancelNewGameButton", "menu.confirm.no");

        UpdateLanguageToggles();
    }

    private void ApplyText(
        VisualElement root,
        string elementName,
        string tableKey
    )
    {
        VisualElement element = root.Q<VisualElement>(elementName);

        if (element == null)
        {
            Debug.LogWarning(
                $"LanguageManager: No se encontró '{elementName}'."
            );

            return;
        }

        LocalizedString localizedString = new LocalizedString(
            tableName,
            tableKey
        );

        localizedString.GetLocalizedStringAsync().Completed += operation =>
        {
            if (element == null)
                return;

            string translatedText = operation.Result;

            if (element is Button button)
            {
                button.text = translatedText;
            }
            else if (element is Label label)
            {
                label.text = translatedText;
            }
            else if (element is Toggle toggle)
            {
                toggle.text = translatedText;
            }
        };
    }

    private void UpdateLanguageToggles()
    {
        if (spanishToggle == null || englishToggle == null)
            return;

        Locale currentLocale = LocalizationSettings.SelectedLocale;

        if (currentLocale == null)
            return;

        string code = currentLocale.Identifier.Code;

        changingLanguage = true;

        spanishToggle.SetValueWithoutNotify(code == "es");
        englishToggle.SetValueWithoutNotify(code == "en");

        changingLanguage = false;
    }
}