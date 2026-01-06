using UnityEngine;
using UnityEditor;
using Game.Player;

namespace Game.Editor
{
    /// <summary>
    /// 自动配置Player预制件的装备系统
    /// 使用方法：Unity顶部菜单 -> Tools -> Setup Player Equipment System
    /// </summary>
    public class PlayerEquipmentSetup : EditorWindow
    {
        private const string PLAYER_PREFAB_PATH = "Assets/GorodiskiGames/Survivalio/ResourcesStatic/Prefabs/Player/Player.prefab";
        private const string CHARACTER_FBX_GUID = "2beed5376eb37ac48b23a24754e4bf6d"; // Character.fbx

        [MenuItem("Tools/Setup Player Equipment System")]
        public static void ShowWindow()
        {
            var window = GetWindow<PlayerEquipmentSetup>("装备系统配置");
            window.minSize = new Vector2(400, 300);
            window.Show();
        }

        private void OnGUI()
        {
            GUILayout.Space(10);
            EditorGUILayout.HelpBox("此工具将自动为Player预制件配置装备系统：\n\n" +
                                    "1. 创建5个装备部位的SkinnedMeshRenderer\n" +
                                    "2. 配置骨骼和材质引用\n" +
                                    "3. 更新PlayerView组件引用", MessageType.Info);

            GUILayout.Space(20);

            if (GUILayout.Button("开始配置", GUILayout.Height(40)))
            {
                SetupPlayerEquipment();
            }

            GUILayout.Space(10);

            if (GUILayout.Button("测试：检查当前配置", GUILayout.Height(30)))
            {
                CheckCurrentSetup();
            }
        }

        private static void CheckCurrentSetup()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PLAYER_PREFAB_PATH);
            if (prefab == null)
            {
                Debug.LogError($"找不到Player预制件: {PLAYER_PREFAB_PATH}");
                return;
            }

            var playerView = prefab.GetComponent<PlayerView>();
            if (playerView == null)
            {
                Debug.LogError("Player预制件上没有PlayerView组件");
                return;
            }

            // 使用SerializedObject来检查私有字段
            var so = new SerializedObject(playerView);

            Debug.Log("=== Player装备系统当前配置 ===");
            Debug.Log($"_full: {so.FindProperty("_full").objectReferenceValue}");
            Debug.Log($"_head: {so.FindProperty("_head").objectReferenceValue}");
            Debug.Log($"_top: {so.FindProperty("_top").objectReferenceValue}");
            Debug.Log($"_bottom: {so.FindProperty("_bottom").objectReferenceValue}");
            Debug.Log($"_headgear: {so.FindProperty("_headgear").objectReferenceValue}");
            Debug.Log($"_shoes: {so.FindProperty("_shoes").objectReferenceValue}");
            Debug.Log($"_gloves: {so.FindProperty("_gloves").objectReferenceValue}");
        }

        private static void SetupPlayerEquipment()
        {
            // 加载Player预制件
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PLAYER_PREFAB_PATH);
            if (prefab == null)
            {
                Debug.LogError($"找不到Player预制件: {PLAYER_PREFAB_PATH}");
                return;
            }

            // 创建预制件实例进行编辑
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);

            try
            {
                // 查找NinjaBlack GameObject（包含现有的SkinnedMeshRenderer）
                var ninjaBlack = FindChildByName(instance.transform, "NinjaBlack");
                if (ninjaBlack == null)
                {
                    Debug.LogError("找不到NinjaBlack GameObject");
                    DestroyImmediate(instance);
                    return;
                }

                var existingRenderer = ninjaBlack.GetComponent<SkinnedMeshRenderer>();
                if (existingRenderer == null)
                {
                    Debug.LogError("NinjaBlack上没有SkinnedMeshRenderer组件");
                    DestroyImmediate(instance);
                    return;
                }

                // 获取父节点
                var bodyParent = ninjaBlack.parent;

                // 查找或创建装备材质（使用NinjaBlack的材质）
                Material equipmentMaterial = existingRenderer.sharedMaterial;

                // 创建5个装备部位
                var topRenderer = CreateEquipmentPart(bodyParent, "Top", existingRenderer, equipmentMaterial);
                var bottomRenderer = CreateEquipmentPart(bodyParent, "Bottom", existingRenderer, equipmentMaterial);
                var headgearRenderer = CreateEquipmentPart(bodyParent, "Headgear", existingRenderer, equipmentMaterial);
                var shoesRenderer = CreateEquipmentPart(bodyParent, "Shoes", existingRenderer, equipmentMaterial);
                var glovesRenderer = CreateEquipmentPart(bodyParent, "Gloves", existingRenderer, equipmentMaterial);

                // 将NinjaBlack重命名为Full
                ninjaBlack.name = "Full";

                // 创建Head GameObject（如果不存在）
                var headRenderer = CreateEquipmentPart(bodyParent, "Head", existingRenderer, equipmentMaterial);

                // 更新PlayerView组件的引用
                var playerView = instance.GetComponent<PlayerView>();
                if (playerView == null)
                {
                    Debug.LogError("Player上没有PlayerView组件");
                    DestroyImmediate(instance);
                    return;
                }

                // 使用SerializedObject来设置私有字段
                var so = new SerializedObject(playerView);
                so.FindProperty("_full").objectReferenceValue = existingRenderer;
                so.FindProperty("_head").objectReferenceValue = headRenderer;
                so.FindProperty("_top").objectReferenceValue = topRenderer;
                so.FindProperty("_bottom").objectReferenceValue = bottomRenderer;
                so.FindProperty("_headgear").objectReferenceValue = headgearRenderer;
                so.FindProperty("_shoes").objectReferenceValue = shoesRenderer;
                so.FindProperty("_gloves").objectReferenceValue = glovesRenderer;
                so.ApplyModifiedProperties();

                // 保存修改到预制件
                PrefabUtility.ApplyPrefabInstance(instance, InteractionMode.UserAction);

                Debug.Log("✅ Player装备系统配置完成！");
                Debug.Log($"已创建装备部位: Top, Bottom, Headgear, Shoes, Gloves, Head");
                Debug.Log($"已更新PlayerView组件引用");

                // 清理
                DestroyImmediate(instance);

                // 刷新AssetDatabase
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                EditorUtility.DisplayDialog("成功", "Player装备系统配置完成！", "确定");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"配置失败: {e.Message}\n{e.StackTrace}");
                DestroyImmediate(instance);
                EditorUtility.DisplayDialog("失败", $"配置失败: {e.Message}", "确定");
            }
        }

        private static SkinnedMeshRenderer CreateEquipmentPart(Transform parent, string name,
            SkinnedMeshRenderer referenceRenderer, Material material)
        {
            // 检查是否已存在
            var existing = parent.Find(name);
            if (existing != null)
            {
                Debug.Log($"装备部位 {name} 已存在，跳过创建");
                return existing.GetComponent<SkinnedMeshRenderer>();
            }

            // 创建新GameObject
            var partObject = new GameObject(name);
            partObject.transform.SetParent(parent, false);
            partObject.transform.localPosition = Vector3.zero;
            partObject.transform.localRotation = Quaternion.identity;
            partObject.transform.localScale = Vector3.one;

            // 添加SkinnedMeshRenderer组件
            var renderer = partObject.AddComponent<SkinnedMeshRenderer>();

            // 复制渲染设置
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = referenceRenderer.shadowCastingMode;
            renderer.receiveShadows = referenceRenderer.receiveShadows;
            renderer.lightProbeUsage = referenceRenderer.lightProbeUsage;
            renderer.reflectionProbeUsage = referenceRenderer.reflectionProbeUsage;

            // 复制骨骼设置
            renderer.bones = referenceRenderer.bones;
            renderer.rootBone = referenceRenderer.rootBone;
            renderer.quality = referenceRenderer.quality;
            renderer.updateWhenOffscreen = referenceRenderer.updateWhenOffscreen;

            // 初始时不设置mesh（将在运行时由PlayerView设置）
            renderer.sharedMesh = null;

            Debug.Log($"✅ 创建装备部位: {name}");
            return renderer;
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
