using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Localization.Settings;
using System.Linq;

public class DayTransition : MonoBehaviour
{
    public enum TransitionType
    {
        Automatic,
        EndOfNight,
        EndOfDay
    }

    [SerializeField] private TransitionType transitionType = TransitionType.Automatic;
    [SerializeField] private float fadeDuration = 0.8f;
    [SerializeField] private float textDuration = 2f;
    [SerializeField] private bool triggerOnlyOnce = true;
    [SerializeField] private int sanityLossPerDay = 15;

    // manejo de menus
    [SerializeField] private UIDocument hudDocument;
    [SerializeField] private UIDocument questJournalDocument;
    [SerializeField] private bool useAutomaticTrigger = true;
    private Image[] icons;

    private UIDocument uiDocument;
    private VisualElement transitionPanel;
    private Label dayTitle;
    private Label daySubtitle;

    private Coroutine transitionCoroutine;
    private float previousTimeScale;
    private bool hasTriggered;

    private Label moneyValue;
    private Label inventoryValue;
    private Label sanityValue;

    private void Awake()
    {
        uiDocument = GetComponent<UIDocument>();

        VisualElement root = uiDocument.rootVisualElement;

        transitionPanel = root.Q<VisualElement>("TransitionDayPanel");
        dayTitle = root.Q<Label>("DayTransitionTitle");
        daySubtitle = root.Q<Label>("DayTransitionSubtitle");

        moneyValue = root.Q<Label>("MoneyValue");
        inventoryValue = root.Q<Label>("InventoryValue");
        sanityValue = root.Q<Label>("SanityValue");

        icons = root
            .Query<Image>(className: "transition-icon")
            .ToList()
            .ToArray();

        if (icons.Length >= 3)
        {
            icons[0].sprite = Resources.Load<Sprite>("Icons/1");
            icons[1].sprite = Resources.Load<Sprite>("Icons/2");
            icons[2].sprite = Resources.Load<Sprite>("Icons/3");
        }

        transitionPanel.style.display = DisplayStyle.None;
        transitionPanel.style.opacity = 0f;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!useAutomaticTrigger) return;

        if (!other.CompareTag("Player"))
            return;

        if (triggerOnlyOnce && hasTriggered)
            return;

        hasTriggered = true;

        GameProgressManager progress = GameProgressManager.Instance;
        TransitionType type = progress.IsNightActive ? TransitionType.EndOfNight : TransitionType.EndOfDay;

        Play(type);
    }

    public void Play(TransitionType type, Action onComplete = null)
    {
        if (transitionCoroutine != null) return;
        transitionCoroutine = StartCoroutine(PlayTransition(type, onComplete));
    }

    private IEnumerator PlayTransition(TransitionType type, Action onComplete)
    {
        PauseGame();

        SetValues();

        if (hudDocument != null)
            hudDocument.rootVisualElement.style.display = DisplayStyle.None;

        if (questJournalDocument != null)
            questJournalDocument.rootVisualElement.style.display = DisplayStyle.None;

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
        else if (type == TransitionType.EndOfNight)
        {
            int day = GameProgressManager.Instance.CurrentDay + 1;

            if (language == "es")
            {
                dayTitle.text = "Día " + day;
                daySubtitle.text = "Un nuevo día comienza...";
            }
            else
            {
                dayTitle.text = "Day " + day;
                daySubtitle.text = "A new day begins...";
            }
        }

        transitionPanel.style.display = DisplayStyle.Flex;
        transitionPanel.style.opacity = 0f;

        yield return Fade(0f, 1f);

        Coroutine breathingCoroutine = StartCoroutine(BreatheSubtitle());

        yield return new WaitForSecondsRealtime(textDuration);

        StopCoroutine(breathingCoroutine);
        daySubtitle.RemoveFromClassList("breathing");

        onComplete?.Invoke();

        yield return Fade(1f, 0f);

        transitionPanel.style.display = DisplayStyle.None;

        if (hudDocument != null)
            hudDocument.rootVisualElement.style.display = DisplayStyle.Flex;

        if (questJournalDocument != null)
            questJournalDocument.rootVisualElement.style.display = DisplayStyle.Flex;

        ResumeGame();

        transitionCoroutine = null;
    }

    private IEnumerator BreatheSubtitle()
    {
        bool breathing = false;

        while (true)
        {
            breathing = !breathing;

            if (breathing)
                daySubtitle.AddToClassList("breathing");
            else
                daySubtitle.RemoveFromClassList("breathing");

            yield return new WaitForSecondsRealtime(1.8f);
        }
    }

    public void PlayDayIntro(int day, Action onComplete = null)
    {
        if (transitionCoroutine != null) return;
        transitionCoroutine = StartCoroutine(PlayDayIntroRoutine(day, onComplete));
    }

    private IEnumerator PlayDayIntroRoutine(int day, Action onComplete)
    {
        PauseGame();
        SetValues();

        if (hudDocument != null)
            hudDocument.rootVisualElement.style.display = DisplayStyle.None;

        if (questJournalDocument != null)
            questJournalDocument.rootVisualElement.style.display = DisplayStyle.None;

        string language = LocalizationSettings.SelectedLocale.Identifier.Code;

        if (language == "es")
        {
            dayTitle.text = "Noche " + day;
            daySubtitle.text = "La oscuridad comienza...";
        }
        else
        {
            dayTitle.text = "Night " + day;
            daySubtitle.text = "Darkness falls...";
        }

        transitionPanel.style.display = DisplayStyle.Flex;
        transitionPanel.style.opacity = 0f;

        yield return Fade(0f, 1f);

        Coroutine breathingCoroutine = StartCoroutine(BreatheSubtitle());
        yield return new WaitForSecondsRealtime(textDuration);
        StopCoroutine(breathingCoroutine);
        daySubtitle.RemoveFromClassList("breathing");

        onComplete?.Invoke();

        yield return Fade(1f, 0f);

        transitionPanel.style.display = DisplayStyle.None;

        if (hudDocument != null)
            hudDocument.rootVisualElement.style.display = DisplayStyle.Flex;

        if (questJournalDocument != null)
            questJournalDocument.rootVisualElement.style.display = DisplayStyle.Flex;

        ResumeGame();
        transitionCoroutine = null;
    }

    public void PlayEndOfNight(int itemsCollected, Action onComplete = null)
    {
        if (transitionCoroutine != null) return;
        transitionCoroutine = StartCoroutine(PlayEndOfNightRoutine(itemsCollected, onComplete));
    }

    private IEnumerator PlayEndOfNightRoutine(int itemsCollected, Action onComplete)
    {
        PauseGame();

        if (hudDocument != null)
            hudDocument.rootVisualElement.style.display = DisplayStyle.None;

        if (questJournalDocument != null)
            questJournalDocument.rootVisualElement.style.display = DisplayStyle.None;

        moneyValue.text = GameProgressManager.Instance.Money.ToString();
        inventoryValue.text = itemsCollected.ToString();
        sanityValue.text = GetSanityText();

        string language = LocalizationSettings.SelectedLocale.Identifier.Code;

        if (language == "es")
        {
            dayTitle.text = "Fin de la noche";
            daySubtitle.text = "Volviste a casa...";
        }
        else
        {
            dayTitle.text = "End of the night";
            daySubtitle.text = "You returned home...";
        }

        transitionPanel.style.display = DisplayStyle.Flex;
        transitionPanel.style.opacity = 0f;

        yield return Fade(0f, 1f);

        Coroutine breathing = StartCoroutine(BreatheSubtitle());
        yield return new WaitForSecondsRealtime(textDuration);
        StopCoroutine(breathing);
        daySubtitle.RemoveFromClassList("breathing");

        onComplete?.Invoke();

        yield return Fade(1f, 0f);

        transitionPanel.style.display = DisplayStyle.None;

        if (hudDocument != null)
            hudDocument.rootVisualElement.style.display = DisplayStyle.Flex;

        if (questJournalDocument != null)
            questJournalDocument.rootVisualElement.style.display = DisplayStyle.Flex;

        ResumeGame();
        transitionCoroutine = null;
    }

    private string GetSanityText()
    {
        GameProgressManager progress = GameProgressManager.Instance;
        int sanity = !progress.IsInsane ? 100 : Mathf.Max(0, 100 - ((progress.CurrentDay - 1) * sanityLossPerDay));
        return sanity + "%";
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

            transitionPanel.style.opacity =
                Mathf.Lerp(from, to, t);

            yield return null;
        }

        transitionPanel.style.opacity = to;
    }

    private void SetValues()
    {
        GameProgressManager progress = GameProgressManager.Instance;

        moneyValue.text = progress.Money.ToString();
        inventoryValue.text = progress.InventoryCount.ToString();
        sanityValue.text = GetSanityText();
    }
}