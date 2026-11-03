
using System;
using UnityEngine;

public enum MatchPhase
{
    Normal,
    Fever,
    Ended
}

public class RTSMatchManager : MonoBehaviour
{
    public static RTSMatchManager Instance { get; private set; }

    [Header("测试版比赛时间")]
    [SerializeField] private float normalDuration = 120f;
    [SerializeField] private float totalDuration = 180f;

    public MatchPhase Phase { get; private set; } = MatchPhase.Normal;
    public float ElapsedTime { get; private set; }

    // 比赛剩余时间，供 UI 使用
    public float RemainingTime =>
        Mathf.Max(0f, totalDuration - ElapsedTime);

    // Fever 剩余时间
    public float FeverRemainingTime =>
        Mathf.Max(0f, totalDuration - Mathf.Max(ElapsedTime, normalDuration));

    public event Action<MatchPhase> PhaseChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError("场景中存在多个 RTSMatchManager。");
            enabled = false;
            return;
        }

        Instance = this;

        if (normalDuration <= 0f || totalDuration <= normalDuration)
        {
            Debug.LogError("比赛时间配置错误：总时间必须大于普通阶段时间。");
            enabled = false;
            return;
        }

        Debug.Log("RTSMatchManager 已初始化，当前阶段：" + Phase);
    }

    private void Update()
    {
        if (Phase == MatchPhase.Ended)
            return;

        ElapsedTime += Time.deltaTime;

        // 优先检查比赛是否结束
        if (ElapsedTime >= totalDuration)
        {
            ElapsedTime = totalDuration;
            SetPhase(MatchPhase.Ended);
        }
        else if (Phase == MatchPhase.Normal &&
                 ElapsedTime >= normalDuration)
        {
            SetPhase(MatchPhase.Fever);
        }
    }

    private void SetPhase(MatchPhase phase)
    {
        if (Phase == phase)
            return;

        Phase = phase;
        PhaseChanged?.Invoke(Phase);

        Debug.Log("比赛阶段切换：" + Phase);
    }

    // 方便测试时重新开始比赛
    public void RestartMatch()
    {
        ElapsedTime = 0f;
        Phase = MatchPhase.Normal;
        PhaseChanged?.Invoke(Phase);

        Debug.Log("比赛已重新开始。");
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}
