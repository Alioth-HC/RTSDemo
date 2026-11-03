
using UnityEngine;

[RequireComponent(typeof(UnitMovement))]
public class Unit : MonoBehaviour
{
    public Faction Owner { get; private set; }
    public bool IsAlive { get; private set; } = true;

    private UnitMovement movement;

    private void Awake()
    {
        movement = GetComponent<UnitMovement>();
    }

    public void Initialize(Faction owner)
    {
        Owner = owner;
        IsAlive = true;
    }

    public void MoveTo(Vector3 destination, BuildingBase target)
    {
        if (!IsAlive || movement == null || target == null)
            return;

        movement.MoveTo(destination, target);
    }

    public void ArriveAtBuilding(BuildingBase target)
    {
        if (!IsAlive)
            return;

        if (target != null && target.IsAlive)
            target.ReceiveArrivingUnit(Owner);

        // 无论是否成功占领，抵达的实体都立即销毁。
        IsAlive = false;
        Destroy(gameObject);
    }

    public void Die()
    {
        if (!IsAlive)
            return;

        IsAlive = false;

        if (movement != null)
            movement.Stop();

        Destroy(gameObject);
    }
}
