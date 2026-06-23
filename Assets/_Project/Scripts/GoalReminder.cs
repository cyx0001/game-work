using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

/// <summary>
/// 屏幕左侧任务目标提醒 —— 鼠标悬停箭头展开查看目标
/// </summary>
public class GoalReminder : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("背景图片")]
    public Sprite bgSprite;

    [Header("箭头图片")]
    public Sprite arrowRight;  // 收起时：向右箭头
    public Sprite arrowLeft;   // 展开时：向左箭头

    [Header("文字")]
    public TMP_FontAsset chineseFont;
    public float fontSize = 22f;
    public Color textColor = Color.white;

    [Header("布局")]
    public float collapsedWidth = 40f;
    public float expandedWidth = 270f;
    public float panelHeight = 240f;
    public float animationSpeed = 10f;

    private RectTransform myRect;
    private Image bgImage;
    private Image arrowImage;
    private TextMeshProUGUI descText;
    private CanvasGroup descGroup;

    private float targetWidth;
    private float currentWidth;

    private const string GOAL_MESSAGE = "<b>任务目标</b>\n\n控制血糖\n保持在\n<b><color=#88FF88>70 ~ 120</color></b>\n的安全范围内\n\n<b>14</b> 天内达标";

    private void Awake()
    {
        if (FindObjectOfType<EventSystem>() == null)
        {
            GameObject es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<StandaloneInputModule>();
        }

        // 始终创建专用画布，避免 FindObjectOfType<Canvas> 在 DontDestroyOnLoad 残留画布存在时
        // 返回错误的父画布（如 SleepFadeCanvas），导致面板渲染异常或不可见
        Canvas parentCanvas;
        GameObject existingCanvas = GameObject.Find("_GoalReminderCanvas");
        if (existingCanvas != null)
        {
            parentCanvas = existingCanvas.GetComponent<Canvas>();
        }
        else
        {
            GameObject canvasGO = new GameObject("_GoalReminderCanvas");
            Canvas c = canvasGO.AddComponent<Canvas>();
            c.renderMode = RenderMode.ScreenSpaceOverlay;
            c.sortingOrder = 50;
            CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            canvasGO.AddComponent<GraphicRaycaster>(); // 必须！否则 IPointerEnter/Exit 不会触发
            parentCanvas = c;
        }

        transform.SetParent(parentCanvas.transform, false);
        BuildUI();
    }

    private void BuildUI()
    {
        // RectTransform
        myRect = gameObject.AddComponent<RectTransform>();
        myRect.anchorMin = new Vector2(0f, 0.5f);
        myRect.anchorMax = new Vector2(0f, 0.5f);
        myRect.pivot = new Vector2(0f, 0.5f);
        myRect.anchoredPosition = Vector2.zero;
        currentWidth = collapsedWidth;
        targetWidth = collapsedWidth;
        myRect.sizeDelta = new Vector2(collapsedWidth, panelHeight);

        // 背景图片
        bgImage = gameObject.AddComponent<Image>();
        bgImage.sprite = bgSprite;
        bgImage.type = Image.Type.Simple;
        bgImage.raycastTarget = true;

        // 箭头图片 —— 固定在面板右边缘
        GameObject arrowGO = new GameObject("Arrow");
        arrowGO.transform.SetParent(transform, false);
        arrowGO.layer = gameObject.layer;
        RectTransform arrowRect = arrowGO.AddComponent<RectTransform>();
        arrowRect.anchorMin = new Vector2(1f, 0.5f);
        arrowRect.anchorMax = new Vector2(1f, 0.5f);
        arrowRect.pivot = new Vector2(1f, 0.5f);
        arrowRect.anchoredPosition = new Vector2(-4, 0);
        arrowRect.sizeDelta = new Vector2(32, 32);
        arrowImage = arrowGO.AddComponent<Image>();
        arrowImage.sprite = arrowRight;
        arrowImage.raycastTarget = false;
        arrowImage.preserveAspect = true;

        // 描述文字
        GameObject descGO = new GameObject("Description");
        descGO.transform.SetParent(transform, false);
        descGO.layer = gameObject.layer;
        RectTransform descRect = descGO.AddComponent<RectTransform>();
        descRect.anchorMin = Vector2.zero;
        descRect.anchorMax = Vector2.one;
        descRect.offsetMin = new Vector2(8, 8);
        descRect.offsetMax = new Vector2(-40, -8);

        descText = descGO.AddComponent<TextMeshProUGUI>();
        descText.text = GOAL_MESSAGE;
        descText.font = chineseFont;
        descText.fontSize = fontSize;
        descText.color = textColor;
        descText.alignment = TextAlignmentOptions.Center;
        descText.raycastTarget = false;

        descGroup = descGO.AddComponent<CanvasGroup>();
        descGroup.alpha = 0f;
        descGroup.blocksRaycasts = false;
    }

    private void Update()
    {
        float next = Mathf.Lerp(currentWidth, targetWidth, Time.deltaTime * animationSpeed);
        if (Mathf.Abs(next - targetWidth) < 0.3f)
            next = targetWidth;
        currentWidth = next;
        myRect.sizeDelta = new Vector2(currentWidth, panelHeight);

        float progress = Mathf.InverseLerp(collapsedWidth, expandedWidth, currentWidth);
        if (descGroup != null)
            descGroup.alpha = progress * progress;

        if (arrowImage != null)
            arrowImage.sprite = progress > 0.5f ? arrowLeft : arrowRight;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        targetWidth = expandedWidth;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        targetWidth = collapsedWidth;
    }
}
