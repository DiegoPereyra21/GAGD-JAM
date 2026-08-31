using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class SanityVisualEffect : MonoBehaviour
{
    [SerializeField] private VolumeProfile profile;
    [SerializeField] private int maxDay = 7;

    [Header("Intensidad máxima de cada efecto (día 7) — mantener bajo para no molestar la visibilidad")]
    [SerializeField] private float maxVignetteIntensity = 0.35f;
    [SerializeField] private float maxChromaticAberration = 0.3f;
    [SerializeField] private float maxFilmGrain = 0.4f;
    [SerializeField] private float minSaturation = -20f;

    private Vignette vignette;
    private ChromaticAberration chromaticAberration;
    private FilmGrain filmGrain;
    private ColorAdjustments colorAdjustments;

    private void Awake()
    {
        profile.TryGet(out vignette);
        profile.TryGet(out chromaticAberration);
        profile.TryGet(out filmGrain);
        profile.TryGet(out colorAdjustments);
    }

    private void OnEnable()
    {
        GameProgressManager.Instance.OnNightStarted += Refresh;
        GameProgressManager.Instance.OnDayStarted += Refresh;
        Refresh();
    }

    private void OnDisable()
    {
        GameProgressManager.Instance.OnNightStarted -= Refresh;
        GameProgressManager.Instance.OnDayStarted -= Refresh;
    }

    private void Refresh()
    {
        int day = GameProgressManager.Instance.CurrentDay;
        float t = Mathf.Clamp01((day - 1) / (float)(maxDay - 1));

        if (vignette != null)
        {
            vignette.intensity.overrideState = true;
            vignette.intensity.value = Mathf.Lerp(0f, maxVignetteIntensity, t);
        }

        if (chromaticAberration != null)
        {
            chromaticAberration.intensity.overrideState = true;
            chromaticAberration.intensity.value = Mathf.Lerp(0f, maxChromaticAberration, t);
        }

        if (filmGrain != null)
        {
            filmGrain.intensity.overrideState = true;
            filmGrain.intensity.value = Mathf.Lerp(0f, maxFilmGrain, t);
        }

        if (colorAdjustments != null)
        {
            colorAdjustments.saturation.overrideState = true;
            colorAdjustments.saturation.value = Mathf.Lerp(0f, minSaturation, t);
        }
    }
}