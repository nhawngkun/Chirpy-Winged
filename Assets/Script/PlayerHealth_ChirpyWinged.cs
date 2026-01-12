using UnityEngine;
using UnityEngine.Events;
using DG.Tweening;

public class PlayerHealth_ChirpyWinged : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHealth = 3;
    public int currentHealth = 3;

    [Header("VFX")]
    public ParticleSystem damageVFX;

    [Header("Damage Animation")]
    public bool playDamageAnimation = true;
    public float damagePunchScale = 0.8f;
    public float damagePunchDuration = 0.3f;
    public Color damageColor = Color.red;
    public float damageColorDuration = 0.5f;

    [Header("Invincibility Blink - Scale Pulse")]
    public float invincibilityDuration = 1.5f;
    public bool blinkWhenInvincible = true;
    [Range(0.7f, 0.95f)] public float minScale = 0.85f;
    [Range(1.05f, 1.3f)] public float maxScale = 1.15f;
    public float pulseSpeed = 0.2f;
    public Ease pulseEase = Ease.InOutSine;

    [Header("References")]
    public UIGameplay_ChirpyWinged uiGameplay;
    public CakePickupSystem_ChirpyWinged cakeSystem;

    [Header("Events")]
    public UnityEvent onDamage;
    public UnityEvent onDeath;

    [Header("Debug")]
    public bool showDebug = true;

    private bool isInvincible = false;
    private Renderer[] renderers;
    private Material[] originalMaterials;
    private Vector3 originalScale;
    private Tween pulseTween;

    void Start()
    {
        currentHealth = maxHealth;

        if (cakeSystem == null)
        {
            cakeSystem = GetComponent<CakePickupSystem_ChirpyWinged>();
        }

        if (uiGameplay == null)
        {
            uiGameplay = FindFirstObjectByType<UIGameplay_ChirpyWinged>();
        }

        renderers = GetComponentsInChildren<Renderer>();
        originalMaterials = new Material[renderers.Length];
        for (int i = 0; i < renderers.Length; i++)
        {
            originalMaterials[i] = renderers[i].material;
        }

        originalScale = transform.localScale;

        if (damageVFX == null)
        {
            damageVFX = GetComponentInChildren<ParticleSystem>();
        }

        UpdateUI();
    }

    void OnCollisionEnter(Collision collision)
    {
        ChefAI_ChirpyWinged chef = collision.gameObject.GetComponent<ChefAI_ChirpyWinged>();
        if (chef != null)
        {
            OnHitByChef(chef, collision.contacts[0].point);
        }
    }

    void OnHitByChef(ChefAI_ChirpyWinged chef, Vector3 hitPoint)
    {
        if (isInvincible) return;

        if (cakeSystem != null)
        {
            cakeSystem.ForceDropCake();
        }

        TakeDamage(1);
    }

    public void TakeDamage(int damage)
    {
        if (isInvincible) return;

        currentHealth -= damage;
        currentHealth = Mathf.Max(0, currentHealth);

        if (showDebug)
            Debug.Log($"[PlayerHealth] Took {damage} damage. Health: {currentHealth}/{maxHealth}");

        PlayDamageVFX(transform.position);
        PlayDamageAnimation();
        UpdateUI();
        onDamage?.Invoke();
        StartInvincibility();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void PlayDamageVFX(Vector3 position)
    {
        if (damageVFX != null)
        {
            damageVFX.transform.position = position;
            damageVFX.Play();
            SoundManager_ChirpyWinged.Instance.PlayVFXSound(3);
        }
    }

    void PlayDamageAnimation()
    {
        if (!playDamageAnimation) return;

        transform.DOPunchScale(Vector3.one * damagePunchScale, damagePunchDuration, 5, 0.5f);

        foreach (Renderer rend in renderers)
        {
            if (rend != null && rend.material != null)
            {
                rend.material.DOColor(damageColor, damageColorDuration).SetLoops(2, LoopType.Yoyo);
            }
        }
    }

    void StartInvincibility()
    {
        isInvincible = true;

        if (blinkWhenInvincible)
        {
            StartScalePulse();
        }

        Invoke(nameof(EndInvincibility), invincibilityDuration);
    }

    void EndInvincibility()
    {
        isInvincible = false;
        StopScalePulse();

        if (showDebug)
            Debug.Log("[PlayerHealth] Invincibility ended");
    }

    void StartScalePulse()
    {
        pulseTween?.Kill();

        Sequence pulseSeq = DOTween.Sequence();

        pulseSeq.Append(
            transform.DOScale(originalScale * minScale, pulseSpeed)
                .SetEase(pulseEase)
        );

        pulseSeq.Append(
            transform.DOScale(originalScale * maxScale, pulseSpeed)
                .SetEase(pulseEase)
        );

        pulseSeq.Append(
            transform.DOScale(originalScale, pulseSpeed)
                .SetEase(pulseEase)
        );

        pulseSeq.SetLoops(-1);

        pulseTween = pulseSeq;

        if (showDebug)
            Debug.Log("[PlayerHealth] ✅ Started scale pulse");
    }

    void StopScalePulse()
    {
        pulseTween?.Kill();
        transform.DOScale(originalScale, 0.2f).SetEase(Ease.OutQuad);

        if (showDebug)
            Debug.Log("[PlayerHealth] ⏹ Stopped scale pulse");
    }

    void Die()
    {
        if (showDebug)
            Debug.Log("[PlayerHealth] 💀 Player died!");

        SoundManager_ChirpyWinged.Instance.PlayVFXSound(0);

        StopScalePulse();

        if (cakeSystem != null)
        {
            cakeSystem.ForceDropCake();
            cakeSystem.enabled = false;
        }

        onDeath?.Invoke();

        PlayerMovement3D_ChirpyWinged movement = GetComponent<PlayerMovement3D_ChirpyWinged>();
        if (movement != null)
        {
            movement.enabled = false;
        }

        transform.DOScale(0f, 0.5f).SetEase(Ease.InBack).OnComplete(() =>
        {
            gameObject.SetActive(false);

            if (GameManager_ChirpyWinged.Instance != null)
            {
                GameManager_ChirpyWinged.Instance.GameOver();
            }
        });
    }

    // ✅ FIXED: Reset hoàn toàn Player và CakePickupSystem
    public void ResetPlayer()
    {
        currentHealth = maxHealth;
        isInvincible = false;

        // Dừng pulse
        StopScalePulse();

        // Reset scale về gốc
        transform.localScale = originalScale;

        gameObject.SetActive(true);

        PlayerMovement3D_ChirpyWinged movement = GetComponent<PlayerMovement3D_ChirpyWinged>();
        if (movement != null)
        {
            movement.enabled = true;
            movement.isKnockedBack = false;
        }

        // ✅ QUAN TRỌNG: Reset CakePickupSystem hoàn toàn
        if (cakeSystem != null)
        {
            cakeSystem.enabled = false; // Tắt trước
            cakeSystem.ResetSystem();   // Reset state
            cakeSystem.enabled = true;  // Bật lại
            
            if (showDebug)
                Debug.Log("[PlayerHealth] ✅ CakePickupSystem reset");
        }

        UpdateUI();

        if (showDebug)
            Debug.Log("[PlayerHealth] ✅ Player reset complete");
    }

    void UpdateUI()
    {
        if (uiGameplay != null)
        {
            uiGameplay.UpdateHealth(currentHealth);
        }
    }

    public void Heal(int amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Min(currentHealth, maxHealth);
        UpdateUI();

        if (showDebug)
            Debug.Log($"[PlayerHealth] Healed {amount}. Health: {currentHealth}/{maxHealth}");
    }

    public bool IsAlive()
    {
        return currentHealth > 0;
    }

    public bool IsInvincible()
    {
        return isInvincible;
    }

    void OnDestroy()
    {
        pulseTween?.Kill();
        CancelInvoke();
    }
}