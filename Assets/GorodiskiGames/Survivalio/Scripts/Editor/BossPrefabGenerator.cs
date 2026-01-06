using UnityEngine;
using UnityEditor;
using System.IO;
using Game.Enemy;
using Game.Config;

namespace Game.Editor
{
    /// <summary>
    /// Boss Prefab 自动生成器
    /// 使用方法：Unity菜单 Tools > Generate Boss Prefabs
    /// </summary>
    public class BossPrefabGenerator : EditorWindow
    {
        private const string CHARACTER_PATH = "Assets/GorodiskiGames/Layer lab/3D Casual Character/3D Casual Character/Prefabs/Characters";
        private const string BOSS_PREFAB_OUTPUT = "Assets/GorodiskiGames/Survivalio/ResourcesStatic/Prefabs/Enemies/Bosses";
        private const string BOSS_CONFIG_PATH = "Assets/GorodiskiGames/Survivalio/Resources/BossConfigs";

        private static readonly BossInfo[] BOSS_INFOS = new BossInfo[]
        {
            new BossInfo { ConfigName = "BossEast", CharacterName = "Character_53", DisplayName = "东操场Boss" },
            new BossInfo { ConfigName = "BossWest", CharacterName = "Character_60", DisplayName = "西操场Boss" },
            new BossInfo { ConfigName = "BossSunflower", CharacterName = "Character_47", DisplayName = "向日葵花田Boss" },
            new BossInfo { ConfigName = "BossDorm", CharacterName = "Character_50", DisplayName = "寝室Boss" }
        };

        [MenuItem("Tools/Generate Boss Prefabs")]
        public static void ShowWindow()
        {
            GetWindow<BossPrefabGenerator>("Boss Prefab Generator");
        }

        private void OnGUI()
        {
            GUILayout.Label("Boss Prefab 自动生成器", EditorStyles.boldLabel);
            GUILayout.Space(10);

            EditorGUILayout.HelpBox(
                "这个工具会自动生成4个Boss的Prefab：\n" +
                "- BossEast (Character_53)\n" +
                "- BossWest (Character_60)\n" +
                "- BossSunflower (Character_47)\n" +
                "- BossDorm (Character_50)\n\n" +
                "生成位置: " + BOSS_PREFAB_OUTPUT,
                MessageType.Info
            );

            GUILayout.Space(10);

            if (GUILayout.Button("生成所有Boss Prefabs", GUILayout.Height(40)))
            {
                GenerateAllBossPrefabs();
            }
        }

        private static void GenerateAllBossPrefabs()
        {
            // 确保输出目录存在
            if (!Directory.Exists(BOSS_PREFAB_OUTPUT))
            {
                Directory.CreateDirectory(BOSS_PREFAB_OUTPUT);
                AssetDatabase.Refresh();
            }

            int successCount = 0;
            int failCount = 0;

            foreach (var bossInfo in BOSS_INFOS)
            {
                if (GenerateBossPrefab(bossInfo))
                    successCount++;
                else
                    failCount++;
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            string message = $"Boss Prefab生成完成！\n成功: {successCount} | 失败: {failCount}";
            EditorUtility.DisplayDialog("完成", message, "OK");
        }

        private static bool GenerateBossPrefab(BossInfo info)
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

                // 2. 加载Boss配置
                string configPath = $"{BOSS_CONFIG_PATH}/{info.ConfigName}.asset";
                BossConfig bossConfig = AssetDatabase.LoadAssetAtPath<BossConfig>(configPath);
                if (bossConfig == null)
                {
                    Debug.LogError($"找不到Boss配置: {configPath}");
                    return false;
                }

                // 3. 创建Boss GameObject
                GameObject bossObject = PrefabUtility.InstantiatePrefab(characterPrefab) as GameObject;
                bossObject.name = info.ConfigName;

                // 4. 添加EnemyView组件
                EnemyView enemyView = bossObject.GetComponent<EnemyView>();
                if (enemyView == null)
                {
                    enemyView = bossObject.AddComponent<EnemyView>();
                }

                // 5. 配置EnemyView
                SerializedObject serializedView = new SerializedObject(enemyView);
                SerializedProperty configProp = serializedView.FindProperty("_config");
                configProp.objectReferenceValue = bossConfig;
                serializedView.ApplyModifiedProperties();

                // 6. 确保有Collider
                CapsuleCollider collider = bossObject.GetComponent<CapsuleCollider>();
                if (collider == null)
                {
                    collider = bossObject.AddComponent<CapsuleCollider>();
                    collider.radius = 0.5f;
                    collider.height = 2f;
                    collider.center = new Vector3(0, 1f, 0);
                }

                // 7. 保存为Prefab
                string outputPath = $"{BOSS_PREFAB_OUTPUT}/{info.ConfigName}.prefab";
                PrefabUtility.SaveAsPrefabAsset(bossObject, outputPath);

                // 8. 清理Scene中的临时对象
                DestroyImmediate(bossObject);

                Debug.Log($"✓ 成功生成Boss Prefab: {info.DisplayName} -> {outputPath}");
                return true;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"生成Boss Prefab失败 ({info.DisplayName}): {e.Message}");
                return false;
            }
        }

        private struct BossInfo
        {
            public string ConfigName;
            public string CharacterName;
            public string DisplayName;
        }
    }
}
