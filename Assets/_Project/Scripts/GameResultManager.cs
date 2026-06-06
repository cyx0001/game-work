using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement; // 用于重新加载场景
using TMPro;

public class GameResultManager : MonoBehaviour
{
    public static GameResultManager Instance { get; private set; }

    [Header("结果UI面板")]
    public GameObject gameOverPanel;
    public GameObject gameWinPanel;

    [Header("失败原因文本")]
    public TextMeshProUGUI reasonText;

    [Header("控制按钮")]
    public Button restartButton;
    public Button winRestartButton;

    private void Awake()
    {
        Instance = this;

        // 确保一开始都是隐藏的
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (gameWinPanel != null) gameWinPanel.SetActive(false);

        // 绑定按钮事件：点击后重新加载当前关卡
        if (restartButton != null) restartButton.onClick.AddListener(RestartGame);
        if (winRestartButton != null) winRestartButton.onClick.AddListener(RestartGame);
    }

    // 核心方法：检查当前的数值是否触发危机或通关
    public void CheckGameCondition(float sugar, float health, float mood, int currentDay)
    {
        // 1. 判定失败条件
        if (sugar >= 250f)
        {
            TriggerGameOver("由于你连续摄入高糖，血糖飙升至 250 以上，引发了急性并发症，被紧急送往 ICU！");
            return;
        }
        if (health <= 0f)
        {
            TriggerGameOver("你的健康值彻底归零！身体各器官不堪重负，你病倒了……");
            return;
        }
        if (mood <= 0f)
        {
            TriggerGameOver("你的心情极度抑郁归零。在无尽的压力与焦虑下，你放弃了控糖管理……");
            return;
        }

        // 2. 判定通关条件（假设一共要撑过 14 天）
        if (currentDay > 14)
        {
            TriggerGameWin();
        }
    }

    private void TriggerGameOver(string reason)
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            if (reasonText != null)
            {
                reasonText.text = reason;
            }
            // 暂停游戏时间（防止后面还能点物件）
            Time.timeScale = 0f;
        }
    }

    private void TriggerGameWin()
    {
        if (gameWinPanel != null)
        {
            gameWinPanel.SetActive(true);
            Time.timeScale = 0f;
        }
    }

    private void RestartGame()
    {
        // 恢复时间流速
        Time.timeScale = 1f;
        // 重新加载当前场景
        UnityEngine.SceneManagement.SceneManager.LoadScene(
         UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex
     );
    }
}