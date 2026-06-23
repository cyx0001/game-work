using System.Collections.Generic;

public enum FoodType { Green, Yellow, Red }

public class FoodStaticInfo
{
    public string name;
    public FoodType type;
    public int unlockLevel;
    public float sugarDelta;
    public float healthDelta;
    public float moodDelta;
    public int spriteIndex; 
    
    public FoodStaticInfo(string name, FoodType type, int unlockLevel, float sugar, float health, float mood, int spriteIndex)
    {
        this.name = name;
        this.type = type;
        this.unlockLevel = unlockLevel;
        this.sugarDelta = sugar;
        this.healthDelta = health;
        this.moodDelta = mood;
        this.spriteIndex = spriteIndex; 
    }
}

public static class FoodTable
{
    public static List<FoodStaticInfo> Infos = new List<FoodStaticInfo>()
    {
        // ── Lv.1 解锁 ──
        new FoodStaticInfo("西兰花",   FoodType.Green,  1, 0,    0.5f,  0,    0),
        new FoodStaticInfo("水煮蛋",   FoodType.Green,  1, 0,    0.5f,  0,    1),
        new FoodStaticInfo("番茄",     FoodType.Green,  1, 0,    0.3f,  0.2f, 6),
        new FoodStaticInfo("黄瓜",     FoodType.Green,  1, -0.2f, 0.4f,  0,   10),

        new FoodStaticInfo("白米饭",   FoodType.Yellow, 1, 0.5f, 0,     0,    2),
        new FoodStaticInfo("苏打饼干", FoodType.Yellow, 1, 0,    0,     0,    3),

        new FoodStaticInfo("普通可乐", FoodType.Red,    1, 1,    0,     1,    4),
        new FoodStaticInfo("小蛋糕",   FoodType.Red,    1, 1.5f, -0.5f, 1.5f, 5),

        // ── Lv.2 解锁 ──
        new FoodStaticInfo("全麦面包", FoodType.Green,  2, -0.1f, 0.4f,  0,   11),
        new FoodStaticInfo("方便面",   FoodType.Yellow, 2, 0.5f,  0,     0.5f, 7),

        // ── Lv.3 解锁 ──
        new FoodStaticInfo("燕麦粥",   FoodType.Green,  3, -0.5f, 0.5f,  0,    8),
        new FoodStaticInfo("豆腐",     FoodType.Green,  3, 0,     0.6f,  0,   12),

        new FoodStaticInfo("珍珠奶茶", FoodType.Red,    3, 2,     -0.5f, 2,    9),
    };
}