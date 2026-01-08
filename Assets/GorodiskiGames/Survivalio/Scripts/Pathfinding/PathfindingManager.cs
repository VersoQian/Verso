using UnityEngine;

namespace Game.Pathfinding
{
    /// <summary>
    /// 寻路系统管理器 - 单例模式
    /// 自动创建和管理PathfindingGrid,无需手动配置
    /// </summary>
    public class PathfindingManager : MonoBehaviour
    {
        private static PathfindingManager _instance;
        public static PathfindingManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<PathfindingManager>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("PathfindingManager");
                        _instance = go.AddComponent<PathfindingManager>();
                    }
                }
                return _instance;
            }
        }

        [Header("网格配置(可在代码中修改)")]
        [SerializeField] private Vector2 gridWorldSize = new Vector2(100, 100);
        [SerializeField] private float nodeRadius = 0.5f;
        [SerializeField] private LayerMask unwalkableMask;

        private PathfindingGrid _grid;
        public PathfindingGrid Grid => _grid;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);

            InitializePathfinding();
        }

        /// <summary>
        /// 初始化寻路系统
        /// </summary>
        private void InitializePathfinding()
        {
            // 查找或创建PathfindingGrid
            _grid = GetComponent<PathfindingGrid>();
            if (_grid == null)
            {
                _grid = gameObject.AddComponent<PathfindingGrid>();
            }

            // 自动配置参数
            AutoConfigureGrid();

            Debug.Log("[PathfindingManager] 寻路系统初始化完成");
        }

        /// <summary>
        /// 自动配置网格参数
        /// </summary>
        private void AutoConfigureGrid()
        {
            // 如果unwalkableMask未设置,自动配置
            if (unwalkableMask == 0)
            {
                // 尝试获取常见的障碍物层
                int defaultLayer = LayerMask.NameToLayer("Default");
                int obstacleLayer = LayerMask.NameToLayer("Obstacle");
                int wallLayer = LayerMask.NameToLayer("Wall");

                unwalkableMask = 0;
                if (defaultLayer >= 0) unwalkableMask |= (1 << defaultLayer);
                if (obstacleLayer >= 0) unwalkableMask |= (1 << obstacleLayer);
                if (wallLayer >= 0) unwalkableMask |= (1 << wallLayer);

                Debug.Log($"[PathfindingManager] 自动配置UnwalkableMask: {unwalkableMask.value}");
            }

            // 直接设置Grid的公开字段
            _grid.gridWorldSize = gridWorldSize;
            _grid.nodeRadius = nodeRadius;
            _grid.unwalkableMask = unwalkableMask;

            // 初始化网格
            if (!_grid.IsInitialized)
            {
                _grid.InitializeGrid();
            }
        }

        /// <summary>
        /// 外部调用:设置网格大小
        /// </summary>
        public void SetGridSize(Vector2 size)
        {
            gridWorldSize = size;
            if (_grid != null)
            {
                AutoConfigureGrid();
            }
        }

        /// <summary>
        /// 外部调用:设置节点精度
        /// </summary>
        public void SetNodeRadius(float radius)
        {
            nodeRadius = radius;
            if (_grid != null)
            {
                AutoConfigureGrid();
            }
        }

        /// <summary>
        /// 外部调用:设置不可行走层
        /// </summary>
        public void SetUnwalkableMask(LayerMask mask)
        {
            unwalkableMask = mask;
            if (_grid != null)
            {
                AutoConfigureGrid();
            }
        }
    }
}
