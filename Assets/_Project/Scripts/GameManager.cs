using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("游戏流程状态")]
    public int currentDay = 1;
    public int remainingAP = 5; // 每天 5 点行动点

    [HideInInspector] public UnityEvent OnAPChanged = new UnityEvent();

    [Header("测试用事件")]
    public EventData testEvent;

    private void Awake()
    {
        // 干净的场景单例，不再 DontDestroyOnLoad
        Instance = this;
    }

    private void Start()
    {
        // 每次场景重启，硬性确保数值干净归位
        currentDay = 1;
        remainingAP = 5;
        OnAPChanged.Invoke();
    }

    // 尝试消耗行动点
    public bool UseAP(int amount)
    {
        if (remainingAP >= amount)
        {
            remainingAP -= amount;
            Debug.Log($"【AP消耗】消耗了 {amount} 点AP，剩余 AP: {remainingAP}");
            OnAPChanged.Invoke();
            return true;
        }
        else
        {
            Debug.LogWarning("行动点不足！无法执行此操作。");
            return false;
        }
    }

    public void EndDay()
    {
        currentDay++;
        remainingAP = 5; // 回满行动点
        OnAPChanged.Invoke();

        // 每次进入新一天，先检查是否满足通关要求（比如撑过14天）
        if (GameResultManager.Instance != null)
        {
            GameResultManager.Instance.CheckGameCondition(
                PlayerDataManager.Instance.bloodSugar,
                PlayerDataManager.Instance.health,
                PlayerDataManager.Instance.mood,
                currentDay
            );
        }

        // 触发早间事件弹窗
        if (testEvent != null && EventPopupController.Instance != null && !GameResultManager.Instance.gameOverPanel.activeSelf && !GameResultManager.Instance.gameWinPanel.activeSelf)
        {
            EventPopupController.Instance.DisplayEvent(testEvent);
        }
    }
}