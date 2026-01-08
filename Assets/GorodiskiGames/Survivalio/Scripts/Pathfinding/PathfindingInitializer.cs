using UnityEngine;
using Game.Pathfinding;

namespace Game.Managers
{
    /// <summary>
    /// 寻路系统启动器 - 放在游戏管理器中调用
    /// 或者作为RuntimeInitializeOnLoadMethod自动启动
    /// </summary>
    public static class PathfindingInitializer
    {
        /// <summary>
        /// 游戏启动时自动初始化寻路系统
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void AutoInitialize()
        {
            // 自动创建PathfindingManager
            var manager = PathfindingManager.Instance;

            Debug.Log("[PathfindingInitializer] 寻路系统自动启动完成");
        }

        /// <summary>
        /// 手动初始化(可选,如果需要自定义参数)
        /// </summary>
        public static void Initialize(Vector2 gridSize, float nodeRadius = 0.5f)
        {
            var manager = PathfindingManager.Instance;
            manager.SetGridSize(gridSize);
            manager.SetNodeRadius(nodeRadius);

            Debug.Log($"[PathfindingInitializer] 手动初始化寻路系统: GridSize={gridSize}, NodeRadius={nodeRadius}");
        }

        /// <summary>
        /// 根据关卡自动配置网格大小
        /// </summary>
        public static void ConfigureForLevel(string levelName)
        {
            var manager = PathfindingManager.Instance;

            // 根据不同关卡设置不同的网格大小
            switch (levelName)
            {
                case "LevelEast":
                    manager.SetGridSize(new Vector2(80, 80));
                    manager.SetNodeRadius(0.5f);
                    break;

                case "LevelWest":
                    manager.SetGridSize(new Vector2(100, 100));
                    manager.SetNodeRadius(0.5f);
                    break;

                case "LevelSunFlower":
                    manager.SetGridSize(new Vector2(120, 120));
                    manager.SetNodeRadius(0.6f);
                    break;

                case "LevelDorm":
                    manager.SetGridSize(new Vector2(90, 90));
                    manager.SetNodeRadius(0.4f);
                    break;

                default:
                    // 默认配置
                    manager.SetGridSize(new Vector2(100, 100));
                    manager.SetNodeRadius(0.5f);
                    break;
            }

            Debug.Log($"[PathfindingInitializer] 为关卡 [{levelName}] 配置寻路网格");
        }
    }
}
