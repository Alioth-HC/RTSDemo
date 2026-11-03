
using UnityEngine;

public class DefenseTowerBuilding : BuildingBase
{
    [SerializeField, Min(0.1f)] private float attackRange = 8f;
    [SerializeField, Min(0.1f)] private float attackInterval = 1.5f;

    private float attackTimer;

    protected override void Awake()
    {
        base.Awake();
        SetBuildingType(BuildingType.DefenseTower);
    }

    protected override void Update()
    {
        base.Update();

        if (!IsAlive || Owner == Faction.Neutral)
            return;

        attackTimer += Time.deltaTime;

        if (attackTimer < attackInterval)
            return;

        Unit target = FindNearestEnemy();

        if (target == null)
            return;

        attackTimer = 0f;
        target.Die();
    }

    private Unit FindNearestEnemy()
    {
        Unit[] allUnits = FindObjectsByType<Unit>(
            FindObjectsSortMode.None);

        Unit nearest = null;
        float nearestDistance = attackRange * attackRange;

        foreach (Unit unit in allUnits)
        {
            if (unit == null || !unit.IsAlive ||
                unit.Owner == Owner ||
                unit.Owner == Faction.Neutral)
                continue;

            float distance =
                (unit.transform.position - transform.position).sqrMagnitude;

            if (distance <= nearestDistance)
            {
                nearestDistance = distance;
                nearest = unit;
            }
        }

        return nearest;
    }
}
