using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using Game.Modules;

namespace Game.Editor
{
    /// <summary>
    /// EnemiesModule配置助手
    /// 自动将生成的Enemy Prefab添加到EnemiesModule的Enemies数组
    /// </summary>
    public class EnemiesModuleConfigHelper : EditorWindow
    {
        private const string ENEMY_PREFAB_PATH = "Assets/GorodiskiGames/Survivalio/ResourcesStatic/Prefabs/Enemies";
        private const string BOSS_PREFAB_PATH = "Assets/GorodiskiGames/Survivalio/ResourcesStatic/Prefabs/Enemies/Bosses";

        [MenuItem("Tools/Configure EnemiesModule")]
        public static void ShowWindow()
        {
            GetWindow<EnemiesModuleConfigHelper>("EnemiesModule配置助手");
        }

        private void OnGUI()
        {
            GUILayout.Label("EnemiesModule 配置助手", EditorStyles.boldLabel);
            GUILayout.Space(10);

            EditorGUILayout.HelpBox(
                "这个工具会自动配置Gameplay Scene中的EnemiesModule：\n\n" +
                "1. 加载所有Enemy Prefab（Enemy0-Enemy13）\n" +
                "2. 添加到EnemiesModuleView的Enemies数组\n" +
                "3. 设置Boss Pool\n\n" +
                "使用前请确保已经运行过Enemy和Boss生成器！",
                MessageType.Info
            );

            GUILayout.Space(10);

            if (GUILayout.Button("自动配置EnemiesModule", GUILayout.Height(40)))
            {
                ConfigureEnemiesModule();
            }

            GUILayout.Space(10);

            if (GUILayout.Button("仅显示当前配置", GUILayout.Height(30)))
            {
                ShowCurrentConfiguration();
            }
        }

        private static void ConfigureEnemiesModule()
        {
            // 1. 打开Gameplay Scene
            var scene = EditorSceneManager.OpenScene("Assets/GorodiskiGames/Survivalio/Scenes/Gameplay.unity");

            // 2. 找到EnemiesModule GameObject
            EnemiesModuleView enemiesModuleView = FindObjectOfType<EnemiesModuleView>();
            if (enemiesModuleView == null)
            {
                EditorUtility.DisplayDialog("错误", "找不到EnemiesModuleView组件！\n请确保Gameplay Scene中有EnemiesModule GameObject。", "OK");
                return;
            }

            // 3. 加载所有Enemy Prefab
            string[] enemyNames = new string[]
            {
                "Enemy0", "Enemy1", "Enemy2", "Enemy3", "Enemy4",
                "Enemy5", "Enemy6", "Enemy7", "Enemy8", "Enemy9",
                "Enemy10", "Enemy11", "Enemy12", "Enemy13"
            };

            var enemyFactories = new System.Collections.Generic.List<ComponentPoolFactory>();
            int loadedCount = 0;

            foreach (var enemyName in enemyNames)
            {
                string path = $"{ENEMY_PREFAB_PATH}/{enemyName}.prefab";
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

                if (prefab != null)
                {
                    // 创建ComponentPoolFactory
                    ComponentPoolFactory factory = ScriptableObject.CreateInstance<ComponentPoolFactory>();

                    // 使用反射设置私有字段
                    var prefabField = typeof(ComponentPoolFactory).GetField("_prefab",
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if (prefabField != null)
                    {
                        prefabField.SetValue(factory, prefab);
                        enemyFactories.Add(factory);
                        loadedCount++;
                        Debug.Log($"✓ 加载Enemy: {enemyName}");
                    }
                }
                else
                {
                    Debug.LogWarning($"⚠ 找不到Enemy Prefab: {path}");
                }
            }

            // 4. 设置到EnemiesModuleView
            SerializedObject serializedView = new SerializedObject(enemiesModuleView);
            SerializedProperty enemiesProp = serializedView.FindProperty("Enemies");

            enemiesProp.arraySize = enemyFactories.Count;
            for (int i = 0; i < enemyFactories.Count; i++)
            {
                enemiesProp.GetArrayElementAtIndex(i).objectReferenceValue = enemyFactories[i];
            }

            serializedView.ApplyModifiedProperties();

            // 5. 加载并设置默认Boss（BossEast）
            string bossPath = $"{BOSS_PREFAB_PATH}/BossEast.prefab";
            GameObject bossPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(bossPath);

            if (bossPrefab != null)
            {
                ComponentPoolFactory bossFactory = ScriptableObject.CreateInstance<ComponentPoolFactory>();
                var prefabField = typeof(ComponentPoolFactory).GetField("_prefab",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                if (prefabField != null)
                {
                    prefabField.SetValue(bossFactory, bossPrefab);

                    SerializedProperty bossPoolProp = serializedView.FindProperty("BossPool");
                    bossPoolProp.objectReferenceValue = bossFactory;
                    serializedView.ApplyModifiedProperties();

                    Debug.Log($"✓ 设置默认Boss: BossEast");
                }
            }

            // 6. 保存Scene
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);

            string message = $"配置完成！\n\n" +
                           $"已添加 {loadedCount} 个Enemy到Enemies数组\n" +
                           $"已设置默认Boss: BossEast\n\n" +
                           $"Scene已保存。";

            EditorUtility.DisplayDialog("完成", message, "OK");
        }

        private static void ShowCurrentConfiguration()
        {
            EnemiesModuleView enemiesModuleView = FindObjectOfType<EnemiesModuleView>();
            if (enemiesModuleView == null)
            {
                EditorUtility.DisplayDialog("提示", "找不到EnemiesModuleView！\n请打开Gameplay Scene。", "OK");
                return;
            }

            SerializedObject serializedView = new SerializedObject(enemiesModuleView);
            SerializedProperty enemiesProp = serializedView.FindProperty("Enemies");
            SerializedProperty bossPoolProp = serializedView.FindProperty("BossPool");

            string info = $"当前配置：\n\n";
            info += $"Enemies数组大小: {enemiesProp.arraySize}\n";
            info += $"Boss Pool: {(bossPoolProp.objectReferenceValue != null ? "已设置" : "未设置")}\n\n";
            info += "Enemy列表：\n";

            for (int i = 0; i < enemiesProp.arraySize && i < 20; i++)
            {
                var element = enemiesProp.GetArrayElementAtIndex(i).objectReferenceValue;
                info += $"  [{i}] {(element != null ? element.name : "null")}\n";
            }

            Debug.Log(info);
            EditorUtility.DisplayDialog("当前配置", info, "OK");
        }
    }
}
