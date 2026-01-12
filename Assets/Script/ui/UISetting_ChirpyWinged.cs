using UnityEngine;
using UnityEngine.UI;

public class UISetting_PolarRescue : UICanvas_ChirpyWinged
{
    [Header("Sliders")]
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;

    [Header("Fill Images")]
    [SerializeField] private Image musicFillImage;
    [SerializeField] private Image sfxFillImage;

    [Header("★ Buttons Tăng/Giảm")]
    [SerializeField] private Button musicPlusButton;
    [SerializeField] private Button musicMinusButton;
    [SerializeField] private Button sfxPlusButton;
    [SerializeField] private Button sfxMinusButton;

    [Header("Settings")]
    [SerializeField] private float volumeStep = 0.1f;

    public override void Setup()
    {
        base.Setup();
        InitializeVolume();
        SetupSliders();
        SetupButtons();
    }

    private void InitializeVolume()
    {
        float musicVol = SoundManager_ChirpyWinged.Instance.GetMusicVolume();
        float sfxVol = SoundManager_ChirpyWinged.Instance.GetSFXVolume();

        if (musicSlider != null)
            musicSlider.value = musicVol;

        if (sfxSlider != null)
            sfxSlider.value = sfxVol;

        if (musicFillImage != null)
            musicFillImage.fillAmount = musicVol;

        if (sfxFillImage != null)
            sfxFillImage.fillAmount = sfxVol;
    }

    private void SetupSliders()
    {
        if (musicSlider != null)
        {
            musicSlider.onValueChanged.RemoveAllListeners();
            musicSlider.onValueChanged.AddListener(OnMusicSliderChanged);
        }

        if (sfxSlider != null)
        {
            sfxSlider.onValueChanged.RemoveAllListeners();
            sfxSlider.onValueChanged.AddListener(OnSFXSliderChanged);
        }
    }

    private void SetupButtons()
    {
        if (musicPlusButton != null)
        {
            musicPlusButton.onClick.RemoveAllListeners();
            musicPlusButton.onClick.AddListener(OnMusicPlus);
        }

        if (musicMinusButton != null)
        {
            musicMinusButton.onClick.RemoveAllListeners();
            musicMinusButton.onClick.AddListener(OnMusicMinus);
        }

        if (sfxPlusButton != null)
        {
            sfxPlusButton.onClick.RemoveAllListeners();
            sfxPlusButton.onClick.AddListener(OnSFXPlus);
        }

        if (sfxMinusButton != null)
        {
            sfxMinusButton.onClick.RemoveAllListeners();
            sfxMinusButton.onClick.AddListener(OnSFXMinus);
        }
    }

    public void OnMusicSliderChanged(float value)
    {
        SoundManager_ChirpyWinged.Instance.SetMusicVolume(value);

        if (musicFillImage != null)
            musicFillImage.fillAmount = value;
    }

    public void OnSFXSliderChanged(float value)
    {
        SoundManager_ChirpyWinged.Instance.SetSFXVolume(value);

        if (sfxFillImage != null)
            sfxFillImage.fillAmount = value;
    }

    private void OnMusicPlus()
    {
        if (musicSlider != null)
        {
            float newValue = Mathf.Clamp01(musicSlider.value + volumeStep);
            musicSlider.value = newValue;
        }

        if (SoundManager_ChirpyWinged.Instance != null)
        {
            SoundManager_ChirpyWinged.Instance.PlayVFXSound(1);
        }
    }

    private void OnMusicMinus()
    {
        if (musicSlider != null)
        {
            float newValue = Mathf.Clamp01(musicSlider.value - volumeStep);
            musicSlider.value = newValue;
        }

        if (SoundManager_ChirpyWinged.Instance != null)
        {
            SoundManager_ChirpyWinged.Instance.PlayVFXSound(1);
        }
    }

    private void OnSFXPlus()
    {
        if (sfxSlider != null)
        {
            float newValue = Mathf.Clamp01(sfxSlider.value + volumeStep);
            sfxSlider.value = newValue;
        }

        if (SoundManager_ChirpyWinged.Instance != null)
        {
            SoundManager_ChirpyWinged.Instance.PlayVFXSound(1);
        }
    }

    private void OnSFXMinus()
    {
        if (sfxSlider != null)
        {
            float newValue = Mathf.Clamp01(sfxSlider.value - volumeStep);
            sfxSlider.value = newValue;
        }
    }

    public void Back()
    {
        // Đóng Settings Panel
        UIManager_ChirpyWinged.Instance.EnableSettingPanel(false);
        
        // Mở Home (Setup sẽ tự động bật lại buttons)
        UIManager_ChirpyWinged.Instance.EnableHome(true);
    }
}