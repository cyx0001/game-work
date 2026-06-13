using UnityEngine;

public static class GameConstants
{
    // 血糖阈值（对应设计文档 4.6.4 / 附录 A）
    public const float BS_LOW_CRITICAL = 50f;
    public const float BS_LOW_WARNING = 70f;
    public const float BS_SAFE_LOW = 70f;
    public const float BS_SAFE_HIGH = 120f;
    public const float BS_HIGH_WARNING = 160f;
    public const float BS_HIGH_CRITICAL = 200f;

    public const float MIN_BLOOD_SUGAR = 0f;
    public const float MAX_BLOOD_SUGAR = 250f;
    public const float MAX_HEALTH = 100f;
    public const float MAX_MOOD = 100f;

    public const int DAILY_AP = 5;
    public const int LOW_SUGAR_AP_PENALTY = 2;

    public const int NIGHT_SUGAR_RISE_MIN = 8;
    public const int NIGHT_SUGAR_RISE_MAX = 12;
    public const float SAFE_ZONE_HEALTH_BONUS = 2f;
    public const float HIGH_SUGAR_HEALTH_PENALTY = 10f;
    public const float LOW_SUGAR_HEALTH_PENALTY = 20f;

    public const int EMERGENCY_HOSPITAL_COST = 150;
    public const float EMERGENCY_SUGAR_REDUCE = 50f;
    public const float EMERGENCY_HEALTH_PENALTY = 10f;

    public const int HEALTH_ZERO_HOSPITAL_COST = 200;
    public const float HEALTH_ZERO_RESTORE = 30f;

    public const string MSG_BS_LOW_WARNING = "血糖有点低哦，考虑吃点东西或者减少运动量！";
    public const string MSG_BS_HIGH_WARNING = "血糖偏高了！快运动一下或者吃点药吧~";
    public const string MSG_BS_HYPOGLYCEMIA = "血糖进入低血糖危险区！请立刻补充能量！";
    public const string MSG_BS_HYPERGLYCEMIA = "血糖严重偏高！身体正在承受巨大压力！";
    public const string MSG_BS_EXTREME = "血糖严重超标，必须立刻去医院！系统自动扣除150元急诊费。";
    public const string MSG_HEALTH_ZERO = "病人身体扛不住了，被紧急送往医院观察。金钱-200，健康恢复至30。";
    public const string MSG_HOSPITAL_NO_MONEY_EMERGENCY = "余额不足，无法承担150元急诊费，未能强制送医！请尽快想办法控糖或赚钱。";
    public const string MSG_HOSPITAL_NO_MONEY_HEALTH = "余额不足，无法承担200元住院费，未能送医！病人情况仍未好转。";
}
