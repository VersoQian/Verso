using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AdvancedEnvironmentSpawner : MonoBehaviour
{
    [Header("向日葵设置")]
    public GameObject[] sunflowerPrefabs;
    public int sunflowerCount = 200;
    public int sunflowersPerFrame = 10; // 每帧生成的数量，数值越小越不卡

    [Header("风车设置")]
    public GameObject windmillPrefab;
    public int windmillCount = 6;
    public float minWindmillDistance = 15f; // 风车之间的最小间距
    public int maxRetryAttempts = 50;      // 放置单个风车的最大尝试次数

    [Header("中心避让设置")]
    public bool avoidWorldZero = true;     // 是否避开世界零点
    public float zeroAvoidRadius = 10f;    // 避开零点的半径范围

    [Header("生成区域")]
    [Tooltip("会自动根据Quad的Scale计算，也可以手动微调")]
    public Vector2 areaPadding = new Vector2(5f, 5f);

    private List<Vector3> windmillPositions = new List<Vector3>();

    void Start()
    {
        // 启动异步生成协程
        StartCoroutine(SpawnSequence());
    }

    IEnumerator SpawnSequence()
    {
        // 计算生成范围 (基于 Quad 的 Scale)
        // 注意：Quad 的 Mesh 默认是 1x1，Scale 为 10 则长度为 10
        float rangeX = (transform.localScale.x / 2) - areaPadding.x;
        float rangeZ = (transform.localScale.y / 2) - areaPadding.y;

        // 1. 生成风车 (带距离检测 & 零点避让)
        yield return StartCoroutine(SpawnWindmills(rangeX, rangeZ));

        // 2. 生成向日葵 (分帧密植)
        yield return StartCoroutine(SpawnSunflowers(rangeX, rangeZ));

        Debug.Log("所有物体生成完毕！");
    }

    // --- 风车生成逻辑 ---
    IEnumerator SpawnWindmills(float rx, float rz)
    {
        int spawned = 0;
        while (spawned < windmillCount)
        {
            bool foundPos = false;
            for (int i = 0; i < maxRetryAttempts; i++)
            {
                Vector3 testPos = GetRandomPos(rx, rz);

                // 同时检测：1.是否远离零点 2.是否远离其他风车
                if (IsFarEnoughFromZero(testPos) && IsFarEnoughFromOthers(testPos))
                {
                    InstantiateObject(windmillPrefab, testPos);
                    windmillPositions.Add(testPos);
                    spawned++;
                    foundPos = true;
                    break;
                }
            }

            yield return null;

            if (!foundPos)
            {
                Debug.LogWarning($"无法为第 {spawned + 1} 个风车找到空位，建议减小间距或避让半径");
                break;
            }
        }
    }

    // --- 向日葵生成逻辑 ---
    IEnumerator SpawnSunflowers(float rx, float rz)
    {
        for (int i = 0; i < sunflowerCount; i++)
        {
            GameObject prefab = sunflowerPrefabs[Random.Range(0, sunflowerPrefabs.Length)];
            Vector3 pos = GetRandomPos(rx, rz);

            // 如果你也希望向日葵避开中心点，可以取消下面注释
            // if(avoidWorldZero && Vector3.Distance(pos, Vector3.zero) < zeroAvoidRadius) continue;

            InstantiateObject(prefab, pos);

            if (i > 0 && i % sunflowersPerFrame == 0)
            {
                yield return null;
            }
        }
    }

    // --- 工具函数 ---

    Vector3 GetRandomPos(float rx, float rz)
    {
        return new Vector3(
            Random.Range(-rx, rx),
            0.05f,
            Random.Range(-rz, rz)
        ) + transform.position;
    }

    // 检测是否远离世界零点
    bool IsFarEnoughFromZero(Vector3 pos)
    {
        if (!avoidWorldZero) return true;

        // 计算当前点到世界原点 (0,0,0) 的距离
        float distToZero = Vector3.Distance(pos, Vector3.zero);
        return distToZero >= zeroAvoidRadius;
    }

    // 检测是否远离其他已生成的风车
    bool IsFarEnoughFromOthers(Vector3 pos)
    {
        foreach (Vector3 existingPos in windmillPositions)
        {
            if (Vector3.Distance(pos, existingPos) < minWindmillDistance)
                return false;
        }
        return true;
    }

    void InstantiateObject(GameObject prefab, Vector3 pos)
    {
        GameObject go = Instantiate(prefab, pos, Quaternion.Euler(0, Random.Range(0, 360), 0));
        go.transform.SetParent(this.transform);
    }
}