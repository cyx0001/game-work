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

    private int lastEventIndex = -1;

    private void Awake()
    {
        // 经典单例模式：无需 DontDestroyOnLoad
        Instance = this;

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
                    InteractableObject.TUTORIAL_MESSAGE, "知道了", () =>
                    {
                        PlayerPrefs.SetInt(TUTORIAL_PREF_KEY, 1);
                        PlayerPrefs.Save();
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
        if (ThresholdEventManager.Instance != null)
        {
            ThresholdEventManager.Instance.ProcessNightSettlement(FinishEndDay);
            return;
        }

        FinishEndDay();
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

    private void TriggerDailyRandomEvent()
    {
        if (EventPopupController.Instance == null || GameResultManager.Instance == null)
            return;

        if (GameResultManager.Instance.gameOverPanel.activeSelf || GameResultManager.Instance.gameWinPanel.activeSelf)
            return;

        EventData dailyEvent = PickRandomDailyEvent();
        if (dailyEvent != null)
        {
            EventPopupController.Instance.DisplayEvent(dailyEvent);
        }
    }

    private EventData[] GetValidEvents()
    {
        if (dailyEventPoolAsset == null || dailyEventPoolAsset.events == null)
            return System.Array.Empty<EventData>();

        return System.Array.FindAll(dailyEventPoolAsset.events, e => e != null);
    }

    private EventData PickRandomDailyEvent()
    {
        EventData[] validEvents = GetValidEvents();
        if (validEvents.Length == 0)
            return null;

        if (validEvents.Length == 1)
        {
            lastEventIndex = 0;
            return validEvents[0];
        }

        int index;
        do
        {
            index = Random.Range(0, validEvents.Length);
        } while (index == lastEventIndex);

        lastEventIndex = index;
        return validEvents[index];
    }
}