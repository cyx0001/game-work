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

    [Header("控制按钮")]
    public Button restartButton;
    public Button winRestartButton;

    private void Awake()
    {
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
            TriggerGameOver("由于你连续摄入高糖，血糖飙升至 250 以上，引发了急性并发症，被紧急送往 ICU！");
            return;
        }
        if (sugar <= GameConstants.MIN_BLOOD_SUGAR)
        {
            TriggerGameOver("血糖过低归零！因严重低血糖昏倒，被紧急送往 ICU！");
            return;
        }
        // 健康归零由 ThresholdEventManager 触发强制入院，游戏继续
        if (mood <= 0f)
        {
            TriggerGameOver("你的心情极度抑郁归零。在无尽的压力与焦虑下，你放弃了控糖管理……");
            return;
        }

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
        Time.timeScale = 1f;
        SceneManager.LoadScene(StartSceneName);
    }
}
