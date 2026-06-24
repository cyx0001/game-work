using System.Collections.Generic;

public enum FoodType { Green, Yellow, Red }

public class FoodStaticInfo
{
    public string name;
    public FoodType type;
    public int unlockLevel;
    public int scoreValue;
    public int spriteIndex;

    public FoodStaticInfo(string name, FoodType type, int unlockLevel, int scoreValue, int spriteIndex)
    {
        this.name = name;
        this.type = type;
        this.unlockLevel = unlockLevel;
        this.scoreValue = scoreValue;
        this.spriteIndex = spriteIndex;
    }
}

public static class FoodTable
{
    // 评分阈值
    public const int SCORE_S = 200;
    public const int SCORE_A = 120;
    public const int SCORE_B = 50;

    public static List<FoodStaticInfo> Infos = new List<FoodStaticInfo>()
    {
        // ── Lv.1 解锁 ──
        new FoodStaticInfo("西兰花",   FoodType.Green,  1, 10,  0),
        new FoodStaticInfo("水煮蛋",   FoodType.Green,  1, 10,  1),
        new FoodStaticInfo("番茄",     FoodType.Green,  1, 12,  6),
        new FoodStaticInfo("黄瓜",     FoodType.Green,  1, 15,  10),

        new FoodStaticInfo("白米饭",   FoodType.Yellow, 1,  3,  2),
        new FoodStaticInfo("苏打饼干", FoodType.Yellow, 1,  5,  3),

        new FoodStaticInfo("普通可乐", FoodType.Red,    1, -5,  4),
        new FoodStaticInfo("小蛋糕",   FoodType.Red,    1,-10,  5),

        // ── Lv.2 解锁 ──
        new FoodStaticInfo("方便面",   FoodType.Yellow, 2,  0,  7),

        // ── Lv.3 解锁 ──
        new FoodStaticInfo("燕麦粥",   FoodType.Green,  3, 20,  8),
        new FoodStaticInfo("珍珠奶茶", FoodType.Red,    3,-15,  9),
    };
}