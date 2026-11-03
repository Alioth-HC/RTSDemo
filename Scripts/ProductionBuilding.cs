
using UnityEngine;

public class ProductionBuilding : BuildingBase
{
    protected UnitSpawner spawner;

    protected override void Awake()
    {
        base.Awake();
        spawner = GetComponent<UnitSpawner>();
    }

    protected override void OnInitialized()
    {
        base.OnInitialized();
        spawner = GetComponent<UnitSpawner>();
    }

    public virtual bool CanDispatch(int count, Faction playerFaction)
    {
        return CanAcceptCommand(playerFaction)
            && spawner != null
            && spawner.CanDispatch(count);
    }

    public virtual bool DispatchUnits(
        int count,
        Vector3 destination,
        BuildingBase target,
        Faction playerFaction)
    {
        // 旧版派兵接口不再直接执行派兵。
        // 统一由 RTSCommandManager 管理扣除驻军和生成士兵。
        Debug.LogWarning(
            "请通过 RTSCommandManager 执行派兵。");

        return false;
    }
}
