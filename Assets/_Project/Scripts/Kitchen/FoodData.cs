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
        new FoodStaticInfo("西兰花", FoodType.Green, 1, 0, 1, 0, 0),
        new FoodStaticInfo("水煮蛋", FoodType.Green, 1, 0, 1, 0, 1),
        new FoodStaticInfo("白米饭", FoodType.Yellow, 1, 1, 0, 0, 2),
        new FoodStaticInfo("苏打饼干", FoodType.Yellow, 1, 0, 0, 0, 3),
        new FoodStaticInfo("普通可乐", FoodType.Red, 1, 2, 0, 2, 4),
        new FoodStaticInfo("小蛋糕", FoodType.Red, 1, 3, -1, 3, 5),
        new FoodStaticInfo("方便面", FoodType.Yellow, 2, 1, 0, 1, 7),
        new FoodStaticInfo("燕麦粥", FoodType.Green, 3, -1, 1, 0, 8),
        new FoodStaticInfo("珍珠奶茶", FoodType.Red, 3, 4, -1, 4, 9)
    };
}