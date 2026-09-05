using UnityEngine;
using UnityEngine.UIElements;

public class AudioSettingsUI : MonoBehaviour
{
    [SerializeField] private UIDocument uiDocument;

    private Slider menuSlider;
    private Slider musicSlider;
    private Slider sfxSlider;
    private Slider ambientSlider;

    private const string MenuKey = "MenuVolume";
    private const string MusicKey = "MusicVolume";
    private const string SFXKey = "SFXVolume";
    private const string AmbientKey = "AmbientVolume";

    private const string MenuRTPC= "MenuVolume";
    private const string MusicRTPC = "MusicVolume";
    private const string SFXRTPC = "SFXVolume";
    private const string AmbientRTPC = "AmbientVolume";

    private const float DefaultVolume = 75f;

    private void OnEnable()
    {
        VisualElement root = uiDocument.rootVisualElement;

        menuSlider = root.Q<Slider>("MenuSlider");
        musicSlider = root.Q<Slider>("MusicSlider");
        sfxSlider = root.Q<Slider>("SFXSlider");
        ambientSlider = root.Q<Slider>("AmbientSlider");

        LoadSettings();

        menuSlider.RegisterValueChangedCallback(OnMenuChanged);
        musicSlider.RegisterValueChangedCallback(OnMusicChanged);
        sfxSlider.RegisterValueChangedCallback(OnSFXChanged);
        ambientSlider.RegisterValueChangedCallback(OnAmbientChanged);
    }

    private void OnDisable()
    {
        if (menuSlider != null)
            menuSlider.UnregisterValueChangedCallback(OnMenuChanged);

        if (musicSlider != null)
            musicSlider.UnregisterValueChangedCallback(OnMusicChanged);

        if (sfxSlider != null)
            sfxSlider.UnregisterValueChangedCallback(OnSFXChanged);

        if (ambientSlider != null)
            ambientSlider.UnregisterValueChangedCallback(OnAmbientChanged);
    }

    private void LoadSettings()
    {
        float menuVolume = PlayerPrefs.GetFloat(MenuKey, DefaultVolume);
        float musicVolume = PlayerPrefs.GetFloat(MusicKey, DefaultVolume);
        float sfxVolume = PlayerPrefs.GetFloat(SFXKey, DefaultVolume);
        float ambientVolume = PlayerPrefs.GetFloat(AmbientKey, DefaultVolume);

        if (menuSlider != null) menuSlider.value = menuVolume;
        if (musicSlider != null) musicSlider.value = musicVolume;
        if (sfxSlider != null) sfxSlider.value = sfxVolume;
        if (ambientSlider != null) ambientSlider.value = ambientVolume;

        AkSoundEngine.SetRTPCValue(MenuRTPC, menuVolume);
        AkSoundEngine.SetRTPCValue(MusicRTPC, musicVolume);
        AkSoundEngine.SetRTPCValue(SFXRTPC, sfxVolume);
        AkSoundEngine.SetRTPCValue(AmbientRTPC, ambientVolume);
    }

    private void OnMenuChanged(ChangeEvent<float> evt)
    {
        AkSoundEngine.SetRTPCValue(MenuRTPC, evt.newValue);

        PlayerPrefs.SetFloat(MenuKey, evt.newValue);
        PlayerPrefs.Save();
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

    public void ResetToDefaults()
    {
        menuSlider.value = DefaultVolume;
        musicSlider.value = DefaultVolume;
        sfxSlider.value = DefaultVolume;
        ambientSlider.value = DefaultVolume;

        AkSoundEngine.SetRTPCValue(MenuRTPC, DefaultVolume);
        AkSoundEngine.SetRTPCValue(MusicRTPC, DefaultVolume);
        AkSoundEngine.SetRTPCValue(SFXRTPC, DefaultVolume);
        AkSoundEngine.SetRTPCValue(AmbientRTPC, DefaultVolume);

        PlayerPrefs.SetFloat(MenuKey, DefaultVolume);
        PlayerPrefs.SetFloat(MusicKey, DefaultVolume);
        PlayerPrefs.SetFloat(SFXKey, DefaultVolume);
        PlayerPrefs.SetFloat(AmbientKey, DefaultVolume);
        PlayerPrefs.Save();
    }
}