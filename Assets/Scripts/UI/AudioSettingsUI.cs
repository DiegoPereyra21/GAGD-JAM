using UnityEngine;
using UnityEngine.UIElements;

public class AudioSettingsUI : MonoBehaviour
{
    [SerializeField] private UIDocument uiDocument;

    private Slider musicSlider;
    private Slider sfxSlider;
    private Slider ambientSlider;

    private const string MusicKey = "MusicVolume";
    private const string SFXKey = "SFXVolume";
    private const string AmbientKey = "AmbientVolume";

    private const string MusicRTPC = "MusicVolume";
    private const string SFXRTPC = "SFXVolume";
    private const string AmbientRTPC = "AmbientVolume";

    private const float DefaultVolume = 100f;

    private void OnEnable()
    {
        VisualElement root = uiDocument.rootVisualElement;

        musicSlider = root.Q<Slider>("MusicSlider");
        sfxSlider = root.Q<Slider>("SFXSlider");
        ambientSlider = root.Q<Slider>("AmbientSlider");

        LoadSettings();

        musicSlider.RegisterValueChangedCallback(OnMusicChanged);
        sfxSlider.RegisterValueChangedCallback(OnSFXChanged);
        ambientSlider.RegisterValueChangedCallback(OnAmbientChanged);
    }

    private void OnDisable()
    {
        if (musicSlider != null)
            musicSlider.UnregisterValueChangedCallback(OnMusicChanged);

        if (sfxSlider != null)
            sfxSlider.UnregisterValueChangedCallback(OnSFXChanged);

        if (ambientSlider != null)
            ambientSlider.UnregisterValueChangedCallback(OnAmbientChanged);
    }

    private void LoadSettings()
    {
        float musicVolume = PlayerPrefs.GetFloat(MusicKey, DefaultVolume);
        float sfxVolume = PlayerPrefs.GetFloat(SFXKey, DefaultVolume);
        float ambientVolume = PlayerPrefs.GetFloat(AmbientKey, DefaultVolume);

        musicSlider.value = musicVolume;
        sfxSlider.value = sfxVolume;
        ambientSlider.value = ambientVolume;

        AkSoundEngine.SetRTPCValue(MusicRTPC, musicVolume);
        AkSoundEngine.SetRTPCValue(SFXRTPC, sfxVolume);
        AkSoundEngine.SetRTPCValue(AmbientRTPC, ambientVolume);
    }

    private void OnMusicChanged(ChangeEvent<float> evt)
    {
        AkSoundEngine.SetRTPCValue(MusicRTPC, evt.newValue);

        PlayerPrefs.SetFloat(MusicKey, evt.newValue);
        PlayerPrefs.Save();
    }

    private void OnSFXChanged(ChangeEvent<float> evt)
    {
        AkSoundEngine.SetRTPCValue(SFXRTPC, evt.newValue);

        PlayerPrefs.SetFloat(SFXKey, evt.newValue);
        PlayerPrefs.Save();
    }

    private void OnAmbientChanged(ChangeEvent<float> evt)
    {
        AkSoundEngine.SetRTPCValue(AmbientRTPC, evt.newValue);

        PlayerPrefs.SetFloat(AmbientKey, evt.newValue);
        PlayerPrefs.Save();
    }
}