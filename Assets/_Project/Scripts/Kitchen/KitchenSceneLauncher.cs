using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 厨房迷你游戏——场景加载桥接器
/// </summary>
public static class KitchenSceneLauncher
{
    public static InteractableObject SourceObject { get; private set; }
    public static int KitchenLevel { get; private set; }

    public const string SCENE_NAME = "Kitchen";

    public static void Launch(InteractableObject obj)
    {
        if (obj == null) return;

        SourceObject = obj;
        KitchenLevel = obj.currentLevel;

        // 将等级写入 Bridge，供厨房场景读取
        KitchenGameBridge.Input_KitchenLevel = obj.currentLevel;
        KitchenGameBridge.IsDataReady = false;

        if (GameManager.Instance != null)
            GameManager.Instance.isInMinigame = true;

        SceneManager.LoadScene(SCENE_NAME, LoadSceneMode.Additive);
        Debug.Log($"[厨房] 加载场景 {SCENE_NAME}，厨房等级 Lv.{KitchenLevel}");
    }

    public static void ReturnToMain()
    {
        SourceObject = null;

        if (GameManager.Instance != null)
            GameManager.Instance.isInMinigame = false;

        SceneManager.UnloadSceneAsync(SCENE_NAME);
        Debug.Log("[厨房] 返回主场景");
    }
}
