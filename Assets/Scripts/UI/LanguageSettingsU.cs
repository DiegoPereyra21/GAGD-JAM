using UnityEngine;
using UnityEngine.UIElements;

public class LanguageSettingsUI : MonoBehaviour
{
    [SerializeField] private UIDocument uiDocument;

    private Toggle spanishToggle;
    private Toggle englishToggle;

    private const string LanguageKey = "Language";
    private const string Spanish = "es";
    private const string English = "en";

    private void OnEnable()
    {
        VisualElement root = uiDocument.rootVisualElement;

        spanishToggle = root.Q<Toggle>("SpanishToggle");
        englishToggle = root.Q<Toggle>("EnglishToggle");

        LoadLanguage();

        spanishToggle.RegisterValueChangedCallback(OnSpanishChanged);
        englishToggle.RegisterValueChangedCallback(OnEnglishChanged);
    }

    private void OnDisable()
    {
        if (spanishToggle != null)
            spanishToggle.UnregisterValueChangedCallback(OnSpanishChanged);

        if (englishToggle != null)
            englishToggle.UnregisterValueChangedCallback(OnEnglishChanged);
    }

    private void LoadLanguage()
    {
        string language = PlayerPrefs.GetString(LanguageKey, Spanish);

        spanishToggle.SetValueWithoutNotify(language == Spanish);
        englishToggle.SetValueWithoutNotify(language == English);
    }

    private void OnSpanishChanged(ChangeEvent<bool> evt)
    {
        if (!evt.newValue)
        {
            spanishToggle.SetValueWithoutNotify(true);
            return;
        }

        englishToggle.SetValueWithoutNotify(false);

        SaveLanguage(Spanish);
    }

    private void OnEnglishChanged(ChangeEvent<bool> evt)
    {
        if (!evt.newValue)
        {
            englishToggle.SetValueWithoutNotify(true);
            return;
        }

        spanishToggle.SetValueWithoutNotify(false);

        SaveLanguage(English);
    }

    private void SaveLanguage(string language)
    {
        PlayerPrefs.SetString(LanguageKey, language);
        PlayerPrefs.Save();
    }

    public static string GetLanguage()
    {
        return PlayerPrefs.GetString(LanguageKey, Spanish);
    }
}