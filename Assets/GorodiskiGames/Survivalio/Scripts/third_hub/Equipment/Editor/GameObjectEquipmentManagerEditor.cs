using UnityEditor;
using UnityEngine;

namespace Game.ThirdHub.Equipment.Editor
{
    [CustomEditor(typeof(GameObjectEquipmentManager))]
    public class GameObjectEquipmentManagerEditor : UnityEditor.Editor
    {
        private GameObjectEquipmentManager _manager;
        private Transform _partsRoot;

        private void OnEnable()
        {
            _manager = (GameObjectEquipmentManager)target;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("GameObject 装备管理器", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "这个管理器用于管理基于 GameObject 激活/禁用的换装系统。\n" +
                "适用于 Character_114 等通过显示/隐藏 GameObject 来切换装备的角色模型。",
                MessageType.Info);

            EditorGUILayout.Space(10);

            // 自动设置区域
            EditorGUILayout.LabelField("快速设置", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical("box");
            {
                EditorGUILayout.LabelField("将装备根节点（Parts）拖到下方：", EditorStyles.miniLabel);
                _partsRoot = (Transform)EditorGUILayout.ObjectField(
                    "Parts 根节点",
                    _partsRoot,
                    typeof(Transform),
                    true);

                EditorGUI.BeginDisabledGroup(_partsRoot == null);
                if (GUILayout.Button("自动设置装备槽位", GUILayout.Height(30)))
                {
                    _manager.EditorSetupSlots(_partsRoot);
                    EditorUtility.SetDirty(target);
                    serializedObject.Update();
                }
                EditorGUI.EndDisabledGroup();

                if (_partsRoot == null)
                {
                    EditorGUILayout.HelpBox(
                        "请将 Character_114/Parts 节点拖到上方，然后点击按钮自动设置。",
                        MessageType.Warning);
                }
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(10);

            // 显示默认属性
            DrawDefaultInspector();

            serializedObject.ApplyModifiedProperties();
        }
    }
}
