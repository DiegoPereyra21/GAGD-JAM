using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

[Serializable]
public class IntroSlide
{
    public Texture2D backgroundImage;
    [TextArea(3, 6)] public string text;
}

[RequireComponent(typeof(UIDocument))]
public class IntroSequence : MonoBehaviour
{
    [SerializeField] private IntroSlide[] slides;
    [SerializeField] private string continuePrompt = "Presioná E para continuar";
    [SerializeField] private string finishPrompt = "Presioná E para comenzar la historia";
    [SerializeField] private float fadeDuration = 0.5f;
    [SerializeField] private float typewriterSpeed = 60f;

    private VisualElement root;
    private VisualElement background;
    private VisualElement fadeOverlay;
    private Label textLabel;
    private Label promptLabel;
    private int currentIndex;
    private Action onComplete;
    private bool isPlaying;
    private bool isTyping;
    private bool isTransitioning;
    private Coroutine typeRoutine;

    private void Awake()
    {
        root = GetComponent<UIDocument>().rootVisualElement.Q<VisualElement>("IntroRoot");
        background = root.Q<VisualElement>("IntroBackground");
        textLabel = root.Q<Label>("IntroText");
        promptLabel = root.Q<Label>("IntroPrompt");
        fadeOverlay = root.Q<VisualElement>("FadeOverlay");

        root.style.display = DisplayStyle.None;
    }

    private void Update()
    {
        if (!isPlaying) return;

        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
            AdvanceOrFinish();
    }

    public void Play(Action onCompleteCallback)
    {
        if (slides == null || slides.Length == 0)
        {
            onCompleteCallback?.Invoke();
            return;
        }

        onComplete = onCompleteCallback;
        currentIndex = 0;
        isPlaying = true;
        root.style.display = DisplayStyle.Flex;
        fadeOverlay.style.opacity = 1f;
        promptLabel.style.display = DisplayStyle.None;

        StartCoroutine(ShowSlideRoutine(currentIndex));
    }

    private void AdvanceOrFinish()
    {
        if (isTransitioning) return;

        if (isTyping)
        {
            SkipTyping();
            return;
        }

        currentIndex++;

        if (currentIndex >= slides.Length)
        {
            isPlaying = false;
            Action callback = onComplete;
            onComplete = null;
            callback?.Invoke();
            return;
        }

        StartCoroutine(ShowSlideRoutineWithFadeIn(currentIndex));
    }

    private IEnumerator ShowSlideRoutineWithFadeIn(int index)
    {
        isTransitioning = true;
        yield return Fade(0f, 1f);
        yield return ShowSlideRoutine(index);
    }

    private IEnumerator ShowSlideRoutine(int index)
    {
        isTransitioning = true;
        promptLabel.style.display = DisplayStyle.None;

        IntroSlide slide = slides[index];
        background.style.backgroundImage = new StyleBackground(slide.backgroundImage);

        yield return Fade(1f, 0f);

        isTransitioning = false;
        typeRoutine = StartCoroutine(TypeText(slide.text, index == slides.Length - 1));
    }

    private IEnumerator Fade(float from, float to)
    {
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            fadeOverlay.style.opacity = Mathf.Lerp(from, to, elapsed / fadeDuration);
            yield return null;
        }

        fadeOverlay.style.opacity = to;
    }

    private IEnumerator TypeText(string fullText, bool isLastSlide)
    {
        isTyping = true;
        textLabel.text = "";

        float secondsPerChar = 1f / typewriterSpeed;
        float timer = 0f;
        int shown = 0;

        while (shown < fullText.Length)
        {
            timer += Time.deltaTime;

            while (timer >= secondsPerChar && shown < fullText.Length)
            {
                timer -= secondsPerChar;
                shown++;
                textLabel.text = fullText.Substring(0, shown);
            }

            yield return null;
        }

        isTyping = false;
        promptLabel.text = isLastSlide ? finishPrompt : continuePrompt;
        promptLabel.style.display = DisplayStyle.Flex;
    }

    private void SkipTyping()
    {
        if (typeRoutine != null) StopCoroutine(typeRoutine);

        bool isLastSlide = currentIndex == slides.Length - 1;
        textLabel.text = slides[currentIndex].text;
        isTyping = false;

        promptLabel.text = isLastSlide ? finishPrompt : continuePrompt;
        promptLabel.style.display = DisplayStyle.Flex;
    }
}