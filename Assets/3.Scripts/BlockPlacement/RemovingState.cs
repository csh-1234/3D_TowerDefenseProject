using System;
using UnityEngine;

public class RemovingState : IBuildingState
{
    Grid grid;
    PreviewSystem previewSystem;
    GridData BlockData;
    GridData TowerData;
    ObjectPlacer objectPlacer;
    AGrid aGrid;
    InputManager inputManager;

    public RemovingState(Grid grid, PreviewSystem previewSystem, GridData blockData, GridData towerData, ObjectPlacer objectPlacer, AGrid aGrid, InputManager inputManager)
    {
        this.grid = grid;
        this.previewSystem = previewSystem;
        this.BlockData = blockData;
        this.TowerData = towerData;
        this.objectPlacer = objectPlacer;
        this.aGrid = aGrid;
        this.inputManager = inputManager;
        previewSystem.StartShowingRemovePreview();
    }

    public void EndState()
    {
        previewSystem.StopShowingPreview();
        if (aGrid != null)
        {
            aGrid.RestoreTemporaryNodes();
        }
    }

    public void OnAction(Vector3Int gridPosition)
    {
        if (inputManager.IsPointerOverUI())
        {
            return;
        }

        PlacementData data = GridData.Instance.GetPlacementData(gridPosition);
        
        if (data != null)
        {
            aGrid.RestoreTemporaryNodes();
            
            GridData.Instance.RemoveObjectAt(gridPosition);
            
            foreach (var pos in data.occupiedPositions)
            {
                aGrid.UpdateNode(pos, false);
            }

            if (PathManager.Instance != null)
            {
                PathManager.Instance.UpdateAllPaths();
            }
        }
    }

    private bool CheckIfSelectionIsValid(Vector3Int gridPosition)
    {
        return TowerData.GetRepresentationIndex(gridPosition) != -1 ||  BlockData.GetRepresentationIndex(gridPosition) != -1;
    }

    public void UpdateState(Vector3Int gridPosition)
    {
        bool validity = CheckIfSelectionIsValid(gridPosition);
        previewSystem.UpdatePosition(grid.CellToWorld(gridPosition), validity);
    }

    public void OnAction(object gridpo)
    {
        throw new NotImplementedException();
    }
}