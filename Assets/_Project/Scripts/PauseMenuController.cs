using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class PauseMenuController : MonoBehaviour
{
    public static PauseMenuController Instance { get; private set; }

    [Header("暂停面板")]
    public GameObject pausePanel;
    public GameObject maskBackground;

    [Header("按钮")]
    public Button btnReturnTitle;
    public Button btnExitGame;

    private bool isPaused;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (pausePanel != null) pausePanel.SetActive(false);
        if (maskBackground != null) maskBackground.SetActive(false);

        btnReturnTitle.onClick.AddListener(OnReturnToTitle);
        btnExitGame.onClick.AddListener(OnExitGame);
    }

    private void Update()
    {
        if (!Input.GetKeyDown(KeyCode.Escape)) return;

        // 游戏结束/胜利面板显示时，ESC 不响应
        if (GameResultManager.Instance != null)
        {
            if ((GameResultManager.Instance.gameOverPanel != null && GameResultManager.Instance.gameOverPanel.activeSelf) ||
                (GameResultManager.Instance.gameWinPanel != null && GameResultManager.Instance.gameWinPanel.activeSelf))
                return;
        }

        // 事件弹窗显示时（教程、随机事件），ESC 不响应，避免冲突
        if (EventPopupController.Instance != null && EventPopupController.Instance.gameObject.activeSelf)
            return;

        if (isPaused) ResumeGame();
        else PauseGame();
    }

    public void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f;
        if (pausePanel != null) pausePanel.SetActive(true);
        if (maskBackground != null) maskBackground.SetActive(true);
    }

    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;
        if (pausePanel != null) pausePanel.SetActive(false);
        if (maskBackground != null) maskBackground.SetActive(false);
    }

    private void OnReturnToTitle()
    {
        Time.timeScale = 1f;
        isPaused = false;

        if (SleepFadeController.Instance != null)
            Destroy(SleepFadeController.Instance.gameObject);

        GameObject oldPromptCanvas = GameObject.Find("_PromptCanvas");
        if (oldPromptCanvas != null) Destroy(oldPromptCanvas);

        GameObject oldGoalCanvas = GameObject.Find("_GoalReminderCanvas");
        if (oldGoalCanvas != null) Destroy(oldGoalCanvas);

        SceneManager.LoadScene("Start");
    }

    private void OnExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
