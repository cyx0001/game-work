using UnityEngine;

public enum BloodSugarZone
{
    Hypoglycemia,   // 0-49
    Low,            // 50-69
    Safe,           // 70-120
    High,           // 121-160
    Hyperglycemia,  // 161-200
    Extreme         // >200
}

public class ThresholdEventManager : MonoBehaviour
{
    public static ThresholdEventManager Instance { get; private set; }

    private BloodSugarZone lastBloodSugarZone = BloodSugarZone.Safe;
    private int nextDayApPenalty;
    private bool healthZeroHandled;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (PlayerDataManager.Instance != null)
        {
            lastBloodSugarZone = GetBloodSugarZone(PlayerDataManager.Instance.bloodSugar);
        }
    }

    public static BloodSugarZone GetBloodSugarZone(float sugar)
    {
        if (sugar < GameConstants.BS_LOW_CRITICAL) return BloodSugarZone.Hypoglycemia;
        if (sugar < GameConstants.BS_SAFE_LOW) return BloodSugarZone.Low;
        if (sugar <= GameConstants.BS_SAFE_HIGH) return BloodSugarZone.Safe;
        if (sugar <= GameConstants.BS_HIGH_WARNING) return BloodSugarZone.High;
        if (sugar <= GameConstants.BS_HIGH_CRITICAL) return BloodSugarZone.Hyperglycemia;
        return BloodSugarZone.Extreme;
    }

    public int ConsumeNextDayApPenalty()
    {
        int penalty = nextDayApPenalty;
        nextDayApPenalty = 0;
        return penalty;
    }

    /// <summary>属性变动后检查是否跨越血糖危险区并弹出警告。</summary>
    public void OnStatsChanged(float oldSugar, float newSugar, float health)
    {
        BloodSugarZone newZone = GetBloodSugarZone(newSugar);
        if (newZone != lastBloodSugarZone)
        {
            HandleZoneTransition(lastBloodSugarZone, newZone);
            lastBloodSugarZone = newZone;
        }

        if (health <= 0f && !healthZeroHandled)
        {
            TriggerHealthZeroHospitalization();
        }
    }

    /// <summary>结束一天时执行夜晚自然变化与危险区结算（设计文档 4.9.3）。</summary>
    public void ProcessNightSettlement(System.Action onComplete = null)
    {
        if (PlayerDataManager.Instance == null)
        {
            onComplete?.Invoke();
            return;
        }

        PlayerDataManager pd = PlayerDataManager.Instance;
        healthZeroHandled = false;

        float nightRise = Random.Range(GameConstants.NIGHT_SUGAR_RISE_MIN, GameConstants.NIGHT_SUGAR_RISE_MAX + 1);
        pd.ModifyStats(nightRise, 0, 0, 0, skipThresholdCheck: true);

        BloodSugarZone zone = GetBloodSugarZone(pd.bloodSugar);
        bool needsForcedEvent = false;
        System.Action forcedComplete = () =>
        {
            lastBloodSugarZone = GetBloodSugarZone(pd.bloodSugar);
            if (pd.health <= 0f && !healthZeroHandled)
            {
                TriggerHealthZeroHospitalization(onComplete);
            }
            else
            {
                onComplete?.Invoke();
            }
        };

        switch (zone)
        {
            case BloodSugarZone.Safe:
                pd.ModifyStats(0, GameConstants.SAFE_ZONE_HEALTH_BONUS, 0, 0, skipThresholdCheck: true);
                Debug.Log($"【夜晚结算】血糖在安全区，健康 +{GameConstants.SAFE_ZONE_HEALTH_BONUS}");
                break;

            case BloodSugarZone.Hyperglycemia:
                pd.ModifyStats(0, -GameConstants.HIGH_SUGAR_HEALTH_PENALTY, 0, 0, skipThresholdCheck: true);
                Debug.Log($"【夜晚结算】高血糖惩罚，健康 -{GameConstants.HIGH_SUGAR_HEALTH_PENALTY}");
                break;

            case BloodSugarZone.Hypoglycemia:
                pd.ModifyStats(0, -GameConstants.LOW_SUGAR_HEALTH_PENALTY, 0, 0, skipThresholdCheck: true);
                nextDayApPenalty += GameConstants.LOW_SUGAR_AP_PENALTY;
                Debug.Log($"【夜晚结算】低血糖惩罚，健康 -{GameConstants.LOW_SUGAR_HEALTH_PENALTY}，次日 AP -{GameConstants.LOW_SUGAR_AP_PENALTY}");
                break;

            case BloodSugarZone.Extreme:
                needsForcedEvent = true;
                TriggerEmergencyHospitalization(forcedComplete);
                return;
        }

        lastBloodSugarZone = GetBloodSugarZone(pd.bloodSugar);

        if (pd.health <= 0f && !healthZeroHandled)
            TriggerHealthZeroHospitalization(onComplete);
        else if (!needsForcedEvent)
            onComplete?.Invoke();
    }

    private void HandleZoneTransition(BloodSugarZone from, BloodSugarZone to)
    {
        switch (to)
        {
            case BloodSugarZone.Hypoglycemia:
                ShowWarning(GameConstants.MSG_BS_HYPOGLYCEMIA);
                break;
            case BloodSugarZone.Low:
                if (from >= BloodSugarZone.Safe)
                    ShowWarning(GameConstants.MSG_BS_LOW_WARNING);
                break;
            case BloodSugarZone.High:
                if (from <= BloodSugarZone.Safe)
                    ShowWarning(GameConstants.MSG_BS_HIGH_WARNING);
                break;
            case BloodSugarZone.Hyperglycemia:
                if (from <= BloodSugarZone.High)
                    ShowWarning(GameConstants.MSG_BS_HYPERGLYCEMIA);
                break;
            case BloodSugarZone.Extreme:
                if (from <= BloodSugarZone.Hyperglycemia)
                    ShowWarning(GameConstants.MSG_BS_HYPERGLYCEMIA);
                break;
        }
    }

    private void TriggerEmergencyHospitalization(System.Action onComplete = null)
    {
        if (PlayerDataManager.Instance == null)
        {
            onComplete?.Invoke();
            return;
        }

        PlayerDataManager pd = PlayerDataManager.Instance;
        if (pd.money < GameConstants.EMERGENCY_HOSPITAL_COST)
        {
            ShowWarning(GameConstants.MSG_HOSPITAL_NO_MONEY_EMERGENCY);
            onComplete?.Invoke();
            return;
        }

        ShowForcedEvent(GameConstants.MSG_BS_EXTREME, () =>
        {
            pd.ModifyStats(
                -GameConstants.EMERGENCY_SUGAR_REDUCE,
                -GameConstants.EMERGENCY_HEALTH_PENALTY,
                0,
                -GameConstants.EMERGENCY_HOSPITAL_COST,
                skipThresholdCheck: true);

            lastBloodSugarZone = GetBloodSugarZone(pd.bloodSugar);
            Debug.Log("【强制就医】已扣除急诊费并降低血糖。");
            onComplete?.Invoke();
        });
    }

    private void TriggerHealthZeroHospitalization(System.Action onComplete = null)
    {
        if (PlayerDataManager.Instance == null || healthZeroHandled)
        {
            onComplete?.Invoke();
            return;
        }

        PlayerDataManager pd = PlayerDataManager.Instance;
        if (pd.money < GameConstants.HEALTH_ZERO_HOSPITAL_COST)
        {
            healthZeroHandled = true;
            ShowWarning(GameConstants.MSG_HOSPITAL_NO_MONEY_HEALTH);
            onComplete?.Invoke();
            return;
        }

        healthZeroHandled = true;

        ShowForcedEvent(GameConstants.MSG_HEALTH_ZERO, () =>
        {
            pd.ModifyStats(0, 0, 0, -GameConstants.HEALTH_ZERO_HOSPITAL_COST, skipThresholdCheck: true);
            pd.SetHealth(GameConstants.HEALTH_ZERO_RESTORE);
            Debug.Log("【强制入院】健康已恢复至 30。");
            onComplete?.Invoke();
        });
    }

    private void ShowWarning(string message)
    {
        Debug.Log($"【阈值警告】{message}");
        if (EventPopupController.Instance != null)
            EventPopupController.Instance.DisplayNotice(message);
    }

    private void ShowForcedEvent(string message, System.Action onConfirm)
    {
        Debug.Log($"【阈值事件】{message}");
        if (EventPopupController.Instance != null)
            EventPopupController.Instance.DisplayNotice(message, "知道了", onConfirm);
        else
            onConfirm?.Invoke();
    }
}
