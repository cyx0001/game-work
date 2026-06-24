public static class KitchenGameBridge
{
    // === 进入厨房前，由主场景写入 ===
    public static int Input_KitchenLevel = 1;

    // === 厨房游戏结束，写入产出数据 ===
    public static int Output_Score = 0;
    public static float Output_Multiplier = 1.0f;
    public static bool IsDataReady = false;
}
