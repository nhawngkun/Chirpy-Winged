using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UILoss_ChirpyWinged : UICanvas_ChirpyWinged
{
    [Header("Score Display")]
    public TextMeshProUGUI cakeScoreText;
    public string scoreTextFormat = "Cakes Collected: {0}";

    [Header("Buttons")]
    public Button resetButton;
    public Button homeButton;

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
        UpdateScore();
    }

    public override void Open()
    {
        base.Open();
        UpdateScore();
    }

    void SetupButtons()
    {
        if (resetButton != null)
        {
            resetButton.onClick.RemoveAllListeners();
            resetButton.onClick.AddListener(OnReset);
        }

        if (homeButton != null)
        {
            homeButton.onClick.RemoveAllListeners();
            homeButton.onClick.AddListener(OnHome);
        }
    }

    void UpdateScore()
    {
        if (cakeScoreText == null) return;

        // Lấy số cake từ UIGameplay
        UIGameplay_ChirpyWinged uiGameplay = FindFirstObjectByType<UIGameplay_ChirpyWinged>();
        
        if (uiGameplay != null)
        {
            int cakeCount = uiGameplay.GetCakeCount();
            cakeScoreText.text = string.Format(scoreTextFormat, cakeCount);

            if (showDebug)
                Debug.Log($"[UILoss] Displayed cake count: {cakeCount}");
        }
        else
        {
            cakeScoreText.text = string.Format(scoreTextFormat, 0);
            
            if (showDebug)
                Debug.LogWarning("[UILoss] UIGameplay not found!");
        }
    }

    void OnReset()
    {
        if (showDebug)
            Debug.Log("[UILoss] Reset button clicked");

        if (GameManager_ChirpyWinged.Instance != null)
        {
            // Tắt UI Loss
            UIManager_ChirpyWinged.Instance.EnableLoss(false);
            
            // Reset game
            GameManager_ChirpyWinged.Instance.ResetGame();
        }
    }

    void OnHome()
    {
        if (showDebug)
            Debug.Log("[UILoss] Home button clicked");

        if (GameManager_ChirpyWinged.Instance != null)
        {
            GameManager_ChirpyWinged.Instance.GoToHome();
        }
    }
}