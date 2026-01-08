using UnityEngine;
using UnityEditor;
using System.IO;
using Game.Config;

namespace Game.Editor
{
    /// <summary>
    /// 批量渲染装备预览图标
    /// 使用方法：Unity顶部菜单 -> Tools -> Render Equipment Icons
    /// </summary>
    public class EquipmentIconRenderer : EditorWindow
    {
        private const string ICON_OUTPUT_PATH = "Assets/GorodiskiGames/Survivalio/Resources/InventoryConfigs/ClothConfigs/Icons";
        private const int ICON_SIZE = 512; // 图标分辨率
        private const float CAMERA_DISTANCE = 1.5f;

        private Camera renderCamera;
        private RenderTexture renderTexture;
        private GameObject previewObject;

        [MenuItem("Tools/Render Equipment Icons")]
        public static void ShowWindow()
        {
            var window = GetWindow<EquipmentIconRenderer>("装备图标渲染器");
            window.minSize = new Vector2(400, 300);
            window.Show();
        }

        private void OnGUI()
        {
            GUILayout.Space(10);
            EditorGUILayout.HelpBox("此工具将批量渲染所有装备的3D预览图标：\n\n" +
                                    "1. 读取所有ClothConfig配置\n" +
                                    "2. 渲染每个装备的3D模型预览图\n" +
                                    "3. 保存为PNG并自动更新Icon引用", MessageType.Info);

            GUILayout.Space(20);

            if (GUILayout.Button("开始渲染所有装备图标", GUILayout.Height(40)))
            {
                RenderAllEquipmentIcons();
            }

            GUILayout.Space(10);

            EditorGUILayout.HelpBox($"图标将保存到: {ICON_OUTPUT_PATH}", MessageType.None);
        }

        private void RenderAllEquipmentIcons()
        {
            // 创建输出目录
            if (!Directory.Exists(ICON_OUTPUT_PATH))
            {
                Directory.CreateDirectory(ICON_OUTPUT_PATH);
                AssetDatabase.Refresh();
            }

            // 设置渲染环境
            SetupRenderEnvironment();

            try
            {
                // 加载所有ClothConfig
                string[] configPaths = AssetDatabase.FindAssets("t:ClothConfig", new[] { "Assets/GorodiskiGames/Survivalio/Resources/InventoryConfigs/ClothConfigs" });

                int totalConfigs = configPaths.Length;
                int processedCount = 0;

                foreach (string guid in configPaths)
                {
                    string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                    ClothConfig config = AssetDatabase.LoadAssetAtPath<ClothConfig>(assetPath);

                    if (config != null && config.Mesh != null)
                    {
                        processedCount++;
                        float progress = (float)processedCount / totalConfigs;
                        EditorUtility.DisplayProgressBar("渲染装备图标", $"正在渲染: {config.name}", progress);

                        // 渲染并保存图标
                        RenderEquipmentIcon(config);
                    }
                }

                EditorUtility.ClearProgressBar();
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                EditorUtility.DisplayDialog("成功", $"已成功渲染 {processedCount} 个装备图标！", "确定");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"渲染失败: {e.Message}\n{e.StackTrace}");
                EditorUtility.DisplayDialog("失败", $"渲染失败: {e.Message}", "确定");
            }
            finally
            {
                CleanupRenderEnvironment();
            }
        }

        private void SetupRenderEnvironment()
        {
            // 创建渲染相机
            GameObject cameraObject = new GameObject("EquipmentIconCamera");
            renderCamera = cameraObject.AddComponent<Camera>();
            renderCamera.clearFlags = CameraClearFlags.SolidColor;
            renderCamera.backgroundColor = new Color(0.2f, 0.2f, 0.2f, 0f); // 透明背景
            renderCamera.orthographic = false;
            renderCamera.fieldOfView = 30;
            renderCamera.nearClipPlane = 0.1f;
            renderCamera.farClipPlane = 10f;

            // 创建RenderTexture
            renderTexture = new RenderTexture(ICON_SIZE, ICON_SIZE, 24, RenderTextureFormat.ARGB32);
            renderTexture.antiAliasing = 8;
            renderCamera.targetTexture = renderTexture;

            // 添加灯光
            GameObject lightObject = new GameObject("IconLight");
            lightObject.transform.SetParent(cameraObject.transform);
            Light light = lightObject.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.5f;
            light.transform.rotation = Quaternion.Euler(50, -30, 0);
        }

        private void CleanupRenderEnvironment()
        {
            if (renderCamera != null)
            {
                DestroyImmediate(renderCamera.gameObject);
            }
            if (renderTexture != null)
            {
                renderTexture.Release();
                DestroyImmediate(renderTexture);
            }
            if (previewObject != null)
            {
                DestroyImmediate(previewObject);
            }
        }

        private void RenderEquipmentIcon(ClothConfig config)
        {
            // 创建预览对象
            if (previewObject != null)
            {
                DestroyImmediate(previewObject);
            }

            previewObject = new GameObject("PreviewObject");
            MeshFilter meshFilter = previewObject.AddComponent<MeshFilter>();
            MeshRenderer meshRenderer = previewObject.AddComponent<MeshRenderer>();

            meshFilter.sharedMesh = config.Mesh;

            // 使用默认材质（稍后可以改进为使用装备的真实材质）
            Material defaultMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/GorodiskiGames/Survivalio/ResourcesStatic/Materials/Player/Player.mat");
            if (defaultMaterial == null)
            {
                defaultMaterial = new Material(Shader.Find("Standard"));
            }
            meshRenderer.sharedMaterial = defaultMaterial;

            // 计算合适的相机位置
            Bounds bounds = meshFilter.sharedMesh.bounds;
            float maxSize = Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z);
            Vector3 targetPosition = bounds.center;

            // 设置相机位置（从正面稍微向上看）
            renderCamera.transform.position = targetPosition + new Vector3(0, maxSize * 0.3f, -CAMERA_DISTANCE * maxSize);
            renderCamera.transform.LookAt(targetPosition);
            previewObject.transform.position = Vector3.zero;

            // 渲染
            renderCamera.Render();

            // 保存为PNG
            Texture2D iconTexture = new Texture2D(ICON_SIZE, ICON_SIZE, TextureFormat.ARGB32, false);
            RenderTexture.active = renderTexture;
            iconTexture.ReadPixels(new Rect(0, 0, ICON_SIZE, ICON_SIZE), 0, 0);
            iconTexture.Apply();
            RenderTexture.active = null;

            byte[] pngData = iconTexture.EncodeToPNG();
            string iconPath = $"{ICON_OUTPUT_PATH}/{config.name}_Icon.png";
            File.WriteAllBytes(iconPath, pngData);
            DestroyImmediate(iconTexture);

            // 刷新AssetDatabase
            AssetDatabase.ImportAsset(iconPath);

            // 更新ClothConfig的Icon引用
            Sprite newIcon = AssetDatabase.LoadAssetAtPath<Sprite>(iconPath);
            if (newIcon != null)
            {
                SerializedObject so = new SerializedObject(config);
                so.FindProperty("Icon").objectReferenceValue = newIcon;
                so.ApplyModifiedProperties();
                EditorUtility.SetDirty(config);
            }

            Debug.Log($"✅ 已渲染图标: {config.name} -> {iconPath}");
        }

        private void OnDestroy()
        {
            CleanupRenderEnvironment();
        }
    }
}
