/// <summary>
/// 电脑问答小游戏——主场景与问答场景之间的数据桥
/// </summary>
public static class ComputerGameBridge
{
    public static int Input_ComputerLevel = 1;

    public static int Output_CorrectCount = 0;
    public static int Output_TotalCount = 0;
    public static float Output_SugarDelta = 0f;
    public static float Output_HealthDelta = 0f;
    public static float Output_MoodDelta = 0f;
    public static bool IsDataReady = false;
}
