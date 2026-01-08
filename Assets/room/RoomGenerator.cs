using System.Collections.Generic;
using UnityEngine;

public class RoomGridGenerator : MonoBehaviour
{
    [Header("资源引用")]
    [Tooltip("将 30 个 Room Variant 预制体拖入此列表")]
    public List<GameObject> roomPrefabs;

    [Tooltip("拖入场景中那个 Scale 为 (100, 100, 1) 的 Quad")]
    public Transform quadFloor;

    [Header("生成配置")]
    private int gridCount = 5; // 5x5 网格
    // 缩放改为 2，正好适配 20x20 的网格空间
    public Vector3 targetScale = new Vector3(2f, 1f, 2f);

    void Start()
    {
        GenerateGrid();
    }

    [ContextMenu("Generate Grid")]
    public void GenerateGrid()
    {
        if (roomPrefabs == null || roomPrefabs.Count < 25 || quadFloor == null)
        {
            Debug.LogError("错误：Prefab 数量不足 25 个或未关联 Quad！");
            return;
        }

        // 1. 彻底清理，防止旧房间堆叠导致视觉上的“重叠”和“闪烁”
        ClearExistingRooms();

        // 2. 实现不重复逻辑：洗牌算法 (Shuffle)
        List<GameObject> shuffledRooms = new List<GameObject>(roomPrefabs);
        for (int i = 0; i < shuffledRooms.Count; i++)
        {
            GameObject temp = shuffledRooms[i];
            int randomIndex = Random.Range(i, shuffledRooms.Count);
            shuffledRooms[i] = shuffledRooms[randomIndex];
            shuffledRooms[randomIndex] = temp;
        }

        // 3. 计算网格中心
        // Quad 总长 100，5x5 网格，步进间距 step 必须是 20
        float step = 20f;
        // 计算起始偏移：使 5x5 阵列中心对齐 Quad 的中心
        // 第一个格子中心点在 -40, -20, 0, 20, 40
        float startOffset = -40f;

        Vector3 floorPos = quadFloor.position;
        int roomIndex = 0;

        // 4. 嵌套循环在每个网格中心生成
        for (int x = 0; x < gridCount; x++)
        {
            for (int z = 0; z < gridCount; z++)
            {
                // 计算精确的网格中心点世界坐标
                float posX = floorPos.x + startOffset + (x * step);
                float posZ = floorPos.z + startOffset + (z * step);

                // Y轴抬高 0.1f 彻底解决地面闪烁 (Z-Fighting)
                // 确保房间地板略高于 Quad 表面
                Vector3 spawnPos = new Vector3(posX, floorPos.y + 0.1f, posZ);

                // 随机旋转：90 或 180 度
                float randomY = Random.Range(0, 2) == 0 ? 90f : 180f;
                Quaternion spawnRot = Quaternion.Euler(0, randomY, 0);

                // 5. 实例化打乱后的不重复房间
                GameObject roomInstance = Instantiate(shuffledRooms[roomIndex], spawnPos, spawnRot);
                roomIndex++;

                // 6. 应用缩放 (2, 1, 2) 并设为子物体
                roomInstance.transform.localScale = targetScale;
                roomInstance.transform.SetParent(quadFloor);

                // 命名，方便清理逻辑精准识别
                roomInstance.name = "Room_Instance_Clean";
            }
        }

        Debug.Log("5x5 网格生成完毕，房间互不重复且已对齐中心。");
    }

    private void ClearExistingRooms()
    {
        // 查找并删除所有名为 Room_Instance_Clean 的子物体
        List<GameObject> toDestroy = new List<GameObject>();
        foreach (Transform child in quadFloor)
        {
            if (child.name == "Room_Instance_Clean")
                toDestroy.Add(child.gameObject);
        }

        foreach (GameObject obj in toDestroy)
        {
            if (Application.isPlaying) Destroy(obj);
            else DestroyImmediate(obj);
        }
    }
}