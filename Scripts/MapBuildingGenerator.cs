
using System.Collections.Generic;
using UnityEngine;

public class MapBuildingGenerator : MonoBehaviour
{
    [Header("地图大小")]
    [SerializeField] private Vector2 mapSize = new Vector2(120f, 100f);
    [SerializeField, Min(1f)] private float safeZoneRadius = 20f;

    [Header("随机设置")]
    [SerializeField] private int randomSeed = 12345;
    [SerializeField, Min(0)] private int neutralBuildingCount = 30;
    [SerializeField, Min(0.5f)] private float minimumSpacing = 7f;
    [SerializeField, Min(1)] private int maxAttemptsPerBuilding = 150;

    [Header("建筑预制体")]
    [SerializeField] private GameObject blueBasePrefab;
    [SerializeField] private GameObject redBasePrefab;
    [SerializeField] private GameObject barracksPrefab;
    [SerializeField] private GameObject defenseTowerPrefab;
    [SerializeField] private GameObject resourceBuildingPrefab;
    [SerializeField] private GameObject feverTowerPrefab;

    private readonly List<GameObject> generatedBuildings =
        new List<GameObject>();

    private readonly List<Bounds> occupiedBounds =
        new List<Bounds>();

    private System.Random random;
    private bool isGenerating;

    private void Start()
    {
        GenerateMap();
    }

    [ContextMenu("重新生成地图")]
    public void GenerateMap()
    {
        if (isGenerating)
            return;

        // 运行时 Destroy 是延迟执行的，下一帧再生成，
        // 避免旧建筑尚未销毁时又生成一批新建筑。
        if (Application.isPlaying && generatedBuildings.Count > 0)
        {
            ClearGeneratedBuildings();
            CancelInvoke(nameof(GenerateMapInternal));
            Invoke(nameof(GenerateMapInternal), 0.1f);
            return;
        }

        GenerateMapInternal();
    }

    private void GenerateMapInternal()
    {
        if (isGenerating)
            return;

        isGenerating = true;

        ClearGeneratedBuildings();
        occupiedBounds.Clear();
        random = new System.Random(randomSeed);

        if (!CheckRequiredPrefabs())
        {
            isGenerating = false;
            return;
        }

        Vector3 bluePosition =
            new Vector3(-mapSize.x * 0.34f, 0f, 0f);

        Vector3 redPosition =
            new Vector3(mapSize.x * 0.34f, 0f, 0f);

        bool blueCreated =
            SpawnBuilding(blueBasePrefab, bluePosition, Faction.Blue, true);

        bool redCreated =
            SpawnBuilding(redBasePrefab, redPosition, Faction.Red, true);

        if (!blueCreated || !redCreated)
        {
            Debug.LogError("基地生成失败，请先检查双方基地预制体。");
            isGenerating = false;
            return;
        }

        int created = 0;

        for (int i = 0; i < neutralBuildingCount; i++)
        {
            bool placed = false;

            for (int attempt = 0;
                 attempt < maxAttemptsPerBuilding;
                 attempt++)
            {
                Vector3 position = GetRandomPosition();

                if (!IsInsideMap(position))
                    continue;

                if (Vector3.Distance(position, bluePosition)
                    < safeZoneRadius + 3f)
                    continue;

                if (Vector3.Distance(position, redPosition)
                    < safeZoneRadius + 3f)
                    continue;

                GameObject prefab = ChooseBuilding(position);

                if (prefab == null)
                    continue;

                if (SpawnBuilding(
                    prefab, position, Faction.Neutral, false))
                {
                    created++;
                    placed = true;
                    break;
                }
            }

            if (!placed)
            {
                Debug.LogWarning(
                    $"第 {i + 1} 座中立建筑找不到合适位置，已跳过。");
            }
        }

        Debug.Log(
            $"地图生成完成：中立建筑 {created}/{neutralBuildingCount} 座，随机种子 {randomSeed}。");

        isGenerating = false;
    }

    private bool CheckRequiredPrefabs()
    {
        if (mapSize.x <= 0f || mapSize.y <= 0f)
        {
            Debug.LogError("地图长宽必须大于 0。");
            return false;
        }

        if (blueBasePrefab == null || redBasePrefab == null)
        {
            Debug.LogError("请设置蓝方和红方基地预制体。");
            return false;
        }

        if (barracksPrefab == null ||
            defenseTowerPrefab == null ||
            resourceBuildingPrefab == null ||
            feverTowerPrefab == null)
        {
            Debug.LogError(
                "请在 Inspector 中设置兵营、防御塔、资源建筑和 Fever 塔预制体。");
            return false;
        }

        return true;
    }

    private Vector3 GetRandomPosition()
    {
        float x = ((float)random.NextDouble() - 0.5f) * mapSize.x;
        float z = ((float)random.NextDouble() - 0.5f) * mapSize.y;

        return new Vector3(x, 0f, z);
    }

    private bool IsInsideMap(Vector3 position)
    {
        float nx = position.x / (mapSize.x * 0.5f);
        float nz = position.z / (mapSize.y * 0.5f);

        float angle = Mathf.Atan2(nz, nx);
        float boundary =
            1f
            + 0.06f * Mathf.Sin(angle * 3f)
            + 0.04f * Mathf.Cos(angle * 5f);

        return nx * nx + nz * nz <= boundary * boundary;
    }

    private GameObject ChooseBuilding(Vector3 position)
    {
        float distanceToCenter = Mathf.Abs(position.x);

        // 靠近双方后方：兵营概率较高。
        if (distanceToCenter > mapSize.x * 0.22f &&
            random.NextDouble() < 0.55)
        {
            return barracksPrefab;
        }

        // 中间争夺区域：防御塔概率较高。
        if (distanceToCenter < mapSize.x * 0.20f &&
            random.NextDouble() < 0.60)
        {
            return defenseTowerPrefab;
        }

        double roll = random.NextDouble();

        if (roll < 0.40) return resourceBuildingPrefab;
        if (roll < 0.70) return barracksPrefab;
        if (roll < 0.90) return defenseTowerPrefab;

        return feverTowerPrefab;
    }

    private bool SpawnBuilding(
        GameObject prefab,
        Vector3 position,
        Faction owner,
        bool safeZone)
    {
        if (prefab == null)
            return false;

        GameObject obj = Instantiate(
            prefab, position, Quaternion.identity, transform);

        // 在根物体或子物体上查找，避免组件挂在子物体时误报。
        BuildingBase building =
            obj.GetComponentInChildren<BuildingBase>(true);

        if (building == null)
        {
            Debug.LogError(
                $"预制体“{prefab.name}”没有 BuildingBase 或其子类组件。"
                + "请检查组件是否存在，以及相关脚本是否编译成功。");

            Destroy(obj);
            return false;
        }

        Bounds bounds = GetBuildingBounds(obj);

        if (OverlapsExistingBuildings(bounds))
        {
            Destroy(obj);
            return false;
        }

        building.Initialize(owner, safeZone);

        generatedBuildings.Add(obj);
        occupiedBounds.Add(bounds);

        return true;
    }

    private Bounds GetBuildingBounds(GameObject obj)
    {
        Renderer[] renderers =
            obj.GetComponentsInChildren<Renderer>(true);

        bool foundRenderer = false;
        Bounds bounds = new Bounds(obj.transform.position, Vector3.one);

        foreach (Renderer renderer in renderers)
        {
            if (renderer == null)
                continue;

            if (!foundRenderer)
            {
                bounds = renderer.bounds;
                foundRenderer = true;
            }
            else
            {
                bounds.Encapsulate(renderer.bounds);
            }
        }

        // 没有 Renderer 时使用默认占地范围，防止所有建筑
        // 因为没有可见模型而被当成零尺寸建筑。
        if (!foundRenderer)
        {
            bounds = new Bounds(
                obj.transform.position,
                new Vector3(4f, 2f, 4f));
        }

        return bounds;
    }

    private bool OverlapsExistingBuildings(Bounds candidate)
    {
        for (int i = 0; i < occupiedBounds.Count; i++)
        {
            Bounds existing = occupiedBounds[i];

            // 只检查地图平面 X/Z，并为两座建筑留出间隔。
            existing.Expand(
                new Vector3(minimumSpacing * 2f, 0f,
                            minimumSpacing * 2f));

            if (candidate.Intersects(existing))
                return true;
        }

        return false;
    }

    private void ClearGeneratedBuildings()
    {
        for (int i = generatedBuildings.Count - 1; i >= 0; i--)
        {
            GameObject obj = generatedBuildings[i];

            if (obj == null)
                continue;

            if (Application.isPlaying)
                Destroy(obj);
            else
                DestroyImmediate(obj);
        }

        generatedBuildings.Clear();
        occupiedBounds.Clear();
    }
}

