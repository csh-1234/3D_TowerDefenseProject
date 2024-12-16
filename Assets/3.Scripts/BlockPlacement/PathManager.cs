using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class PathManager : MonoBehaviour
{
    private class PathData
    {
        public Vector3[] CurrentPath { get; set; }
        public bool IsValid { get; set; }
        public AUnit PathUnit { get; set; }
    }

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
    private Material actualPathMaterial;
    [SerializeField]
    private Material previewPathMaterial;

    [SerializeField]
    private List<Transform> spawnPoints = new List<Transform>(); 
    [SerializeField]
    private Transform targetPoint;
    private Dictionary<Transform, AUnit> pathUnits = new Dictionary<Transform, AUnit>();
    
    private bool isUpdating = false;
    private int pendingUpdates = 0;

    private Dictionary<Transform, PathData> pathDataMap = new Dictionary<Transform, PathData>();

    public event System.Action<Transform, Vector3[]> OnPathUpdated;
    public event System.Action<bool> OnValidityChanged;
    public event System.Action<Transform, Vector3[]> OnActualPathUpdated;
    private Dictionary<Transform, Vector3[]> actualPaths = new Dictionary<Transform, Vector3[]>();

    private void Start()
    {
        actualPathMaterial = Resources.Load<Material>("Line/PathMat_Red");
        if (actualPathMaterial == null)
        {
            actualPathMaterial = new Material(Shader.Find("Sprites/Default"));
            actualPathMaterial.color = Color.red;
        }

        previewPathMaterial = Resources.Load<Material>("Line/PathMat_Gray");
        if (previewPathMaterial == null)
        {
            previewPathMaterial = new Material(Shader.Find("Sprites/Default"));
            previewPathMaterial.color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
        }

        pathUnits = new Dictionary<Transform, AUnit>();
        pathDataMap = new Dictionary<Transform, PathData>();

        InitializePathUnits();
    }

    private void NotifyPathUpdate(Transform spawnPoint, Vector3[] path)
    {
        OnPathUpdated?.Invoke(spawnPoint, path);
    }

    private void NotifyValidityChange(bool isValid)
    {
        OnValidityChanged?.Invoke(isValid);
    }

    public Vector3[] GetCurrentPath(Transform spawnPoint = null)
    {
        if (spawnPoint == null)
            spawnPoint = GetBestSpawnPoint();

        if (pathDataMap.TryGetValue(spawnPoint, out PathData data))
        {
            if (data.CurrentPath == null || data.CurrentPath.Length == 0)
            {
                AUnit unit = pathUnits[spawnPoint];
                if (unit != null)
                {
                    return unit.GetCurrentPath();
                }
            }
            return data.CurrentPath;
        }
        return null;
    }

    public void UpdateAllPaths()
    {
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

    public List<Transform> GetSpawnPoints() => new List<Transform>(spawnPoints);
    public Transform GetTargetPoint() => targetPoint;
    public Vector3 GetTargetPosition() => targetPoint != null ? targetPoint.position : Vector3.zero;
    public Vector3 GetSpawnPosition(Transform spawnPoint = null)
    {
        if (spawnPoint == null)
        {
            spawnPoint = GetBestSpawnPoint();
        }

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
        Material savedActualMaterial = actualPathMaterial;
        Material savedPreviewMaterial = previewPathMaterial;

        pathDataMap.Clear();
        actualPaths.Clear();
        
        foreach (var unit in pathUnits.Values)
        {
            if (unit != null)
            {
                unit.Clear();
                Destroy(unit.gameObject);
            }
        }
        pathUnits.Clear();

        OnPathUpdated = null;
        OnValidityChanged = null;
        OnActualPathUpdated = null;

        isUpdating = false;
        pendingUpdates = 0;
        HasValidPath = false;

        StopAllCoroutines();

        actualPathMaterial = savedActualMaterial;
        previewPathMaterial = savedPreviewMaterial;

        InitializePathUnits();
    }

    public void InitializePathUnits()
    {
        if (spawnPoints == null || spawnPoints.Count == 0 || targetPoint == null)
        {
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
} 