using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class GameResultManager : MonoBehaviour
{
    private const string StartSceneName = "Start";

    public static GameResultManager Instance { get; private set; }

    [Header("结果UI面板")]
    public GameObject gameOverPanel;
    public GameObject gameWinPanel;

    [Header("失败原因文本")]
    public TextMeshProUGUI reasonText;

    [Header("胜利结局文本")]
    public TextMeshProUGUI winResultText;

    [Header("重开按钮")]
    public Button restartButton;
    public Button winRestartButton;

    private void Awake()
    {
        // 如果已有实例，销毁自己（防止重复挂载导致冲突）
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (gameWinPanel != null) gameWinPanel.SetActive(false);

        if (restartButton != null) restartButton.onClick.AddListener(RestartGame);
        if (winRestartButton != null) winRestartButton.onClick.AddListener(RestartGame);
    }

    public void CheckGameCondition(float sugar, float health, float mood, int currentDay)
    {
        if (sugar >= GameConstants.MAX_BLOOD_SUGAR)
        {
            TriggerGameOver("血糖彻底失控！血糖飙升到 250 以上，引发了急性并发症，被紧急送进 ICU。很遗憾，你的控糖之旅到此结束。");
            return;
        }
        if (sugar <= GameConstants.MIN_BLOOD_SUGAR)
        {
            TriggerGameOver("血糖过低危险！发生严重的低血糖昏迷，被路人发现送进 ICU。很遗憾，你的控糖之旅到此结束。");
            return;
        }
        if (mood <= 0f)
        {
            TriggerGameOver("心情极度崩溃，抑郁情绪让你再也无法坚持控糖计划。在巨大的精神压力与焦虑下，你选择了放弃控糖计划……");
            return;
        }

        if (currentDay > 14)
        {
            // 14天结束，根据血糖判定三种结局
            TriggerGameWin(sugar, health, mood);
            return;
        }
    }

    private void TriggerGameOver(string reason)
    {
        if (SleepFadeController.Instance != null)
            SleepFadeController.Instance.ClearOverlay();

        if (EventPopupController.Instance != null)
            EventPopupController.Instance.ForceClose();

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            if (reasonText != null)
            {
                reasonText.text = reason;
            }
            Time.timeScale = 0f;
        }
    }

    private void TriggerGameWin(float sugar, float health, float mood)
    {
        if (SleepFadeController.Instance != null)
            SleepFadeController.Instance.ClearOverlay();

        if (EventPopupController.Instance != null)
            EventPopupController.Instance.ForceClose();

        if (gameWinPanel != null)
        {
            gameWinPanel.SetActive(true);

            string endingText;
            if (sugar < 70f)
            {
                endingText = "<b>低血糖结局</b>\n\n14天过去了，你的血糖控制得有些过头了，长期处于偏低状态。\n\n虽然避免了高血糖的风险，但低血糖同样危险——你已经好几次感到头晕乏力、手脚发抖，甚至有一次在办公室差点晕倒。\n\n医生语重心长地告诉你：「控糖不是越低越好，平衡才是关键。」\n\n最终血糖：" + sugar.ToString("F0") + "（偏低）\n最终健康：" + health.ToString("F0") + "\n最终心情：" + mood.ToString("F0");
            }
            else if (sugar > 120f)
            {
                endingText = "<b>高血糖结局</b>\n\n14天的控糖挑战结束了，你的血糖依旧偏高。\n\n虽然你没有出现急性并发症，但医生看着你的体检报告摇了摇头：长期高血糖正在悄悄地损害你的血管、肾脏和视网膜。\n\n你有些自责，但回想起这14天，你也确实努力过。只是那些奶茶、烧烤和深夜零食的诱惑实在太难抵挡了……\n\n“控糖是一辈子的事。”，医生说，“下次再来过吧。”\n\n最终血糖：" + sugar.ToString("F0") + "（偏高）\n最终健康：" + health.ToString("F0") + "\n最终心情：" + mood.ToString("F0");
            }
            else
            {
                endingText = "<b>完美控糖！</b>\n\n恭喜你！14天的控糖挑战圆满完成！\n\n你的血糖稳稳地控制在了 70~120 的安全范围内。医生看着你的体检报告露出了欣慰的笑容：“你做得非常好，继续保持！”\n\n这14天里，你拒绝了奶茶的诱惑，坚持了跑步机上的汗水，在厨房里学会了健康烹饪，还在电脑前充实了糖尿病知识。\n\n你证明了自己可以掌控血糖，而不是被血糖掌控。\n\n新的生活方式已经养成，未来的每一天都会更加健康！\n\n最终血糖：" + sugar.ToString("F0") + "（安全范围）\n最终健康：" + health.ToString("F0") + "\n最终心情：" + mood.ToString("F0");
            }

            if (winResultText != null)
                winResultText.text = endingText;

            Time.timeScale = 0f;
        }
    }

    private void RestartGame()
    {
        Time.timeScale = 1f;

        // ===== 清理所有 DontDestroyOnLoad 残留的 UI 对象，确保新一局干净初始化 =====
        // 1. 销毁 SleepFadeController（会在需要时重新创建）
        if (SleepFadeController.Instance != null)
        {
            Destroy(SleepFadeController.Instance.gameObject);
            Debug.Log("[Restart] 已销毁残留的 SleepFadeController");
        }

        // 2. 销毁交互提示画布 _PromptCanvas（会在 InteractableObject 初始化时重建）
        GameObject oldPromptCanvas = GameObject.Find("_PromptCanvas");
        if (oldPromptCanvas != null)
        {
            Destroy(oldPromptCanvas);
            Debug.Log("[Restart] 已销毁残留的 _PromptCanvas");
        }

        // 3. 销毁目标提醒面板画布 _GoalReminderCanvas（会在 HUDController 初始化时重建）
        GameObject oldGoalCanvas = GameObject.Find("_GoalReminderCanvas");
        if (oldGoalCanvas != null)
        {
            Destroy(oldGoalCanvas);
            Debug.Log("[Restart] 已销毁残留的 _GoalReminderCanvas");
        }

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StopBGM();
            SceneManager.LoadScene(StartSceneName);
            AudioManager.Instance.PlayDefaultBGM(); // 加载完Start后立即播放
        }
        else
        {
            SceneManager.LoadScene(StartSceneName);
        }
    }
}
