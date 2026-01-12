using UnityEngine;

public class ThrownCake_ChirpyWinged : MonoBehaviour
{
    [HideInInspector] public UIGameplay_ChirpyWinged uiGameplay;

    [Header("Settings")]
    public float autoDestroyTime = 10f;
     // ✅ Tự hủy sau 10s nếu không trúng gì

    [Header("VFX")]
    public ParticleSystem explosionVFX;

    [Header("Debug")]
    public bool showDebug = true;

    private bool hasScored = false;
    private bool hasHitChef = false; // ✅ THÊM: Flag để tránh double-hit

    void Start()
    {
        // ✅ Tự hủy sau một thời gian
        Invoke(nameof(AutoDestroy), autoDestroyTime);

        if (explosionVFX == null)
        {
            explosionVFX = GetComponentInChildren<ParticleSystem>();
            if (explosionVFX != null && showDebug)
            {
                Debug.Log($"[ThrownCake] Auto-found VFX: {explosionVFX.name}");
            }
        }
    }

    void AutoDestroy()
    {
        if (showDebug)
            Debug.Log("[ThrownCake] Auto-destroyed (timeout)");
        
        Destroy(gameObject);
    }

    // ✅ Xử lý khi CakeBox là Trigger
    void OnTriggerEnter(Collider other)
    {
        if (hasScored || hasHitChef) return;

        // Kiểm tra va chạm với CakeBox
        CakeBox_ChirpyWinged box = other.GetComponent<CakeBox_ChirpyWinged>();
        if (box != null)
        {
            ScoreCake();
            return;
        }

        // ✅ Kiểm tra va chạm với Chef (nếu Chef là trigger)
        ChefAI_ChirpyWinged chef = other.GetComponent<ChefAI_ChirpyWinged>();
        if (chef != null)
        {
            HitChef();
            return;
        }
    }

    // ✅ Xử lý khi CakeBox KHÔNG phải Trigger (Solid)
    void OnCollisionEnter(Collision collision)
    {
        if (hasScored || hasHitChef) return;

        // Kiểm tra va chạm với CakeBox
        CakeBox_ChirpyWinged box = collision.gameObject.GetComponent<CakeBox_ChirpyWinged>();
        if (box != null)
        {
            ScoreCake();
            return;
        }

        // ✅ Kiểm tra va chạm với Chef
        ChefAI_ChirpyWinged chef = collision.gameObject.GetComponent<ChefAI_ChirpyWinged>();
        if (chef != null)
        {
            HitChef();
            return;
        }
    }

    // ✅ THÊM: Hàm riêng xử lý khi trúng Chef
    void HitChef()
    {
        if (hasHitChef) return; // ✅ Tránh gọi nhiều lần
        hasHitChef = true;

        if (showDebug)
            Debug.Log("[ThrownCake] 💥 Hit Chef! Destroying cake...");

        // Play VFX
        PlayExplosionVFX();

        // ✅ HỦY NGAY cake
        CancelInvoke(nameof(AutoDestroy)); // Hủy auto-destroy timer
        Destroy(gameObject, 0.1f); // Hủy ngay (delay nhỏ để VFX kịp play)
    }

    void PlayExplosionVFX()
    {
        if (explosionVFX != null)
        {
            // Detach VFX từ cake để nó không bị destroy cùng
            explosionVFX.transform.SetParent(null);
            explosionVFX.Play();
            SoundManager_ChirpyWinged.Instance.PlayVFXSound(5);
         
            
            // Destroy VFX sau khi chạy xong
            float vfxDuration = explosionVFX.main.duration + explosionVFX.main.startLifetime.constantMax;
            Destroy(explosionVFX.gameObject, vfxDuration);
        }
        else
        {
            if (showDebug)
                Debug.LogWarning("[ThrownCake] No explosion VFX assigned!");
        }
    }

    void ScoreCake()
    {
        if (hasScored) return; // ✅ Tránh gọi nhiều lần
        hasScored = true;

        // Cộng điểm
        if (uiGameplay != null)
        {
            uiGameplay.AddCakeCount();
        }

        if (showDebug)
            Debug.Log("[ThrownCake] ✅ Cake scored!");

        // Hủy bánh
        CancelInvoke(nameof(AutoDestroy));
        Destroy(gameObject);
    }

    void OnDestroy()
    {
        // Cleanup
        CancelInvoke(nameof(AutoDestroy));
    }
}