using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("��Ϸ����״̬")]
    public int currentDay = 1;
    public int remainingAP = 5; // ÿ�� 5 ���ж���

    [HideInInspector] public bool isInMinigame = false; // �Ƿ�����С��Ϸ

    [HideInInspector] public UnityEvent OnAPChanged = new UnityEvent();

    [Header("每日随机事件")]
    public DailyEventPool dailyEventPoolAsset;

    private int lastAdaptIndex = -1;
    private int lastCrisisIndex = -1;
    private int lastSprintIndex = -1;

    private void Awake()
    {
        // 经典单例模式：无需 DontDestroyOnLoad
        Instance = this;

        // 自动创建每日目标管理器
        if (FindObjectOfType<DailyGoalManager>() == null)
            gameObject.AddComponent<DailyGoalManager>();

#if UNITY_EDITOR
        // 编辑器中每次运行自动清除游戏记录，确保测试每次从零开始
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        Debug.Log("[Editor] 已清除所有 PlayerPrefs，测试环境已重置");
#endif
    }

    private const string TUTORIAL_PREF_KEY = "Tutorial_Shown";

    private void Start()
    {
        // 确保主场景BGM播放（处理重启后音乐被停的情况）
        if (AudioManager.Instance != null && !AudioManager.Instance.bgmSource.isPlaying)
            AudioManager.Instance.PlayDefaultBGM();

        currentDay = 1;
        remainingAP = GameConstants.DAILY_AP;
        OnAPChanged.Invoke();

        // 首次运行显示主场景操作教程（延迟一帧确保所有单例就绪）
        StartCoroutine(ShowMainTutorialIfNeeded());
    }

    private IEnumerator ShowMainTutorialIfNeeded()
    {
        yield return null; // 等一帧，确保 EventPopupController 等初始化完成

        if (PlayerPrefs.GetInt(TUTORIAL_PREF_KEY, 0) == 0)
        {
            if (EventPopupController.Instance != null)
            {
                EventPopupController.Instance.DisplayNotice(
                    InteractableObject.TUTORIAL_MESSAGE, "继续", () =>
                    {
                        // 第一个弹窗关闭后，弹出第二个：物件说明
                        EventPopupController.Instance.DisplayNotice(
                            InteractableObject.TUTORIAL_OBJECTS_MESSAGE, "开始游戏", () =>
                            {
                                PlayerPrefs.SetInt(TUTORIAL_PREF_KEY, 1);
                                PlayerPrefs.Save();
                            });
                    });
            }
            else
            {
                PlayerPrefs.SetInt(TUTORIAL_PREF_KEY, 1);
                PlayerPrefs.Save();
            }
        }
    }

    public bool UseAP(int amount)
    {
        if (remainingAP >= amount)
        {
            remainingAP -= amount;
            Debug.Log($"��AP���ġ������� {amount} ��AP��ʣ�� AP: {remainingAP}");
            OnAPChanged.Invoke();
            return true;
        }
        else
        {
            Debug.LogWarning("�ж��㲻�㣡�޷�ִ�д˲�����");
            return false;
        }
    }

    public void EndDay()
    {
        // 小游戏期间不允许结算
        if (isInMinigame) return;

        if (ThresholdEventManager.Instance != null)
        {
            ThresholdEventManager.Instance.ProcessNightSettlement(() =>
            {
                // 结算弹窗确认后 → 黑屏过渡 → 进入下一天
                EnsureSleepFadeController();
                SleepFadeController.Instance.PlaySleepTransition(FinishEndDay);
            });
            return;
        }

        FinishEndDay();
    }

    private void EnsureSleepFadeController()
    {
        if (SleepFadeController.Instance == null)
        {
            GameObject go = new GameObject("SleepFadeController");
            go.AddComponent<SleepFadeController>();
        }
    }

    private void FinishEndDay()
    {
        currentDay++;

        int apPenalty = ThresholdEventManager.Instance != null
            ? ThresholdEventManager.Instance.ConsumeNextDayApPenalty()
            : 0;
        remainingAP = Mathf.Max(0, GameConstants.DAILY_AP - apPenalty);
        OnAPChanged.Invoke();

        // ÿ�ν�����һ�죬�ȼ���Ƿ�����ͨ��Ҫ�󣨱���Ź�14�죩
        if (GameResultManager.Instance != null)
        {
            GameResultManager.Instance.CheckGameCondition(
                PlayerDataManager.Instance.bloodSugar,
                PlayerDataManager.Instance.health,
                PlayerDataManager.Instance.mood,
                currentDay
            );
        }

        TriggerDailyRandomEvent();
    }

    private void TriggerDailyRandomEvent(System.Action onComplete = null)
    {
        if (EventPopupController.Instance == null || GameResultManager.Instance == null)
        {
            onComplete?.Invoke();
            return;
        }

        if (GameResultManager.Instance.gameOverPanel.activeSelf || GameResultManager.Instance.gameWinPanel.activeSelf)
        {
            onComplete?.Invoke();
            return;
        }

        EventData dailyEvent = PickRandomDailyEvent();
        if (dailyEvent != null)
        {
            // 随机事件关闭后 → 显示每日目标
            EventPopupController.Instance.DisplayEvent(dailyEvent, () =>
            {
                ShowDailyGoalPopup(onComplete);
            });
        }
        else
        {
            ShowDailyGoalPopup(onComplete);
        }
    }

    private void ShowDailyGoalPopup(System.Action onComplete)
    {
        if (DailyGoalManager.Instance == null)
        {
            onComplete?.Invoke();
            return;
        }

        string goalMsg = DailyGoalManager.Instance.GenerateNewGoal();
        if (string.IsNullOrEmpty(goalMsg))
        {
            onComplete?.Invoke();
            return;
        }

        if (EventPopupController.Instance != null)
            EventPopupController.Instance.DisplayNotice(
                $"<b>今日目标</b>\n\n{goalMsg}", "知道了", onComplete);
        else
            onComplete?.Invoke();
    }

    private EventData[] GetValidEvents()
    {
        if (dailyEventPoolAsset == null)
            return System.Array.Empty<EventData>();

        EventData[] pool = dailyEventPoolAsset.GetEventsForDay(currentDay);
        if (pool == null || pool.Length == 0)
            return System.Array.Empty<EventData>();

        return System.Array.FindAll(pool, e => e != null);
    }

    private ref int GetPhaseLastIndex()
    {
        if (currentDay <= 5) return ref lastAdaptIndex;
        if (currentDay <= 10) return ref lastCrisisIndex;
        return ref lastSprintIndex;
    }

    private EventData PickRandomDailyEvent()
    {
        EventData[] validEvents = GetValidEvents();
        if (validEvents.Length == 0)
            return null;

        ref int lastIdx = ref GetPhaseLastIndex();

        if (validEvents.Length == 1)
        {
            lastIdx = 0;
            return validEvents[0];
        }

        int index;
        do
        {
            index = Random.Range(0, validEvents.Length);
        } while (index == lastIdx);

        lastIdx = index;
        return validEvents[index];
    }
}