using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Game.Enemy;

namespace Game.Editor
{
    /// <summary>
    /// Enemy Prefab批量修复工具
    /// 自动配置EnemyView组件中缺失的引用
    /// 使用方法：Unity菜单 Tools > Fix Enemy Prefabs
    /// </summary>
    public class EnemyPrefabFixer : EditorWindow
    {
        private const string ENEMY_PREFAB_PATH = "Assets/GorodiskiGames/Survivalio/ResourcesStatic/Prefabs/Enemies";
        private const string BOSS_PREFAB_PATH = "Assets/GorodiskiGames/Survivalio/ResourcesStatic/Prefabs/Enemies/Bosses";

        private Vector2 scrollPosition;
        private List<string> prefabPaths = new List<string>();
        private bool includeBosses = true;

        [MenuItem("Tools/Fix Enemy Prefabs")]
        public static void ShowWindow()
        {
            GetWindow<EnemyPrefabFixer>("Enemy Prefab Fixer");
        }

        private void OnEnable()
        {
            RefreshPrefabList();
        }

        private void RefreshPrefabList()
        {
            prefabPaths.Clear();

            // 查找所有Enemy预制体
            if (Directory.Exists(ENEMY_PREFAB_PATH))
            {
                var files = Directory.GetFiles(ENEMY_PREFAB_PATH, "*.prefab", SearchOption.TopDirectoryOnly);
                prefabPaths.AddRange(files);
            }

            // 查找所有Boss预制体
            if (includeBosses && Directory.Exists(BOSS_PREFAB_PATH))
            {
                var files = Directory.GetFiles(BOSS_PREFAB_PATH, "*.prefab", SearchOption.AllDirectories);
                prefabPaths.AddRange(files);
            }

            prefabPaths = prefabPaths.OrderBy(p => p).ToList();
        }

        private void OnGUI()
        {
            GUILayout.Label("Enemy Prefab 批量修复工具", EditorStyles.boldLabel);
            GUILayout.Space(10);

            EditorGUILayout.HelpBox(
                "这个工具会自动修复所有Enemy预制体的EnemyView组件引用：\n" +
                "1. Collider - 自动查找CapsuleCollider\n" +
                "2. Animator - 自动查找Animator组件\n" +
                "3. RotateNode - 自动查找Armature或Character节点\n" +
                "4. BulletNode - 自动创建在根节点下\n" +
                "5. AimNode - 自动创建在根节点下\n" +
                "6. Renderers - 自动查找所有Renderer组件",
                MessageType.Info
            );

            GUILayout.Space(10);

            EditorGUI.BeginChangeCheck();
            includeBosses = EditorGUILayout.Toggle("包含Boss预制体", includeBosses);
            if (EditorGUI.EndChangeCheck())
            {
                RefreshPrefabList();
            }

            GUILayout.Space(10);

            // 显示预制体列表
            GUILayout.Label($"找到 {prefabPaths.Count} 个预制体:", EditorStyles.boldLabel);
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(200));
            foreach (var path in prefabPaths)
            {
                EditorGUILayout.LabelField("• " + Path.GetFileName(path));
            }
            EditorGUILayout.EndScrollView();

            GUILayout.Space(10);

            if (GUILayout.Button("修复所有Enemy预制体", GUILayout.Height(40)))
            {
                FixAllEnemyPrefabs();
            }

            GUILayout.Space(10);

            if (GUILayout.Button("刷新预制体列表", GUILayout.Height(30)))
            {
                RefreshPrefabList();
            }
        }

        private void FixAllEnemyPrefabs()
        {
            if (prefabPaths.Count == 0)
            {
                EditorUtility.DisplayDialog("提示", "没有找到需要修复的预制体", "OK");
                return;
            }

            int successCount = 0;
            int skipCount = 0;
            int failCount = 0;

            foreach (var prefabPath in prefabPaths)
            {
                EditorUtility.DisplayProgressBar(
                    "修复Enemy预制体",
                    $"正在处理: {Path.GetFileName(prefabPath)}",
                    (float)successCount / prefabPaths.Count
                );

                try
                {
                    var result = FixEnemyPrefab(prefabPath);
                    if (result == FixResult.Success)
                        successCount++;
                    else if (result == FixResult.Skip)
                        skipCount++;
                    else
                        failCount++;
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"修复预制体失败 [{Path.GetFileName(prefabPath)}]: {e.Message}");
                    failCount++;
                }
            }

            EditorUtility.ClearProgressBar();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            string message = $"修复完成！\n成功: {successCount} | 跳过: {skipCount} | 失败: {failCount}";
            EditorUtility.DisplayDialog("完成", message, "OK");
        }

        private enum FixResult
        {
            Success,
            Skip,
            Fail
        }

        private FixResult FixEnemyPrefab(string prefabPath)
        {
            // 加载预制体
            GameObject prefabAsset = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefabAsset == null)
            {
                Debug.LogError($"无法加载预制体: {prefabPath}");
                return FixResult.Fail;
            }

            // 实例化预制体到场景中进行修改
            GameObject prefabInstance = PrefabUtility.InstantiatePrefab(prefabAsset) as GameObject;
            if (prefabInstance == null)
            {
                Debug.LogError($"无法实例化预制体: {prefabPath}");
                return FixResult.Fail;
            }

            try
            {
                // 获取EnemyView组件
                EnemyView enemyView = prefabInstance.GetComponent<EnemyView>();
                if (enemyView == null)
                {
                    Debug.LogWarning($"预制体没有EnemyView组件，跳过: {Path.GetFileName(prefabPath)}");
                    return FixResult.Skip;
                }

                // 使用SerializedObject来修改预制体
                SerializedObject serializedView = new SerializedObject(enemyView);

                bool needsUpdate = false;

                // 1. 修复Collider引用
                SerializedProperty colliderProp = serializedView.FindProperty("_collider");
                if (colliderProp.objectReferenceValue == null)
                {
                    CapsuleCollider collider = prefabInstance.GetComponent<CapsuleCollider>();
                    if (collider != null)
                    {
                        colliderProp.objectReferenceValue = collider;
                        needsUpdate = true;
                        Debug.Log($"✓ [{Path.GetFileName(prefabPath)}] 配置Collider");
                    }
                }

                // 2. 修复Animator引用
                SerializedProperty animatorProp = serializedView.FindProperty("_animator");
                if (animatorProp.objectReferenceValue == null)
                {
                    Animator animator = prefabInstance.GetComponentInChildren<Animator>();
                    if (animator != null)
                    {
                        animatorProp.objectReferenceValue = animator;
                        needsUpdate = true;
                        Debug.Log($"✓ [{Path.GetFileName(prefabPath)}] 配置Animator: {animator.name}");
                    }
                    else
                    {
                        Debug.LogWarning($"! [{Path.GetFileName(prefabPath)}] 未找到Animator组件");
                    }
                }

                // 3. 修复RotateNode引用 - 查找Armature或Character节点
                SerializedProperty rotateNodeProp = serializedView.FindProperty("_rotateNode");
                if (rotateNodeProp.objectReferenceValue == null)
                {
                    Transform rotateNode = FindRotateNode(prefabInstance.transform);
                    if (rotateNode != null)
                    {
                        rotateNodeProp.objectReferenceValue = rotateNode;
                        needsUpdate = true;
                        Debug.Log($"✓ [{Path.GetFileName(prefabPath)}] 配置RotateNode: {rotateNode.name}");
                    }
                }

                // 4. 修复BulletNode引用
                SerializedProperty bulletNodeProp = serializedView.FindProperty("_bulletNode");
                if (bulletNodeProp.objectReferenceValue == null)
                {
                    Transform bulletNode = prefabInstance.transform.Find("BulletNode");
                    if (bulletNode == null)
                    {
                        // 创建BulletNode
                        GameObject bulletNodeObj = new GameObject("BulletNode");
                        bulletNodeObj.transform.SetParent(prefabInstance.transform);
                        bulletNodeObj.transform.localPosition = new Vector3(0, 1.5f, 0);
                        bulletNodeObj.transform.localRotation = Quaternion.identity;
                        bulletNode = bulletNodeObj.transform;
                    }
                    bulletNodeProp.objectReferenceValue = bulletNode;
                    needsUpdate = true;
                    Debug.Log($"✓ [{Path.GetFileName(prefabPath)}] 配置BulletNode");
                }

                // 5. 修复AimNode引用
                SerializedProperty aimNodeProp = serializedView.FindProperty("_aimNode");
                if (aimNodeProp.objectReferenceValue == null)
                {
                    Transform aimNode = prefabInstance.transform.Find("AimNode");
                    if (aimNode == null)
                    {
                        // 创建AimNode
                        GameObject aimNodeObj = new GameObject("AimNode");
                        aimNodeObj.transform.SetParent(prefabInstance.transform);
                        aimNodeObj.transform.localPosition = new Vector3(0, 1.2f, 0);
                        aimNodeObj.transform.localRotation = Quaternion.identity;
                        aimNode = aimNodeObj.transform;
                    }
                    aimNodeProp.objectReferenceValue = aimNode;
                    needsUpdate = true;
                    Debug.Log($"✓ [{Path.GetFileName(prefabPath)}] 配置AimNode");
                }

                // 6. 修复Renderers数组
                SerializedProperty renderersProp = serializedView.FindProperty("_renderers");
                if (renderersProp.arraySize == 0)
                {
                    // 查找所有Renderer,排除粒子系统的Renderer
                    Renderer[] renderers = prefabInstance.GetComponentsInChildren<Renderer>()
                        .Where(r => !(r is ParticleSystemRenderer))
                        .ToArray();

                    if (renderers.Length > 0)
                    {
                        renderersProp.arraySize = renderers.Length;
                        for (int i = 0; i < renderers.Length; i++)
                        {
                            renderersProp.GetArrayElementAtIndex(i).objectReferenceValue = renderers[i];
                        }
                        needsUpdate = true;
                        Debug.Log($"✓ [{Path.GetFileName(prefabPath)}] 配置{renderers.Length}个Renderers");
                    }
                }

                // 应用修改
                if (needsUpdate)
                {
                    serializedView.ApplyModifiedProperties();

                    // 保存修改回预制体
                    PrefabUtility.SaveAsPrefabAsset(prefabInstance, prefabPath);

                    Debug.Log($"✓✓ 成功修复预制体: {Path.GetFileName(prefabPath)}");
                    return FixResult.Success;
                }
                else
                {
                    Debug.Log($"○ 预制体已配置完整，跳过: {Path.GetFileName(prefabPath)}");
                    return FixResult.Skip;
                }
            }
            finally
            {
                // 清理场景中的临时实例
                Object.DestroyImmediate(prefabInstance);
            }
        }

        /// <summary>
        /// 查找RotateNode - 通常是Armature或Character节点
        /// </summary>
        private Transform FindRotateNode(Transform root)
        {
            // 常见的旋转节点名称
            string[] rotateNodeNames = { "Armature", "Character", "Model", "Skeleton", "Root" };

            // 首先尝试在直接子物体中查找
            foreach (string nodeName in rotateNodeNames)
            {
                Transform node = root.Find(nodeName);
                if (node != null)
                    return node;
            }

            // 递归查找所有子物体
            foreach (string nodeName in rotateNodeNames)
            {
                Transform node = root.GetComponentsInChildren<Transform>()
                    .FirstOrDefault(t => t.name.Equals(nodeName, System.StringComparison.OrdinalIgnoreCase));
                if (node != null)
                    return node;
            }

            // 如果找不到特定名称的节点，返回根节点
            return root;
        }
    }
}
