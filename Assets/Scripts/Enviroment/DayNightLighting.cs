using UnityEngine;

public class DayNightLighting : MonoBehaviour
{
    [SerializeField] private Camera targetCamera;
    [SerializeField] private Color nightBackgroundColor = new Color(0.05f, 0.05f, 0.15f);
    [SerializeField] private Color dayBackgroundColor = new Color(0.6f, 0.8f, 1f);
    [SerializeField] private Color nightAmbientColor = new Color(0.05f, 0.05f, 0.1f);
    [SerializeField] private Color dayAmbientColor = new Color(0.8f, 0.8f, 0.8f);

    //para el skybox
    private static readonly int CubemapTransitionID = Shader.PropertyToID("_CubemapTransition");

    private void Update()
    {
        float t = GameProgressManager.Instance.NightProgress;

        RenderSettings.ambientLight = Color.Lerp(nightAmbientColor, dayAmbientColor, t);

        if (RenderSettings.skybox != null)
            RenderSettings.skybox.SetFloat(CubemapTransitionID, 1f - t);
    }
}