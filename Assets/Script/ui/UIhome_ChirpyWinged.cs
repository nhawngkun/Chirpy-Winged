using UnityEngine;
using UnityEngine.UI;

public class UIhome_ChirpyWinged : UICanvas_ChirpyWinged
{
    [Header("Buttons")]
    public Button playButton;
    public Button settingsButton;
    public Button howToPlayButton;

    [Header("Debug")]
    public bool showDebug = true;

    protected override void Awake()
    {
        base.Awake();
        SetupButtons();
    }

    public override void Setup()
    {
        base.Setup();
        SetupButtons();
        EnableButtons(true); // Bật lại buttons khi Setup (khi quay về Home)
    }

    private void SetupButtons()
    {
        if (playButton != null)
        {
            playButton.onClick.RemoveAllListeners();
            playButton.onClick.AddListener(OnPlay);
        }

        if (settingsButton != null)
        {
            settingsButton.onClick.RemoveAllListeners();
            settingsButton.onClick.AddListener(OnSettings);
        }

        if (howToPlayButton != null)
        {
            howToPlayButton.onClick.RemoveAllListeners();
            howToPlayButton.onClick.AddListener(OnHowToPlay);
        }
    }

    // ✅ FIXED: Ẩn/hiện buttons thay vì chỉ disable
    public void EnableButtons(bool enable)
    {
        if (playButton != null)
            playButton.gameObject.SetActive(enable); // ✅ Ẩn hẳn

        if (settingsButton != null)
            settingsButton.gameObject.SetActive(enable); // ✅ Ẩn hẳn

        if (howToPlayButton != null)
            howToPlayButton.gameObject.SetActive(enable); // ✅ Ẩn hẳn

        if (showDebug)
            Debug.Log($"[UIHome] Buttons {(enable ? "shown" : "hidden")}");
    }

    private void OnPlay()
    {
        if (showDebug)
            Debug.Log("[UIHome] Play button clicked");

        // Ẩn buttons trước khi chuyển scene
        EnableButtons(false);

        // Đóng UI Home
        UIManager_ChirpyWinged.Instance.EnableHome(false);

        // Mở UI Gameplay
        UIManager_ChirpyWinged.Instance.EnableGameplay(true);

        // Bắt đầu game
        if (GameManager_ChirpyWinged.Instance != null)
        {
            GameManager_ChirpyWinged.Instance.StartGame();
        }
    }

    private void OnSettings()
    {
        if (showDebug)
            Debug.Log("[UIHome] Settings button clicked");

        // Ẩn buttons trước khi mở Settings
        EnableButtons(false);

        UIManager_ChirpyWinged.Instance.EnableSettingPanel(true);
    }

    private void OnHowToPlay()
    {
        if (showDebug)
            Debug.Log("[UIHome] How To Play button clicked");

        // Ẩn buttons trước khi mở How To Play
        EnableButtons(false);

        UIManager_ChirpyWinged.Instance.EnableHowToPlay(true);
    }
}