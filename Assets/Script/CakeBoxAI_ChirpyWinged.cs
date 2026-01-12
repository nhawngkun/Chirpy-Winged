using UnityEngine;
using DG.Tweening;

[RequireComponent(typeof(CakeBox_ChirpyWinged))]
public class CakeBoxAI_ChirpyWinged : MonoBehaviour
{
    [Header("AI Settings")]
    public float moveInterval = 2f; // Mỗi 2s di chuyển 1 lần
    public float minMoveDistance = 3f; // Khoảng cách di chuyển tối thiểu
    public float maxMoveDistance = 8f; // Khoảng cách di chuyển tối đa
    public float moveDuration = 1.5f; // Thời gian di chuyển đến vị trí mới
    public Ease moveEase = Ease.InOutQuad; // Kiểu easing

    [Header("Boundary Reference")]
    public CircleBoundary_ChirpyWinged boundary; // Kéo CircleBoundary vào đây

    [Header("Visual")]
    public bool rotateToMoveDirection = true; // Xoay theo hướng di chuyển
    public float rotateSpeed = 5f;

    [Header("Bounce Animation")]
    public Transform visualModel; // Model của CakeBox (child object)
    public float bounceHeight = 0.3f; // Độ cao nhảy
    public float bounceDuration = 0.3f; // Thời gian 1 lần nhảy
    public Ease bounceEase = Ease.OutQuad; // Kiểu easing

    [Header("Debug")]
    public bool showDebug = true;
    public bool showTargetGizmo = true;
    public Color targetGizmoColor = Color.cyan;

    private Vector3 currentTarget;
    private float moveTimer = 0f;
    private Tween moveTween;
    private Tween bounceTween; // ✅ Tween cho animation nhảy
    private bool isMoving = false;

    void Start()
    {
        // ✅ Setup Collider để Player không đi xuyên qua
        SetupCollider();

        // ✅ Tự động tìm visual model nếu chưa assign
        if (visualModel == null)
        {
            // Thử tìm child đầu tiên có Renderer
            foreach (Transform child in transform)
            {
                if (child.GetComponent<Renderer>() != null)
                {
                    visualModel = child;
                    if (showDebug)
                        Debug.Log($"[CakeBoxAI] Auto-found visual model: {visualModel.name}");
                    break;
                }
            }

            if (visualModel == null && showDebug)
            {
                Debug.LogWarning("[CakeBoxAI] Visual model not assigned! Bounce effect won't work.");
            }
        }

        // Tự động tìm CircleBoundary nếu chưa assign
        if (boundary == null)
        {
            boundary = FindFirstObjectByType<CircleBoundary_ChirpyWinged>();
            if (boundary == null)
            {
                Debug.LogError("[CakeBoxAI] CircleBoundary not found! Please assign manually.");
                enabled = false;
                return;
            }
            else
            {
                if (showDebug)
                    Debug.Log($"[CakeBoxAI] Auto-found boundary: {boundary.gameObject.name}");
            }
        }

        // Set vị trí ban đầu hợp lệ trong boundary
        if (!boundary.IsPositionValid(transform.position))
        {
            transform.position = boundary.ClampPosition(transform.position);
            if (showDebug)
                Debug.Log("[CakeBoxAI] Initial position clamped to boundary");
        }

        currentTarget = transform.position;

        // Bắt đầu di chuyển ngay
        moveTimer = moveInterval;
    }

    // ✅ ==================== SETUP COLLIDER ====================
    void SetupCollider()
    {
        Collider col = GetComponent<Collider>();
        
        if (col != null)
        {
            // ✅ TẮT Trigger để Player không đi xuyên qua
            col.isTrigger = false;

            if (showDebug)
                Debug.Log("[CakeBoxAI] Collider set to solid (isTrigger = false)");
        }
        else
        {
            Debug.LogWarning("[CakeBoxAI] No collider found! Player will pass through CakeBox.");
        }

        // ✅ Thêm Rigidbody nếu chưa có (để va chạm physics)
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
            if (showDebug)
                Debug.Log("[CakeBoxAI] Added Rigidbody to CakeBox");
        }

        // ✅ Cấu hình Rigidbody
        rb.isKinematic = true; // Không bị đẩy bởi physics
        rb.useGravity = false; // Không rơi xuống
        rb.constraints = RigidbodyConstraints.FreezeRotation; // Không bị lật

        if (showDebug)
            Debug.Log("[CakeBoxAI] Rigidbody configured (kinematic, no gravity)");
    }

    void Update()
    {
        moveTimer += Time.deltaTime;

        // Mỗi 2s (hoặc moveInterval) chọn vị trí mới
        if (moveTimer >= moveInterval && !isMoving)
        {
            moveTimer = 0f;
            MoveToRandomPosition();
        }
    }

    void MoveToRandomPosition()
    {
        // Random khoảng cách
        float distance = Random.Range(minMoveDistance, maxMoveDistance);

        // Random góc (360 độ)
        float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;

        // Tính vị trí mới
        Vector3 direction = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle));
        Vector3 targetPosition = transform.position + direction * distance;

        // Giữ nguyên Y
        targetPosition.y = transform.position.y;

        // Clamp vào trong boundary
        targetPosition = boundary.ClampPosition(targetPosition);

        // Kiểm tra nếu vị trí mới quá gần vị trí hiện tại (< 1 unit) thì thử lại
        if (Vector3.Distance(transform.position, targetPosition) < 1f)
        {
            if (showDebug)
                Debug.Log("[CakeBoxAI] Target too close, trying again...");
            
            moveTimer = moveInterval - 0.5f; // Thử lại sau 0.5s
            return;
        }

        currentTarget = targetPosition;

        // Di chuyển bằng DOTween
        MoveTo(currentTarget);

        if (showDebug)
            Debug.Log($"[CakeBoxAI] Moving to: {currentTarget}, distance: {distance:F1}");
    }

    void MoveTo(Vector3 target)
    {
        // Kill tween cũ nếu có
        moveTween?.Kill();

        isMoving = true;

        // ✅ Bắt đầu animation nhảy
        StartBounce();

        // Xoay về hướng di chuyển (nếu bật)
        if (rotateToMoveDirection)
        {
            Vector3 direction = (target - transform.position).normalized;
            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.DORotateQuaternion(targetRotation, 0.3f).SetEase(Ease.OutQuad);
            }
        }

        // Di chuyển đến target
        moveTween = transform.DOMove(target, moveDuration)
            .SetEase(moveEase)
            .OnComplete(() =>
            {
                isMoving = false;
                // ✅ Dừng animation nhảy khi đến nơi
                StopBounce();
                
                if (showDebug)
                    Debug.Log("[CakeBoxAI] Reached target");
            });
    }

    // ✅ ==================== BOUNCE ANIMATION ====================
    void StartBounce()
    {
        if (visualModel == null) return;

        // Kill tween cũ nếu có
        bounceTween?.Kill();

        // Tạo animation nhảy lên xuống liên tục (loop)
        bounceTween = visualModel.DOLocalMoveY(bounceHeight, bounceDuration)
            .SetEase(bounceEase)
            .SetLoops(-1, LoopType.Yoyo) // -1 = loop vô hạn, Yoyo = lên xuống
            .SetRelative(true); // Relative = di chuyển từ vị trí hiện tại

        if (showDebug)
            Debug.Log("[CakeBoxAI] Started bounce animation");
    }

    void StopBounce()
    {
        if (visualModel == null) return;

        // Kill animation
        bounceTween?.Kill();

        // Reset vị trí Y về 0 mượt mà
        visualModel.DOLocalMoveY(0f, 0.2f).SetEase(Ease.OutQuad);

        if (showDebug)
            Debug.Log("[CakeBoxAI] Stopped bounce animation");
    }

    void OnDrawGizmos()
    {
        if (!showTargetGizmo) return;

        // Vẽ vị trí target
        if (Application.isPlaying)
        {
            Gizmos.color = targetGizmoColor;
            Gizmos.DrawWireSphere(currentTarget, 0.5f);
            Gizmos.DrawLine(transform.position, currentTarget);
        }
    }

    void OnDrawGizmosSelected()
    {
        if (boundary == null) return;

        // Vẽ range di chuyển min/max
        Gizmos.color = Color.green;
        DrawCircle(transform.position, minMoveDistance, 32);

        Gizmos.color = Color.yellow;
        DrawCircle(transform.position, maxMoveDistance, 32);
    }

    void DrawCircle(Vector3 center, float radius, int segments)
    {
        float angleStep = 360f / segments;

        for (int i = 0; i < segments; i++)
        {
            float angle1 = i * angleStep * Mathf.Deg2Rad;
            float angle2 = (i + 1) * angleStep * Mathf.Deg2Rad;

            Vector3 point1 = center + new Vector3(
                Mathf.Cos(angle1) * radius,
                0,
                Mathf.Sin(angle1) * radius
            );

            Vector3 point2 = center + new Vector3(
                Mathf.Cos(angle2) * radius,
                0,
                Mathf.Sin(angle2) * radius
            );

            Gizmos.DrawLine(point1, point2);
        }
    }

    void OnDestroy()
    {
        // Cleanup tween
        moveTween?.Kill();
        bounceTween?.Kill(); // ✅ Cleanup bounce tween
    }

    // Public methods để control từ bên ngoài

    /// <summary>
    /// Dừng AI di chuyển
    /// </summary>
    public void StopAI()
    {
        moveTween?.Kill();
        StopBounce(); // ✅ Dừng bounce khi stop AI
        isMoving = false;
        enabled = false;

        if (showDebug)
            Debug.Log("[CakeBoxAI] AI stopped");
    }

    /// <summary>
    /// Bật lại AI di chuyển
    /// </summary>
    public void ResumeAI()
    {
        enabled = true;
        moveTimer = moveInterval; // Di chuyển ngay

        if (showDebug)
            Debug.Log("[CakeBoxAI] AI resumed");
    }

    /// <summary>
    /// Đặt tốc độ di chuyển mới
    /// </summary>
    public void SetMoveInterval(float newInterval)
    {
        moveInterval = Mathf.Max(0.5f, newInterval);

        if (showDebug)
            Debug.Log($"[CakeBoxAI] Move interval set to: {moveInterval}s");
    }

    /// <summary>
    /// Đặt range di chuyển mới
    /// </summary>
    public void SetMoveRange(float min, float max)
    {
        minMoveDistance = Mathf.Max(1f, min);
        maxMoveDistance = Mathf.Max(minMoveDistance + 1f, max);

        if (showDebug)
            Debug.Log($"[CakeBoxAI] Move range set to: {minMoveDistance}-{maxMoveDistance}");
    }
}