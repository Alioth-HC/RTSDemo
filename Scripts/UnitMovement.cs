
using UnityEngine;

public class UnitMovement : MonoBehaviour
{
    [Header("移动设置")]
    [SerializeField, Min(0.1f)] private float moveSpeed = 4f;
    [SerializeField, Min(0.1f)] private float extraArrivalDistance = 1.5f;

    private Vector3 destination;
    private BuildingBase targetBuilding;
    private bool isMoving;

    public bool IsMoving => isMoving;

    public void MoveTo(Vector3 position, BuildingBase target)
    {
        destination = position;
        targetBuilding = target;
        isMoving = true;
    }

    private void Update()
    {
        if (!isMoving)
            return;

        if (targetBuilding == null || !targetBuilding.IsAlive)
        {
            Stop();
            return;
        }

        Vector3 current = transform.position;
        Vector3 direction = destination - current;
        direction.y = 0f;

        float arrivalDistance = GetArrivalDistance(targetBuilding);

        if (direction.magnitude <= arrivalDistance)
        {
            Arrive();
            return;
        }

        Vector3 next = Vector3.MoveTowards(
            current,
            destination,
            moveSpeed * Time.deltaTime);

        // 暂时保持当前高度，避免直接移动时上下抖动。
        next.y = current.y;
        transform.position = next;

        Vector3 facing = next - current;
        facing.y = 0f;

        if (facing.sqrMagnitude > 0.001f)
            transform.forward = facing.normalized;
    }

    private float GetArrivalDistance(BuildingBase target)
    {
        Collider[] colliders =
            target.GetComponentsInChildren<Collider>(true);

        float radius = 0f;

        foreach (Collider col in colliders)
        {
            if (col == null || !col.enabled)
                continue;

            Bounds bounds = col.bounds;
            float distance = new Vector2(
                bounds.extents.x,
                bounds.extents.z).magnitude;

            radius = Mathf.Max(radius, distance);
        }

        return radius + extraArrivalDistance;
    }

    private void Arrive()
    {
        isMoving = false;

        Unit unit = GetComponent<Unit>();

        if (unit != null)
            unit.ArriveAtBuilding(targetBuilding);

        targetBuilding = null;
    }

    public void Stop()
    {
        isMoving = false;
        targetBuilding = null;
    }
}
