// 移除了道具输入和复杂的折算，只保留核心属性增量
public static class KitchenGameBridge
{
    // === 进入厨房前，由主场景写入 ===
    public static int Input_KitchenLevel = 1; // 当前厨房等级

    // === 厨房游戏结束，切回主场景前，将产出数据写入这里 ===
    public static float Output_SugarDelta = 0f;
    public static float Output_HealthDelta = 0f;
    public static float Output_MoodDelta = 0f;
    public static bool IsDataReady = false;   // 标记是否有新产生的厨房结算数据
}