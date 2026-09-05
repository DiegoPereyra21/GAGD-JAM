using UnityEngine;
using UnityEngine.UIElements;

public class AudioSettingsUI : MonoBehaviour
{
    [SerializeField] private UIDocument uiDocument;

    private Slider generalSlider;
    private Slider menuSlider;
    private Slider musicSlider;
    private Slider sfxSlider;
    private Slider ambientSlider;

    private const string GeneralKey = "GeneralVolume";
    private const string MenuKey = "MenuVolume";
    private const string MusicKey = "MusicVolume";
    private const string SFXKey = "SFXVolume";
    private const string AmbientKey = "AmbientVolume";

    private const string MenuRTPC = "MenuVolume";
    private const string GeneralRTPC = "GeneralVolume";
    private const string MusicRTPC = "MusicVolume";
    private const string SFXRTPC = "SFXVolume";
    private const string AmbientRTPC = "AmbientVolume";

    private const float GeneralDefaultVolume = 100f;
    private const float DefaultVolume = 50f;

    private void OnEnable()
    {
        VisualElement root = uiDocument.rootVisualElement;

        generalSlider = root.Q<Slider>("GeneralSlider");
        menuSlider = root.Q<Slider>("MenuSlider");
        musicSlider = root.Q<Slider>("MusicSlider");
        sfxSlider = root.Q<Slider>("SFXSlider");
        ambientSlider = root.Q<Slider>("AmbientSlider");

        LoadSettings();

        generalSlider.RegisterValueChangedCallback(OnGeneralChanged);
        menuSlider.RegisterValueChangedCallback(OnMenuChanged);
        musicSlider.RegisterValueChangedCallback(OnMusicChanged);
        sfxSlider.RegisterValueChangedCallback(OnSFXChanged);
        ambientSlider.RegisterValueChangedCallback(OnAmbientChanged);
    }

    private void OnDisable()
    {
        if (generalSlider != null)
            generalSlider.UnregisterValueChangedCallback(OnGeneralChanged);

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
        float generalVolume = PlayerPrefs.GetFloat(GeneralKey, GeneralDefaultVolume);
        float menuVolume = PlayerPrefs.GetFloat(MenuKey, DefaultVolume);
        float musicVolume = PlayerPrefs.GetFloat(MusicKey, DefaultVolume);
        float sfxVolume = PlayerPrefs.GetFloat(SFXKey, DefaultVolume);
        float ambientVolume = PlayerPrefs.GetFloat(AmbientKey, DefaultVolume);

        if (generalSlider != null) generalSlider.value = generalVolume;
        if (menuSlider != null) menuSlider.value = menuVolume;
        if (musicSlider != null) musicSlider.value = musicVolume;
        if (sfxSlider != null) sfxSlider.value = sfxVolume;
        if (ambientSlider != null) ambientSlider.value = ambientVolume;

        AkSoundEngine.SetRTPCValue(GeneralRTPC, generalVolume);
        AkSoundEngine.SetRTPCValue(MenuRTPC, menuVolume);
        AkSoundEngine.SetRTPCValue(MusicRTPC, musicVolume);
        AkSoundEngine.SetRTPCValue(SFXRTPC, sfxVolume);
        AkSoundEngine.SetRTPCValue(AmbientRTPC, ambientVolume);
    }

    private void OnGeneralChanged(ChangeEvent<float> evt)
    {
        AkSoundEngine.SetRTPCValue(GeneralRTPC, evt.newValue);

        PlayerPrefs.SetFloat(GeneralKey, evt.newValue);
        PlayerPrefs.Save();
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
        generalSlider.value = GeneralDefaultVolume;
        menuSlider.value = DefaultVolume;
        musicSlider.value = DefaultVolume;
        sfxSlider.value = DefaultVolume;
        ambientSlider.value = DefaultVolume;

        AkSoundEngine.SetRTPCValue(GeneralRTPC, GeneralDefaultVolume);
        AkSoundEngine.SetRTPCValue(MenuRTPC, DefaultVolume);
        AkSoundEngine.SetRTPCValue(MusicRTPC, DefaultVolume);
        AkSoundEngine.SetRTPCValue(SFXRTPC, DefaultVolume);
        AkSoundEngine.SetRTPCValue(AmbientRTPC, DefaultVolume);

        PlayerPrefs.SetFloat(GeneralKey, GeneralDefaultVolume);
        PlayerPrefs.SetFloat(MenuKey, DefaultVolume);
        PlayerPrefs.SetFloat(MusicKey, DefaultVolume);
        PlayerPrefs.SetFloat(SFXKey, DefaultVolume);
        PlayerPrefs.SetFloat(AmbientKey, DefaultVolume);
        PlayerPrefs.Save();
    }
}