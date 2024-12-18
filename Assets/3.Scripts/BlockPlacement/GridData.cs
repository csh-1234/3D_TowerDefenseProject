using System.Collections.Generic;
using UnityEngine;

public class GridData
{
    private static GridData instance;
    public static GridData Instance
    {
        get
        {
            if (instance == null)
                instance = new GridData();
            return instance;
        }
    }

    private Dictionary<Vector3Int, PlacementData> placedObjects = new();

    public void AddObjectAt(Vector3Int gridPosition, List<Vector2Int> occupiedCells, int ID, int placedObjectIndex, int floor = 0)
    {
        gridPosition.y = floor;
        List<Vector3Int> positionToOccupy = new List<Vector3Int>();

        bool isTower = (ID >= ObjectsDatabaseSO.TOWER_ID_START) && (ID <= ObjectsDatabaseSO.TOWER_ID_END);

        if (isTower)
        {
            positionToOccupy.Add(new Vector3Int(gridPosition.x, floor, gridPosition.z));
        }
        else
        {
            foreach (var cell in occupiedCells)
            {
                Vector3Int pos = new Vector3Int(gridPosition.x + cell.x, floor, gridPosition.z + cell.y);
                positionToOccupy.Add(pos);
            }
        }

        PlacementData data = new PlacementData(positionToOccupy, ID, placedObjectIndex);

        foreach (var pos in positionToOccupy)
        {
            if (placedObjects.ContainsKey(pos))
            {
                var existingData = placedObjects[pos];
            }
        }

        foreach (var pos in positionToOccupy)
        {
            placedObjects[pos] = data;
        }
    }
    
    private List<Vector3Int> CalculatePositions(Vector3Int gridPosition, List<Vector2Int> occupiedCells)
    {
        List<Vector3Int> returnVal = new();
        foreach (var cell in occupiedCells)
        {
            returnVal.Add(new Vector3Int(gridPosition.x + cell.x, gridPosition.y,gridPosition.z + cell.y));
        }
        return returnVal;
    }

    public bool CanPlaceObjectAt(Vector3Int gridPosition, List<Vector2Int> occupiedCells, int floor = 0)
    {
        gridPosition.y = floor;
        List<Vector3Int> positionToOccupy = CalculatePositions(gridPosition, occupiedCells);
        
        foreach (var pos in positionToOccupy)
        {
            if (IsPositionOccupied(pos))
                return false;
        }
        return true;
    }

    public int GetRepresentationIndex(Vector3Int gridPosition)
    {
        if (placedObjects.ContainsKey(gridPosition) == false)
            return -1;
        return placedObjects[gridPosition].PlacedObjectIndex;
    }

    public void RemoveObjectAt(Vector3Int gridPosition)
    {
        if (!placedObjects.ContainsKey(gridPosition))
        {
            return;
        }

        PlacementData data = placedObjects[gridPosition];
        bool isTower = (data.ID >= ObjectsDatabaseSO.TOWER_ID_START) && (data.ID <= ObjectsDatabaseSO.TOWER_ID_END);

        foreach (var pos in data.occupiedPositions)
        {
            if (placedObjects.ContainsKey(pos))
            {
                placedObjects.Remove(pos);
            }
        }
    }

    public PlacementData GetPlacementData(Vector3Int gridPosition)
    {
        if (placedObjects.ContainsKey(gridPosition))
            return placedObjects[gridPosition];
        return null;
    }

    public bool IsPositionOccupied(Vector3Int position)
    {
        bool isOccupied = placedObjects.ContainsKey(position);
        return isOccupied;
    }

    public Vector3Int WorldToGridPosition(Vector3 worldPosition)
    {
        Vector3Int gridPos = new Vector3Int(
            Mathf.FloorToInt(worldPosition.x),
            Mathf.RoundToInt(worldPosition.y),
            Mathf.FloorToInt(worldPosition.z));
        return gridPos;
    }

    public void Clear()
    {
        placedObjects.Clear();
        instance = null;
    }
}

public class PlacementData
{
    public List<Vector3Int> occupiedPositions;
    public int ID { get; private set; }
    public int PlacedObjectIndex { get; private set; }

    public PlacementData(List<Vector3Int> occupiedPositions, int iD, int placedObjectIndex)
    {
        this.occupiedPositions = occupiedPositions;
        ID = iD;
        PlacedObjectIndex = placedObjectIndex;
    }
}