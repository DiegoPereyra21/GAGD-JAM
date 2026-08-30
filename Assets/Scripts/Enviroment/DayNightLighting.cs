using UnityEngine;

public class DayNightLighting : MonoBehaviour
{
    [SerializeField] private Camera targetCamera;
    [SerializeField] private Color nightBackgroundColor = new Color(0.05f, 0.05f, 0.15f);
    [SerializeField] private Color dayBackgroundColor = new Color(0.6f, 0.8f, 1f);
    [SerializeField] private Color nightAmbientColor = new Color(0.05f, 0.05f, 0.1f);
    [SerializeField] private Color dayAmbientColor = new Color(0.8f, 0.8f, 0.8f);

    private void Update()
    {
        float t = GameProgressManager.Instance.NightProgress;

        targetCamera.backgroundColor = Color.Lerp(nightBackgroundColor, dayBackgroundColor, t);
        RenderSettings.ambientLight = Color.Lerp(nightAmbientColor, dayAmbientColor, t);
    }
}