using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

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
    private ObjectPlacer placer;

    public bool HasObjectAt(Vector3Int position)
    {
        return placedObjects.ContainsKey(position);
    }

    public PlacementData GetObjectAt(Vector3Int position)
    {
        if (placedObjects.ContainsKey(position))
            return placedObjects[position];
        return null;
    }

    public void AddObjectAt(Vector3Int gridPosition, List<Vector2Int> occupiedCells, int ID, int placedObjectIndex, int floor = 0)
    {
        gridPosition.y = floor;
        List<Vector3Int> positionToOccupy = new List<Vector3Int>();

        // 타워인지 확인
        bool isTower = ID >= ObjectsDatabaseSO.TOWER_ID_START && ID <= ObjectsDatabaseSO.TOWER_ID_END;
        Debug.Log($"Adding {(isTower ? "Tower" : "Block")} - ID: {ID}, Floor: {floor}, Index: {placedObjectIndex}");

        // 타워인 경우 자신의 위치만 점유
        if (isTower)
        {
            positionToOccupy.Add(new Vector3Int(gridPosition.x, floor, gridPosition.z));
            Debug.Log($"Tower will occupy position: ({gridPosition.x}, {floor}, {gridPosition.z})");
        }
        // 블록인 경우 모든 셀 점유
        else
        {
            foreach (var cell in occupiedCells)
            {
                Vector3Int pos = new Vector3Int(
                    gridPosition.x + cell.x,
                    floor,
                    gridPosition.z + cell.y
                );
                positionToOccupy.Add(pos);
            }
        }

        PlacementData data = new PlacementData(positionToOccupy, ID, placedObjectIndex);

        // 기존 데이터 확인
        foreach (var pos in positionToOccupy)
        {
            if (placedObjects.ContainsKey(pos))
            {
                var existingData = placedObjects[pos];
                Debug.LogError($"Position {pos} is already occupied by {(existingData.ID >= ObjectsDatabaseSO.TOWER_ID_START ? "Tower" : "Block")} with ID: {existingData.ID}, Index: {existingData.PlacedObjectIndex}");
                throw new Exception($"Dictionary already contains this cell position {pos}");
            }
        }

        // 새 데이터 추가
        foreach (var pos in positionToOccupy)
        {
            placedObjects[pos] = data;
            Debug.Log($"Added position {pos} to {(isTower ? "Tower" : "Block")}");
        }

        // 현재 Dictionary 상태 출력
        Debug.Log($"Total objects after addition: {placedObjects.Count}");
        foreach (var kvp in placedObjects)
        {
            Debug.Log($"Stored - Position: {kvp.Key}, ID: {kvp.Value.ID}, Floor: {kvp.Key.y}, Index: {kvp.Value.PlacedObjectIndex}");
        }
    }

    public void UpdateMapObject(Vector2 placedPosition)
    {
        Vector3Int newPos = new Vector3Int((int)placedPosition.x, 0, (int)placedPosition.y);

        List<Vector2Int> occupiedCells = new List<Vector2Int> { new Vector2Int(0, 0) };
        List<Vector3Int> positionToOccupy = CalculatePositions(newPos, occupiedCells);

        int blockID = ObjectsDatabaseSO.BLOCK_ID_START + 1;
        PlacementData data = new PlacementData(positionToOccupy, blockID, ObjectPlacer.Instance.placedGameObjects.Count);

        if (placedObjects.ContainsKey(newPos))
        {
            placedObjects.Remove(newPos);
        }

        placedObjects[newPos] = data;

        GameObject go = new GameObject("MapBlock");
        go.transform.position = newPos;
        go.transform.rotation = Quaternion.identity;
        ObjectPlacer.Instance.placedGameObjects.Add(go);

        Debug.Log($"Added map block at position: {newPos}, ID: {data.ID}, Index: {data.PlacedObjectIndex}");
    }

    private List<Vector3Int> CalculatePositions(Vector3Int gridPosition, List<Vector2Int> occupiedCells)
    {
        List<Vector3Int> returnVal = new();
        foreach (var cell in occupiedCells)
        {
            returnVal.Add(new Vector3Int(
                gridPosition.x + cell.x,
                gridPosition.y,
                gridPosition.z + cell.y
            ));
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
            Debug.LogWarning($"No object found at position: {gridPosition}, Floor: {gridPosition.y}");
            return;
        }

        PlacementData data = placedObjects[gridPosition];
        bool isTower = data.ID >= ObjectsDatabaseSO.TOWER_ID_START && data.ID <= ObjectsDatabaseSO.TOWER_ID_END;
        Debug.Log($"Removing {(isTower ? "Tower" : "Block")} at {gridPosition}, ID: {data.ID}, Floor: {gridPosition.y}, Index: {data.PlacedObjectIndex}");

        // 제거 전 Dictionary 상태 출력
        Debug.Log($"Objects before removal: {placedObjects.Count}");

        // 모든 점유된 위치에서 데이터 제거
        foreach (var pos in data.occupiedPositions)
        {
            if (placedObjects.ContainsKey(pos))
            {
                placedObjects.Remove(pos);
                Debug.Log($"Removed {(isTower ? "tower" : "block")} data at position: {pos}");
            }
        }

        // ObjectPlacer에서 오브젝트 제거
        //if (data.PlacedObjectIndex >= 0)
        //{
        //    ObjectPlacer.Instance?.RemoveObjectAt(data.PlacedObjectIndex);
        //}

        // 제거 후 Dictionary 상태 출력
        Debug.Log($"Remaining objects after removal: {placedObjects.Count}");
        foreach (var kvp in placedObjects)
        {
            Debug.Log($"Position: {kvp.Key}, ID: {kvp.Value.ID}, Floor: {kvp.Key.y}");
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
        if (isOccupied)
        {
            Debug.Log($"Position {position} is occupied by object with ID: {placedObjects[position].ID}, Floor: {position.y}");
            Debug.Log($"Total objects in dictionary: {placedObjects.Count}");
        }
        return isOccupied;
    }

    public Vector3Int WorldToGridPosition(Vector3 worldPosition)
    {
        Vector3Int gridPos = new Vector3Int(
            Mathf.FloorToInt(worldPosition.x),
            Mathf.RoundToInt(worldPosition.y),
            Mathf.FloorToInt(worldPosition.z)
        );
        Debug.Log($"Converting world position {worldPosition} to grid position {gridPos}");
        return gridPos;
    }

    public void Clear()
    {
        // 모든 배치된 오브젝트 데이터 초기화
        placedObjects.Clear();
        
        // ObjectPlacer 참조 초기화
        placer = null;
        
        // 싱글톤 인스턴스 초기화
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