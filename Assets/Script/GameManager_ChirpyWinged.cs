using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager_ChirpyWinged : Singleton_ChirpyWinged<GameManager_ChirpyWinged>
{
    [Header("References")]
    public ChefSpawner_ChirpyWinged chefSpawner;
    public PlayerHealth_ChirpyWinged playerHealth;
    public UIGameplay_ChirpyWinged uiGameplay;

    [Header("Game State")]
    public bool isGameStarted = false;
    public bool isGameOver = false;

    [Header("Debug")]
    public bool showDebug = true;

    public override void Awake()
    {
        base.Awake();
        FindReferences();
    }

    void Start()
    {
        // Game chưa bắt đầu, tắt spawner
        if (chefSpawner != null)
        {
            chefSpawner.enabled = false;
            if (showDebug)
                Debug.Log("[GameManager] Chef Spawner disabled at start");
        }
    }

    void FindReferences()
    {
        if (chefSpawner == null)
        {
            chefSpawner = FindFirstObjectByType<ChefSpawner_ChirpyWinged>();
        }

        if (playerHealth == null)
        {
            playerHealth = FindFirstObjectByType<PlayerHealth_ChirpyWinged>();
        }

        if (uiGameplay == null)
        {
            uiGameplay = FindFirstObjectByType<UIGameplay_ChirpyWinged>();
        }

        if (showDebug)
        {
            Debug.Log($"[GameManager] Found - Spawner: {chefSpawner != null}, Health: {playerHealth != null}, UI: {uiGameplay != null}");
        }
    }

    /// <summary>
    /// Bắt đầu game - gọi khi ấn nút Play
    /// </summary>
    public void StartGame()
    {
        if (isGameStarted) return;

        isGameStarted = true;
        isGameOver = false;

        // ✅ Reset cake count TRƯỚC KHI bắt đầu
        if (uiGameplay != null)
        {
            uiGameplay.ResetCakeCount();
        }

        // Bật Chef Spawner
        if (chefSpawner != null)
        {
            chefSpawner.enabled = true;
            if (showDebug)
                Debug.Log("[GameManager] ✅ Game Started - Chef Spawner enabled");
        }

        // Reset player health nếu cần
        if (playerHealth != null)
        {
            playerHealth.currentHealth = playerHealth.maxHealth;
            playerHealth.enabled = true;
        }

        if (showDebug)
            Debug.Log("[GameManager] ✅ Game Started!");
    }

    /// <summary>
    /// Reset game - gọi khi ấn nút Reset
    /// </summary>
    public void ResetGame()
    {
        if (showDebug)
            Debug.Log("[GameManager] 🔄 Resetting game...");

        // Reset flags
        isGameStarted = false;
        isGameOver = false;

        // Reset Chef Spawner
        if (chefSpawner != null)
        {
            chefSpawner.ResetDifficulty();
            chefSpawner.enabled = false;

            // Xóa tất cả Chef đang có
            ChefAI_ChirpyWinged[] allChefs = FindObjectsByType<ChefAI_ChirpyWinged>(FindObjectsSortMode.None);
            foreach (var chef in allChefs)
            {
                Destroy(chef.gameObject);
            }
        }

        // ✅ Reset Player bằng hàm mới
        if (playerHealth != null)
        {
            playerHealth.ResetPlayer();
        }

        // ✅ Reset UI TRƯỚC KHI mở lại
        if (uiGameplay != null)
        {
            uiGameplay.ResetCakeCount();
            uiGameplay.UpdateHealth(playerHealth != null ? playerHealth.maxHealth : 3);
        }

        // Xóa tất cả cake đang có
        Cake_ChirpyWinged[] allCakes = FindObjectsByType<Cake_ChirpyWinged>(FindObjectsSortMode.None);
        foreach (var cake in allCakes)
        {
            Destroy(cake.gameObject);
        }

        // Xóa tất cả thrown cake
        ThrownCake_ChirpyWinged[] allThrownCakes = FindObjectsByType<ThrownCake_ChirpyWinged>(FindObjectsSortMode.None);
        foreach (var thrownCake in allThrownCakes)
        {
            Destroy(thrownCake.gameObject);
        }

        // ✅ Đóng UI Loss (nếu đang mở)
        UIManager_ChirpyWinged.Instance.EnableLoss(false);

        // ✅ Mở lại UI Gameplay - Setup() sẽ tự động reset UI
        UIManager_ChirpyWinged.Instance.EnableGameplay(true);

        // ✅ Bắt đầu game mới
        StartGame();

        if (showDebug)
            Debug.Log("[GameManager] ✅ Reset complete!");
    }

    /// <summary>
    /// Về Home - gọi khi ấn nút Home
    /// </summary>
    public void GoToHome()
    {
        if (showDebug)
            Debug.Log("[GameManager] 🏠 Going to Home...");

        // Reset game trước
        isGameStarted = false;
        isGameOver = false;

        // Tắt Chef Spawner
        if (chefSpawner != null)
        {
            chefSpawner.enabled = false;
            chefSpawner.ResetDifficulty();

            // Xóa tất cả Chef
            ChefAI_ChirpyWinged[] allChefs = FindObjectsByType<ChefAI_ChirpyWinged>(FindObjectsSortMode.None);
            foreach (var chef in allChefs)
            {
                Destroy(chef.gameObject);
            }
        }

        // ✅ Reset Player bằng hàm mới
        if (playerHealth != null)
        {
            playerHealth.ResetPlayer();
        }

        // ✅ Reset UI
        if (uiGameplay != null)
        {
            uiGameplay.ResetCakeCount();
        }

        // Xóa tất cả objects
        Cake_ChirpyWinged[] allCakes = FindObjectsByType<Cake_ChirpyWinged>(FindObjectsSortMode.None);
        foreach (var cake in allCakes)
        {
            Destroy(cake.gameObject);
        }

        ThrownCake_ChirpyWinged[] allThrownCakes = FindObjectsByType<ThrownCake_ChirpyWinged>(FindObjectsSortMode.None);
        foreach (var thrownCake in allThrownCakes)
        {
            Destroy(thrownCake.gameObject);
        }

        // ✅ Đóng tất cả UI
        UIManager_ChirpyWinged.Instance.EnableGameplay(false);
        UIManager_ChirpyWinged.Instance.EnableLoss(false);

        // ✅ Mở UI Home - Setup() sẽ tự động reset
        UIManager_ChirpyWinged.Instance.EnableHome(true);

        if (showDebug)
            Debug.Log("[GameManager] ✅ Returned to Home!");
    }

    /// <summary>
    /// Game Over - gọi khi player chết
    /// </summary>
    public void GameOver()
    {
        if (isGameOver) return;

        isGameOver = true;
        isGameStarted = false;

        if (showDebug)
            Debug.Log("[GameManager] 💀 Game Over!");

        // Tắt Chef Spawner
        if (chefSpawner != null)
        {
            chefSpawner.enabled = false;
        }

        // Hiện UI Loss
        UIManager_ChirpyWinged.Instance.EnableGameplay(false);
        UIManager_ChirpyWinged.Instance.EnableLoss(true);
    }

    /// <summary>
    /// Reload scene (alternative reset method)
    /// </summary>
    public void ReloadScene()
    {
        Time.timeScale = 1f; // Reset time scale
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}