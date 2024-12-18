using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class AGrid : MonoBehaviour
{
    public bool displayGridGizmos;
    public LayerMask unwalkableMask;
    public Vector2 gridWorldSize;
    public float nodeRadius;
    private float nodeDiameter;
    private int gridSizeX, gridSizeY;
    ANode[,] nodeArray;

    [SerializeField]
    private List<Vector2Int> obstaclePositions = new List<Vector2Int>();

    [SerializeField]
    private ObjectsDatabaseSO database;

    private GridData BlockData;
    private Dictionary<Vector3Int, bool> temporaryNodeStates = new Dictionary<Vector3Int, bool>();
    private Dictionary<GameObject, HashSet<Vector3Int>> obstacleOccupiedPositions = new Dictionary<GameObject, HashSet<Vector3Int>>();
    public int MaxSize
    {
        get
        {
            return gridSizeX * gridSizeY;
        }
    }

    void Awake()
    {
        nodeDiameter = nodeRadius * 2;
        gridSizeX = Mathf.RoundToInt(gridWorldSize.x / nodeDiameter);
        gridSizeY = Mathf.RoundToInt(gridWorldSize.y / nodeDiameter);
        BlockData = GridData.Instance;
        CreateGrid();
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

        GenerateNode(worldBottomLeft);
        CheckOccupiedByNode();
        ChangeNodeState(worldBottomLeft);

        if (PathManager.Instance != null)
        {
            PathManager.Instance.UpdateAllPaths();
        }
    }
    private void GenerateNode(Vector3 standardPos)
    {
        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                Vector3 worldPoint = standardPos + Vector3.right * (x + 0.5f) + Vector3.forward * (y + 0.5f);
                worldPoint.y = transform.position.y;
                Vector3Int gridPosition = new Vector3Int(x - gridSizeX / 2, 0, y - gridSizeY / 2);

                Vector3 checkPoint = worldPoint + Vector3.up * 0.1f;
                Collider[] obstacles = Physics.OverlapBox(checkPoint, new Vector3(0.4f, 0.1f, 0.4f), Quaternion.identity, unwalkableMask);

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
    }
    private void CheckOccupiedByNode()
    {
        foreach (var obstacleData in obstacleOccupiedPositions)
        {
            GameObject obstacle = obstacleData.Key;
            HashSet<Vector3Int> positions = obstacleData.Value;

            if (!ObjectPlacer.Instance.placedGameObjects.Contains(obstacle))
            {
                ObjectPlacer.Instance.placedGameObjects.Add(obstacle);
                int index = ObjectPlacer.Instance.placedGameObjects.Count - 1;

                Vector3Int basePosition = positions.First();
                List<Vector2Int> occupiedCells = new List<Vector2Int>();

                foreach (var pos in positions)
                {
                    Vector2Int relativePos = new Vector2Int(
                        pos.x - basePosition.x,
                        pos.z - basePosition.z
                    );
                    occupiedCells.Add(relativePos);
                }

                GridData.Instance.AddObjectAt(basePosition, occupiedCells, 0, index);
            }
        }
    }
    private void ChangeNodeState(Vector3 standardPos)
    {
        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                Vector3 worldPoint = standardPos + Vector3.right * (x + 0.5f) + Vector3.forward * (y + 0.5f);
                worldPoint.y = transform.position.y;
                Vector3Int gridPosition = new Vector3Int(x - gridSizeX / 2, 0, y - gridSizeY / 2);

                bool isBlocked = GridData.Instance.IsPositionOccupied(gridPosition);
                bool walkable = !isBlocked;
                nodeArray[x, y] = new ANode(walkable, worldPoint, x, y);
            }
        }

    }

    /// <summary>
    /// 한칸씩 경로를 계산하도록 유도
    /// </summary>
    /// <param name="node"></param>
    /// <returns></returns>
    public List<ANode> GetNeighbours(ANode node)
    {
        List<ANode> neighbours = new List<ANode>();

        Vector2Int[] directions = new Vector2Int[]
        {
            new Vector2Int(0, 1),   
            new Vector2Int(1, 1),   
            new Vector2Int(1, 0),   
            new Vector2Int(1, -1),  
            new Vector2Int(0, -1),  
            new Vector2Int(-1, -1), 
            new Vector2Int(-1, 0),  
            new Vector2Int(-1, 1)   
        };

        foreach (var dir in directions)
        {
            int checkX = node.gridX + dir.x;
            int checkY = node.gridY + dir.y;

            if (checkX >= 0 && checkX < gridSizeX && checkY >= 0 && checkY < gridSizeY)
            {
                if (Mathf.Abs(dir.x) == 1 && Mathf.Abs(dir.y) == 1)
                {
                    bool canMoveDiagonally = true;

                    int horizontalX = node.gridX + dir.x;
                    int horizontalY = node.gridY;
                    if (horizontalX >= 0 && horizontalX < gridSizeX)
                    {
                        if (!nodeArray[horizontalX, horizontalY].walkable)
                            canMoveDiagonally = false;
                    }

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

    public void UpdateNode(Vector3Int position, bool isBlocked)
    {
        int gridX = position.x + gridSizeX / 2;
        int gridY = position.z + gridSizeY / 2;

        if (gridX >= 0 && gridX < gridSizeX && gridY >= 0 && gridY < gridSizeY)
        {
            bool previousState = nodeArray[gridX, gridY].walkable;
            nodeArray[gridX, gridY].walkable = !isBlocked;
            
            bool gridDataOccupied = GridData.Instance.IsPositionOccupied(position);
        }
    }

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
            }
        }
    }

    public void RestoreTemporaryNodes()
    {
        foreach (var kvp in temporaryNodeStates)
        {
            int gridX = kvp.Key.x + gridSizeX / 2;
            int gridY = kvp.Key.z + gridSizeY / 2;

            if (gridX >= 0 && gridX < gridSizeX && gridY >= 0 && gridY < gridSizeY)
            {
                nodeArray[gridX, gridY].walkable = kvp.Value;
            }
        }
        
        int count = temporaryNodeStates.Count;
        temporaryNodeStates.Clear();
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

    public void Clear()
    {
        if (BlockData != null)
        {
            BlockData.Clear();
        }

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

        obstaclePositions.Clear();
        obstacleOccupiedPositions.Clear();

        CreateGrid();

        if (PathManager.Instance != null)
        {
            PathManager.Instance.UpdateAllPaths();
        }
    }
}