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

    [Header("�������¼�")]
    public EventData testEvent;

    private void Awake()
    {
        // �ɾ��ĳ������������� DontDestroyOnLoad
        Instance = this;
    }

    private void Start()
    {
        // ÿ�γ���������Ӳ��ȷ����ֵ�ɾ���λ
        currentDay = 1;
        remainingAP = 5;
        OnAPChanged.Invoke();
    }

    // ���������ж���
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
        currentDay++;
        remainingAP = 5; // �����ж���
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

        // ��������¼�����
        if (testEvent != null && EventPopupController.Instance != null && !GameResultManager.Instance.gameOverPanel.activeSelf && !GameResultManager.Instance.gameWinPanel.activeSelf)
        {
            EventPopupController.Instance.DisplayEvent(testEvent);
        }
    }
}