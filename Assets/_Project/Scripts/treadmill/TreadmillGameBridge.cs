/// <summary>
/// 跑步机迷你游戏——数据桥接器
/// 跑步机游戏结束后将产出数据写入此处，供主场景读取并应用到玩家属性
/// </summary>
public static class TreadmillGameBridge
{
    /// <summary>跑步机游戏最终得分</summary>
    public static int Output_Score = 0;

    /// <summary>是否有待处理的跑步机结算数据</summary>
    public static bool IsDataReady = false;

    /// <summary>重置桥接数据（应在消费后调用）</summary>
    public static void Reset()
    {
        Output_Score = 0;
        IsDataReady = false;
    }
}
