using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class VariableJoystick_ChirpyWinged : MonoBehaviour, IDragHandler, IPointerUpHandler, IPointerDownHandler
{
    [Header("Joystick Components")]
    public RectTransform background;
    public RectTransform handle;

    [Header("Settings")]
    public float handleRange = 1f;
    public float deadZone = 0.1f;

    [Header("Animation")]
    public float resetSpeed = 10f; // Tốc độ handle về giữa

    private bool isResetting = false;

    [Header("Debug")]
    public bool showDebug = true;

    private Vector2 input = Vector2.zero;
    private Canvas canvas;
    private Camera cam;

    public float Horizontal => input.x;
    public float Vertical => input.y;
    public Vector2 Direction => new Vector2(Horizontal, Vertical);

    void Start()
    {
        canvas = GetComponentInParent<Canvas>();

        if (canvas == null)
        {
           
            return;
        }

        if (canvas.renderMode == RenderMode.ScreenSpaceCamera)
            cam = canvas.worldCamera;

        if (background == null)
        {
            
            background = GetComponent<RectTransform>();
        }

        if (handle == null)
        {
            
        }
        else
        {
            // Force setup handle về tâm
            ResetHandle();
        }

        
    }

    void ResetHandle()
    {
        if (handle == null) return;

        // Set anchor về center nếu chưa đúng
        handle.anchorMin = new Vector2(0.5f, 0.5f);
        handle.anchorMax = new Vector2(0.5f, 0.5f);
        handle.pivot = new Vector2(0.5f, 0.5f);

        // Reset position
        handle.anchoredPosition = Vector2.zero;
        handle.localPosition = new Vector3(0, 0, handle.localPosition.z);

        if (showDebug)
            Debug.Log($"[Joystick] Handle reset to center: {handle.anchoredPosition}");
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (showDebug)
            Debug.Log("[Joystick] OnPointerDown");

        isResetting = false; // ✅ Dừng animation reset khi chạm vào
        OnDrag(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (isResetting) return; // ✅ Không xử lý drag khi đang reset

        Vector2 position;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            background,
            eventData.position,
            cam,
            out position))
        {
            // Normalize position theo kích thước background
            position.x = (position.x / background.sizeDelta.x);
            position.y = (position.y / background.sizeDelta.y);

            float x = position.x * 2;
            float y = position.y * 2;

            input = new Vector2(x, y);
            input = (input.magnitude > 1.0f) ? input.normalized : input;

            // Apply deadzone
            if (input.magnitude < deadZone)
                input = Vector2.zero;

            // Move handle
            if (handle != null)
            {
                handle.anchoredPosition = new Vector2(
                    input.x * (background.sizeDelta.x / 2f) * handleRange,
                    input.y * (background.sizeDelta.y / 2f) * handleRange
                );
            }

         
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        // ✅ BẮT BUỘC: Reset dữ liệu input về 0 ngay lập tức
        input = Vector2.zero;

        // Reset hình ảnh về tâm
        if (handle != null)
        {
            handle.anchoredPosition = Vector2.zero;
        }

        // Tắt cờ reset (nếu có dùng logic reset mượt)
        isResetting = false; 
    }
    void Update()
    {
        // ✅ Animate handle về giữa khi thả tay
        if (isResetting && handle != null)
        {
            // Debug mỗi frame
            if (showDebug)
              
            // Lerp về Vector2.zero
            handle.anchoredPosition = Vector2.Lerp(
                handle.anchoredPosition,
                Vector2.zero,
                resetSpeed * Time.deltaTime
            );

            // ✅ Dừng animation khi đã gần đủ (< 0.1 pixel)
            if (handle.anchoredPosition.magnitude < 0.1f)
            {
                handle.anchoredPosition = Vector2.zero;
                isResetting = false;

               
            }
        }
    }

    
}