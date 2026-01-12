using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Cake_ChirpyWinged : MonoBehaviour
{
    [Header("Settings")]
    public bool isPickedUp = false;

    [Header("Visual")]
    public float rotateSpeed = 50f; // Xoay bánh cho đẹp
    public float bobSpeed = 2f; // Nhấp nhô lên xuống
    public float bobHeight = 0.2f;
     public float DestroyTime = 7f;

    private Vector3 startPosition;

    void Start()
    {
        startPosition = transform.position;

        // Đảm bảo có collider và set trigger
        Collider col = GetComponent<Collider>();
        col.isTrigger = true;
    }

    void Update()
    {
        if (!isPickedUp)
        {
            // Animation xoay và nhấp nhô
            transform.Rotate(Vector3.up, rotateSpeed * Time.deltaTime);
            
            float newY = startPosition.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);
        }
    }
}