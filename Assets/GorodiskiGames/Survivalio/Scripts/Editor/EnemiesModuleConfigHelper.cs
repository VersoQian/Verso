using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using Game.Modules;
using Game.UI.Pool;

namespace Game.Editor
{
    /// <summary>
    /// EnemiesModule配置助手
    /// 显示需要添加的Enemy Prefab列表
    /// </summary>
    public class EnemiesModuleConfigHelper : EditorWindow
    {
        private const string ENEMY_PREFAB_PATH = "Assets/GorodiskiGames/Survivalio/ResourcesStatic/Prefabs/Enemies";
        private const string BOSS_PREFAB_PATH = "Assets/GorodiskiGames/Survivalio/ResourcesStatic/Prefabs/Enemies/Bosses";

        [MenuItem("Tools/EnemiesModule配置指南")]
        public static void ShowWindow()
        {
            GetWindow<EnemiesModuleConfigHelper>("EnemiesModule配置指南");
        }

        private void OnGUI()
        {
            GUILayout.Label("EnemiesModule 配置指南", EditorStyles.boldLabel);
            GUILayout.Space(10);

            EditorGUILayout.HelpBox(
                "请按以下步骤手动配置Gameplay Scene中的EnemiesModule：\n\n" +
                "1. 打开 Gameplay Scene\n" +
                "2. 选中 EnemiesModule GameObject\n" +
                "3. 在Inspector中找到 EnemiesModuleView 组件\n" +
                "4. 展开 Enemies 数组\n" +
                "5. 将下面列出的Enemy Prefab拖入数组\n" +
                "6. 设置 Boss Pool",
                MessageType.Info
            );

            GUILayout.Space(10);

            if (GUILayout.Button("打开Gameplay Scene", GUILayout.Height(30)))
            {
                EditorSceneManager.OpenScene("Assets/GorodiskiGames/Survivalio/Scenes/Gameplay.unity");
            }

            GUILayout.Space(10);

            GUILayout.Label("需要添加的Enemy Prefab：", EditorStyles.boldLabel);

            EditorGUILayout.HelpBox(
                "Enemy0, Enemy1, Enemy2, Enemy3, Enemy4\n" +
                "Enemy5, Enemy6, Enemy7, Enemy8, Enemy9\n" +
                "Enemy10, Enemy11, Enemy12, Enemy13\n\n" +
                "位置: " + ENEMY_PREFAB_PATH,
                MessageType.None
            );

            GUILayout.Space(10);

            GUILayout.Label("可用的Boss Prefab：", EditorStyles.boldLabel);

            EditorGUILayout.HelpBox(
                "BossEast - 东操场Boss\n" +
                "BossWest - 西操场Boss\n" +
                "BossSunflower - 向日葵花田Boss\n" +
                "BossDorm - 寝室Boss\n\n" +
                "位置: " + BOSS_PREFAB_PATH,
                MessageType.None
            );

            GUILayout.Space(10);

            if (GUILayout.Button("选中EnemiesModule GameObject", GUILayout.Height(30)))
            {
                SelectEnemiesModule();
            }

            GUILayout.Space(10);

            if (GUILayout.Button("Ping Enemy Prefab文件夹", GUILayout.Height(30)))
            {
                var folder = AssetDatabase.LoadAssetAtPath<Object>(ENEMY_PREFAB_PATH);
                if (folder != null)
                {
                    EditorGUIUtility.PingObject(folder);
                    Selection.activeObject = folder;
                }
            }
        }

        private static void SelectEnemiesModule()
        {
            // 打开Scene
            EditorSceneManager.OpenScene("Assets/GorodiskiGames/Survivalio/Scenes/Gameplay.unity");

            // 查找并选中EnemiesModule
            var enemiesModule = FindObjectOfType<EnemiesModuleView>();
            if (enemiesModule != null)
            {
                Selection.activeGameObject = enemiesModule.gameObject;
                EditorGUIUtility.PingObject(enemiesModule.gameObject);
                Debug.Log("已选中EnemiesModule GameObject");
            }
            else
            {
                EditorUtility.DisplayDialog("提示", "找不到EnemiesModuleView组件！", "OK");
            }
        }
    }
}
