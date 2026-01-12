using UnityEngine;
using TMPro;
using UnityEngine.UI;
using DG.Tweening;

public class UIGameplay_ChirpyWinged : UICanvas_ChirpyWinged
{
    [Header("Cake UI")]
    public TextMeshProUGUI cakeCountText;
    public string cakeTextFormat = "{0}";

    [Header("Health UI")]
    public GameObject[] healthIcons;

    [Header("Alternative Health Display")]
    public TextMeshProUGUI healthText;
    public string healthTextFormat = "HP: {0}/{1}";

    [Header("Buttons")]
    public Button resetButton;
    public Button homeButton;

    [Header("Animation")]
    public float punchScale = 1.2f;
    public float punchDuration = 0.3f;

    [Header("Health Animation")]
    public float healthLossPunchScale = 0.8f;
    public float healthLossPunchDuration = 0.4f;

    private int cakeCount = 0;
    private int maxHealth = 3;

    protected override void Awake()
    {
        base.Awake();
        SetupButtons();
    }

    void Start()
    {
        UpdateCakeUI();

        if (healthIcons != null && healthIcons.Length > 0)
        {
            maxHealth = healthIcons.Length;
        }
    }

    // ✅ THÊM: Override Setup để reset UI khi mở
    public override void Setup()
    {
        base.Setup();

        // ✅ Reset cake count về 0 mỗi khi Setup (mở UI)
        ResetCakeCount();

        // ✅ Reset health về max
        UpdateHealth(maxHealth);
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

    void OnReset()
    {
        Debug.Log("[UIGameplay] Reset button clicked");

        if (GameManager_ChirpyWinged.Instance != null)
        {
            GameManager_ChirpyWinged.Instance.ResetGame();
        }
    }

    void OnHome()
    {
        Debug.Log("[UIGameplay] Home button clicked");

        if (GameManager_ChirpyWinged.Instance != null)
        {
            GameManager_ChirpyWinged.Instance.GoToHome();
        }
    }

    // ==================== CAKE UI ====================
    public void AddCakeCount()
    {
        cakeCount++;
        UpdateCakeUI();
        AnimateCakeText();

        Debug.Log($"[UIGameplay] Cake count: {cakeCount}");
    }

    void UpdateCakeUI()
    {
        if (cakeCountText != null)
        {
            cakeCountText.text = string.Format(cakeTextFormat, cakeCount);
        }
    }

    void AnimateCakeText()
    {
        if (cakeCountText != null)
        {
            cakeCountText.transform.DOPunchScale(Vector3.one * punchScale, punchDuration, 5, 0.5f);
        }
    }

    public int GetCakeCount()
    {
        return cakeCount;
    }

    public void ResetCakeCount()
    {
        cakeCount = 0;
        UpdateCakeUI();

        Debug.Log("[UIGameplay] ✅ Cake count reset to 0");
    }

    // ==================== HEALTH UI ====================
    public void UpdateHealth(int currentHealth)
    {
        if (healthIcons == null || healthIcons.Length == 0) return;

        for (int i = 0; i < healthIcons.Length; i++)
        {
            if (i < currentHealth)
            {
                healthIcons[i].SetActive(true);
            }
            else
            {
                healthIcons[i].SetActive(false);
            }
        }

        // Cập nhật text health nếu có
        if (healthText != null)
        {
            healthText.text = string.Format(healthTextFormat, currentHealth, maxHealth);
        }

        // Animation khi mất máu
        if (currentHealth < maxHealth)
        {
            AnimateHealthLoss();
        }
    }

    void AnimateHealthLoss()
    {
        // Rung màn hình
        Camera.main?.transform.DOShakePosition(0.2f, 0.1f, 10, 90, false, true);

        // Punch scale health UI
        if (healthText != null)
        {
            healthText.transform.DOPunchScale(Vector3.one * healthLossPunchScale, healthLossPunchDuration, 5, 0.5f);
        }
    }

    public void SetMaxHealth(int max)
    {
        maxHealth = max;
    }
}