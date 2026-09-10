using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class AnalogClockUI : MonoBehaviour
{
    private VisualElement root;
    private VisualElement hourHand;

    private void Awake()
    {
        VisualElement uiRoot = GetComponent<UIDocument>().rootVisualElement;
        root = uiRoot.Q<VisualElement>("ClockRoot");
        hourHand = uiRoot.Q<VisualElement>("HourHand");
    }

    private void Update()
    {
        bool visible = GameProgressManager.Instance.IsOutside;
        root.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;

        if (!visible) return;

        float t = GameProgressManager.Instance.LinearNightProgress;
        float angle = (300f + t * 240f) % 360f;

        hourHand.style.rotate = new StyleRotate(new Rotate(new Angle(angle, AngleUnit.Degree)));
    }
}