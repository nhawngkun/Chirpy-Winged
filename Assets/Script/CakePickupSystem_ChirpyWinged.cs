using UnityEngine;
using DG.Tweening;

public class CakePickupSystem_ChirpyWinged : MonoBehaviour
{
    [Header("Cake Settings")]
    public GameObject cakeModelOnHead;
    public Transform cakeHoldPoint;
    public float pickupRadius = 1.5f;

    [Header("Throw Settings")]
    public float throwForce = 15f;
    public float throwHeight = 3f;
    public Transform throwPoint;
    public VariableJoystick_ChirpyWinged throwJoystick;
    public float minJoystickThreshold = 0.3f;

    [Header("Pickup Cooldown")]
    public float pickupCooldown = 0.5f;

    [Header("Aim Arrow")]
    public GameObject aimArrow;
    public float arrowDistance = 2f;

    [Header("Aim Line")]
    public LineRenderer aimLine;
    public Material lineMaterial;
    public float lineLength = 5f;
    public float lineHeight = 0.5f;
    public Color lineColor = Color.red;
    public float lineWidth = 0.1f;
    public bool useDottedLine = true;

    [Header("References")]
    public UIGameplay_ChirpyWinged uiGameplay;

    [Header("Debug")]
    public bool showDebug = true;

    private Cake_ChirpyWinged currentPickedCake;
    private bool isHoldingCake = false;
    private Vector3 aimDirection = Vector3.forward;
    private bool wasJoystickActive = false;
    private float lastThrowTime = -999f;
    private bool isSystemEnabled = true;

    void Start()
    {
        if (cakeModelOnHead != null)
        {
            cakeModelOnHead.SetActive(false);
        }

        if (aimArrow != null)
        {
            aimArrow.SetActive(false);
        }

        SetupAimLine();

        if (cakeHoldPoint == null)
        {
            GameObject holdPoint = new GameObject("CakeHoldPoint");
            holdPoint.transform.SetParent(transform);
            holdPoint.transform.localPosition = new Vector3(0, 2.5f, 0);
            cakeHoldPoint = holdPoint.transform;
        }

        if (throwPoint == null)
        {
            GameObject tPoint = new GameObject("ThrowPoint");
            tPoint.transform.SetParent(transform);
            tPoint.transform.localPosition = new Vector3(0, 2f, 0.5f);
            throwPoint = tPoint.transform;
        }

        if (uiGameplay == null)
        {
            uiGameplay = FindFirstObjectByType<UIGameplay_ChirpyWinged>();
        }

        if (throwJoystick == null && showDebug)
        {
            Debug.LogWarning("[CakeSystem] Throw joystick not assigned!");
        }
    }

    void SetupAimLine()
    {
        if (aimLine == null)
        {
            GameObject lineObj = new GameObject("AimLine");
            lineObj.transform.SetParent(transform);
            aimLine = lineObj.AddComponent<LineRenderer>();
        }

        aimLine.positionCount = 2;
        aimLine.startWidth = lineWidth;
        aimLine.endWidth = lineWidth;

        if (lineMaterial != null)
        {
            aimLine.material = lineMaterial;
        }
        else
        {
            aimLine.material = new Material(Shader.Find("Sprites/Default"));
        }

        aimLine.startColor = lineColor;
        aimLine.endColor = lineColor;

        if (useDottedLine)
        {
            aimLine.textureMode = LineTextureMode.Tile;
            aimLine.material.mainTextureScale = new Vector2(lineLength * 2f, 1f);
        }

        aimLine.enabled = false;
    }

    void Update()
    {
        if (!isSystemEnabled)
        {
            if (aimLine != null && aimLine.enabled)
            {
                ForceDisableLine();
            }
            return;
        }

        if (!isHoldingCake && CanPickup())
        {
            TryPickupNearestCake();
        }
        else if (isHoldingCake)
        {
            UpdateAimDirection();
            CheckThrowOnJoystickRelease();
        }
    }

    bool CanPickup()
    {
        return Time.time >= lastThrowTime + pickupCooldown;
    }

    void TryPickupNearestCake()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, pickupRadius);

        foreach (Collider col in colliders)
        {
            if (col.transform == transform) continue;

            Cake_ChirpyWinged cake = col.GetComponent<Cake_ChirpyWinged>();
            if (cake != null && !cake.isPickedUp)
            {
                PickupCake(cake);
                break;
            }
        }
    }

    void PickupCake(Cake_ChirpyWinged cake)
    {
        if (!isSystemEnabled) return;

        isHoldingCake = true;
        currentPickedCake = cake;
        cake.isPickedUp = true;

        cake.gameObject.SetActive(false);

        if (cakeModelOnHead != null)
        {
            cakeModelOnHead.SetActive(true);
            SoundManager_ChirpyWinged.Instance.PlayVFXSound(4);
            cakeModelOnHead.transform.localScale = Vector3.zero;
            cakeModelOnHead.transform.DOScale(1f, 0.3f).SetEase(Ease.OutBack);
        }

        if (aimArrow != null)
        {
            aimArrow.SetActive(true);
        }
    }

    void UpdateAimDirection()
    {
        if (!isSystemEnabled) return;
        if (throwJoystick == null) return;

        Vector2 throwInput = throwJoystick.Direction;

        if (throwInput.magnitude > 0.1f)
        {
            aimDirection = new Vector3(throwInput.x, 0, throwInput.y).normalized;

            if (aimArrow != null)
            {
                Vector3 arrowPosition = transform.position + aimDirection * arrowDistance;
                arrowPosition.y = transform.position.y + 0.5f;
                aimArrow.transform.position = arrowPosition;
                aimArrow.transform.rotation = Quaternion.LookRotation(aimDirection, Vector3.up);
            }

            UpdateAimLine(true);
        }
        else
        {
            UpdateAimLine(false);
        }
    }

    void UpdateAimLine(bool show)
    {
        if (!isSystemEnabled)
        {
            ForceDisableLine();
            return;
        }

        if (aimLine == null) return;

        if (show)
        {
            aimLine.enabled = true;

            Vector3 startPos = transform.position + Vector3.up * lineHeight;
            Vector3 endPos = startPos + aimDirection * lineLength;

            aimLine.SetPosition(0, startPos);
            aimLine.SetPosition(1, endPos);
        }
        else
        {
            aimLine.enabled = false;
        }
    }

    void ForceDisableLine()
    {
        if (aimLine == null) return;

        aimLine.enabled = false;
        aimLine.SetPosition(0, Vector3.zero);
        aimLine.SetPosition(1, Vector3.zero);
    }

    void CheckThrowOnJoystickRelease()
    {
        if (!isSystemEnabled) return;
        if (throwJoystick == null) return;

        Vector2 throwInput = throwJoystick.Direction;
        bool isJoystickActive = throwInput.magnitude >= minJoystickThreshold;

        if (wasJoystickActive && !isJoystickActive)
        {
            ThrowCake();
        }

        wasJoystickActive = isJoystickActive;
    }

    void ThrowCake()
    {
        if (!isHoldingCake || currentPickedCake == null) return;

        GameObject thrownCake = Instantiate(currentPickedCake.gameObject, throwPoint.position, Quaternion.identity);
        thrownCake.SetActive(true);

        Rigidbody rb = thrownCake.GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = thrownCake.AddComponent<Rigidbody>();
        }
        rb.useGravity = true;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

        Cake_ChirpyWinged cakeScript = thrownCake.GetComponent<Cake_ChirpyWinged>();
        if (cakeScript != null)
        {
            Destroy(cakeScript);
        }

        ThrownCake_ChirpyWinged thrownCakeScript = thrownCake.AddComponent<ThrownCake_ChirpyWinged>();
        thrownCakeScript.uiGameplay = uiGameplay;

        Vector3 throwVelocity = aimDirection * throwForce + Vector3.up * throwHeight;
        rb.linearVelocity = throwVelocity;

        if (cakeModelOnHead != null)
        {
            cakeModelOnHead.transform.DOScale(0f, 0.2f).SetEase(Ease.InBack).OnComplete(() =>
            {
                cakeModelOnHead.SetActive(false);
                cakeModelOnHead.transform.localScale = Vector3.one;
            });
        }

        if (aimArrow != null)
        {
            aimArrow.SetActive(false);
        }

        ForceDisableLine();

        if (currentPickedCake != null)
        {
            Destroy(currentPickedCake.gameObject);
        }

        currentPickedCake = null;
        isHoldingCake = false;
        wasJoystickActive = false;

        lastThrowTime = Time.time;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!isHoldingCake) return;

        CakeBox_ChirpyWinged box = other.GetComponent<CakeBox_ChirpyWinged>();
        if (box != null)
        {
            DepositCakeInBox(box);
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!isHoldingCake) return;

        CakeBox_ChirpyWinged box = collision.gameObject.GetComponent<CakeBox_ChirpyWinged>();
        if (box != null)
        {
            DepositCakeInBox(box);
            SoundManager_ChirpyWinged.Instance.PlayVFXSound(2);
        }
    }

    void DepositCakeInBox(CakeBox_ChirpyWinged box)
    {
        if (uiGameplay != null)
        {
            uiGameplay.AddCakeCount();
        }

        if (box != null)
        {
            box.PlayScoreEffects(box.transform.position);
        }

        if (cakeModelOnHead != null)
        {
            cakeModelOnHead.transform.DOScale(0f, 0.3f).SetEase(Ease.InBack).OnComplete(() =>
            {
                cakeModelOnHead.SetActive(false);
                cakeModelOnHead.transform.localScale = Vector3.one;
            });
        }

        if (aimArrow != null)
        {
            aimArrow.SetActive(false);
        }
        ForceDisableLine();

        if (currentPickedCake != null)
        {
            Destroy(currentPickedCake.gameObject);
        }

        currentPickedCake = null;
        isHoldingCake = false;
        wasJoystickActive = false;
    }

    public void ForceDropCake()
    {
        if (!isHoldingCake) return;

        isSystemEnabled = false;

        if (cakeModelOnHead != null)
        {
            cakeModelOnHead.transform.DOScale(0f, 0.2f).SetEase(Ease.InBack).OnComplete(() =>
            {
                cakeModelOnHead.SetActive(false);
                cakeModelOnHead.transform.localScale = Vector3.one;
            });
        }

        if (aimArrow != null)
        {
            aimArrow.SetActive(false);
        }

        ForceDisableLine();

        if (currentPickedCake != null)
        {
            Destroy(currentPickedCake.gameObject);
        }

        currentPickedCake = null;
        isHoldingCake = false;
        wasJoystickActive = false;

        Invoke(nameof(ReenableSystem), 0.5f);
    }

    void ReenableSystem()
    {
        isSystemEnabled = true;
    }

    // ✅ HÀM MỚI: Reset hoàn toàn system
    public void ResetSystem()
    {
        // Reset flags
        isSystemEnabled = true;
        isHoldingCake = false;
        wasJoystickActive = false;
        lastThrowTime = -999f;

        // Destroy cake nếu còn
        if (currentPickedCake != null)
        {
            Destroy(currentPickedCake.gameObject);
            currentPickedCake = null;
        }

        // Ẩn model trên đầu
        if (cakeModelOnHead != null)
        {
            cakeModelOnHead.SetActive(false);
            cakeModelOnHead.transform.localScale = Vector3.one;
        }

        // Ẩn arrow
        if (aimArrow != null)
        {
            aimArrow.SetActive(false);
        }

        // Tắt line
        ForceDisableLine();

        // Hủy invoke nếu có
        CancelInvoke(nameof(ReenableSystem));

        if (showDebug)
            Debug.Log("[CakeSystem] ✅ System reset complete");
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, pickupRadius);

        if (isHoldingCake && Application.isPlaying)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawRay(transform.position, aimDirection * arrowDistance);
        }

        if (Application.isPlaying && Time.time < lastThrowTime + pickupCooldown)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position + Vector3.up * 2f, 0.3f);
        }

        if (Application.isPlaying && !isSystemEnabled)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(transform.position + Vector3.up * 3f, Vector3.one * 0.5f);
        }
    }

    void OnDisable()
    {
        ForceDisableLine();
        CancelInvoke(nameof(ReenableSystem));
    }

    void OnDestroy()
    {
        ForceDisableLine();
        CancelInvoke(nameof(ReenableSystem));
    }
}