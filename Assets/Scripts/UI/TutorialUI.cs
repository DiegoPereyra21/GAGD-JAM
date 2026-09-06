using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class TutorialUI : MonoBehaviour
{
    public static TutorialUI Instance { get; private set; }

    private VisualElement root;
    private Label nameLabel;
    private Label textLabel;

    private void Awake()
    {
        Instance = this;
        root = GetComponent<UIDocument>().rootVisualElement;
        nameLabel = root.Q<Label>("TutorialName");
        textLabel = root.Q<Label>("TutorialText");
        Hide();
    }

    public void Show(string title, string text)
    {
        nameLabel.text = title;
        textLabel.text = text;
        root.style.display = DisplayStyle.Flex;
    }

    public void Hide()
    {
        root.style.display = DisplayStyle.None;
    }
}