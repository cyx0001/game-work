using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

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
    private Canvas parentCanvas;
    private int savedSortingOrder;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        parentCanvas = GetComponent<Canvas>();

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

        // 临时提升 Canvas 层级，确保盖住小游戏的 Canvas
        if (parentCanvas != null)
        {
            savedSortingOrder = parentCanvas.sortingOrder;
            parentCanvas.overrideSorting = true;
            parentCanvas.sortingOrder = 300;
        }

        if (pausePanel != null) pausePanel.SetActive(true);
        if (maskBackground != null) maskBackground.SetActive(true);
    }

    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;

        // 恢复 Canvas 层级
        if (parentCanvas != null)
        {
            parentCanvas.sortingOrder = savedSortingOrder;
            parentCanvas.overrideSorting = false;
        }

        if (pausePanel != null) pausePanel.SetActive(false);
        if (maskBackground != null) maskBackground.SetActive(false);
    }

    private void OnReturnToTitle()
    {
        Time.timeScale = 1f;
        isPaused = false;

        // 销毁所有 DontDestroyOnLoad 单例，确保返回标题后全新初始化
        // 否则小游戏切过的 BGM 会残留在 AudioManager 里，再进游戏音乐不对
        if (AudioManager.Instance != null)
            Destroy(AudioManager.Instance.gameObject);

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
