using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;

public class PlacementState : IBuildingState
{
    private int selectedObjectIndex = -1;
    private int currentRotation = 0;

    private Grid grid;
    private PreviewSystem preview;
    private ObjectsDatabaseSO database;
    private GridData BlockData;
    private GridData TowerData;
    private ObjectPlacer objectPlacer;
    private InputManager inputManager;
    private AGrid aGrid;

    private LayerMask towerPlaceableLayer;
    private bool isDisposed = false;
    private List<IEnumerator> activeCoroutines = new List<IEnumerator>();

    public PlacementState(int ID, Grid grid, PreviewSystem preview, ObjectsDatabaseSO database, GridData blockData,
            GridData towerData, ObjectPlacer objectPlacer, InputManager inputManager, AGrid aGrid)
    {
        this.grid = grid;
        this.preview = preview;
        this.database = database;
        this.BlockData = blockData;
        this.TowerData = towerData;
        this.objectPlacer = objectPlacer;
        this.inputManager = inputManager;
        this.aGrid = aGrid;

        selectedObjectIndex = database.objectsData.FindIndex(data => data.ID == ID);
        if (selectedObjectIndex > -1)
        {
            preview.StartShowingPlacementPreview(
                database.objectsData[selectedObjectIndex].Prefab,
                database.objectsData[selectedObjectIndex].GetRotatedCells(currentRotation),
                currentRotation);
        }

        inputManager.OnRotate += RotateStructure;

        towerPlaceableLayer = LayerMask.GetMask("TowerPlaceable");
    }

    private void RotateStructure()
    {
        if (!inputManager.IsPointerOverUI()) 
        {
            currentRotation = (currentRotation + 1) % 4;
            preview.UpdateRotation(currentRotation);
        }
    }

    public void EndState()
    {
        isDisposed = true;
        preview.StopShowingPreview();
        inputManager.OnRotate -= RotateStructure;
        RestoreTemporaryNodes();
        activeCoroutines.Clear();
    }

    private bool CheckPlacementValidity(Vector3Int gridPosition, List<Vector2Int> cells, bool isPreview = false)
    {
        foreach (var cell in cells)
        {
            Vector3Int blockPos = new Vector3Int(
                gridPosition.x + cell.x,
                gridPosition.y,
                gridPosition.z + cell.y
            );

            if (BlockData.IsPositionOccupied(blockPos) || TowerData.IsPositionOccupied(blockPos))
            {
                return false;
            }
        }

        Vector3Int targetGridPos = grid.WorldToCell(PathManager.Instance.GetTargetPosition());
        foreach (var spawnPoint in PathManager.Instance.GetSpawnPoints())
        {
            Vector3Int spawnGridPos = grid.WorldToCell(spawnPoint.position);
            
            foreach (var cell in cells)
            {
                Vector3Int blockPos = new Vector3Int(
                    gridPosition.x + cell.x,
                    gridPosition.y,
                    gridPosition.z + cell.y
                );
                
                if ((blockPos.x == spawnGridPos.x && blockPos.z == spawnGridPos.z) ||
                    (blockPos.x == targetGridPos.x && blockPos.z == targetGridPos.z))
                {
                    return false;
                }
            }
        }

        foreach (var cell in cells)
        {
            Vector3Int blockPos = new Vector3Int(
                gridPosition.x + cell.x,
                gridPosition.y,
                gridPosition.z + cell.y
            );
            aGrid.SetTemporaryNodeState(blockPos, false);
        }

        PathManager.Instance.CheckPreviewPath();
        bool isValid = PathManager.Instance.HasValidPath;

        aGrid.RestoreTemporaryNodes();

        return isValid && GridData.Instance.CanPlaceObjectAt(gridPosition, cells);
    }

    public void UpdateState(Vector3Int gridPosition)
    {
        int floor = 0;
        bool validity = false;
        
        if (database.IsTower(database.objectsData[selectedObjectIndex].ID))
        {
            Vector3Int positionBelow = new Vector3Int(gridPosition.x, 0, gridPosition.z);
            bool hasBlockBelow = BlockData.GetRepresentationIndex(positionBelow) != -1;
            bool hasObstacleBelow = false;
            bool canPlaceTower = false;

            Vector3 checkPosition = grid.CellToWorld(positionBelow) + new Vector3(0.5f, 0, 0.5f);
            Collider[] obstacles = Physics.OverlapSphere(checkPosition, 0.3f, aGrid.unwalkableMask);
            foreach (var obstacle in obstacles)
            {
                hasObstacleBelow = true;
                if (obstacle.CompareTag("TowerPlaceable"))
                {
                    canPlaceTower = true;
                    break;
                }
            }

            if (hasBlockBelow || (hasObstacleBelow && canPlaceTower))
            {
                floor = 1;
                validity = TowerData.CanPlaceObjectAt(gridPosition, 
                    database.objectsData[selectedObjectIndex].GetRotatedCells(currentRotation), 
                    floor);
            }
            else
            {
                validity = false; 
            }
        }
        else
        {
            validity = GridData.Instance.CanPlaceObjectAt(gridPosition, 
                database.objectsData[selectedObjectIndex].GetRotatedCells(currentRotation)) &&
                CheckPlacementValidity(gridPosition, 
                    database.objectsData[selectedObjectIndex].GetRotatedCells(currentRotation), 
                    true);
        }

        preview.UpdatePosition(grid.CellToWorld(gridPosition), validity, floor);
    }

    public void OnAction(Vector3Int gridPosition)
    {
        if (inputManager.IsPointerOverUI())
        {
            return;
        }

        activeCoroutines.Clear();

        int floor = 0;
        bool canPlace = false;
        int currentID = database.objectsData[selectedObjectIndex].ID;
        bool canPlaceTower = false;

        if (database.IsTower(currentID))
        {
            Vector3Int positionBelow = new Vector3Int(gridPosition.x, 0, gridPosition.z);
            bool hasBlockBelow = BlockData.GetRepresentationIndex(positionBelow) != -1;
            bool hasObstacleBelow = false;

            Vector3 checkPosition = grid.CellToWorld(positionBelow) + new Vector3(0.5f, 0, 0.5f);
            Collider[] obstacles = Physics.OverlapSphere(checkPosition, 0.3f, aGrid.unwalkableMask);
            foreach (var obstacle in obstacles)
            {
                hasObstacleBelow = true;
                if (obstacle.CompareTag("TowerPlaceable"))
                {
                    canPlaceTower = true;
                    break;
                }
            }

            if ((hasBlockBelow || (hasObstacleBelow && canPlaceTower)))
            {
                floor = 1;
                canPlace = TowerData.CanPlaceObjectAt(gridPosition, 
                    database.objectsData[selectedObjectIndex].GetRotatedCells(currentRotation), 
                    floor);
            }
        }
        else
        {
            List<Vector2Int> cells = database.objectsData[selectedObjectIndex].GetRotatedCells(currentRotation);
            canPlace = CheckPlacementValidity(gridPosition, cells, false);
        }

        if (canPlace)
        {
            RestoreTemporaryNodes();

            GridData selectedData = database.IsBlock(currentID) ? BlockData : TowerData;
            Vector3 position = grid.CellToWorld(gridPosition);
            position += new Vector3(0.5f, floor, 0.5f);

            foreach (var cell in database.objectsData[selectedObjectIndex].GetRotatedCells(currentRotation))
            {
                Vector3 checkPosition = position + new Vector3(cell.x, 0, cell.y);
                Collider[] colliders = Physics.OverlapSphere(checkPosition, 0.5f);
                foreach (var collider in colliders)
                {
                    TreeDisapearEffect treeEffect = collider.GetComponent<TreeDisapearEffect>();
                    if (treeEffect != null)
                    {
                        ParticleSystem particle = Object.Instantiate(treeEffect.effect, treeEffect.transform.position, Quaternion.identity);
                        Object.Destroy(particle.gameObject, particle.main.duration);
                        Object.Destroy(treeEffect.gameObject);
                    }
                }
            }

            if (database.IsTower(currentID))
            {
                Vector3Int checkPos = new Vector3Int(gridPosition.x, floor, gridPosition.z);
                if (TowerData.IsPositionOccupied(checkPos))
                {
                    Debug.LogWarning($"Position {checkPos} is already occupied. Skipping tower placement.");
                    return;
                }
            }

            int index = objectPlacer.PlaceObject(database.objectsData[selectedObjectIndex].Prefab, 
                position, Quaternion.Euler(0, 90 * currentRotation, 0));

            selectedData.AddObjectAt(gridPosition, 
                database.objectsData[selectedObjectIndex].GetRotatedCells(currentRotation), 
                currentID, index, floor);

            if (!database.IsTower(currentID))
            {
                foreach (var cell in database.objectsData[selectedObjectIndex].GetRotatedCells(currentRotation))
                {
                    Vector3Int blockPos = new Vector3Int(gridPosition.x + cell.x, gridPosition.y, gridPosition.z + cell.y);
                    aGrid.UpdateNode(blockPos, true);
                }

                if (PathManager.Instance != null)
                {
                    PathManager.Instance.UpdateAllPaths();
                }
            }

            if (database.IsTower(currentID))
            {
                for (int i = 0; i < database.objectsData.Count; i++)
                {
                    if (database.objectsData[i].ID == currentID)
                    {
                        SoundManager.Instance.Play("TowerBuy", SoundManager.Sound.Effect);
                        GameManager.Instance.CurrentMoney -= database.objectsData[i].Price;
                        break;
                    }
                }
            }
        }

    }

    public bool IsValidPlacement(Vector3Int gridPosition)
    {
        if (database.IsTower(database.objectsData[selectedObjectIndex].ID))
        {
            return CheckTowerPlacement(gridPosition);
        }
        else
        {
            return CheckBlockPlacement(gridPosition);
        }
    }

    private bool CheckTowerPlacement(Vector3Int gridPosition)
    {
        Vector3Int positionBelow = new Vector3Int(gridPosition.x, 0, gridPosition.z);
        bool hasBlockBelow = BlockData.GetRepresentationIndex(positionBelow) != -1;
        bool hasObstacleBelow = false;
        bool canPlaceTower = false;

        Vector3 checkPosition = grid.CellToWorld(positionBelow) + new Vector3(0.5f, 0, 0.5f);
        Collider[] obstacles = Physics.OverlapSphere(checkPosition, 0.3f, aGrid.unwalkableMask);
        foreach (var obstacle in obstacles)
        {
            hasObstacleBelow = true;
            if (obstacle.CompareTag("TowerPlaceable"))
            {
                canPlaceTower = true;
                break;
            }
        }

        return (hasBlockBelow || (hasObstacleBelow && canPlaceTower)) &&
               TowerData.CanPlaceObjectAt(gridPosition, 
                   database.objectsData[selectedObjectIndex].GetRotatedCells(currentRotation), 
                   1);
    }

    private bool CheckBlockPlacement(Vector3Int gridPosition)
    {
        List<Vector2Int> cells = database.objectsData[selectedObjectIndex].GetRotatedCells(currentRotation);
        return GridData.Instance.CanPlaceObjectAt(gridPosition, cells) && CheckPlacementValidity(gridPosition, cells, true);
    }

    public void SetTemporaryNodes(Vector3Int gridPosition, bool walkable)
    {
        RestoreTemporaryNodes();

        List<Vector2Int> cells = database.objectsData[selectedObjectIndex].GetRotatedCells(currentRotation);
        foreach (var cell in cells)
        {
            Vector3Int blockPos = new Vector3Int(
                gridPosition.x + cell.x,
                gridPosition.y,
                gridPosition.z + cell.y
            );
            aGrid.SetTemporaryNodeState(blockPos, walkable);
        }
    }

    public void RestoreTemporaryNodes()
    {
        aGrid.RestoreTemporaryNodes();
    }

    public int GetCurrentID()
    {
        return database.objectsData[selectedObjectIndex].ID;
    }

    public GridData GetTowerData()
    {
        return TowerData;
    }
}
