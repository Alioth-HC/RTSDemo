
using UnityEngine;

public enum Faction
{
    Blue,
    Red,
    Neutral
}

public enum BuildingType
{
    Base,
    Barracks,
    DefenseTower,
    Resource,
    FeverTower
}

public class BuildingBase : MonoBehaviour
{
    [Header("建筑设置")]
    [SerializeField] private BuildingType buildingType;
    [SerializeField] private Faction faction = Faction.Neutral;
    [SerializeField] private bool canBeCaptured = true;

    [Header("区域设置")]
    [SerializeField] private bool isInSafeZone;

    public BuildingType Type => buildingType;
    public Faction Owner => faction;
    public bool CanBeCaptured => canBeCaptured;
    public bool IsInSafeZone => isInSafeZone;
    public bool IsAlive { get; private set; } = true;
    public bool IsInitialized { get; private set; }

    protected virtual void Awake()
    {
        IsInitialized = true;
    }

    public virtual void Initialize(Faction owner, bool safeZone)
    {
        faction = owner;
        isInSafeZone = safeZone;
        IsAlive = true;
        IsInitialized = true;

        if (owner == Faction.Neutral)
        {
            UnitSpawner spawner = GetComponent<UnitSpawner>();

            if (spawner != null)
                spawner.SetTroops(0);
        }

        OnInitialized();
    }

    protected virtual void OnInitialized() { }

    protected void SetBuildingType(BuildingType type)
    {
        buildingType = type;
    }

    public virtual bool CanAcceptCommand(Faction playerFaction)
    {
        return IsAlive
            && isActiveAndEnabled
            && IsInitialized
            && playerFaction != Faction.Neutral
            && faction == playerFaction;
    }

    public virtual bool CanBeCapturedNow(Faction attacker)
    {
        if (!IsAlive || !isActiveAndEnabled || !IsInitialized)
            return false;

        if (!canBeCaptured ||
            attacker == Faction.Neutral ||
            attacker == faction)
            return false;

        if (buildingType == BuildingType.Base)
        {
            if (RTSMatchManager.Instance == null ||
                RTSMatchManager.Instance.Phase != MatchPhase.Fever)
                return false;
        }

        return true;
    }

    public bool SetOwner(Faction newOwner)
    {
        if (!IsAlive ||
            !IsInitialized ||
            newOwner == Faction.Neutral ||
            newOwner == faction)
            return false;

        if (!CanBeCapturedNow(newOwner))
            return false;

        ChangeOwner(newOwner);
        return true;
    }

    private void ChangeOwner(Faction newOwner)
    {
        faction = newOwner;
        OnOwnerChanged();
    }

    protected virtual void OnOwnerChanged()
    {
        Debug.Log($"{name} 的所属阵营变为 {faction}");
    }

    public void SetSafeZone(bool value)
    {
        isInSafeZone = value;
    }

    public void SetCapturable(bool value)
    {
        canBeCaptured = value;
    }

    public void ReceiveArrivingUnit(Faction attacker)
    {
        if (!IsAlive || attacker == Faction.Neutral)
            return;

        UnitSpawner garrison = GetComponent<UnitSpawner>();

        if (garrison == null)
        {
            Debug.LogError($"{name} 缺少 UnitSpawner，无法接收驻军。");
            return;
        }

        // 一、己方士兵入驻：只增加驻军，不发生战斗
        if (attacker == Owner)
        {
            garrison.AddTroops(1);
            Debug.Log($"{name} 接收己方士兵，当前驻军：{garrison.TroopCount}");
            return;
        }

        // 二、敌方士兵到达，但当前不能占领
        if (!CanBeCapturedNow(attacker))
            return;

        // 三、存在守军：双方交战，守军减少一个
        if (garrison.TroopCount > 0)
        {
            garrison.RemoveTroops(1);
            Debug.Log($"{name} 发生战斗，剩余驻军：{garrison.TroopCount}");
            return;
        }

        // 四、没有守军：占领建筑，第一名士兵成为驻军
        bool captured = SetOwner(attacker);

        if (captured)
        {
            garrison.AddTroops(1);
            Debug.Log($"{name} 被 {attacker} 占领，当前驻军：{garrison.TroopCount}");
        }
    }

    protected virtual void Update() { }

    public void MarkDestroyed()
    {
        if (!IsAlive)
            return;

        IsAlive = false;
        Destroy(gameObject);
    }
}
