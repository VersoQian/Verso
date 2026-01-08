using System.Collections.Generic;
using UnityEngine;

namespace Game.Pathfinding
{
    /// <summary>
    /// 寻路网格系统 - 管理整个关卡的寻路网格
    /// </summary>
    public class PathfindingGrid : MonoBehaviour
    {
        [Header("网格设置")]
        public Vector2 gridWorldSize = new Vector2(100, 100); // 网格世界大小
        public float nodeRadius = 0.5f; // 节点半径(网格精度)
        public LayerMask unwalkableMask; // 不可行走的层(墙壁、障碍物等)

        [Header("调试设置")]
        [SerializeField] private bool displayGridGizmos = true;

        private GridNode[,] grid;
        private float nodeDiameter;
        private int gridSizeX, gridSizeY;

        public int MaxSize => gridSizeX * gridSizeY;
        public bool IsInitialized => grid != null;

        private void Awake()
        {
            InitializeGrid();
        }

        /// <summary>
        /// 初始化网格(可手动调用)
        /// </summary>
        public void InitializeGrid()
        {
            nodeDiameter = nodeRadius * 2;
            gridSizeX = Mathf.RoundToInt(gridWorldSize.x / nodeDiameter);
            gridSizeY = Mathf.RoundToInt(gridWorldSize.y / nodeDiameter);

            CreateGrid();
        }

        /// <summary>
        /// 创建网格 - 烘焙场景障碍物
        /// </summary>
        public void CreateGrid()
        {
            grid = new GridNode[gridSizeX, gridSizeY];
            Vector3 worldBottomLeft = transform.position - Vector3.right * gridWorldSize.x / 2 - Vector3.forward * gridWorldSize.y / 2;

            for (int x = 0; x < gridSizeX; x++)
            {
                for (int y = 0; y < gridSizeY; y++)
                {
                    Vector3 worldPoint = worldBottomLeft + Vector3.right * (x * nodeDiameter + nodeRadius) + Vector3.forward * (y * nodeDiameter + nodeRadius);

                    // 检测该位置是否可行走(使用球形检测)
                    bool walkable = !Physics.CheckSphere(worldPoint, nodeRadius, unwalkableMask);

                    grid[x, y] = new GridNode(x, y, worldPoint, walkable);
                }
            }

            Debug.Log($"[PathfindingGrid] 网格创建完成: {gridSizeX}x{gridSizeY} = {MaxSize}个节点");
        }

        /// <summary>
        /// 从世界坐标获取对应的网格节点
        /// </summary>
        public GridNode NodeFromWorldPoint(Vector3 worldPosition)
        {
            float percentX = (worldPosition.x - transform.position.x + gridWorldSize.x / 2) / gridWorldSize.x;
            float percentY = (worldPosition.z - transform.position.z + gridWorldSize.y / 2) / gridWorldSize.y;
            percentX = Mathf.Clamp01(percentX);
            percentY = Mathf.Clamp01(percentY);

            int x = Mathf.RoundToInt((gridSizeX - 1) * percentX);
            int y = Mathf.RoundToInt((gridSizeY - 1) * percentY);

            return grid[x, y];
        }

        /// <summary>
        /// 获取节点的相邻节点(8方向)
        /// </summary>
        public List<GridNode> GetNeighbors(GridNode node)
        {
            List<GridNode> neighbors = new List<GridNode>();

            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    if (x == 0 && y == 0)
                        continue;

                    int checkX = node.X + x;
                    int checkY = node.Y + y;

                    if (checkX >= 0 && checkX < gridSizeX && checkY >= 0 && checkY < gridSizeY)
                    {
                        neighbors.Add(grid[checkX, checkY]);
                    }
                }
            }

            return neighbors;
        }

        /// <summary>
        /// 获取节点的相邻节点(4方向,不含对角线)
        /// </summary>
        public List<GridNode> GetNeighbors4Direction(GridNode node)
        {
            List<GridNode> neighbors = new List<GridNode>();

            int[][] directions = new int[][] {
                new int[] { 0, 1 },  // 上
                new int[] { 0, -1 }, // 下
                new int[] { -1, 0 }, // 左
                new int[] { 1, 0 }   // 右
            };

            foreach (var dir in directions)
            {
                int checkX = node.X + dir[0];
                int checkY = node.Y + dir[1];

                if (checkX >= 0 && checkX < gridSizeX && checkY >= 0 && checkY < gridSizeY)
                {
                    neighbors.Add(grid[checkX, checkY]);
                }
            }

            return neighbors;
        }

        /// <summary>
        /// 动态更新节点的可行走状态(用于动态障碍物)
        /// </summary>
        public void UpdateNodeWalkable(Vector3 worldPosition, bool walkable)
        {
            GridNode node = NodeFromWorldPoint(worldPosition);
            if (node != null)
            {
                node.Walkable = walkable;
            }
        }

        /// <summary>
        /// 编辑器调试可视化
        /// </summary>
        private void OnDrawGizmos()
        {
            Gizmos.DrawWireCube(transform.position, new Vector3(gridWorldSize.x, 1, gridWorldSize.y));

            if (grid != null && displayGridGizmos)
            {
                foreach (GridNode node in grid)
                {
                    Gizmos.color = node.Walkable ? new Color(1, 1, 1, 0.3f) : new Color(1, 0, 0, 0.5f);
                    Gizmos.DrawCube(node.WorldPosition, Vector3.one * (nodeDiameter - 0.1f));
                }
            }
        }

        // 公开属性供外部访问
        public Vector2 GridWorldSize => gridWorldSize;
        public float NodeRadius => nodeRadius;
        public int GridSizeX => gridSizeX;
        public int GridSizeY => gridSizeY;
    }
}
