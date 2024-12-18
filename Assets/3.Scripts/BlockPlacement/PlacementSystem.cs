using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlacementSystem : MonoBehaviour
{
    private static PlacementSystem instance;
    public static PlacementSystem Instance { get { return instance; } }

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

        BlockData = new GridData();
        TowerData = new GridData();
        StopPlacement();
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
        currentCardID = cardID;
        gridVisualization.SetActive(true);
        buildingState = new PlacementState(ID, grid, preview, database, BlockData, TowerData, objectPlacer, inputManager, aGrid);
        inputManager.OnClicked += PlaceStructure;
        inputManager.OnClicked += StopPlacement;
        inputManager.OnExit += StopPlacement;
    }

    public void StartPlacement2(int ID)
    {
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
            return;
        }

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
        if (!placementState.IsValidPlacement(gridPosition))
        {
            isPlacing = false;
            yield break;
        }

        bool pathValid = false;
        bool checkComplete = false;

        placementState.SetTemporaryNodes(gridPosition, false);

        PathManager.Instance.UpdatePreviewPathWithDelay((isValid) => {
            pathValid = isValid;
            checkComplete = true;
        });

        float timeout = Time.time + 0.5f;
        while (!checkComplete && Time.time < timeout)
        {
            if (!placementState.IsValidPlacement(gridPosition))
            {
                placementState.RestoreTemporaryNodes();
                isPlacing = false;
                yield break;
            }
            yield return null;
        }

        placementState.RestoreTemporaryNodes();
        
        if (pathValid && placementState.IsValidPlacement(gridPosition))
        {
            placementState.OnAction(gridPosition);      
            OnPlacementSuccess?.Invoke(placementState.GetCurrentID(), currentCardID);
        }

        isPlacing = false;
    }


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
        yield return new WaitForEndOfFrame();

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
        StopPlacement();
        
        BlockData = new GridData();
        TowerData = new GridData();
        
        if (preview != null)
        {
            preview.StopShowingPreview();
        }
        
        if (gridVisualization != null)
        {
            gridVisualization.SetActive(false);
        }

        OnPlacementSuccess = null;
        
        isPlacing = false;
        lastDetectedPosition = Vector3Int.zero;
        currentCardID = null;
        
        StopAllCoroutines();
    }
}
