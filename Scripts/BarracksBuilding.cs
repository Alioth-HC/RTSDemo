
using UnityEngine;

public class BarracksBuilding : ProductionBuilding
{
    protected override void Awake()
    {
        base.Awake();
        SetBuildingType(BuildingType.Barracks);
    }
}
