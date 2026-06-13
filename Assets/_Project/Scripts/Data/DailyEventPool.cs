using UnityEngine;

[CreateAssetMenu(fileName = "DailyEventPool", menuName = "SugarBoss/Daily Event Pool")]
public class DailyEventPool : ScriptableObject
{
    public EventData[] events;
}
