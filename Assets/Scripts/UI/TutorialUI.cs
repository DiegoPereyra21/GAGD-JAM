using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class TutorialUI : MonoBehaviour
{
    public static TutorialUI Instance { get; private set; }

    private VisualElement root;
    private Label nameLabel;
    private Label textLabel;
    private Button nextButton;

    private readonly Queue<string> messageQueue = new Queue<string>();

    private void Awake()
    {
        Instance = this;

        VisualElement uiRoot = GetComponent<UIDocument>().rootVisualElement;
        root = uiRoot.Q<VisualElement>("TutorialPanel");
        nameLabel = root.Q<Label>("TutorialName");
        textLabel = root.Q<Label>("TutorialText");
        nextButton = root.Q<Button>("NextButton");

        if (nextButton != null)
            nextButton.clicked += AdvanceQueue;

        Hide();
    }

    // Un solo mensaje fijo, sin botón de avanzar (igual que antes)
    public void Show(string title, string text)
    {
        messageQueue.Clear();
        SetNextButtonVisible(false);

        nameLabel.text = title;
        textLabel.text = text;
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

    private void ShowNextInQueue()
    {
        textLabel.text = messageQueue.Dequeue();
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

    public void Hide()
    {
        root.style.display = DisplayStyle.None;
    }
}