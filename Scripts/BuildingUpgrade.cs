
using UnityEngine;

[RequireComponent(typeof(BuildingBase))]
public class BuildingUpgrade : MonoBehaviour
{
    [Header("升级设置")]
    [SerializeField, Range(1, 4)] private int level = 1;

    private BuildingBase building;

    public int Level => level;
    public bool IsMaxLevel => level >= 4;

    private void Awake()
    {
        building = GetComponent<BuildingBase>();

        UnitSpawner spawner = GetComponent<UnitSpawner>();
        if (spawner != null)
        {
            spawner.SetUpgradeLevel(level);
        }
    }

    public int GetUpgradeCost()
    {
        if (level == 1) return 50;
        if (level == 2) return 100;
        if (level == 3) return 175;

        return 0;
    }

    // 由升级面板的按钮调用
    public void Upgrade()
    {
        if (building == null ||
            !building.IsAlive ||
            !building.IsInitialized)
            return;

        if (building.Owner == Faction.Neutral)
        {
            Debug.Log("中立建筑不能升级，请先占领。");
            return;
        }

        if (IsMaxLevel)
        {
            Debug.Log("该建筑已经达到最高等级。");
            return;
        }

        if (FactionResourceManager.Instance == null)
        {
            Debug.LogError("场景中没有 FactionResourceManager。");
            return;
        }

        int cost = GetUpgradeCost();

        if (!FactionResourceManager.Instance.SpendResources(
                building.Owner, cost))
        {
            Debug.Log("资源不足，无法升级。");
            return;
        }

        level++;

        Debug.Log($"{name} 升级到 Lv.{level}，消耗资源 {cost}。");

        ApplyUpgradeEffect();
    }


    private void ApplyUpgradeEffect()
    {
        UnitSpawner spawner = GetComponent<UnitSpawner>();

        if (spawner != null)
        {
            spawner.SetUpgradeLevel(level);
        }

        // 防御塔、资源建筑和 Fever 塔的升级效果，
        // 下一步分别接入各自的功能。
    }

}
