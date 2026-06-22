using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class GameResultManager : MonoBehaviour
{
    private const string StartSceneName = "Start";

    public static GameResultManager Instance { get; private set; }

    [Header("���UI���")]
    public GameObject gameOverPanel;
    public GameObject gameWinPanel;

    [Header("ʧ��ԭ���ı�")]
    public TextMeshProUGUI reasonText;

    [Header("���ư�ť")]
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
            TriggerGameOver("����������������ǣ�Ѫ������� 250 ���ϣ������˼��Բ���֢������������ ICU��");
            return;
        }
        if (sugar <= GameConstants.MIN_BLOOD_SUGAR)
        {
            TriggerGameOver("Ѫ�ǹ��͹��㣡�����ص�Ѫ�ǻ赹������������ ICU��");
            return;
        }
        // ���������� ThresholdEventManager ����ǿ����Ժ����Ϸ����
        if (mood <= 0f)
        {
            TriggerGameOver("������鼫���������㡣���޾���ѹ���뽹���£�������˿��ǹ�������");
            return;
        }

        if (currentDay > 14)
        {
            TriggerGameWin();
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

    private void TriggerGameWin()
    {
        if (SleepFadeController.Instance != null)
            SleepFadeController.Instance.ClearOverlay();

        if (EventPopupController.Instance != null)
            EventPopupController.Instance.ForceClose();

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
