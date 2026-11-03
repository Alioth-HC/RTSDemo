
using UnityEngine;

public class FeverTowerBuilding : BuildingBase
{
    [SerializeField, Range(0f, 0.5f)]
    private float productionBonus = 0.05f;

    protected override void Awake()
    {
        base.Awake();
        SetBuildingType(BuildingType.FeverTower);
    }

    public float GetProductionMultiplier(Faction faction)
    {
        if (!IsAlive || Owner != faction || faction == Faction.Neutral)
            return 1f;

        if (RTSMatchManager.Instance == null ||
            RTSMatchManager.Instance.Phase != MatchPhase.Fever)
            return 1f;

        return 1f + productionBonus;
    }
}
