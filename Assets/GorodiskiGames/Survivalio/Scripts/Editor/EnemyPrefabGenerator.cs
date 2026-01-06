using UnityEngine;
using UnityEditor;
using System.IO;
using Game.Enemy;
using Game.Config;
using Game.Modules;

namespace Game.Editor
{
    /// <summary>
    /// Enemy Prefab 批量生成器
    /// 使用方法：Unity菜单 Tools > Generate Enemy Prefabs
    /// </summary>
    public class EnemyPrefabGenerator : EditorWindow
    {
        private const string CHARACTER_PATH = "Assets/GorodiskiGames/Layer lab/3D Casual Character/3D Casual Character/Prefabs/Characters";
        private const string ENEMY_PREFAB_OUTPUT = "Assets/GorodiskiGames/Survivalio/ResourcesStatic/Prefabs/Enemies";
        private const string ENEMY_CONFIG_PATH = "Assets/GorodiskiGames/Survivalio/Resources/EnemyConfigs";

        private static readonly EnemyInfo[] ENEMY_INFOS = new EnemyInfo[]
        {
            new EnemyInfo { ConfigName = "Enemy5", CharacterName = "Character_1", Speed = 3.5f, Health = 8, Damage = 12 },
            new EnemyInfo { ConfigName = "Enemy6", CharacterName = "Character_2", Speed = 3.2f, Health = 7, Damage = 13 },
            new EnemyInfo { ConfigName = "Enemy7", CharacterName = "Character_11", Speed = 4f, Health = 6, Damage = 14 },
            new EnemyInfo { ConfigName = "Enemy8", CharacterName = "Character_22", Speed = 3.8f, Health = 9, Damage = 11 },
            new EnemyInfo { ConfigName = "Enemy9", CharacterName = "Character_33", Speed = 3f, Health = 10, Damage = 10 },
            new EnemyInfo { ConfigName = "Enemy10", CharacterName = "Character_47", Speed = 3.6f, Health = 8, Damage = 12 },
            new EnemyInfo { ConfigName = "Enemy11", CharacterName = "Character_58", Speed = 4.2f, Health = 7, Damage = 13 },
            new EnemyInfo { ConfigName = "Enemy12", CharacterName = "Character_61", Speed = 3.4f, Health = 9, Damage = 11 },
            new EnemyInfo { ConfigName = "Enemy13", CharacterName = "Character_81", Speed = 3.7f, Health = 8, Damage = 12 }
        };

        [MenuItem("Tools/Generate Enemy Prefabs")]
        public static void ShowWindow()
        {
            GetWindow<EnemyPrefabGenerator>("Enemy Prefab Generator");
        }

        private void OnGUI()
        {
            GUILayout.Label("Enemy Prefab 批量生成器", EditorStyles.boldLabel);
            GUILayout.Space(10);

            EditorGUILayout.HelpBox(
                "这个工具会自动生成9个普通敌人的配置和Prefab：\n" +
                "Character_1, 2, 11, 22, 33, 47, 58, 61, 81\n\n" +
                "生成内容：\n" +
                "1. EnemyConfig配置文件\n" +
                "2. Enemy Prefab文件\n\n" +
                "生成位置: " + ENEMY_PREFAB_OUTPUT,
                MessageType.Info
            );

            GUILayout.Space(10);

            if (GUILayout.Button("生成所有Enemy配置和Prefabs", GUILayout.Height(40)))
            {
                GenerateAllEnemies();
            }
        }

        private static void GenerateAllEnemies()
        {
            // 确保输出目录存在
            if (!Directory.Exists(ENEMY_PREFAB_OUTPUT))
            {
                Directory.CreateDirectory(ENEMY_PREFAB_OUTPUT);
            }
            if (!Directory.Exists(ENEMY_CONFIG_PATH))
            {
                Directory.CreateDirectory(ENEMY_CONFIG_PATH);
            }
            AssetDatabase.Refresh();

            int successCount = 0;
            int failCount = 0;

            foreach (var enemyInfo in ENEMY_INFOS)
            {
                // 1. 创建配置文件
                if (!CreateEnemyConfig(enemyInfo))
                {
                    Debug.LogError($"创建Enemy配置失败: {enemyInfo.ConfigName}");
                    failCount++;
                    continue;
                }

                // 2. 创建Prefab
                if (GenerateEnemyPrefab(enemyInfo))
                    successCount++;
                else
                    failCount++;
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            string message = $"Enemy生成完成！\n成功: {successCount} | 失败: {failCount}";
            EditorUtility.DisplayDialog("完成", message, "OK");
        }

        private static bool CreateEnemyConfig(EnemyInfo info)
        {
            string configPath = $"{ENEMY_CONFIG_PATH}/{info.ConfigName}.asset";

            // 检查是否已存在
            EnemyConfig existingConfig = AssetDatabase.LoadAssetAtPath<EnemyConfig>(configPath);
            if (existingConfig != null)
            {
                Debug.Log($"Enemy配置已存在，跳过: {configPath}");
                return true;
            }

            try
            {
                // 创建新的EnemyConfig
                EnemyConfig config = ScriptableObject.CreateInstance<EnemyConfig>();
                config.WalkSpeed = info.Speed;
                config.Health = info.Health;
                config.Damage = info.Damage;
                config.UINotificationColor = UINotificationColorType.Default;
                config.Reward = new ResourceInfo[]
                {
                    new ResourceInfo { ResourceType = ResourceItemType.GemsGreen, Amount = 3 }
                };

                AssetDatabase.CreateAsset(config, configPath);
                Debug.Log($"✓ 创建Enemy配置: {configPath}");
                return true;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"创建Enemy配置失败 ({info.ConfigName}): {e.Message}");
                return false;
            }
        }

        private static bool GenerateEnemyPrefab(EnemyInfo info)
        {
            try
            {
                // 1. 加载角色Prefab
                string characterPath = $"{CHARACTER_PATH}/{info.CharacterName}.prefab";
                GameObject characterPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(characterPath);
                if (characterPrefab == null)
                {
                    Debug.LogError($"找不到角色Prefab: {characterPath}");
                    return false;
                }

                // 2. 加载Enemy配置
                string configPath = $"{ENEMY_CONFIG_PATH}/{info.ConfigName}.asset";
                EnemyConfig enemyConfig = AssetDatabase.LoadAssetAtPath<EnemyConfig>(configPath);
                if (enemyConfig == null)
                {
                    Debug.LogError($"找不到Enemy配置: {configPath}");
                    return false;
                }

                // 3. 创建Enemy GameObject
                GameObject enemyObject = PrefabUtility.InstantiatePrefab(characterPrefab) as GameObject;
                enemyObject.name = info.ConfigName;

                // 4. 添加EnemyView组件
                EnemyView enemyView = enemyObject.GetComponent<EnemyView>();
                if (enemyView == null)
                {
                    enemyView = enemyObject.AddComponent<EnemyView>();
                }

                // 5. 配置EnemyView
                SerializedObject serializedView = new SerializedObject(enemyView);
                SerializedProperty configProp = serializedView.FindProperty("_config");
                configProp.objectReferenceValue = enemyConfig;
                serializedView.ApplyModifiedProperties();

                // 6. 确保有Collider
                CapsuleCollider collider = enemyObject.GetComponent<CapsuleCollider>();
                if (collider == null)
                {
                    collider = enemyObject.AddComponent<CapsuleCollider>();
                    collider.radius = 0.5f;
                    collider.height = 2f;
                    collider.center = new Vector3(0, 1f, 0);
                }

                // 7. 保存为Prefab
                string outputPath = $"{ENEMY_PREFAB_OUTPUT}/{info.ConfigName}.prefab";
                PrefabUtility.SaveAsPrefabAsset(enemyObject, outputPath);

                // 8. 清理Scene中的临时对象
                DestroyImmediate(enemyObject);

                Debug.Log($"✓ 成功生成Enemy Prefab: {info.CharacterName} -> {outputPath}");
                return true;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"生成Enemy Prefab失败 ({info.ConfigName}): {e.Message}");
                return false;
            }
        }

        private struct EnemyInfo
        {
            public string ConfigName;
            public string CharacterName;
            public float Speed;
            public int Health;
            public int Damage;
        }
    }
}
