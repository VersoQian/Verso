using UnityEngine;
using UnityEditor;

namespace Game.Pathfinding
{
    /// <summary>
    /// PathfindingGrid 编辑器扩展
    /// 提供可视化和烘焙工具
    /// </summary>
    [CustomEditor(typeof(PathfindingGrid))]
    public class PathfindingGridEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            PathfindingGrid grid = (PathfindingGrid)target;

            GUILayout.Space(10);
            EditorGUILayout.HelpBox(
                "网格寻路系统配置:\n" +
                "1. 设置GridWorldSize(网格覆盖范围)\n" +
                "2. 设置NodeRadius(网格精度,越小越精确但性能消耗越大)\n" +
                "3. 设置UnwalkableMask(不可行走的层)\n" +
                "4. 点击下方按钮烘焙网格",
                MessageType.Info
            );

            GUILayout.Space(10);

            if (GUILayout.Button("烘焙网格 (Bake Grid)", GUILayout.Height(40)))
            {
                grid.CreateGrid();
                EditorUtility.DisplayDialog("完成", "网格烘焙完成!", "OK");
            }

            GUILayout.Space(5);

            EditorGUILayout.HelpBox(
                $"网格信息:\n" +
                $"• 网格大小: {grid.GridSizeX}x{grid.GridSizeY} = {grid.MaxSize}个节点\n" +
                $"• 节点半径: {grid.NodeRadius}m\n" +
                $"• 世界大小: {grid.GridWorldSize}",
                MessageType.None
            );
        }
    }
}
