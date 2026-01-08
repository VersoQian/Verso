using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Pathfinding
{
    /// <summary>
    /// A*寻路算法实现
    /// 包含路径规划、路径平滑、路径缓存优化
    /// </summary>
    public class AStarPathfinder
    {
        private PathfindingGrid grid;

        // 路径缓存(优化性能)
        private Dictionary<(Vector3, Vector3), PathResult> pathCache = new Dictionary<(Vector3, Vector3), PathResult>();
        private const float CACHE_EXPIRY_TIME = 2f; // 缓存过期时间

        public AStarPathfinder(PathfindingGrid grid)
        {
            this.grid = grid;
        }

        /// <summary>
        /// 寻找从起点到终点的路径
        /// </summary>
        public List<Vector3> FindPath(Vector3 startPos, Vector3 targetPos, bool smoothPath = true)
        {
            GridNode startNode = grid.NodeFromWorldPoint(startPos);
            GridNode targetNode = grid.NodeFromWorldPoint(targetPos);

            // 起点或终点不可行走
            if (!startNode.Walkable || !targetNode.Walkable)
            {
                return null;
            }

            // 使用优先队列(二叉堆)优化
            Heap<GridNode> openSet = new Heap<GridNode>(grid.MaxSize);
            HashSet<GridNode> closedSet = new HashSet<GridNode>();

            openSet.Add(startNode);

            while (openSet.Count > 0)
            {
                GridNode currentNode = openSet.RemoveFirst();
                closedSet.Add(currentNode);

                // 找到目标
                if (currentNode == targetNode)
                {
                    List<Vector3> path = RetracePath(startNode, targetNode);

                    // 路径平滑
                    if (smoothPath && path != null && path.Count > 2)
                    {
                        path = SmoothPath(path);
                    }

                    return path;
                }

                // 检查相邻节点
                foreach (GridNode neighbor in grid.GetNeighbors(currentNode))
                {
                    if (!neighbor.Walkable || closedSet.Contains(neighbor))
                        continue;

                    int newMovementCostToNeighbor = currentNode.GCost + GetDistance(currentNode, neighbor);

                    if (newMovementCostToNeighbor < neighbor.GCost || !openSet.Contains(neighbor))
                    {
                        neighbor.GCost = newMovementCostToNeighbor;
                        neighbor.HCost = GetDistance(neighbor, targetNode);
                        neighbor.Parent = currentNode;

                        if (!openSet.Contains(neighbor))
                        {
                            openSet.Add(neighbor);
                        }
                        else
                        {
                            openSet.UpdateItem(neighbor);
                        }
                    }
                }
            }

            // 未找到路径
            return null;
        }

        /// <summary>
        /// 回溯路径
        /// </summary>
        private List<Vector3> RetracePath(GridNode startNode, GridNode endNode)
        {
            List<Vector3> path = new List<Vector3>();
            GridNode currentNode = endNode;

            while (currentNode != startNode)
            {
                path.Add(currentNode.WorldPosition);
                currentNode = currentNode.Parent;
            }

            path.Reverse();
            return path;
        }

        /// <summary>
        /// 路径平滑 - 移除不必要的路径点
        /// 使用射线检测,如果两点之间没有障碍物,则可以直线连接
        /// </summary>
        private List<Vector3> SmoothPath(List<Vector3> path)
        {
            List<Vector3> smoothedPath = new List<Vector3>();
            smoothedPath.Add(path[0]);

            int currentIndex = 0;
            for (int targetIndex = 2; targetIndex < path.Count; targetIndex++)
            {
                // 检查从当前点到目标点是否有障碍物
                Vector3 direction = (path[targetIndex] - path[currentIndex]).normalized;
                float distance = Vector3.Distance(path[currentIndex], path[targetIndex]);

                if (!Physics.Raycast(path[currentIndex] + Vector3.up * 0.5f, direction, distance))
                {
                    // 没有障碍物,继续检查下一个点
                    continue;
                }
                else
                {
                    // 有障碍物,保留上一个点
                    smoothedPath.Add(path[targetIndex - 1]);
                    currentIndex = targetIndex - 1;
                }
            }

            // 添加终点
            smoothedPath.Add(path[path.Count - 1]);

            return smoothedPath;
        }

        /// <summary>
        /// 计算两节点间的距离代价(支持对角线移动)
        /// </summary>
        private int GetDistance(GridNode nodeA, GridNode nodeB)
        {
            int dstX = Mathf.Abs(nodeA.X - nodeB.X);
            int dstY = Mathf.Abs(nodeA.Y - nodeB.Y);

            if (dstX > dstY)
                return 14 * dstY + 10 * (dstX - dstY); // 对角线14, 直线10
            return 14 * dstX + 10 * (dstY - dstX);
        }

        /// <summary>
        /// 清除缓存
        /// </summary>
        public void ClearCache()
        {
            pathCache.Clear();
        }
    }

    /// <summary>
    /// 路径结果(用于缓存)
    /// </summary>
    public class PathResult
    {
        public List<Vector3> Path;
        public float Timestamp;

        public PathResult(List<Vector3> path, float timestamp)
        {
            Path = path;
            Timestamp = timestamp;
        }
    }

    /// <summary>
    /// 二叉堆 - 用于A*算法的优先队列
    /// 性能优化: O(log n) 插入和删除
    /// </summary>
    public class Heap<T> where T : GridNode
    {
        private T[] items;
        private int currentItemCount;

        public Heap(int maxHeapSize)
        {
            items = new T[maxHeapSize];
        }

        public void Add(T item)
        {
            item.HeapIndex = currentItemCount;
            items[currentItemCount] = item;
            SortUp(item);
            currentItemCount++;
        }

        public T RemoveFirst()
        {
            T firstItem = items[0];
            currentItemCount--;
            items[0] = items[currentItemCount];
            items[0].HeapIndex = 0;
            SortDown(items[0]);
            return firstItem;
        }

        public void UpdateItem(T item)
        {
            SortUp(item);
        }

        public int Count => currentItemCount;

        public bool Contains(T item)
        {
            return Equals(items[item.HeapIndex], item);
        }

        private void SortDown(T item)
        {
            while (true)
            {
                int childIndexLeft = item.HeapIndex * 2 + 1;
                int childIndexRight = item.HeapIndex * 2 + 2;
                int swapIndex = 0;

                if (childIndexLeft < currentItemCount)
                {
                    swapIndex = childIndexLeft;

                    if (childIndexRight < currentItemCount)
                    {
                        if (items[childIndexLeft].FCost > items[childIndexRight].FCost ||
                            (items[childIndexLeft].FCost == items[childIndexRight].FCost && items[childIndexLeft].HCost > items[childIndexRight].HCost))
                        {
                            swapIndex = childIndexRight;
                        }
                    }

                    if (item.FCost > items[swapIndex].FCost ||
                        (item.FCost == items[swapIndex].FCost && item.HCost > items[swapIndex].HCost))
                    {
                        Swap(item, items[swapIndex]);
                    }
                    else
                    {
                        return;
                    }
                }
                else
                {
                    return;
                }
            }
        }

        private void SortUp(T item)
        {
            int parentIndex = (item.HeapIndex - 1) / 2;

            while (true)
            {
                T parentItem = items[parentIndex];
                if (item.FCost < parentItem.FCost ||
                    (item.FCost == parentItem.FCost && item.HCost < parentItem.HCost))
                {
                    Swap(item, parentItem);
                }
                else
                {
                    break;
                }

                parentIndex = (item.HeapIndex - 1) / 2;
            }
        }

        private void Swap(T itemA, T itemB)
        {
            items[itemA.HeapIndex] = itemB;
            items[itemB.HeapIndex] = itemA;
            int itemAIndex = itemA.HeapIndex;
            itemA.HeapIndex = itemB.HeapIndex;
            itemB.HeapIndex = itemAIndex;
        }
    }
}
