
using UnityEngine;

public class BaseBuilding : ProductionBuilding
{
    protected override void Awake()
    {
        base.Awake();
        SetBuildingType(BuildingType.Base);
        SetCapturable(false);
    }

    public override bool CanBeCapturedNow(Faction attacker)
    {
        if (RTSMatchManager.Instance == null ||
            RTSMatchManager.Instance.Phase != MatchPhase.Fever)
            return false;

        return base.CanBeCapturedNow(attacker);
    }

    protected override void Update()
    {
        // Fever 开始时开放基地占领。
        bool feverActive =
            RTSMatchManager.Instance != null &&
            RTSMatchManager.Instance.Phase == MatchPhase.Fever;

        SetCapturable(feverActive);

        base.Update();
    }
}
