using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayerDataManager : MonoBehaviour
{
    public static PlayerDataManager Instance { get; private set; }

    [Header("玩家当前属性")]
    public float bloodSugar = 100f;
    public float health = 100f;
    public float mood = 80f;
    public int money = 500;

    [HideInInspector] public UnityEvent OnDataChanged = new UnityEvent();

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        // 重启场景时强制重置满血状态
        bloodSugar = 100f;
        health = 100f;
        mood = 80f;
        money = 500;

        // 强行广播一次，催促 UI 刷新
        OnDataChanged?.Invoke();
    }

    public void ModifyStats(float sugarDelta, float healthDelta, float moodDelta, int moneyDelta)
    {
        bloodSugar = Mathf.Clamp(bloodSugar + sugarDelta, 0f, GameConstants.MAX_BLOOD_SUGAR);
        health = Mathf.Clamp(health + healthDelta, 0f, GameConstants.MAX_HEALTH);
        mood = Mathf.Clamp(mood + moodDelta, 0f, GameConstants.MAX_MOOD);
        money = Mathf.Max(0, money + moneyDelta);

        Debug.Log($"【数据更新】当前状态 -> 血糖: {bloodSugar}, 健康: {health}, 心情: {mood}, 金钱: {money}");

        // 状态更新通知 UI
        OnDataChanged?.Invoke();

        // 每次属性变动立刻交给结果管理器判定是否进 ICU
        if (GameResultManager.Instance != null)
        {
            int day = GameManager.Instance != null ? GameManager.Instance.currentDay : 1;
            GameResultManager.Instance.CheckGameCondition(bloodSugar, health, mood, day);
        }
    }
}