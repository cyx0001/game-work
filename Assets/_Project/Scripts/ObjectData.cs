using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct LevelData
{
    public float bloodSugarDelta;  // 血糖即时变化
    public float healthDelta;      // 健康即时变化
    public float moodDelta;        // 心情即时变化
    public int moneyDelta;         // 金钱即时变化
    public int upgradeCost;        // 升级所需金钱

    // 每晚结算时的被动收益（比如 Lv3 的床自动回健康）
    public float passiveBloodSugar;
    public float passiveHealth;
    public int passiveMoney;
}

[CreateAssetMenu(fileName = "NewObjectData", menuName = "SugarBoss/Object Data")]
public class ObjectData : ScriptableObject
{
    public string objectName;      // 物件英文ID（如 Bed）
    public string displayName;     // 游戏内显示的中文名（如 床铺）
    public LevelData[] levels = new LevelData[3]; // 定义 3 个等级的数据槽
}
