using UnityEngine;

[CreateAssetMenu(fileName = "DailyEventPool", menuName = "SugarBoss/Daily Event Pool")]
public class DailyEventPool : ScriptableObject
{
    [Header("通用事件（无时期事件时作为兜底）")]
    public EventData[] events;

    [Header("适应期 第1~5天（影响较轻）")]
    public EventData[] adaptationEvents;

    [Header("危机期 第6~10天（影响加重）")]
    public EventData[] crisisEvents;

    [Header("冲刺期 第11~14天（影响剧烈）")]
    public EventData[] sprintEvents;

    /// <summary>根据当前天数获取对应时期的事件池</summary>
    public EventData[] GetEventsForDay(int day)
    {
        if (day <= 5 && adaptationEvents != null && adaptationEvents.Length > 0)
            return adaptationEvents;
        if (day <= 10 && crisisEvents != null && crisisEvents.Length > 0)
            return crisisEvents;
        if (sprintEvents != null && sprintEvents.Length > 0)
            return sprintEvents;
        return events;
    }
}
