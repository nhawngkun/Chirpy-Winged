using UnityEngine;

[System.Serializable]
public struct ChefRoute_ChirpyWinged
{
    public string routeName;
    public Transform startPoint;
    public Transform endPoint;
    [Tooltip("✅ Nếu true, Chef có thể đi 2 chiều (Start↔End)")]
    public bool bidirectional; // ✅ THÊM: Cho phép đi 2 chiều
}

public class ChefSpawner_ChirpyWinged : MonoBehaviour
{
    [Header("Spawn Settings")]
    public GameObject chefPrefab; 
    public ChefRoute_ChirpyWinged[] routes; 

    [Header("Progressive Difficulty")]
    public float initialSpawnInterval = 3f;
    public float minSpawnInterval = 0.5f;
    public float intervalDecreaseRate = 0.1f;
    
    public int initialMaxChefs = 2;
    public int maxMaxChefs = 10;
    public float chefsIncreaseInterval = 10f;

    [Header("Current Stats")]
    [SerializeField] private float currentSpawnInterval;
    [SerializeField] private int currentMaxChefs;
    [SerializeField] private int currentActiveChefs = 0;

    [Header("Boundary Reference")]
    public CircleBoundary_ChirpyWinged boundary;

    [Header("Debug")]
    public bool showDebug = true;

    private float spawnTimer = 0f;
    private float difficultyTimer = 0f;

    void Start()
    {
        if (boundary == null)
            boundary = FindFirstObjectByType<CircleBoundary_ChirpyWinged>();

        if (chefPrefab == null || routes == null || routes.Length == 0)
        {
            
            enabled = false;
            return;
        }

        currentSpawnInterval = initialSpawnInterval;
        currentMaxChefs = initialMaxChefs;
        spawnTimer = currentSpawnInterval;

       
    }

    void Update()
    {
        UpdateDifficulty();

        spawnTimer += Time.deltaTime;

        if (spawnTimer >= currentSpawnInterval && currentActiveChefs < currentMaxChefs)
        {
            spawnTimer = 0f;
            SpawnChef();
        }
    }

    void UpdateDifficulty()
    {
        difficultyTimer += Time.deltaTime;

        if (difficultyTimer >= chefsIncreaseInterval)
        {
            difficultyTimer = 0f;

            if (currentMaxChefs < maxMaxChefs)
            {
                currentMaxChefs++;
                
               
            }
        }
    }

    void SpawnChef()
    {
        // 1. Chọn ngẫu nhiên Route
        ChefRoute_ChirpyWinged selectedRoute = routes[Random.Range(0, routes.Length)];

        if (selectedRoute.startPoint == null || selectedRoute.endPoint == null) 
        {
           
            return;
        }

        // ✅ 2. Quyết định hướng đi
        Transform spawnPoint;
        Transform targetPoint;

        if (selectedRoute.bidirectional && Random.value > 0.5f)
        {
            // ✅ ĐI NGƯỢC: End → Start
            spawnPoint = selectedRoute.endPoint;
            targetPoint = selectedRoute.startPoint;

            
        }
        else
        {
            // ✅ ĐI THUẬN: Start → End
            spawnPoint = selectedRoute.startPoint;
            targetPoint = selectedRoute.endPoint;

            
        }

        // 3. Spawn tại điểm đã chọn
        GameObject chefObj = Instantiate(chefPrefab, spawnPoint.position, Quaternion.identity);

        // 4. Setup Chef
        ChefAI_ChirpyWinged chef = chefObj.GetComponent<ChefAI_ChirpyWinged>();
        if (chef != null)
        {
            chef.boundary = boundary;
            chef.SetFixedTarget(targetPoint); // ✅ Set target theo hướng đã chọn

            currentActiveChefs++;
            currentSpawnInterval = Mathf.Max(minSpawnInterval, currentSpawnInterval - intervalDecreaseRate);

            
        }
        else
        {
            Destroy(chefObj);
            return;
        }

        StartCoroutine(WaitForChefDestroy(chefObj));
    }

    System.Collections.IEnumerator WaitForChefDestroy(GameObject chef)
    {
        while (chef != null)
        {
            yield return null;
        }
        
        currentActiveChefs--;
        if (currentActiveChefs < 0) currentActiveChefs = 0;

       
    }

    // void OnDrawGizmos()
    // {
    //     if (routes == null) return;

    //     foreach (var route in routes)
    //     {
    //         if (route.startPoint != null && route.endPoint != null)
    //         {
    //             // ✅ Vẽ Start
    //             Gizmos.color = Color.green;
    //             Gizmos.DrawWireSphere(route.startPoint.position, 0.5f);
                
    //             // ✅ Vẽ End
    //             Gizmos.color = Color.red;
    //             Gizmos.DrawWireSphere(route.endPoint.position, 0.5f);

    //             // ✅ Vẽ đường đi
    //             Gizmos.color = route.bidirectional ? Color.yellow : Color.cyan;
    //             Gizmos.DrawLine(route.startPoint.position, route.endPoint.position);
                
    //             // ✅ Vẽ mũi tên chiều thuận
    //             Vector3 direction = (route.endPoint.position - route.startPoint.position).normalized;
    //             Vector3 arrowStart = route.startPoint.position + direction * 1f;
    //             Gizmos.DrawRay(arrowStart, direction * 1.5f);

    //             // ✅ Nếu bidirectional, vẽ mũi tên chiều ngược
    //             if (route.bidirectional)
    //             {
    //                 Gizmos.color = Color.magenta;
    //                 Vector3 reverseDir = -direction;
    //                 Vector3 reverseArrowStart = route.endPoint.position + reverseDir * 1f;
    //                 Gizmos.DrawRay(reverseArrowStart, reverseDir * 1.5f);
    //             }

    //             // ✅ Vẽ text label (nếu có UnityEditor)
    //             #if UNITY_EDITOR
    //             UnityEditor.Handles.Label(
    //                 Vector3.Lerp(route.startPoint.position, route.endPoint.position, 0.5f) + Vector3.up,
    //                 route.bidirectional ? $"{route.routeName} (↔)" : $"{route.routeName} (→)"
    //             );
    //             #endif
    //         }
    //     }
    // }

    public void ResetDifficulty()
    {
        currentSpawnInterval = initialSpawnInterval;
        currentMaxChefs = initialMaxChefs;
        difficultyTimer = 0f;

        
    }

    public void SetDifficulty(float spawnInterval, int maxChefs)
    {
        currentSpawnInterval = Mathf.Max(minSpawnInterval, spawnInterval);
        currentMaxChefs = Mathf.Clamp(maxChefs, 1, maxMaxChefs);

       
    }
}