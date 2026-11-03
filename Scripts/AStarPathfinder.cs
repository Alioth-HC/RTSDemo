
using System.Collections.Generic;
using UnityEngine;

public class AStarPathfinder : MonoBehaviour
{
    [Header("网格引用")]
    public GridManager gridManager;

    // 一个节点代表搜索过程中的一个格子
    private class Node
    {
        public Vector2Int position;
        public int gCost;
        public int hCost;
        public Node parent;

        public int fCost
        {
            get { return gCost + hCost; }
        }

        public Node(Vector2Int position)
        {
            this.position = position;
        }
    }

    // 八个方向：上下左右，以及四个对角方向
    private readonly Vector2Int[] directions =
    {
        new Vector2Int( 0,  1),
        new Vector2Int( 0, -1),
        new Vector2Int( 1,  0),
        new Vector2Int(-1,  0),
        new Vector2Int( 1,  1),
        new Vector2Int( 1, -1),
        new Vector2Int(-1,  1),
        new Vector2Int(-1, -1)
    };

    // 输入起点和终点，返回路径上的格子坐标
    public List<Vector2Int> FindPath(
        Vector2Int start,
        Vector2Int target)
    {
        if (gridManager == null)
        {
            Debug.LogError("AStarPathfinder 没有设置 GridManager！");
            return null;
        }

        if (!gridManager.IsWalkable(start) ||
            !gridManager.IsWalkable(target))
        {
            Debug.LogWarning("起点或终点不可通行！");
            return null;
        }

        List<Node> openList = new List<Node>();
        HashSet<Vector2Int> closedSet = new HashSet<Vector2Int>();

        Node startNode = new Node(start);
        startNode.gCost = 0;
        startNode.hCost = GetDistance(start, target);

        openList.Add(startNode);

        while (openList.Count > 0)
        {
            // 找出当前代价最低的节点
            Node currentNode = GetLowestFCostNode(openList);

            // 找到终点，回溯生成路径
            if (currentNode.position == target)
            {
                return RetracePath(currentNode);
            }

            openList.Remove(currentNode);
            closedSet.Add(currentNode.position);

            // 检查当前格子的所有邻居
            foreach (Vector2Int direction in directions)
            {
                Vector2Int neighborPosition =
                    currentNode.position + direction;

                if (!gridManager.IsWalkable(neighborPosition))
                    continue;

                if (closedSet.Contains(neighborPosition))
                    continue;

                // 防止士兵从两个障碍物的夹角中斜穿过去
                bool isDiagonal =
                    direction.x != 0 && direction.y != 0;

                if (isDiagonal)
                {
                    Vector2Int sideA = new Vector2Int(
                        currentNode.position.x + direction.x,
                        currentNode.position.y
                    );

                    Vector2Int sideB = new Vector2Int(
                        currentNode.position.x,
                        currentNode.position.y + direction.y
                    );

                    if (!gridManager.IsWalkable(sideA) ||
                        !gridManager.IsWalkable(sideB))
                    {
                        continue;
                    }
                }

                int moveCost = isDiagonal ? 14 : 10;
                int newGCost = currentNode.gCost + moveCost;

                Node neighborNode = openList.Find(
                    node => node.position == neighborPosition
                );

                // 已经发现过这个格子，且新路线没有更优
                if (neighborNode != null &&
                    newGCost >= neighborNode.gCost)
                {
                    continue;
                }

                // 首次发现，或找到了一条更短的路线
                if (neighborNode == null)
                {
                    neighborNode = new Node(neighborPosition);
                    openList.Add(neighborNode);
                }

                neighborNode.gCost = newGCost;
                neighborNode.hCost =
                    GetDistance(neighborPosition, target);
                neighborNode.parent = currentNode;
            }
        }

        // 搜索结束仍然没有找到目标
        Debug.LogWarning("没有找到可行路径！");
        return null;
    }

    // 选择 F 值最小的节点
    private Node GetLowestFCostNode(List<Node> openList)
    {
        Node bestNode = openList[0];

        for (int i = 1; i < openList.Count; i++)
        {
            Node node = openList[i];

            if (node.fCost < bestNode.fCost ||
                (node.fCost == bestNode.fCost &&
                 node.hCost < bestNode.hCost))
            {
                bestNode = node;
            }
        }

        return bestNode;
    }

    // 计算两个格子之间的估算距离
    private int GetDistance(Vector2Int a, Vector2Int b)
    {
        int dx = Mathf.Abs(a.x - b.x);
        int dy = Mathf.Abs(a.y - b.y);

        int diagonal = Mathf.Min(dx, dy);
        int straight = Mathf.Max(dx, dy) - diagonal;

        return diagonal * 14 + straight * 10;
    }

    // 从终点沿 parent 回溯，再反转为起点到终点
    private List<Vector2Int> RetracePath(Node endNode)
    {
        List<Vector2Int> path = new List<Vector2Int>();
        Node currentNode = endNode;

        while (currentNode != null)
        {
            path.Add(currentNode.position);
            currentNode = currentNode.parent;
        }

        path.Reverse();
        return path;
    }
}
