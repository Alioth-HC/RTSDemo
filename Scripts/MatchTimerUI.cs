
using UnityEngine;
using TMPro;

public class MatchTimerUI : MonoBehaviour
{
    [Header("UI 文本")]
    [SerializeField] private TMP_Text matchTimeText;
    [SerializeField] private TMP_Text phaseText;

    private void Update()
    {
        RTSMatchManager manager = RTSMatchManager.Instance;

        if (manager == null)
        {
            if (matchTimeText != null)
                matchTimeText.text = "等待比赛开始";

            if (phaseText != null)
                phaseText.text = "";

            return;
        }

        // 整场比赛剩余时间
        float remaining = manager.RemainingTime;

        int minutes = Mathf.FloorToInt(remaining / 60f);
        int seconds = Mathf.FloorToInt(remaining % 60f);

        if (matchTimeText != null)
        {
            matchTimeText.text = $"比赛剩余：{minutes:00}:{seconds:00}";
        }

        // 根据真实比赛阶段显示状态
        if (phaseText != null)
        {
            switch (manager.Phase)
            {
                case MatchPhase.Normal:
                    float feverTime = Mathf.Max(
                        0f, 120f - manager.ElapsedTime);

                    int feverMinutes =
                        Mathf.FloorToInt(feverTime / 60f);

                    int feverSeconds =
                        Mathf.FloorToInt(feverTime % 60f);

                    phaseText.text =
                        $"普通阶段  |  Fever 倒计时：{feverMinutes:00}:{feverSeconds:00}";
                    break;

                case MatchPhase.Fever:
                    float feverRemaining = manager.FeverRemainingTime;

                    int feverRemainMinutes =
                        Mathf.FloorToInt(feverRemaining / 60f);

                    int feverRemainSeconds =
                        Mathf.FloorToInt(feverRemaining % 60f);

                    phaseText.text =
                        $"FEVER！剩余时间：{feverRemainMinutes:00}:{feverRemainSeconds:00}";
                    break;

                case MatchPhase.Ended:
                    phaseText.text = "比赛结束";
                    break;
            }
        }
    }
}
