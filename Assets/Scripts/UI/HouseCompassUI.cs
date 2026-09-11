using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class HouseCompassUI : MonoBehaviour
{
    [SerializeField] private Transform houseTarget;
    [SerializeField] private Camera mainCamera;

    private VisualElement root;
    private VisualElement arrow;

    private void Awake()
    {
        VisualElement uiRoot = GetComponent<UIDocument>().rootVisualElement;
        root = uiRoot.Q<VisualElement>("CompassRoot");
        arrow = uiRoot.Q<VisualElement>("CompassArrow");
        root.style.display = DisplayStyle.None;
    }

    private void Update()
    {
        bool visible = GameProgressManager.Instance.IsOutside;
        root.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;

        if (!visible) return;

        Vector3 viewportPos = mainCamera.WorldToViewportPoint(houseTarget.position);
        Vector2 dir = new Vector2(viewportPos.x - 0.5f, viewportPos.y - 0.5f);

        // Si el objetivo está detrás de la cámara, el viewport point se invierte solo — hay que darlo vuelta
        if (viewportPos.z < 0f)
            dir = -dir;

        float angle = Mathf.Atan2(dir.x, dir.y) * Mathf.Rad2Deg;

        arrow.style.rotate = new StyleRotate(new Rotate(new Angle(angle, AngleUnit.Degree)));
    }
}