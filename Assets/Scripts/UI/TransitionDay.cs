using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Localization.Settings;

public class TransitionDay : MonoBehaviour
{
    public enum TransitionType
    {
        Automatic,
        EndOfDay,
        Sleep
    }

    [Header("Tipo de transición")]
    [SerializeField] private TransitionType transitionType = TransitionType.Automatic;

    [Header("Transición")]
    [SerializeField] private float fadeDuration = 0.8f;
    [SerializeField] private float textDuration = 2f;

    [Header("Trigger")]
    [SerializeField] private bool triggerOnlyOnce = true;

    private UIDocument uiDocument;

    private VisualElement transitionPanel;
    private Label dayTitle;
    private Label daySubtitle;

    private Coroutine transitionCoroutine;

    private float previousTimeScale;
    private bool hasTriggered;

    private void Awake()
    {
        uiDocument = GetComponent<UIDocument>();

        VisualElement root = uiDocument.rootVisualElement;

        transitionPanel = root.Q<VisualElement>("TransitionDayPanel");
        dayTitle = root.Q<Label>("DayTransitionTitle");
        daySubtitle = root.Q<Label>("DayTransitionSubtitle");

        transitionPanel.style.display = DisplayStyle.None;
        transitionPanel.style.opacity = 0f;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (triggerOnlyOnce && hasTriggered)
            return;

        GameProgressManager progress = GameProgressManager.Instance;

        if (progress == null)
            return;

        TransitionType selectedTransition = transitionType;

        if (selectedTransition == TransitionType.Automatic)
        {
            selectedTransition = progress.IsNightActive
                ? TransitionType.Sleep
                : TransitionType.EndOfDay;
        }

        hasTriggered = true;

        if (transitionCoroutine != null)
            StopCoroutine(transitionCoroutine);

        transitionCoroutine = StartCoroutine(
            PlayTransition(selectedTransition, progress.CurrentDay)
        );
    }

    private IEnumerator PlayTransition(TransitionType type, int day)
    {
        PauseGame();

        string language = LocalizationSettings.SelectedLocale.Identifier.Code;

        if (type == TransitionType.EndOfDay)
        {
            if (language == "es")
            {
                dayTitle.text = "Fin del día";
                daySubtitle.text = "La noche comienza...";
            }
            else
            {
                dayTitle.text = "End of the day";
                daySubtitle.text = "The night begins...";
            }
        }
        else if (type == TransitionType.Sleep)
        {
            if (language == "es")
            {
                dayTitle.text = "Día " + (day + 1);
                daySubtitle.text = "Una nueva noche comienza...";
            }
            else
            {
                dayTitle.text = "Day " + (day + 1);
                daySubtitle.text = "A new night begins...";
            }
        }

        transitionPanel.style.display = DisplayStyle.Flex;
        transitionPanel.style.opacity = 0f;

        yield return Fade(0f, 1f);

        yield return new WaitForSecondsRealtime(textDuration);

        yield return Fade(1f, 0f);

        transitionPanel.style.display = DisplayStyle.None;

        ResumeGame();

        transitionCoroutine = null;
    }

    private void PauseGame()
    {
        previousTimeScale = Time.timeScale;
        Time.timeScale = 0f;
    }

    private void ResumeGame()
    {
        Time.timeScale = previousTimeScale;
    }

    private IEnumerator Fade(float from, float to)
    {
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;

            float t = Mathf.Clamp01(elapsed / fadeDuration);

            transitionPanel.style.opacity = Mathf.Lerp(from, to, t);

            yield return null;
        }

        transitionPanel.style.opacity = to;
    }
}