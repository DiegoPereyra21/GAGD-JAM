using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class MailboxIndicator : MonoBehaviour
{
    [SerializeField] private Mailbox mailbox;
    [SerializeField] private Transform worldAnchor;
    [SerializeField] private Vector3 worldOffset = new Vector3(0f, 2f, 0f);

    private Label label;
    private VisualElement root;

    private void Awake()
    {
        root = GetComponent<UIDocument>().rootVisualElement;
        label = root.Q<Label>("MailboxCountLabel");
    }

    private void LateUpdate()
    {
        int remaining = mailbox.RemainingCount;

        if (remaining <= 0)
        {
            label.style.display = DisplayStyle.None;
            return;
        }

        Vector3 worldPos = worldAnchor.position + worldOffset;
        Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPos);

        if (screenPos.z < 0f)
        {
            label.style.display = DisplayStyle.None;
            return;
        }

        label.style.display = DisplayStyle.Flex;
        label.text = $"{remaining} cartas";

        Vector2 panelPos = RuntimePanelUtils.ScreenToPanel(root.panel, new Vector2(screenPos.x, Screen.height - screenPos.y));
        label.style.left = panelPos.x;
        label.style.top = panelPos.y;
    }
}