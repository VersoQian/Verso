using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using Game.Modules;
using Game.UI.Pool;

namespace Game.Editor
{
    public class CollectiblesModuleViewFixer : EditorWindow
    {
        [MenuItem("Tools/修复CollectiblesModuleView引用")]
        public static void FixCollectiblesModuleView()
        {
            // 打开Gameplay场景
            var scene = EditorSceneManager.OpenScene("Assets/GorodiskiGames/Survivalio/Scenes/Gameplay.unity");

            // 查找CollectiblesModuleView组件
            var collectiblesModuleView = GameObject.FindObjectOfType<CollectiblesModuleView>();

            if (collectiblesModuleView == null)
            {
                Debug.LogError("未找到CollectiblesModuleView组件!");
                return;
            }

            // 使用反射获取私有字段
            var type = typeof(CollectiblesModuleView);
            var cashPoolField = type.GetField("_cashPool", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var gemsGreenPoolField = type.GetField("_gemsGreenPool", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var gemsPinkPoolField = type.GetField("_gemsPinkPool", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            // 在场景中查找pool对象
            var allPools = GameObject.FindObjectsOfType<ComponentPoolFactory>();

            ComponentPoolFactory cashPool = null;
            ComponentPoolFactory gemsGreenPool = null;
            ComponentPoolFactory gemsPinkPool = null;

            foreach (var pool in allPools)
            {
                var poolName = pool.gameObject.name;

                if (poolName == "Cash")
                {
                    cashPool = pool;
                    Debug.Log($"找到Cash pool: {pool.gameObject.name}");
                }
                else if (poolName == "GemsGreen")
                {
                    gemsGreenPool = pool;
                    Debug.Log($"找到GemsGreen pool: {pool.gameObject.name}");
                }
                else if (poolName == "GemsPink")
                {
                    gemsPinkPool = pool;
                    Debug.Log($"找到GemsPink pool: {pool.gameObject.name}");
                }
            }

            // 分配引用
            if (cashPool != null && cashPoolField != null)
            {
                cashPoolField.SetValue(collectiblesModuleView, cashPool);
                Debug.Log("✅ 已分配_cashPool引用");
            }
            else
            {
                Debug.LogError("❌ 无法分配_cashPool");
            }

            if (gemsGreenPool != null && gemsGreenPoolField != null)
            {
                gemsGreenPoolField.SetValue(collectiblesModuleView, gemsGreenPool);
                Debug.Log("✅ 已分配_gemsGreenPool引用");
            }
            else
            {
                Debug.LogError("❌ 无法分配_gemsGreenPool");
            }

            if (gemsPinkPool != null && gemsPinkPoolField != null)
            {
                gemsPinkPoolField.SetValue(collectiblesModuleView, gemsPinkPool);
                Debug.Log("✅ 已分配_gemsPinkPool引用");
            }
            else
            {
                Debug.LogError("❌ 无法分配_gemsPinkPool");
            }

            // 标记场景为已修改
            EditorUtility.SetDirty(collectiblesModuleView);
            EditorSceneManager.MarkSceneDirty(scene);

            // 保存场景
            EditorSceneManager.SaveScene(scene);

            Debug.Log("=== CollectiblesModuleView引用修复完成! ===");
        }
    }
}
