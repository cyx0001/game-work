using UnityEngine;

[CreateAssetMenu(fileName = "NewEventData", menuName = "SugarBoss/Event Data")]
public class EventData : ScriptableObject
{
    public int eventID;                 // 事件唯一ID
    [TextArea(3, 5)]
    public string eventDescription;     // 事件剧情文案（例如：同事点了下午茶奶茶...）

    [Header("选项 A 设定")]
    public string optionAText;          // 选项A按钮文案
    public float sugarDeltaA;           // 选项A带来的血糖变化
    public float healthDeltaA;          // 选项A带来的健康变化
    public float moodDeltaA;            // 选项A带来的心情变化
    public int moneyDeltaA;             // 选项A带来的金钱变化

    [Header("选项 B 设定")]
    public string optionBText;          // 选项B按钮文案
    public float sugarDeltaB;           // 选项B带来的血糖变化
    public float healthDeltaB;          // 选项B带来的健康变化
    public float moodDeltaB;            // 选项B带来的心情变化
    public int moneyDeltaB;             // 选项B带来的金钱变化
}