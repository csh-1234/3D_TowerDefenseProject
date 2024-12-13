using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class AGrid : MonoBehaviour
{
    public bool displayGridGizmos;
    public LayerMask unwalkableMask;
    public Vector2 gridWorldSize;
    public float nodeRadius;
    ANode[,] nodeArray;
    float nodeDiameter;
    int gridSizeX, gridSizeY;

    [SerializeField]
    private List<Vector2Int> obstaclePositions = new List<Vector2Int>();

    [SerializeField]
    private ObjectsDatabaseSO database;

    private GridData BlockData;

    // 노드의 임시 상태를 저장할 Dictionary 추가
    private Dictionary<Vector3Int, bool> temporaryNodeStates = new Dictionary<Vector3Int, bool>();

    void Awake()
    {
        nodeDiameter = nodeRadius * 2;
        gridSizeX = Mathf.RoundToInt(gridWorldSize.x / nodeDiameter);
        gridSizeY = Mathf.RoundToInt(gridWorldSize.y / nodeDiameter);
        BlockData = GridData.Instance;
        CreateGrid();
    }

    public int MaxSize
    {
        get
        {
            return gridSizeX * gridSizeY;
        }
    }

    void CreateGrid()
    {
        nodeArray = new ANode[gridSizeX, gridSizeY];
        Vector3 worldBottomLeft = transform.position - Vector3.right * gridWorldSize.x / 2 - Vector3.forward * gridWorldSize.y / 2;
        worldBottomLeft.y = transform.position.y;

        if (ObjectPlacer.Instance == null)
        {
            GameObject placerObj = new GameObject("ObjectPlacer");
            placerObj.AddComponent<ObjectPlacer>();
        }

        // 먼저 모든 장애물을 찾고 각각의 크기를 체크
        Dictionary<GameObject, HashSet<Vector3Int>> obstacleOccupiedPositions = new Dictionary<GameObject, HashSet<Vector3Int>>();
        
        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                Vector3 worldPoint = worldBottomLeft + Vector3.right * (x + 0.5f) + Vector3.forward * (y + 0.5f);
                worldPoint.y = transform.position.y;
                Vector3Int gridPosition = new Vector3Int(x - gridSizeX/2, 0, y - gridSizeY/2);

                // 각 노드에서 장애물 체크
                Vector3 checkPoint = worldPoint + Vector3.up * 0.1f;
                Collider[] obstacles = Physics.OverlapBox(
                    checkPoint,
                    new Vector3(0.4f, 0.1f, 0.4f),
                    Quaternion.identity,
                    unwalkableMask
                );

                foreach (var obstacle in obstacles)
                {
                    if (!obstacleOccupiedPositions.ContainsKey(obstacle.gameObject))
                    {
                        obstacleOccupiedPositions[obstacle.gameObject] = new HashSet<Vector3Int>();
                    }
                    obstacleOccupiedPositions[obstacle.gameObject].Add(gridPosition);
                }
            }
        }

        // 각 장애물별로 차지하는 모든 노드를 GridData에 등록
        foreach (var obstacleData in obstacleOccupiedPositions)
        {
            GameObject obstacle = obstacleData.Key;
            HashSet<Vector3Int> positions = obstacleData.Value;

            if (!ObjectPlacer.Instance.placedGameObjects.Contains(obstacle))
            {
                ObjectPlacer.Instance.placedGameObjects.Add(obstacle);
                int index = ObjectPlacer.Instance.placedGameObjects.Count - 1;

                // 장애물이 차지하는 모든 셀을 Vector2Int로 변환
                Vector3Int basePosition = positions.First(); // 기준점
                List<Vector2Int> occupiedCells = new List<Vector2Int>();
                
                foreach (var pos in positions)
                {
                    Vector2Int relativePos = new Vector2Int(
                        pos.x - basePosition.x,
                        pos.z - basePosition.z
                    );
                    occupiedCells.Add(relativePos);
                }

                // GridData에 추가
                GridData.Instance.AddObjectAt(basePosition, occupiedCells, 0, index);
                Debug.Log($"Added large obstacle at base position ({basePosition.x}, {basePosition.z}) occupying {positions.Count} cells");
            }
        }

        // 노드 배열 생성
        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                Vector3 worldPoint = worldBottomLeft + Vector3.right * (x + 0.5f) + Vector3.forward * (y + 0.5f);
                worldPoint.y = transform.position.y;
                Vector3Int gridPosition = new Vector3Int(x - gridSizeX/2, 0, y - gridSizeY/2);
                
                bool isBlocked = GridData.Instance.IsPositionOccupied(gridPosition);
                bool walkable = !isBlocked;
                nodeArray[x, y] = new ANode(walkable, worldPoint, x, y);
            }
        }

        // 경로 재계산
        if (PathManager.Instance != null)
        {
            PathManager.Instance.UpdateAllPaths();
        }
    }

    public List<ANode> GetNeighbours(ANode node)
    {
        List<ANode> neighbours = new List<ANode>();

        Vector2Int[] directions = new Vector2Int[]
        {
            new Vector2Int(0, 1),   // 상
            new Vector2Int(1, 1),   // 우상
            new Vector2Int(1, 0),   // 우
            new Vector2Int(1, -1),  // 우하
            new Vector2Int(0, -1),  // 하
            new Vector2Int(-1, -1), // 좌하
            new Vector2Int(-1, 0),  // 좌
            new Vector2Int(-1, 1)   // 좌상
        };

        foreach (var dir in directions)
        {
            int checkX = node.gridX + dir.x;
            int checkY = node.gridY + dir.y;

            if (checkX >= 0 && checkX < gridSizeX && checkY >= 0 && checkY < gridSizeY)
            {
                // 대각선 이동의 경우 양쪽 노드가 모두 walkable인지 확인
                if (Mathf.Abs(dir.x) == 1 && Mathf.Abs(dir.y) == 1)
                {
                    bool canMoveDiagonally = true;
                    
                    // 수평 이동 가능 확인
                    int horizontalX = node.gridX + dir.x;
                    int horizontalY = node.gridY;
                    if (horizontalX >= 0 && horizontalX < gridSizeX)
                    {
                        if (!nodeArray[horizontalX, horizontalY].walkable)
                            canMoveDiagonally = false;
                    }

                    // 수직 이동 가능 확인
                    int verticalX = node.gridX;
                    int verticalY = node.gridY + dir.y;
                    if (verticalY >= 0 && verticalY < gridSizeY)
                    {
                        if (!nodeArray[verticalX, verticalY].walkable)
                            canMoveDiagonally = false;
                    }

                    if (!canMoveDiagonally)
                        continue;
                }

                neighbours.Add(nodeArray[checkX, checkY]);
            }
        }

        return neighbours;
    }

    public ANode ANodeFromWorldPoint(Vector3 worldPosition)
    {
        float percentX = (worldPosition.x + gridWorldSize.x / 2) / gridWorldSize.x;
        float percentY = (worldPosition.z + gridWorldSize.y / 2) / gridWorldSize.y;
        percentX = Mathf.Clamp01(percentX);
        percentY = Mathf.Clamp01(percentY);

        int x = Mathf.RoundToInt((gridSizeX - 1) * percentX);
        int y = Mathf.RoundToInt((gridSizeY - 1) * percentY);
        return nodeArray[x, y];
    }

    void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(transform.position, new Vector3(gridWorldSize.x, 1, gridWorldSize.y));
        if (nodeArray != null && displayGridGizmos)
        {
            foreach (ANode n in nodeArray)
            {
                Gizmos.color = (n.walkable) ? Color.white : Color.red;
                Gizmos.DrawCube(n.worldPosition, Vector3.one * 0.1f);
            }
        }
    }

    public void UpdateGrid()
    {
        CreateGrid();
    }

    public void UpdateNode(Vector3Int position, bool isBlocked)
    {
        int gridX = position.x + gridSizeX / 2;
        int gridY = position.z + gridSizeY / 2;

        if (gridX >= 0 && gridX < gridSizeX && gridY >= 0 && gridY < gridSizeY)
        {
            bool previousState = nodeArray[gridX, gridY].walkable;
            nodeArray[gridX, gridY].walkable = !isBlocked;
            
            // 노드 상태 변경 로깅 강화
            Debug.Log($"Node at ({gridX}, {gridY}) - Previous state: {previousState}, New state: {!isBlocked}");
            
            // 임시 상태인지 확인
            if (temporaryNodeStates.ContainsKey(position))
            {
                Debug.Log($"Warning: Updating node that has temporary state at ({gridX}, {gridY})");
            }

            // GridData와 상태 비교
            bool gridDataOccupied = GridData.Instance.IsPositionOccupied(position);
            if (gridDataOccupied != isBlocked)
            {
                Debug.LogWarning($"State mismatch at ({gridX}, {gridY}): GridData: {gridDataOccupied}, Node blocked: {isBlocked}");
            }
        }
        else
        {
            Debug.LogError($"Attempted to update node outside grid bounds at ({gridX}, {gridY}). Original position: ({position.x}, {position.z})");
        }
    }

    // 노드의 임시 상태 설정
    public void SetTemporaryNodeState(Vector3Int position, bool walkable)
    {
        int gridX = position.x + gridSizeX / 2;
        int gridY = position.z + gridSizeY / 2;

        if (gridX >= 0 && gridX < gridSizeX && gridY >= 0 && gridY < gridSizeY)
        {
            if (!temporaryNodeStates.ContainsKey(position))
            {
                temporaryNodeStates[position] = nodeArray[gridX, gridY].walkable;
                nodeArray[gridX, gridY].walkable = walkable;
                Debug.Log($"Set temporary state at ({gridX}, {gridY}): {walkable}");
            }
            else
            {
                Debug.LogWarning($"Attempted to set temporary state for node that already has one at ({gridX}, {gridY})");
            }
        }
    }

    // 임시 상태 복원
    public void RestoreTemporaryNodes()
    {
        foreach (var kvp in temporaryNodeStates)
        {
            int gridX = kvp.Key.x + gridSizeX / 2;
            int gridY = kvp.Key.z + gridSizeY / 2;

            if (gridX >= 0 && gridX < gridSizeX && gridY >= 0 && gridY < gridSizeY)
            {
                nodeArray[gridX, gridY].walkable = kvp.Value;
                Debug.Log($"Restored node at ({gridX}, {gridY}) to {kvp.Value}");
            }
        }
        
        int count = temporaryNodeStates.Count;
        temporaryNodeStates.Clear();
        Debug.Log($"Restored {count} temporary nodes");
    }

    private bool CheckPathValidity(Vector3Int gridPosition, List<Vector2Int> cells)
    {
        // 임시로 노드들을 막음
        Dictionary<ANode, bool> originalStates = new Dictionary<ANode, bool>();
        foreach (var cell in cells)
        {
            Vector3Int blockPos = new Vector3Int(
                gridPosition.x + cell.x,
                gridPosition.y,
                gridPosition.z + cell.y
            );
            ANode node = ANodeFromWorldPoint(new Vector3(blockPos.x, 0, blockPos.z));
            if (node != null && !originalStates.ContainsKey(node))
            {
                originalStates[node] = node.walkable;
                node.walkable = false;
            }
        }

        // 프리뷰 경로 업데이트 전에 모든 스폰 포인트에서 경로 체크
        bool anyValidPath = false;
        foreach (var spawnPoint in PathManager.Instance.GetSpawnPoints())
        {
            PathManager.Instance.CheckPreviewPath();
            if (PathManager.Instance.HasValidPath)
            {
                anyValidPath = true;
                break;
            }
        }

        // 노드 상태 복원
        foreach (var pair in originalStates)
        {
            pair.Key.walkable = pair.Value;
        }

        return anyValidPath;
    }

    public void ResetNodes()
    {
        if (nodeArray != null)
        {
            for (int x = 0; x < gridSizeX; x++)
            {
                for (int y = 0; y < gridSizeY; y++)
                {
                    Vector3 worldPoint = WorldPointFromGrid(new Vector2Int(x, y));
                    Vector3Int gridPosition = new Vector3Int(
                        Mathf.RoundToInt(worldPoint.x),
                        Mathf.RoundToInt(worldPoint.y),
                        Mathf.RoundToInt(worldPoint.z)
                    );
                    bool isBlocked = GridData.Instance.IsPositionOccupied(gridPosition);
                    nodeArray[x, y].walkable = !isBlocked;
                }
            }
        }
    }

    public Vector3 WorldPointFromGrid(Vector2Int gridPosition)
    {
        float x = gridPosition.x * nodeDiameter;
        float z = gridPosition.y * nodeDiameter;
        return new Vector3(x + (nodeRadius), 0, z + (nodeRadius));
    }

    public void Clear()
    {
        // GridData 초기화
        if (BlockData != null)
        {
            BlockData.Clear();
        }

        // 노드 배열 초기화
        if (nodeArray != null)
        {
            for (int x = 0; x < gridSizeX; x++)
            {
                for (int y = 0; y < gridSizeY; y++)
                {
                    nodeArray[x, y].walkable = true;
                    nodeArray[x, y].parent = null;
                    nodeArray[x, y].gCost = 0;
                    nodeArray[x, y].hCost = 0;
                    nodeArray[x, y].directionPreference = 0;
                }
            }
        }

        // 장애물 위치 리스트 초기화
        obstaclePositions.Clear();

        // 그리드 재생성
        CreateGrid();

        // 경로 업데이트 요청
        if (PathManager.Instance != null)
        {
            PathManager.Instance.UpdateAllPaths();
        }
    }
}