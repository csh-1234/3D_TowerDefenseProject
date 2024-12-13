using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class PathManager : MonoBehaviour
{
    private static PathManager instance;
    public static PathManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<PathManager>();
                if (instance == null)
                {
                    GameObject go = new GameObject("PathManager");
                    instance = go.AddComponent<PathManager>();
                }
            }
            return instance;
        }
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    [SerializeField]
    private List<Transform> spawnPoints = new List<Transform>();  // 여러 스폰 포인트
    [SerializeField]
    private Transform targetPoint;
    private Dictionary<Transform, AUnit> pathUnits = new Dictionary<Transform, AUnit>();
    
    private bool isUpdating = false;
    private int pendingUpdates = 0;

    // 경로 관리를 위한 추가 속성들
    private Dictionary<Transform, PathData> pathDataMap = new Dictionary<Transform, PathData>();

    private class PathData
    {
        public Vector3[] CurrentPath { get; set; }
        public bool IsValid { get; set; }
        public AUnit PathUnit { get; set; }
    }

    // 경로 업데이트 이벤트
    public event System.Action<Transform, Vector3[]> OnPathUpdated;
    public event System.Action<bool> OnValidityChanged;

    // 실제 경로 업데이트를 위한 이벤트 추가
    public event System.Action<Transform, Vector3[]> OnActualPathUpdated;
    
    // 실제 경로를 저장할 Dictionary 추가
    private Dictionary<Transform, Vector3[]> actualPaths = new Dictionary<Transform, Vector3[]>();

    private void NotifyPathUpdate(Transform spawnPoint, Vector3[] path)
    {
        OnPathUpdated?.Invoke(spawnPoint, path);
    }

    private void NotifyValidityChange(bool isValid)
    {
        OnValidityChanged?.Invoke(isValid);
    }

    [SerializeField]
    private Material actualPathMaterial;  // 실제 경로용 머티리얼
    [SerializeField]
    private Material previewPathMaterial; // 프리뷰 경로용 머티리얼

    private void Start()
    {
        // ��티리얼 로드
        actualPathMaterial = Resources.Load<Material>("Line/PathMat_Red");
        if (actualPathMaterial == null)
        {
            Debug.LogWarning("Failed to load PathMat_Red, creating default material");
            actualPathMaterial = new Material(Shader.Find("Sprites/Default"));
            actualPathMaterial.color = Color.red;
        }

        previewPathMaterial = Resources.Load<Material>("Line/PathMat_Gray");
        if (previewPathMaterial == null)
        {
            Debug.LogWarning("Failed to load PathMat_Gray, creating default material");
            previewPathMaterial = new Material(Shader.Find("Sprites/Default"));
            previewPathMaterial.color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
        }

        // pathUnits와 pathDataMap 초기화
        pathUnits = new Dictionary<Transform, AUnit>();
        pathDataMap = new Dictionary<Transform, PathData>();

        InitializePathUnits();
    }

    public Vector3[] GetCurrentPath(Transform spawnPoint = null)
    {
        if (spawnPoint == null)
            spawnPoint = GetBestSpawnPoint();

        if (pathDataMap.TryGetValue(spawnPoint, out PathData data))
        {
            if (data.CurrentPath == null || data.CurrentPath.Length == 0)
            {
                // AUnit에서 직접 경로 가져오기 시��
                AUnit unit = pathUnits[spawnPoint];
                if (unit != null)
                {
                    return unit.GetCurrentPath();
                }
            }
            return data.CurrentPath;
        }
        
        Debug.LogWarning($"No path data found for spawn point: {spawnPoint.name}");
        return null;
    }

    public void UpdateAllPaths()
    {
        // 경로 업데이트 전에 노드 상태 검증

        UpdatePath();
    }

    public void CheckPreviewPath()
    {
        // 모든 PathData의 IsValid를 초기화
        foreach (var data in pathDataMap.Values)
        {
            data.IsValid = false;
            data.CurrentPath = null;
        }

        // 모든 유닛의 프리뷰 초기화 및 활성화
        foreach (var spawnPoint in spawnPoints)
        {
            if (spawnPoint != null && pathUnits.TryGetValue(spawnPoint, out AUnit unit))
            {
                unit.ShowPreviewPath(true);
                unit.ShowActualPath(true);
                // 각 유닛의 프리뷰 경로를 동시에 계산
                unit.CheckPreviewPath();
            }
        }
    }

    private IEnumerator WaitForAllPathsCalculated()
    {
        yield return new WaitForEndOfFrame();

        bool anyValidPath = false;
        
        // 모든 스폰 포인트의 경로 유효성 확인
        foreach (var spawnPoint in spawnPoints)
        {
            if (spawnPoint != null && pathUnits.TryGetValue(spawnPoint, out AUnit unit))
            {
                Vector3[] currentPath = unit.GetCurrentPath();
                if (currentPath != null && currentPath.Length > 0)
                {
                    anyValidPath = true;
                    if (pathDataMap.ContainsKey(spawnPoint))
                    {
                        pathDataMap[spawnPoint].IsValid = true;
                        pathDataMap[spawnPoint].CurrentPath = currentPath;
                    }
                }
            }
        }

        HasValidPath = anyValidPath;
        NotifyValidityChange(anyValidPath);
    }

    public void UpdatePath()
    {
        if (isUpdating)
        {
            pendingUpdates++;
            return;
        }

        isUpdating = true;

        foreach (var kvp in pathUnits)
        {
            if (kvp.Value != null)
            {
                kvp.Value.UpdatePath();
            }
        }

        // 타임아웃 설정
        StartCoroutine(UpdateTimeout());
    }

    private IEnumerator UpdateTimeout()
    {
        yield return new WaitForSeconds(1f);
        if (isUpdating)
        {
            isUpdating = false;
            if (pendingUpdates > 0)
            {
                pendingUpdates--;
                UpdatePath();
            }
        }
    }

    // 가장 짧은 경로를 가진 스폰 포인트 선택
    public Transform GetBestSpawnPoint()
    {
        Transform bestSpawn = null;
        float shortestPath = float.MaxValue;

        foreach (var kvp in pathUnits)
        {
            Vector3[] path = kvp.Value.GetCurrentPath();
            if (path != null)
            {
                float pathLength = CalculatePathLength(path);
                if (pathLength < shortestPath)
                {
                    shortestPath = pathLength;
                    bestSpawn = kvp.Key;
                }
            }
        }

        return bestSpawn;
    }

    private float CalculatePathLength(Vector3[] path)
    {
        float length = 0;
        for (int i = 0; i < path.Length - 1; i++)
        {
            length += Vector3.Distance(path[i], path[i + 1]);
        }
        return length;
    }

    private void OnDestroy()
    {
        foreach (var unit in pathUnits.Values)
        {
            if (unit != null)
            {
                Destroy(unit.gameObject);
            }
        }
        pathUnits.Clear();
    }

    public void CleanupUnusedPaths()
    {
        List<Transform> keysToRemove = new List<Transform>();
        foreach (var kvp in pathUnits)
        {
            if (kvp.Key == null || kvp.Value == null)
            {
                keysToRemove.Add(kvp.Key);
            }
        }

        foreach (var key in keysToRemove)
        {
            if (pathUnits[key] != null)
            {
                Destroy(pathUnits[key].gameObject);
            }
            pathUnits.Remove(key);
        }
    }

    public List<Transform> GetSpawnPoints() => new List<Transform>(spawnPoints);
    public Transform GetTargetPoint() => targetPoint;
    public Vector3 GetTargetPosition() => targetPoint != null ? targetPoint.position : Vector3.zero;
    public Vector3 GetSpawnPosition(Transform spawnPoint = null)
    {
        if (spawnPoint == null)
            spawnPoint = GetBestSpawnPoint();
            
        return spawnPoint != null ? spawnPoint.position : spawnPoints[0].position;
    }
    public bool HasValidPoints() => spawnPoints != null && spawnPoints.Count > 0 && targetPoint != null;
    public bool HasValidPath { get; private set; }
    public void OnPathCalculated(Vector3[] path, bool success, bool isPreview = false)
    {
        if (!isPreview) // 실제 경로인 경우
        {
            if (success && path != null)
            {
                // 모든 스��� 포인트에 대해 경로 업데이트
                foreach (var spawnPoint in spawnPoints)
                {
                    if (spawnPoint != null && pathUnits.TryGetValue(spawnPoint, out AUnit unit))
                    {
                        // 현재 경로의 시작점이 이 유닛의 위치와 일치하는지 확인
                        float distance = Vector3.Distance(unit.transform.position, path[0]);
                        if (distance < 0.1f)
                        {
                            actualPaths[spawnPoint] = path;
                            pathDataMap[spawnPoint].CurrentPath = path;
                            pathDataMap[spawnPoint].IsValid = true;
                            OnActualPathUpdated?.Invoke(spawnPoint, path);
                            NotifyPathUpdate(spawnPoint, path);
                        }
                    }
                }
            }
            HasValidPath = success;
            NotifyValidityChange(success);
        }
        else // 프리뷰 경로인 경우
        {
            if (success && path != null)
            {
                // 모든 스폰 포인트에 대해 프리뷰 경로 업데이트
                foreach (var spawnPoint in spawnPoints)
                {
                    if (spawnPoint != null && pathUnits.TryGetValue(spawnPoint, out AUnit unit))
                    {
                        float distance = Vector3.Distance(unit.transform.position, path[0]);
                        if (distance < 0.1f)
                        {
                            pathDataMap[spawnPoint].IsValid = true;
                            pathDataMap[spawnPoint].CurrentPath = path;
                            NotifyPathUpdate(spawnPoint, path);
                        }
                    }
                }
            }
            // 하나라도 유효한 경로가 있으면 전체 경로가 유효한 것으로 간주
            HasValidPath = pathDataMap.Values.Any(data => data.IsValid);
        }
    }

    public void UpdatePreviewPathWithDelay(System.Action<bool> callback)
    {
        StartCoroutine(UpdatePreviewPathCoroutine(callback));
    }

    private IEnumerator UpdatePreviewPathCoroutine(System.Action<bool> callback)
    {
        yield return new WaitForEndOfFrame();
        CheckPreviewPath();
        callback?.Invoke(HasValidPath);
    }

    public Vector3[] GetActualPath(Transform spawnPoint)
    {
        if (spawnPoint == null)
            spawnPoint = GetBestSpawnPoint();

        if (actualPaths.ContainsKey(spawnPoint))
            return actualPaths[spawnPoint];
            
        // 실제 경로가 없으면 현재 경로 반환
        return GetCurrentPath(spawnPoint);
    }

    public void CheckPreviewPathForSpawnPoint(Transform spawnPoint)
    {
        if (pathUnits.TryGetValue(spawnPoint, out AUnit unit))
        {
            unit.ShowPreviewPath(true);
            unit.ShowActualPath(true);
            unit.CheckPreviewPath();
        }
    }

    public bool HasValidPathFromSpawnPoint(Transform spawnPoint)
    {
        if (pathDataMap.TryGetValue(spawnPoint, out PathData data))
        {
            return data.IsValid && data.CurrentPath != null && data.CurrentPath.Length > 0;
        }
        return false;
    }

    public void Clear()
    {
        // 모티리얼 임시 저장
        Material savedActualMaterial = actualPathMaterial;
        Material savedPreviewMaterial = previewPathMaterial;

        // 모든 경로 데이터 초기화
        pathDataMap.Clear();
        actualPaths.Clear();
        
        // 모든 유닛 초기화 및 제거
        foreach (var unit in pathUnits.Values)
        {
            if (unit != null)
            {
                unit.Clear();
                Destroy(unit.gameObject);
            }
        }
        pathUnits.Clear();

        // 이벤트 리스너 초기화
        OnPathUpdated = null;
        OnValidityChanged = null;
        OnActualPathUpdated = null;

        // 상태 초기화
        isUpdating = false;
        pendingUpdates = 0;
        HasValidPath = false;

        // 코루틴 정리
        StopAllCoroutines();

        // 머티리얼 복원
        actualPathMaterial = savedActualMaterial;
        previewPathMaterial = savedPreviewMaterial;

        // 경로 유닛 재초기화
        InitializePathUnits();
    }

    public void InitializePathUnits()
    {
        if (spawnPoints == null || spawnPoints.Count == 0 || targetPoint == null)
        {
            Debug.LogWarning("Cannot initialize path units: Missing spawn points or target point");
            return;
        }

        foreach (var spawnPoint in spawnPoints)
        {
            if (spawnPoint == null) continue;

            GameObject unitObj = new GameObject($"PathUnit_{spawnPoint.name}");
            AUnit unit = unitObj.AddComponent<AUnit>();
            
            // 각각 다른 머티리얼 설정
            unit.SetLineMaterials(actualPathMaterial, previewPathMaterial);
            
            // 위치와 타겟 설정
            unit.transform.position = spawnPoint.position;
            unit.SetTarget(targetPoint);

            // PathData 생성 및 저장
            PathData pathData = new PathData
            {
                PathUnit = unit,
                IsValid = false,
                CurrentPath = null
            };
            
            pathUnits.Add(spawnPoint, unit);
            pathDataMap.Add(spawnPoint, pathData);

            // 경로 업데이트 이벤트 구독
            unit.OnPathUpdated += (newPath, success) =>
            {
                if (success && newPath != null)
                {
                    pathData.CurrentPath = newPath;
                    pathData.IsValid = true;
                    NotifyPathUpdate(spawnPoint, newPath);
                }
            };
        }
        
        UpdateAllPaths();
    }

    public void SetupPoints(List<Transform> newSpawnPoints, Transform newTargetPoint)
    {
        spawnPoints = new List<Transform>(newSpawnPoints);
        targetPoint = newTargetPoint;
        
        // 기존 유닛들 정리
        foreach (var unit in pathUnits.Values)
        {
            if (unit != null)
            {
                Destroy(unit.gameObject);
            }
        }
        pathUnits.Clear();
        pathDataMap.Clear();

        // 새로운 포인트로 초기화
        InitializePathUnits();
    }
} 