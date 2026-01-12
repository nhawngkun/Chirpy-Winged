using UnityEngine;
using DG.Tweening;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
public class ChefAI_ChirpyWinged : UICanvas_ChirpyWinged
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float rotateSpeed = 10f;
    public Transform fixedTarget;
    public float reachDistance = 1f;

    [Header("Route Settings")]
    private int maxTargetsBeforeDestroy = 1;

    [Header("Cake Drop Settings")]
    public GameObject cakePrefab;
    [Range(0f, 1f)] public float cakeDropChance = 0.8f;
    public Vector3 cakeDropOffset = Vector3.up;
    public int minCakeDrop = 2;
    public int maxCakeDrop = 4;
    public float dropCheckInterval = 0.3f;

    [Header("Drop Position Settings")]
    public float dropSpreadRadius = 1.5f;
    public bool dropOnPath = true;

    [Header("Boundary Reference")]
    public CircleBoundary_ChirpyWinged boundary;

    [Header("Bounce Animation")]
    public Transform visualModel;
    public float bounceHeight = 0.3f;
    public float bounceDuration = 0.3f;
    public Ease bounceEase = Ease.OutQuad;

    [Header("Debug")]
    public bool showDebug = true;

    private Rigidbody rb;
    private Collider myCollider;
    private bool isMoving = false;
    private Tween bounceTween;

    private bool wasInsideBoundary = false;
    private bool hasDroppedThisTrip = false;
    private float dropCheckTimer = 0f;

    private Vector3 startPosition;

    void Start()
    {
        startPosition = transform.position;

        SetupComponents();
        IgnoreCollisionsWithOtherChefs();
        IgnoreCollisionsWithCakeBoxAI();

        if (fixedTarget != null)
        {
            StartMoving();
        }
    }

    public void SetFixedTarget(Transform target)
    {
        fixedTarget = target;
        startPosition = transform.position;
    }

    void SetupComponents()
    {
        rb = GetComponent<Rigidbody>();

        // ✅ KINEMATIC = TRUE → Chef KHÔNG BỊ ĐẨY bởi bất kỳ lực nào
        rb.isKinematic = true;
        rb.useGravity = false;
        rb.constraints = RigidbodyConstraints.FreezeRotation;

        myCollider = GetComponent<Collider>();
        myCollider.isTrigger = false; // ✅ Solid để va chạm với Player

        if (visualModel == null)
        {
            foreach (Transform child in transform)
            {
                if (child.GetComponent<Renderer>() != null)
                {
                    visualModel = child;
                    break;
                }
            }
        }

        if (showDebug)
            Debug.Log("[ChefAI] Setup: Kinematic=true → Chef như tường cứng, Player KHÔNG ĐẨY ĐƯỢC");
    }

    void IgnoreCollisionsWithOtherChefs()
    {
        if (myCollider == null) return;
        ChefAI_ChirpyWinged[] allChefs = FindObjectsByType<ChefAI_ChirpyWinged>(FindObjectsSortMode.None);
        foreach (var chef in allChefs)
        {
            if (chef == this) continue;
            Collider otherCol = chef.GetComponent<Collider>();
            if (otherCol != null) Physics.IgnoreCollision(myCollider, otherCol);
        }
    }

    void IgnoreCollisionsWithCakeBoxAI()
    {
        if (myCollider == null) return;

        CakeBoxAI_ChirpyWinged[] allCakeBoxes = FindObjectsByType<CakeBoxAI_ChirpyWinged>(FindObjectsSortMode.None);

        foreach (var cakeBox in allCakeBoxes)
        {
            if (cakeBox == null) continue;

            Collider boxCollider = cakeBox.GetComponent<Collider>();
            if (boxCollider != null)
            {
                Physics.IgnoreCollision(myCollider, boxCollider);

                if (showDebug)
                    Debug.Log($"[ChefAI] Ignoring collision with CakeBoxAI: {cakeBox.name}");
            }
        }

        CakeBox_ChirpyWinged[] allNormalBoxes = FindObjectsByType<CakeBox_ChirpyWinged>(FindObjectsSortMode.None);

        foreach (var box in allNormalBoxes)
        {
            if (box == null) continue;

            if (box.GetComponent<CakeBoxAI_ChirpyWinged>() != null)
            {
                Collider boxCollider = box.GetComponent<Collider>();
                if (boxCollider != null)
                {
                    Physics.IgnoreCollision(myCollider, boxCollider);
                }
            }
        }
    }

    void Update()
    {
        if (fixedTarget == null) return;

        MoveToTarget();

        if (isMoving) CheckBoundaryCrossing();

        CheckReachedTarget();
    }

    void MoveToTarget()
    {
        if (!isMoving) return;

        Vector3 direction = (fixedTarget.position - transform.position).normalized;
        direction.y = 0;

        if (direction != Vector3.zero)
        {
            // ✅ Dùng transform.position vì rb.isKinematic = true
            Vector3 movement = direction * moveSpeed * Time.deltaTime;
            transform.position += movement;

            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotateSpeed * Time.deltaTime);
        }
    }

    void CheckBoundaryCrossing()
    {
        if (boundary == null) return;

        dropCheckTimer += Time.deltaTime;
        if (dropCheckTimer < dropCheckInterval) return;
        dropCheckTimer = 0f;

        bool isCurrentlyInside = boundary.IsPositionValid(transform.position);

        if (isCurrentlyInside && !wasInsideBoundary)
        {
            if (!hasDroppedThisTrip)
            {
                TryDropCake();
                hasDroppedThisTrip = true;
            }
        }

        wasInsideBoundary = isCurrentlyInside;
    }

    void CheckReachedTarget()
    {
        float distance = Vector3.Distance(transform.position, fixedTarget.position);

        if (distance <= reachDistance)
        {
            DestroyChef();
        }
    }

    void TryDropCake()
    {
        float roll = Random.Range(0f, 1f);
        if (roll <= cakeDropChance)
        {
            int cakeCount = Random.Range(minCakeDrop, maxCakeDrop + 1);
            DropCakes(cakeCount);
        }
    }

    void DropCakes(int count)
    {
        if (cakePrefab == null || boundary == null) return;

        for (int i = 0; i < count; i++)
        {
            Vector3 dropPosition;

            if (dropOnPath)
            {
                dropPosition = GetPositionOnPath(i, count);
            }
            else
            {
                Vector2 randomOffset = Random.insideUnitCircle * dropSpreadRadius;
                dropPosition = transform.position + cakeDropOffset + new Vector3(randomOffset.x, 0, randomOffset.y);
            }

            dropPosition = boundary.ClampPosition(dropPosition);

            if (!boundary.IsPositionValid(dropPosition))
            {
                dropPosition = transform.position + cakeDropOffset;
            }

            GameObject cake = Instantiate(cakePrefab, dropPosition, Quaternion.identity);
            cake.transform.localScale = Vector3.zero;
            cake.transform.DOScale(1f, 0.3f).SetEase(Ease.OutBack);
        }
    }

    Vector3 GetPositionOnPath(int index, int totalCount)
    {
        float totalDistance = Vector3.Distance(startPosition, fixedTarget.position);
        float currentDistance = Vector3.Distance(startPosition, transform.position);
        float progress = currentDistance / totalDistance;

        float offset = (index + 1) * (dropSpreadRadius / totalCount);
        float t = Mathf.Clamp01(progress - (offset / totalDistance));

        Vector3 position = Vector3.Lerp(startPosition, fixedTarget.position, t);
        position.y = transform.position.y;

        Vector2 randomOffset = Random.insideUnitCircle * 0.3f;
        position += new Vector3(randomOffset.x, 0, randomOffset.y);

        return position + cakeDropOffset;
    }

    void StartMoving()
    {
        isMoving = true;
        StartBounce();
    }

    void StopMoving()
    {
        isMoving = false;
        StopBounce();
    }

    void StartBounce()
    {
        if (visualModel == null) return;
        bounceTween?.Kill();
        bounceTween = visualModel.DOLocalMoveY(bounceHeight, bounceDuration)
            .SetEase(bounceEase)
            .SetLoops(-1, LoopType.Yoyo)
            .SetRelative(true);
    }

    void StopBounce()
    {
        if (visualModel == null) return;
        bounceTween?.Kill();
        visualModel.DOLocalMoveY(0f, 0.2f).SetEase(Ease.OutQuad);
    }

    void OnCollisionEnter(Collision collision)
    {
        // ✅ VA CHẠM với Player → Gây damage (Chef KHÔNG BỊ ĐẨY vì isKinematic = true)
        if (collision.gameObject.CompareTag("Player"))
        {
            if (showDebug)
                Debug.Log("[ChefAI] 💥 Hit Player! (Chef như tường, không bị đẩy)");

            // ✅ BONUS: Hủy velocity của Player để KHÔNG BỊ ĐẨY
            Rigidbody playerRb = collision.gameObject.GetComponent<Rigidbody>();
            if (playerRb != null)
            {
                // Hủy velocity theo hướng va chạm
                Vector3 hitDirection = (collision.transform.position - transform.position).normalized;
                Vector3 velocity = playerRb.linearVelocity;
                Vector3 pushVelocity = Vector3.Project(velocity, hitDirection);

                // Chỉ hủy nếu đang bị đẩy ra ngoài
                if (Vector3.Dot(pushVelocity, hitDirection) > 0)
                {
                    playerRb.linearVelocity = velocity - pushVelocity;

                    if (showDebug)
                        Debug.Log("[ChefAI] ✅ Canceled Player push velocity");
                }
            }
        }

        // ✅ Bị ThrownCake phá hủy
        ThrownCake_ChirpyWinged thrownCake = collision.gameObject.GetComponent<ThrownCake_ChirpyWinged>();
        if (thrownCake != null)
        {
            DestroyChef();
        }
    }

    void DestroyChef()
    {
        StopMoving();
        if (myCollider) myCollider.enabled = false;
        transform.DOScale(0f, 0.3f).SetEase(Ease.InBack).OnComplete(() =>
        {
            Destroy(gameObject);
        });
    }

    void OnDestroy()
    {
        bounceTween?.Kill();
    }

    void OnDrawGizmosSelected()
    {
        if (!showDebug) return;

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position + cakeDropOffset, dropSpreadRadius);

        if (fixedTarget != null && Application.isPlaying)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(startPosition, fixedTarget.position);
        }
    }
}