using UnityEngine;

public class GridManager : MonoBehaviour
{
    [Header("网格设置")]
    public int width = 40;
    public int height = 40;
    public float cellSize = 1f;

    [Header("显示设置")]
    public bool showGrid = true;

    [Header("格子选择")]
    public Camera mainCamera;
    public LayerMask groundLayer = ~0;

    private Vector2Int selectedGrid;
    private bool hasSelection = false;

    //用二维数组记录哪些格子是可行走的，哪些是障碍物
    private bool[,] walkableGrid;

    void Awake()
    {
        walkableGrid = new bool[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                walkableGrid[x, y] = true;
            }
        }
    }

    // 将世界坐标转换成网格坐标
    public Vector2Int WorldToGrid(Vector3 worldPosition)
    {
        int x = Mathf.FloorToInt(worldPosition.x / cellSize + width / 2f);
        int z = Mathf.FloorToInt(worldPosition.z / cellSize + height / 2f);

        return new Vector2Int(x, z);
    }

    // 将网格坐标转换成世界坐标
    public Vector3 GridToWorld(Vector2Int gridPosition)
    {
        float x = (gridPosition.x - width / 2f + 0.5f) * cellSize;
        float z = (gridPosition.y - height / 2f + 0.5f) * cellSize;

        return new Vector3(x, 0f, z);
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                selectedGrid = WorldToGrid(hit.point);

                if (selectedGrid.x >= 0 && selectedGrid.x < width &&
                    selectedGrid.y >= 0 && selectedGrid.y < height)
                {
                    hasSelection = true;

                    Debug.Log("选中格子：" + selectedGrid);
                }
            }
        }
    }

    // 在编辑器中绘制网格线，方便观察
    void OnDrawGizmos()
    {
        if (!showGrid)
            return;

        Gizmos.color = Color.green;

        float startX = -width * cellSize / 2f;
        float startZ = -height * cellSize / 2f;

        for (int x = 0; x <= width; x++)
        {
            float worldX = startX + x * cellSize;

            Gizmos.DrawLine(
                new Vector3(worldX, 0.05f, startZ),
                new Vector3(worldX, 0.05f, -startZ)
            );
        }

        for (int z = 0; z <= height; z++)
        {
            float worldZ = startZ + z * cellSize;

            Gizmos.DrawLine(
                new Vector3(startX, 0.05f, worldZ),
                new Vector3(-startX, 0.05f, worldZ)
            );
        }

        if (hasSelection)
        {
            Gizmos.color = Color.yellow;

            Vector3 center = GridToWorld(selectedGrid);
            center.y = 0.1f;

            Gizmos.DrawCube(
                center,
                new Vector3(cellSize, 0.05f, cellSize)
            );
        }
    }

    public bool IsWalkable(Vector2Int position)
    {
        if (position.x < 0 || position.x >= width ||
            position.y < 0 || position.y >= height)
        {
            return false;
        }

        return walkableGrid[position.x, position.y];
    }

    public void SetWalkable(Vector2Int position, bool walkable)
    {
        if (position.x < 0 || position.x >= width ||
            position.y < 0 || position.y >= height)
        {
            return;
        }

        walkableGrid[position.x, position.y] = walkable;
    }
}
