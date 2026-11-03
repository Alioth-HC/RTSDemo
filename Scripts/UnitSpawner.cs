
using UnityEngine;

public class UnitSpawner : MonoBehaviour
{
    [Header("驻军设置")]
    [SerializeField, Min(0)] private int initialTroops = 10;
    [SerializeField, Min(0)] private int minimumGarrison = 1;

    [Header("基础生产设置")]
    [SerializeField, Min(0.1f)] private float productionInterval = 3f;
    [SerializeField, Min(0)] private int maximumTroops = 100;

    private BuildingBase building;
    private float productionTimer;
    private int troopCount;

    private float baseProductionInterval;
    private int baseMaximumTroops;
    private int currentUpgradeLevel = 1;

    public int TroopCount => troopCount;

    public int AvailableCount =>
        Mathf.Max(0, troopCount - minimumGarrison);

    public float CurrentProductionInterval =>
        productionInterval;

    public int MaximumTroops => maximumTroops;

    private void Awake()
    {
        building = GetComponent<BuildingBase>();

        baseProductionInterval = productionInterval;
        baseMaximumTroops = maximumTroops;

        troopCount = Mathf.Clamp(initialTroops, 0, maximumTroops);
    }

    private void Update()
    {
        if (building == null ||
            !building.IsAlive ||
            !building.IsInitialized ||
            !isActiveAndEnabled)
            return;

        if (building.Type != BuildingType.Base &&
            building.Type != BuildingType.Barracks)
            return;

        if (building.Owner == Faction.Neutral)
            return;

        if (troopCount >= maximumTroops)
            return;

        productionTimer += Time.deltaTime;

        if (productionTimer >= productionInterval)
        {
            productionTimer -= productionInterval;
            AddTroops(1);
        }
    }

    // 由 BuildingUpgrade 调用
    public void SetUpgradeLevel(int level)
    {
        currentUpgradeLevel = Mathf.Clamp(level, 1, 4);

        if (building == null ||
            (building.Type != BuildingType.Base &&
             building.Type != BuildingType.Barracks))
            return;

        float[] intervals = { 3f, 2.6f, 2.2f, 1.8f };
        int[] capacities = { 100, 120, 140, 160 };

        int index = currentUpgradeLevel - 1;

        productionInterval =
            baseProductionInterval * intervals[index] / 3f;

        maximumTroops =
            Mathf.RoundToInt(
                baseMaximumTroops * capacities[index] / 100f);

        // 升级不会凭空增加士兵，只提高兵力上限。
        troopCount = Mathf.Min(troopCount, maximumTroops);

        Debug.Log(
            $"{name} Lv.{currentUpgradeLevel}：" +
            $"生产间隔 {productionInterval:F1} 秒，" +
            $"兵力上限 {maximumTroops}");
    }

    public bool CanDispatch(int count)
    {
        return building != null
            && building.IsAlive
            && building.IsInitialized
            && isActiveAndEnabled
            && count > 0
            && count <= AvailableCount;
    }

    public bool RemoveTroops(int count)
    {
        if (count <= 0 || troopCount < count)
            return false;

        troopCount -= count;
        return true;
    }

    public void AddTroops(int count)
    {
        if (count <= 0)
            return;

        troopCount = Mathf.Min(maximumTroops, troopCount + count);
    }

    public void SetTroops(int count)
    {
        troopCount = Mathf.Clamp(count, 0, maximumTroops);
    }
}
