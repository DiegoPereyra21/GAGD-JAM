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
        root.style.display = DisplayStyle.Flex;

        float angle = GameProgressManager.Instance.FullCycleProgress * 360f;
        hourHand.style.rotate = new StyleRotate(new Rotate(new Angle(angle, AngleUnit.Degree)));
    }
}