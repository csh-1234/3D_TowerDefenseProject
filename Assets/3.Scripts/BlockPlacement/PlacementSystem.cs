using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlacementSystem : MonoBehaviour
{
    private static PlacementSystem instance;
    public static PlacementSystem Instance { get { return instance; } }

    [Header("Required Components")]
    [SerializeField] private InputManager inputManager;
    [SerializeField] private Grid grid;
    [SerializeField] private AGrid aGrid;
    [SerializeField] private ObjectsDatabaseSO database;
    [SerializeField] private GameObject gridVisualization;
    [SerializeField] private PreviewSystem preview;
    [SerializeField] private ObjectPlacer objectPlacer;

    private GridData BlockData;
    private GridData TowerData;
    private IBuildingState buildingState;
    private Vector3Int lastDetectedPosition = Vector3Int.zero;
    private bool isPlacing = false;

    public event System.Action<int, string> OnPlacementSuccess;
    private string currentCardID;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            DestroyImmediate(gameObject);
            return;
        }

        // 필수 컴포넌트 확인
        ValidateComponents();

        // 데이터 초기화
        BlockData = new GridData();
        TowerData = new GridData();

        StopPlacement();
    }

    private void ValidateComponents() 
    {
        if (inputManager == null) inputManager = GetComponent<InputManager>();
        if (grid == null) grid = GetComponent<Grid>();
        if (aGrid == null) aGrid = GetComponent<AGrid>();
        if (preview == null) preview = GetComponent<PreviewSystem>();
        if (objectPlacer == null) objectPlacer = GetComponent<ObjectPlacer>();

        // 필수 컴포넌트 누락 체크
        if (inputManager == null) Debug.LogError("InputManager is missing!");
        if (grid == null) Debug.LogError("Grid is missing!");
        if (aGrid == null) Debug.LogError("AGrid is missing!");
        if (database == null) Debug.LogError("ObjectsDatabaseSO is missing!");
        if (gridVisualization == null) Debug.LogError("GridVisualization is missing!");
        if (preview == null) Debug.LogError("PreviewSystem is missing!");
        if (objectPlacer == null) Debug.LogError("ObjectPlacer is missing!");
    }

    private void Start()
    {
        if (gridVisualization != null)
        {
            gridVisualization.SetActive(false);
        }
    }

    public void StartPlacement(int ID, string cardID)
    {
        // 필수 컴포넌트 재확인
        //if (!ValidateBeforePlacement()) return;

        currentCardID = cardID;
        gridVisualization.SetActive(true);
        buildingState = new PlacementState(ID, grid, preview, database, BlockData, TowerData, objectPlacer, inputManager, aGrid);
        inputManager.OnClicked += PlaceStructure;
        inputManager.OnClicked += StopPlacement;
        inputManager.OnExit += StopPlacement;
    }

    public void StartPlacement2(int ID)
    {
        // 필수 컴포넌트 재확인
        //if (!ValidateBeforePlacement()) return;

        gridVisualization.SetActive(true);
        buildingState = new PlacementState(ID, grid, preview, database, BlockData, TowerData, objectPlacer, inputManager, aGrid);
        inputManager.OnClicked += PlaceStructure;
        inputManager.OnClicked += StopPlacement;
        inputManager.OnExit += StopPlacement;
    }

    public void StartPlacementForDrag(int ID) 
    {
        //StopPlacement();
        gridVisualization.SetActive(true);
        buildingState = new PlacementState(ID, grid, preview, database, BlockData, TowerData, objectPlacer, inputManager, aGrid);
    }

    public void StopPlacementForDrag()
    {
        inputManager.OnClicked += PlaceStructure;
        inputManager.OnClicked += StopPlacement;
        inputManager.OnExit += StopPlacement;
    }

    public void StartRemoving()
    {
        if (inputManager == null || grid == null || preview == null ||
            database == null || objectPlacer == null || aGrid == null)
        {
            Debug.LogError("Cannot start placement: Some required components are missing!");
        }
    }

    public void StartTowerPlacement(int ID)
    {
        if (!database.IsTower(ID))
        {
            Debug.LogError($"Attempted to place non-tower object (ID: {ID}) using StartTowerPlacement");
            return;
        }

        //if (!ValidateBeforePlacement()) return;

        gridVisualization.SetActive(true);
        buildingState = new PlacementState(ID, grid, preview, database, BlockData, TowerData, objectPlacer, inputManager, aGrid);
        inputManager.OnClicked += PlaceStructure;
        inputManager.OnClicked += StopPlacement;
        inputManager.OnExit += StopPlacement;
    }

    private void PlaceStructure()
    {
        if (inputManager.IsPointerOverUI() || isPlacing)
        {
            return;
        }
        
        Vector3 mousePosition = inputManager.GetSelectedMapPosition();
        Vector3Int gridPosition = grid.WorldToCell(mousePosition); 

        if (buildingState is PlacementState placementState)
        {
            StartCoroutine(HandlePlacement(gridPosition, placementState));
        }
        else
        {
            buildingState.OnAction(gridPosition);
        }
    }

    private IEnumerator HandlePlacement(Vector3Int gridPosition, PlacementState placementState)
    {
        isPlacing = true;
        
        // 경로 체크 전에 한번 더 유효성 검사
        if (!placementState.IsValidPlacement(gridPosition))
        {
            isPlacing = false;
            yield break;
        }

        bool pathValid = false;
        bool checkComplete = false;

        // 임시 노드 상태 설정
        placementState.SetTemporaryNodes(gridPosition, false);

        PathManager.Instance.UpdatePreviewPathWithDelay((isValid) => {
            pathValid = isValid;
            checkComplete = true;
        });

        float timeout = Time.time + 0.5f;
        while (!checkComplete && Time.time < timeout)
        {
            // 대기 중에도 지속 유효성 검사
            if (!placementState.IsValidPlacement(gridPosition))
            {
                placementState.RestoreTemporaryNodes();
                isPlacing = false;
                yield break;
            }
            yield return null;
        }

        // 노드 상태 원복
        placementState.RestoreTemporaryNodes();

        // 최종 유효성 검사
        if (pathValid && placementState.IsValidPlacement(gridPosition))
        {
            placementState.OnAction(gridPosition);
            // 배치 성공 시 이벤트에 카드 ID도 함께 전달
            OnPlacementSuccess?.Invoke(placementState.GetCurrentID(), currentCardID);
        }

        isPlacing = false;
    }

    //private bool CheckPlacementValidity(Vector3Int gridPosition, int selectedObjectIndex)
    //{
    //    GridData selectedData = database.objectsData[selectedObjectIndex].ID == 0 ? 
    //        floorData : 
    //        furnitureData;

    //    return selectedData.CanPlaceObejctAt(gridPosition, database.objectsData[selectedObjectIndex].Size);
    //}

    private void StopPlacement()
    {
        if (buildingState == null)
            return;
        gridVisualization.SetActive(false);
        buildingState.EndState();
        inputManager.OnClicked -= PlaceStructure;
        inputManager.OnClicked -= StopPlacement;
        inputManager.OnExit -= StopPlacement;
        lastDetectedPosition = Vector3Int.zero;
        buildingState = null;
    }

    private void Update()
    {
        if (buildingState == null)
            return;
        Vector3 mousePosition = inputManager.GetSelectedMapPosition();
        Vector3Int gridPosition = grid.WorldToCell(mousePosition);
        if (lastDetectedPosition != gridPosition)
        {
            buildingState.UpdateState(gridPosition);
            lastDetectedPosition = gridPosition;
        }
    }

    public void RequestPathUpdate()
    {
        StartCoroutine(UpdatePathsAfterDelay());
    }

    private IEnumerator UpdatePathsAfterDelay()
    {
        // 노드 상태가 완전히 업데이트될 때까지 잠시 대기
        yield return new WaitForEndOfFrame();

        // 경로 업데이트
        if (PathManager.Instance != null)
        {
            PathManager.Instance.UpdateAllPaths();
        }
    }

    public PlacementState GetCurrentPlacementState()
    {
        return buildingState as PlacementState;
    }

    public GridData GetTowerData()
    {
        return TowerData;
    }

    public void Clear()
    {
        // 배치 상태 초기화
        StopPlacement();
        
        // 그리드 데이터 초기화
        BlockData = new GridData();
        TowerData = new GridData();
        
        // 프리뷰 시스템 초기화
        if (preview != null)
        {
            preview.StopShowingPreview();
        }
        
        // 그리드 시각화 비활성화
        if (gridVisualization != null)
        {
            gridVisualization.SetActive(false);
        }

        // 이벤트 리스너 초기화
        OnPlacementSuccess = null;
        
        // 상태 변수 초기화
        isPlacing = false;
        lastDetectedPosition = Vector3Int.zero;
        currentCardID = null;
        
        // 코루틴 정리
        //StopAllCoroutines();
    }
}
