using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 电脑问答小游戏——场景加载桥接器
/// </summary>
public static class ComputerSceneLauncher
{
    private const string DailyCompletedKey = "Computer_LastCompletedDay";

    public static InteractableObject SourceObject { get; private set; }
    public static int ComputerLevel { get; private set; }

    public const string SCENE_NAME = "ComputerQuiz";

    public static bool HasCompletedToday()
    {
        if (GameManager.Instance == null) return false;
        return PlayerPrefs.GetInt(DailyCompletedKey, -1) == GameManager.Instance.currentDay;
    }

    public static void MarkCompletedToday()
    {
        if (GameManager.Instance == null) return;
        PlayerPrefs.SetInt(DailyCompletedKey, GameManager.Instance.currentDay);
        PlayerPrefs.Save();
    }

    public static void Launch(InteractableObject obj)
    {
        if (obj == null) return;

        if (HasCompletedToday())
        {
            if (EventPopupController.Instance != null)
                EventPopupController.Instance.DisplayNotice("今天已经使用过电脑了，明天再来吧。", "知道了");
            else
                Debug.LogWarning("[电脑问答] 今天已经使用过电脑了。");
            return;
        }

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
