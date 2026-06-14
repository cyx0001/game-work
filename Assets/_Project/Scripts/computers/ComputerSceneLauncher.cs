using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 电脑问答小游戏——场景加载桥接器（后续接入主场景时使用）
/// </summary>
public static class ComputerSceneLauncher
{
    public static InteractableObject SourceObject { get; private set; }
    public static int ComputerLevel { get; private set; }

    public const string SCENE_NAME = "ComputerQuiz";

    public static void Launch(InteractableObject obj)
    {
        if (obj == null) return;

        Scene scene = SceneManager.GetSceneByName(SCENE_NAME);
        if (scene.isLoaded)
        {
            Debug.LogWarning($"[电脑问答] 场景 {SCENE_NAME} 已在运行中，忽略重复启动");
            return;
        }

        SourceObject = obj;
        ComputerLevel = obj.currentLevel;
        ComputerGameBridge.Input_ComputerLevel = obj.currentLevel;

        if (GameManager.Instance != null)
            GameManager.Instance.isInMinigame = true;

        SceneManager.LoadScene(SCENE_NAME, LoadSceneMode.Additive);
        Debug.Log($"[电脑问答] 加载场景 {SCENE_NAME}，等级 Lv.{ComputerLevel}");
    }

    public static void ReturnToMain()
    {
        SourceObject = null;

        if (GameManager.Instance != null)
            GameManager.Instance.isInMinigame = false;

        SceneManager.UnloadSceneAsync(SCENE_NAME);
        Debug.Log("[电脑问答] 返回主场景");
    }
}
