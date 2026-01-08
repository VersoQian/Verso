using UnityEngine;

namespace Game.Pathfinding
{
    /// <summary>
    /// 网格节点 - A*寻路的基本单元
    /// </summary>
    public class GridNode
    {
        // 网格坐标
        public int X { get; private set; }
        public int Y { get; private set; }

        // 世界坐标
        public Vector3 WorldPosition { get; private set; }

        // 是否可行走
        public bool Walkable { get; set; }

        // A*算法相关
        public int GCost { get; set; } // 从起点到当前节点的代价
        public int HCost { get; set; } // 从当前节点到终点的启发式代价
        public int FCost => GCost + HCost; // 总代价

        public GridNode Parent { get; set; } // 父节点(用于回溯路径)

        // 用于优先队列排序
        public int HeapIndex { get; set; }

        public GridNode(int x, int y, Vector3 worldPosition, bool walkable)
        {
            X = x;
            Y = y;
            WorldPosition = worldPosition;
            Walkable = walkable;
        }

        /// <summary>
        /// 重置节点的A*计算数据
        /// </summary>
        public void Reset()
        {
            GCost = 0;
            HCost = 0;
            Parent = null;
        }
    }
}
