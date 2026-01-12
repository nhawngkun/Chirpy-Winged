using UnityEngine;
using DG.Tweening; // ✅ Import DOTween

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement3D_ChirpyWinged : MonoBehaviour
{
    [Header("Settings")]
    public float moveSpeed = 8f;
    public float rotateSpeed = 15f;

    [Header("Input Method")]
    public bool useJoystick = true;
    public VariableJoystick_ChirpyWinged joystick;

    [Header("Bounce Animation")]
    public Transform visualModel; // ✅ Kéo model con vẹt vào đây (child object)
    public float bounceHeight = 0.3f; // Độ cao nhảy
    public float bounceDuration = 0.3f; // Thời gian 1 lần nhảy
    public Ease bounceEase = Ease.OutQuad; // Kiểu easing

    [Header("Debug")]
    public bool showDebug = true;

    [HideInInspector] public bool isKnockedBack = false;

    private Rigidbody rb;
    [SerializeField] private Animator animator;
    private Vector3 movementInput;
    private bool isMoving = false;
    private Tween bounceTween; // ✅ Lưu tween để control

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        rb.useGravity = true;

        // ✅ Tự động tìm model nếu chưa assign
        if (visualModel == null)
        {
            // Thử tìm child đầu tiên có Renderer
            foreach (Transform child in transform)
            {
                if (child.GetComponent<Renderer>() != null)
                {
                    visualModel = child;
                    Debug.Log($"[Player] Auto-found visual model: {visualModel.name}");
                    break;
                }
            }

            if (visualModel == null)
            {
                Debug.LogWarning("[Player] Visual model not assigned! Bounce effect won't work.");
            }
        }

        // Tự động tìm joystick
        if (joystick == null && useJoystick)
        {
            joystick = FindFirstObjectByType<VariableJoystick_ChirpyWinged>();
            if (joystick == null)
            {
                Debug.LogWarning("[Player] Joystick not found! Using keyboard input.");
                useJoystick = false;
            }
            else
            {
                Debug.Log($"[Player] Found joystick: {joystick.gameObject.name}");
            }
        }
    }

    void Update()
    {
        // Get input
        float moveX = 0f;
        float moveZ = 0f;

        if (useJoystick && joystick != null)
        {
            moveX = joystick.Horizontal;
            moveZ = joystick.Vertical;
        }
        else
        {
            moveX = Input.GetAxisRaw("Horizontal");
            moveZ = Input.GetAxisRaw("Vertical");
        }

        // ✅ FIX LỖI: Thay vì .normalized ngay lập tức, ta kiểm tra độ lớn trước
        Vector3 rawInput = new Vector3(moveX, 0f, moveZ);

        if (rawInput.magnitude > 0.1f) // Chỉ di chuyển nếu input đủ lớn (Deadzone của Player)
        {
            movementInput = rawInput.normalized;
            
            // ✅ Bắt đầu bounce nếu chưa đang bounce
            if (!isMoving)
            {
                isMoving = true;
                StartBounce();
            }
        }
        else
        {
            // ✅ QUAN TRỌNG: Nếu input quá nhỏ (do thả tay), ép về 0 tuyệt đối
            movementInput = Vector3.zero;
            
            // ✅ Dừng bounce
            if (isMoving)
            {
                isMoving = false;
                StopBounce();
            }
        }
    }

    void FixedUpdate()
    {
        if (!isKnockedBack)
        {
            MovePlayer();
            RotatePlayer();
        }
    }

    void MovePlayer()
    {
        // ✅ Chỉ di chuyển khi có input
        if (movementInput.magnitude > 0.01f)
        {
            Vector3 targetVelocity = movementInput * moveSpeed;
            rb.linearVelocity = new Vector3(targetVelocity.x, rb.linearVelocity.y, targetVelocity.z);
        }
        else
        {
            // ✅ DỪNG NGAY khi không có input
            rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
        }
    }

    void RotatePlayer()
    {
        if (movementInput != Vector3.zero)
        {
            Quaternion toRotation = Quaternion.LookRotation(movementInput, Vector3.up);
            transform.rotation = Quaternion.Lerp(transform.rotation, toRotation, rotateSpeed * Time.deltaTime);
        }
    }

    // ✅ ==================== BOUNCE ANIMATION ====================
    void StartBounce()
    {
        if (visualModel == null) return;

        // Kill tween cũ nếu có
        bounceTween?.Kill();

        // ✅ Tạo animation nhảy lên xuống liên tục (loop)
        bounceTween = visualModel.DOLocalMoveY(bounceHeight, bounceDuration)
            .SetEase(bounceEase)
            .SetLoops(-1, LoopType.Yoyo) // -1 = loop vô hạn, Yoyo = lên xuống
            .SetRelative(true); // Relative = di chuyển từ vị trí hiện tại

        if (showDebug)
            Debug.Log("[Player] Started bounce animation");
    }

    void StopBounce()
    {
        if (visualModel == null) return;

        // Kill animation
        bounceTween?.Kill();

        // ✅ Reset vị trí Y về 0 mượt mà
        visualModel.DOLocalMoveY(0f, 0.2f).SetEase(Ease.OutQuad);

        if (showDebug)
            Debug.Log("[Player] Stopped bounce animation");
    }

    void OnDestroy()
    {
        // ✅ Cleanup khi destroy object
        bounceTween?.Kill();
    }
}