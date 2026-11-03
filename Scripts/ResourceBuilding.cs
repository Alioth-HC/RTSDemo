
using UnityEngine;

public class ResourceBuilding : BuildingBase
{
    [SerializeField, Min(0f)] private float resourcePerSecond = 1f;

    public float StoredResource { get; private set; }

    protected override void Awake()
    {
        base.Awake();
        SetBuildingType(BuildingType.Resource);
    }

    protected override void Update()
    {
        base.Update();

        if (IsAlive && Owner != Faction.Neutral)
            StoredResource += resourcePerSecond * Time.deltaTime;
    }

    public float CollectResource()
    {
        float amount = StoredResource;
        StoredResource = 0f;
        return amount;
    }
}
