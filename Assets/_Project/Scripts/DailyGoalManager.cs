using UnityEngine;

/// <summary>
/// 每日随机目标管理器
/// </summary>
public class DailyGoalManager : MonoBehaviour
{
    public static DailyGoalManager Instance { get; private set; }

    public enum GoalTarget { Kitchen, Treadmill, Computer }

    [System.Serializable]
    public struct GoalEntry
    {
        public GoalTarget target;
        public string description;
        public int rewardMoney;
    }

    [Header("目标池")]
    public GoalEntry[] goalPool = new GoalEntry[]
    {
        new GoalEntry { target = GoalTarget.Kitchen, description = "玩厨房小游戏且不跳过", rewardMoney = 30 },
        new GoalEntry { target = GoalTarget.Treadmill, description = "使用跑步机且不跳过", rewardMoney = 20 },
        new GoalEntry { target = GoalTarget.Computer, description = "做电脑知识问答且不跳过", rewardMoney = 40 },
    };

    private int todayGoalIndex = -1;
    private bool goalCompletedToday;

    private void Awake()
    {
        Instance = this;
    }

    /// <summary>是否有活跃的每日目标</summary>
    public bool HasGoal() => todayGoalIndex >= 0 && !goalCompletedToday;

    /// <summary>随机生成今日目标，返回目标描述文本</summary>
    public string GenerateNewGoal()
    {
        if (goalPool == null || goalPool.Length == 0) return "";

        // 避免连续两天同一目标
        int index;
        do { index = Random.Range(0, goalPool.Length); }
        while (index == todayGoalIndex && goalPool.Length > 1);

        todayGoalIndex = index;
        goalCompletedToday = false;

        GoalEntry goal = goalPool[index];
        return $"{goal.description}\n奖励：<color=#FFD700>+{goal.rewardMoney} 金钱</color>";
    }

    /// <summary>玩家触发了一个小游戏（未跳过），检查是否命中目标</summary>
    public void CheckGoal(string minigameSceneName)
    {
        if (todayGoalIndex < 0 || goalCompletedToday) return;

        GoalTarget played = SceneNameToTarget(minigameSceneName);
        if (played == goalPool[todayGoalIndex].target)
        {
            goalCompletedToday = true;
            int reward = goalPool[todayGoalIndex].rewardMoney;
            if (PlayerDataManager.Instance != null)
                PlayerDataManager.Instance.ModifyStats(0, 0, 0, reward);

            if (EventPopupController.Instance != null)
                EventPopupController.Instance.DisplayNotice(
                    $"<b>目标达成！</b>\n\n{goalPool[todayGoalIndex].description}\n获得 <color=#FFD700>+{reward} 金钱</color>！",
                    "太棒了");
        }
    }

    private GoalTarget SceneNameToTarget(string name)
    {
        switch (name)
        {
            case "Kitchen": return GoalTarget.Kitchen;
            case "Treadmill": return GoalTarget.Treadmill;
            case "ComputerQuiz": return GoalTarget.Computer;
            default: return GoalTarget.Kitchen;
        }
    }
}
