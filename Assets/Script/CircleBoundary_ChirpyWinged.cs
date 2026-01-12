using UnityEngine;

public class CircleBoundary_ChirpyWinged : MonoBehaviour
{
    [Header("Boundary Settings")]
    public Vector3 center; // Tâm vùng (tự điều chỉnh trong Inspector)
    public float radius = 10f; // Bán kính vùng cho phép

    [Header("Player Reference")]
    public Transform player; // Kéo player vào đây

    [Header("Visual Debug")]
    public bool showBoundary = true;
    public Color boundaryColor = Color.yellow;

    [Header("Push Back Settings")]
    public float pushBackStrength = 2f; // Độ mạnh đẩy player về trong
    public bool smoothPushBack = true; // Đẩy mượt hay tức thời

    void Start()
    {
        // Không tự động lấy center nữa, dùng giá trị trong Inspector

        // Tự động tìm player nếu chưa assign
        if (player == null)
        {
            PlayerMovement3D_ChirpyWinged playerScript = FindFirstObjectByType<PlayerMovement3D_ChirpyWinged>();
            if (playerScript != null)
            {
                player = playerScript.transform;
                Debug.Log($"[CircleBoundary] Auto-found player: {player.name}");
            }
            else
            {
                Debug.LogError("[CircleBoundary] Player not found! Please assign manually.");
            }
        }
    }

    void LateUpdate()
    {
        if (player == null) return;

        // Kiểm tra player có nằm ngoài vùng không
        CheckAndConstrainPlayer();
    }

    void CheckAndConstrainPlayer()
    {
        // Tính khoảng cách từ player đến tâm (chỉ tính trên mặt phẳng XZ)
        Vector3 playerPos = player.position;
        Vector3 centerPos = new Vector3(center.x, playerPos.y, center.z); // Giữ nguyên Y của player

        Vector3 offset = playerPos - centerPos;
        float distance = new Vector2(offset.x, offset.z).magnitude; // Khoảng cách 2D

        // Nếu player ra ngoài vùng
        if (distance > radius)
        {
            // Tính hướng từ tâm đến player
            Vector3 direction = offset.normalized;

            // Tính vị trí mới ngay trên biên
            Vector3 targetPos = centerPos + direction * radius;
            targetPos.y = playerPos.y; // Giữ nguyên độ cao

            if (smoothPushBack)
            {
                // Đẩy mượt
                player.position = Vector3.Lerp(playerPos, targetPos, pushBackStrength * Time.deltaTime);
            }
            else
            {
                // Đẩy tức thời
                player.position = targetPos;
            }

            // Hủy vận tốc của player để không bị trượt
            Rigidbody rb = player.GetComponent<Rigidbody>();
            if (rb != null)
            {
                // Chỉ hủy vận tốc theo hướng ra ngoài
                Vector3 velocity = rb.linearVelocity;
                Vector3 radialVelocity = Vector3.Project(velocity, direction);

                // Nếu đang di chuyển ra ngoài thì hủy velocity đó
                if (Vector3.Dot(radialVelocity, direction) > 0)
                {
                    rb.linearVelocity = velocity - radialVelocity;
                }
            }
        }
    }

    void OnDrawGizmos()
    {
        if (!showBoundary) return;

        // Vẽ hình tròn boundary - luôn dùng center từ Inspector
        Gizmos.color = boundaryColor;
        // Vẽ đường tròn
        int segments = 64;
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

        // Vẽ tâm
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(center, 0.3f);
    }

    // Hàm public để check xem vị trí có hợp lệ không
    public bool IsPositionValid(Vector3 position)
    {
        Vector3 centerPos = new Vector3(center.x, position.y, center.z);
        Vector3 offset = position - centerPos;
        float distance = new Vector2(offset.x, offset.z).magnitude;

        return distance <= radius;
    }

    // Hàm public để clamp position về trong boundary
    public Vector3 ClampPosition(Vector3 position)
    {
        Vector3 centerPos = new Vector3(center.x, position.y, center.z);
        Vector3 offset = position - centerPos;
        float distance = new Vector2(offset.x, offset.z).magnitude;

        if (distance > radius)
        {
            Vector3 direction = offset.normalized;
            Vector3 clampedPos = centerPos + direction * radius;
            clampedPos.y = position.y;
            return clampedPos;
        }

        return position;
    }
}