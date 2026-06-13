using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 睡眠过场动画：屏幕渐黑 → 执行回调 → 渐亮
/// 运行时自动创建全屏黑色遮罩，无需手动配置
/// </summary>
public class SleepFadeController : MonoBehaviour
{
    public static SleepFadeController Instance { get; private set; }

    [Header("动画设置")]
    public float fadeDuration = 0.8f;      // 淡入/淡出各耗时
    public float blackHoldTime = 0.3f;     // 全黑停留时间

    private Canvas fadeCanvas;
    private CanvasGroup fadeGroup;
    private Image fadeImage;
    private bool isFading = false;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        CreateFadeOverlay();
    }

    private void CreateFadeOverlay()
    {
        // 创建独立的 Canvas 用于遮罩
        GameObject canvasObj = new GameObject("SleepFadeCanvas");
        canvasObj.transform.SetParent(transform);
        fadeCanvas = canvasObj.AddComponent<Canvas>();
        fadeCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        fadeCanvas.sortingOrder = 999; // 确保在最顶层

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        canvasObj.AddComponent<GraphicRaycaster>();

        // 黑色遮罩 Image（全屏）
        GameObject imageObj = new GameObject("FadeImage");
        imageObj.transform.SetParent(canvasObj.transform);
        fadeImage = imageObj.AddComponent<Image>();
        fadeImage.color = Color.black;

        RectTransform rect = imageObj.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        // 用 CanvasGroup 控制透明度
        fadeGroup = imageObj.AddComponent<CanvasGroup>();
        fadeGroup.alpha = 0f;
        fadeGroup.interactable = false;
        fadeGroup.blocksRaycasts = false;
    }

    // ==================== 公开 API ====================

    /// <summary>
    /// 播放睡眠过场动画
    /// onMidPoint: 在全黑时执行的回调（用于扣 AP、修改属性等）
    /// </summary>
    public void PlaySleepTransition(System.Action onMidPoint)
    {
        if (isFading) return;
        StartCoroutine(FadeRoutine(onMidPoint));
    }

    public bool IsFading => isFading;

    // ==================== 核心协程 ====================

    private IEnumerator FadeRoutine(System.Action onMidPoint)
    {
        isFading = true;

        // --- 阶段1: 屏幕渐黑 ---
        fadeGroup.blocksRaycasts = true; // 阻止点击
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            fadeGroup.alpha = Mathf.Lerp(0f, 1f, t / fadeDuration);
            yield return null;
        }
        fadeGroup.alpha = 1f;

        // --- 阶段2: 全黑，执行回调 ---
        onMidPoint?.Invoke();

        // --- 阶段3: 短暂停留 ---
        yield return new WaitForSeconds(blackHoldTime);

        // --- 阶段4: 屏幕渐亮 ---
        t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            fadeGroup.alpha = Mathf.Lerp(1f, 0f, t / fadeDuration);
            yield return null;
        }
        fadeGroup.alpha = 0f;
        fadeGroup.blocksRaycasts = false;

        isFading = false;
    }
}
