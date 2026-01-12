using UnityEngine;
using DG.Tweening; // ✅ Cho animation

[RequireComponent(typeof(Collider))]
public class CakeBox_ChirpyWinged : MonoBehaviour
{
    [Header("Collision Settings")]
    public bool allowPlayerWalkThrough = false; // ✅ Cho phép Player đi xuyên qua hay không

    [Header("Visual Feedback")]
    public Color normalColor = Color.white;
    public Color highlightColor = Color.green;
    public float highlightDuration = 0.3f;

    [Header("VFX Settings")]
    public ParticleSystem scoreVFX; // ✅ VFX đã gắn sẵn (child object của CakeBox)
   

    [Header("Score Animation")]
    public bool playScoreAnimation = true; // ✅ Animation khi ghi điểm
    public float punchScale = 1.3f; // Phóng to khi ghi điểm
    public float punchDuration = 0.4f;

    private Renderer boxRenderer;
    private Color currentColor;
    private float highlightTimer = 0f;

    void Start()
    {
        // ✅ Setup Collider dựa vào setting
        Collider col = GetComponent<Collider>();
        col.isTrigger = allowPlayerWalkThrough; // Nếu false = Player không đi xuyên qua

        // Lấy renderer để đổi màu
        boxRenderer = GetComponent<Renderer>();
        if (boxRenderer != null)
        {
            currentColor = normalColor;
            boxRenderer.material.color = currentColor;
        }
    }

    void Update()
    {
        // Reset màu sau khi highlight
        if (highlightTimer > 0f)
        {
            highlightTimer -= Time.deltaTime;
            if (highlightTimer <= 0f && boxRenderer != null)
            {
                boxRenderer.material.color = normalColor;
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // ✅ Chỉ hoạt động khi isTrigger = true
        if (!allowPlayerWalkThrough) return;

        // Visual feedback khi có thứ gì đó vào hộp
        if (other.CompareTag("Player"))
        {
            Highlight();
        }

        // ✅ Check nếu là ThrownCake → Play VFX
        if (other.GetComponent<ThrownCake_ChirpyWinged>() != null)
        {
            Highlight();
            PlayScoreEffects(other.transform.position);
             SoundManager_ChirpyWinged.Instance.PlayVFXSound(2);
        }
    }

    // ✅ Thêm OnCollisionEnter để detect khi Player va chạm (không phải trigger)
    void OnCollisionEnter(Collision collision)
    {
        // ✅ Chỉ hoạt động khi isTrigger = false
        if (allowPlayerWalkThrough) return;

        // Visual feedback khi Player va chạm
        if (collision.gameObject.CompareTag("Player"))
        {
            Highlight();
        }

        // ✅ Check nếu là ThrownCake → Play VFX
        if (collision.gameObject.GetComponent<ThrownCake_ChirpyWinged>() != null)
        {
            Highlight();
            PlayScoreEffects(collision.contacts[0].point);
            // Vị trí va chạm
        }
    }

    void Highlight()
    {
        if (boxRenderer != null)
        {
            boxRenderer.material.color = highlightColor;
            highlightTimer = highlightDuration;
        }
    }

    // ✅ ==================== SCORE EFFECTS ====================
    public void PlayScoreEffects(Vector3 hitPosition)
    {
        // ✅ Play VFX (đã gắn sẵn là child của CakeBox)
        if (scoreVFX != null)
        {
            scoreVFX.Play();
        }

        // ✅ Play Sound
      

        // ✅ Play Animation
        if (playScoreAnimation)
        {
            transform.DOPunchScale(Vector3.one * punchScale, punchDuration, 5, 0.5f);
        }
    }
}