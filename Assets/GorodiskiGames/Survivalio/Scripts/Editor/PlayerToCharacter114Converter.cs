using UnityEngine;
using UnityEditor;
using Game.Player;

namespace Game.Editor
{
    /// <summary>
    /// 将Player.prefab转换为使用Character_114的骨骼和模型
    /// 使用方法：Unity顶部菜单 -> Tools -> Convert Player to Character_114
    /// </summary>
    public class PlayerToCharacter114Converter : EditorWindow
    {
        private const string PLAYER_PREFAB_PATH = "Assets/GorodiskiGames/Survivalio/ResourcesStatic/Prefabs/Player/Player.prefab";
        private const string CHARACTER_114_PREFAB_PATH = "Assets/GorodiskiGames/Layer lab/3D Casual Character/3D Casual Character/Prefabs/Characters/Character_114.prefab";

        [MenuItem("Tools/Convert Player to Character_114")]
        public static void ShowWindow()
        {
            var window = GetWindow<PlayerToCharacter114Converter>("Player转Character_114");
            window.minSize = new Vector2(400, 300);
            window.Show();
        }

        private void OnGUI()
        {
            GUILayout.Space(10);
            EditorGUILayout.HelpBox("此工具将Player.prefab转换为使用Character_114的骨骼系统：\n\n" +
                                    "1. 从Character_114复制骨骼层级到Player\n" +
                                    "2. 更新Full body使用Character_114的Body mesh\n" +
                                    "3. 重新绑定所有SkinnedMeshRenderer的骨骼引用", MessageType.Info);

            GUILayout.Space(20);

            if (GUILayout.Button("开始转换", GUILayout.Height(40)))
            {
                ConvertPlayerToCharacter114();
            }

            GUILayout.Space(10);

            EditorGUILayout.HelpBox("注意：此操作会修改Player.prefab，建议先备份！", MessageType.Warning);
        }

        private static void ConvertPlayerToCharacter114()
        {
            // 加载预制件
            var playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PLAYER_PREFAB_PATH);
            var character114Prefab = AssetDatabase.LoadAssetAtPath<GameObject>(CHARACTER_114_PREFAB_PATH);

            if (playerPrefab == null)
            {
                Debug.LogError($"找不到Player预制件: {PLAYER_PREFAB_PATH}");
                return;
            }

            if (character114Prefab == null)
            {
                Debug.LogError($"找不到Character_114预制件: {CHARACTER_114_PREFAB_PATH}");
                return;
            }

            // 创建实例
            var playerInstance = (GameObject)PrefabUtility.InstantiatePrefab(playerPrefab);
            var character114Instance = (GameObject)PrefabUtility.InstantiatePrefab(character114Prefab);

            try
            {
                // 查找Player的Body父节点
                Transform playerBodyParent = FindChildByName(playerInstance.transform, "Full")?.parent;
                if (playerBodyParent == null)
                {
                    Debug.LogError("找不到Player的Body父节点");
                    return;
                }

                // 查找Character_114的骨骼根节点
                Transform character114Root = character114Instance.transform;
                Transform hipsTransform = FindChildByName(character114Root, "QuickRigCharacter2_Hips");

                if (hipsTransform == null)
                {
                    Debug.LogError("找不到Character_114的QuickRigCharacter2_Hips骨骼");
                    return;
                }

                // 删除Player中旧的骨骼（如果存在）
                Transform oldSkeleton = FindChildByName(playerBodyParent, "Armature");
                if (oldSkeleton != null)
                {
                    DestroyImmediate(oldSkeleton.gameObject);
                }

                // 复制Character_114的骨骼层级到Player
                // 找到骨骼的父节点（通常是Armature或直接是Hips的父节点）
                Transform skeletonParent = hipsTransform.parent;
                if (skeletonParent != null && skeletonParent != character114Root)
                {
                    // 复制整个骨骼结构
                    GameObject copiedSkeleton = Instantiate(skeletonParent.gameObject);
                    copiedSkeleton.transform.SetParent(playerBodyParent, false);
                    copiedSkeleton.transform.localPosition = Vector3.zero;
                    copiedSkeleton.transform.localRotation = Quaternion.identity;
                    copiedSkeleton.transform.localScale = Vector3.one;

                    Debug.Log($"✅ 已复制骨骼结构: {copiedSkeleton.name}");
                }
                else
                {
                    // 直接复制Hips
                    GameObject copiedHips = Instantiate(hipsTransform.gameObject);
                    copiedHips.transform.SetParent(playerBodyParent, false);
                    copiedHips.transform.localPosition = Vector3.zero;
                    copiedHips.transform.localRotation = Quaternion.identity;
                    copiedHips.transform.localScale = Vector3.one;

                    Debug.Log($"✅ 已复制根骨骼: {copiedHips.name}");
                }

                // 更新Player中的SkinnedMeshRenderer
                UpdateSkinnedMeshRenderers(playerInstance, character114Instance);

                // 验证引用
                var playerView = playerInstance.GetComponent<PlayerView>();
                if (playerView != null)
                {
                    var so = new SerializedObject(playerView);

                    // 确保_health和_healthText引用存在
                    if (so.FindProperty("_health").objectReferenceValue == null)
                    {
                        Debug.LogWarning("警告: _health引用丢失，请手动在Inspector中设置");
                    }
                    if (so.FindProperty("_healthText").objectReferenceValue == null)
                    {
                        Debug.LogWarning("警告: _healthText引用丢失，请手动在Inspector中设置");
                    }

                    // 打印当前配置用于调试
                    Debug.Log($"_animator: {so.FindProperty("_animator").objectReferenceValue}");
                    Debug.Log($"_collider: {so.FindProperty("_collider").objectReferenceValue}");
                }

                // 保存修改到预制件
                PrefabUtility.ApplyPrefabInstance(playerInstance, InteractionMode.UserAction);

                Debug.Log("✅ Player转换完成！已使用Character_114的骨骼系统");

                // 清理
                DestroyImmediate(playerInstance);
                DestroyImmediate(character114Instance);

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                EditorUtility.DisplayDialog("成功", "Player已转换为使用Character_114骨骼系统！", "确定");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"转换失败: {e.Message}\n{e.StackTrace}");
                DestroyImmediate(playerInstance);
                DestroyImmediate(character114Instance);
                EditorUtility.DisplayDialog("失败", $"转换失败: {e.Message}", "确定");
            }
        }

        private static void UpdateSkinnedMeshRenderers(GameObject player, GameObject character114)
        {
            // 从Character_114获取Body_1的mesh和骨骼设置
            var character114Renderers = character114.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            SkinnedMeshRenderer bodyReference = null;

            foreach (var renderer in character114Renderers)
            {
                if (renderer.gameObject.name == "Body_1")
                {
                    bodyReference = renderer;
                    break;
                }
            }

            if (bodyReference == null)
            {
                Debug.LogError("找不到Character_114的Body_1");
                return;
            }

            // 查找Player的新骨骼根节点
            Transform newHips = FindChildByName(player.transform, "QuickRigCharacter2_Hips");
            if (newHips == null)
            {
                Debug.LogError("找不到复制后的QuickRigCharacter2_Hips");
                return;
            }

            // 收集所有新骨骼
            Transform[] newBones = CollectBones(newHips);

            // 更新Player的所有SkinnedMeshRenderer
            var playerView = player.GetComponent<PlayerView>();
            if (playerView == null)
            {
                Debug.LogError("Player上没有PlayerView组件");
                return;
            }

            var so = new SerializedObject(playerView);

            // 更新Full body
            var fullRenderer = so.FindProperty("_full").objectReferenceValue as SkinnedMeshRenderer;
            if (fullRenderer != null)
            {
                fullRenderer.sharedMesh = bodyReference.sharedMesh;
                fullRenderer.bones = newBones;
                fullRenderer.rootBone = newHips;
                fullRenderer.sharedMaterial = bodyReference.sharedMaterial;
                Debug.Log("✅ 已更新Full body的mesh和骨骼");
            }

            // 更新装备部位的骨骼引用
            UpdateEquipmentRenderer(so, "_head", newBones, newHips);
            UpdateEquipmentRenderer(so, "_top", newBones, newHips);
            UpdateEquipmentRenderer(so, "_bottom", newBones, newHips);
            UpdateEquipmentRenderer(so, "_headgear", newBones, newHips);
            UpdateEquipmentRenderer(so, "_shoes", newBones, newHips);
            UpdateEquipmentRenderer(so, "_gloves", newBones, newHips);

            so.ApplyModifiedProperties();
        }

        private static void UpdateEquipmentRenderer(SerializedObject so, string fieldName, Transform[] bones, Transform rootBone)
        {
            var renderer = so.FindProperty(fieldName).objectReferenceValue as SkinnedMeshRenderer;
            if (renderer != null)
            {
                renderer.bones = bones;
                renderer.rootBone = rootBone;
                Debug.Log($"✅ 已更新 {fieldName} 的骨骼引用");
            }
        }

        private static Transform[] CollectBones(Transform root)
        {
            // 递归收集所有子骨骼
            var bones = new System.Collections.Generic.List<Transform>();
            CollectBonesRecursive(root, bones);
            return bones.ToArray();
        }

        private static void CollectBonesRecursive(Transform current, System.Collections.Generic.List<Transform> bones)
        {
            bones.Add(current);
            foreach (Transform child in current)
            {
                CollectBonesRecursive(child, bones);
            }
        }

        private static Transform FindChildByName(Transform parent, string name)
        {
            if (parent.name == name)
                return parent;

            foreach (Transform child in parent)
            {
                var result = FindChildByName(child, name);
                if (result != null)
                    return result;
            }

            return null;
        }
    }
}
