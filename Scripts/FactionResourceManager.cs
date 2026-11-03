
using UnityEngine;

public class FactionResourceManager : MonoBehaviour
{
    public static FactionResourceManager Instance { get; private set; }

    [Header("初始资源")]
    [SerializeField, Min(0)] private int initialResources = 100;

    private int blueResources;
    private int redResources;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError("场景中存在多个 FactionResourceManager。");
            enabled = false;
            return;
        }

        Instance = this;
        blueResources = initialResources;
        redResources = initialResources;
    }

    public int GetResources(Faction faction)
    {
        if (faction == Faction.Blue) return blueResources;
        if (faction == Faction.Red) return redResources;
        return 0;
    }

    public bool SpendResources(Faction faction, int amount)
    {
        if (amount <= 0) return false;

        if (faction == Faction.Blue)
        {
            if (blueResources < amount) return false;
            blueResources -= amount;
            return true;
        }

        if (faction == Faction.Red)
        {
            if (redResources < amount) return false;
            redResources -= amount;
            return true;
        }

        return false;
    }

    public void AddResources(Faction faction, int amount)
    {
        if (amount <= 0) return;

        if (faction == Faction.Blue)
            blueResources += amount;
        else if (faction == Faction.Red)
            redResources += amount;
    }
}
