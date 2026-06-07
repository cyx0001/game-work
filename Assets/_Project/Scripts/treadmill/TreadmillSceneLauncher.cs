using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 跑步机迷你游戏——场景加载桥接器
/// 负责在主场景和跑步机小游戏场景之间切换（Additive 加载）
/// </summary>
public static class TreadmillSceneLauncher
{
    /// <summary>触发小游戏的跑步机物件（跨场景引用）</summary>
    public static InteractableObject SourceObject { get; private set; }

    /// <summary>跑步机当前等级（用于局内难度调整）</summary>
    public static int TreadmillLevel { get; private set; }

    /// <summary>场景名字（必须在 Build Settings 中注册）</summary>
    public const string SCENE_NAME = "TreadmillScene";

    /// <summary>
    /// 从主场景启动跑步机小游戏
    /// </summary>
    public static void Launch(InteractableObject obj)
    {
        if (obj == null) return;

        SourceObject = obj;
        TreadmillLevel = obj.currentLevel;

        if (GameManager.Instance != null)
            GameManager.Instance.isInMinigame = true;

        // Additive 加载小游戏场景（主场景保留在底层）
        SceneManager.LoadScene(SCENE_NAME, LoadSceneMode.Additive);
        Debug.Log($"[跑步机] 加载场景 {SCENE_NAME}，跑步机等级 Lv.{TreadmillLevel}");
    }

    /// <summary>
    /// 从小游戏场景返回主场景
    /// </summary>
    public static void ReturnToMain()
    {
        SourceObject = null;

        if (GameManager.Instance != null)
            GameManager.Instance.isInMinigame = false;

        SceneManager.UnloadSceneAsync(SCENE_NAME);
        Debug.Log("[跑步机] 返回主场景");
    }
}
