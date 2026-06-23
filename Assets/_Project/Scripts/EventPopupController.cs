using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EventPopupController : MonoBehaviour
{
    [Header("遮罩物体")]
    public GameObject maskBackground;
    public static EventPopupController Instance { get; private set; }

    [Header("UI 元素关联")]
    public TextMeshProUGUI descriptionText;
    public Button buttonA;
    public TextMeshProUGUI textA;
    public Button buttonB;
    public TextMeshProUGUI textB;

    [Header("弹窗尺寸（Inspector 可调）")]
    public float panelWidth = 620f;
    public float panelHeight = 500f;
    public float buttonHeight = 40f;

    private EventData currentEvent;
    private System.Action onNoticeClosed;
    private System.Action onChoiceA;
    private System.Action onChoiceB;
    private System.Action onEventClosed;

    private RectTransform buttonARect;
    private RectTransform buttonBRect;
    private Vector2 buttonAAnchorMin;
    private Vector2 buttonAAnchorMax;
    private Vector2 buttonAAnchoredPosition;
    private Vector2 buttonAPivot;
    private Vector2 buttonBAnchorMin;
    private Vector2 buttonBAnchorMax;
    private Vector2 buttonBAnchoredPosition;
    private Vector2 buttonBPivot;
    private bool buttonALayoutSaved;

    private Canvas parentCanvas;
    private int savedSortingOrder;

    private void Awake()
    {
        Instance = this;
        SaveButtonALayout();

        parentCanvas = GetComponentInParent<Canvas>();

        // 应用弹窗面板尺寸
        RectTransform panelRect = GetComponent<RectTransform>();
        if (panelRect != null)
        {
            panelRect.sizeDelta = new Vector2(panelWidth, panelHeight);
        }

        // 去掉下划线样式，避免中文字体缺失下划线字形的警告
        if (descriptionText != null) descriptionText.fontStyle &= ~FontStyles.Underline;
        if (textA != null) textA.fontStyle &= ~FontStyles.Underline;
        if (textB != null) textB.fontStyle &= ~FontStyles.Underline;

        gameObject.SetActive(false);
        if (maskBackground != null)
            maskBackground.SetActive(false);
    }

    /// <summary>弹窗显示时提升 Canvas 层级，确保盖在小游戏 UI 之上</summary>
    private void PushSortingOrder()
    {
        if (SleepFadeController.Instance != null)
            SleepFadeController.Instance.ClearOverlay();

        if (parentCanvas != null)
        {
            savedSortingOrder = parentCanvas.sortingOrder;
            parentCanvas.overrideSorting = true;
            parentCanvas.sortingOrder = 200;
        }
    }

    /// <summary>弹窗关闭时恢复 Canvas 层级</summary>
    private void PopSortingOrder()
    {
        if (parentCanvas != null)
        {
            parentCanvas.sortingOrder = savedSortingOrder;
            parentCanvas.overrideSorting = false;
        }
    }

    private void SaveButtonALayout()
    {
        if (buttonA != null)
        {
            buttonARect = buttonA.GetComponent<RectTransform>();
            if (buttonARect != null)
            {
                buttonAAnchorMin = buttonARect.anchorMin;
                buttonAAnchorMax = buttonARect.anchorMax;
                buttonAAnchoredPosition = buttonARect.anchoredPosition;
                buttonAPivot = buttonARect.pivot;
            }
        }
        if (buttonB != null)
        {
            buttonBRect = buttonB.GetComponent<RectTransform>();
            if (buttonBRect != null)
            {
                buttonBAnchorMin = buttonBRect.anchorMin;
                buttonBAnchorMax = buttonBRect.anchorMax;
                buttonBAnchoredPosition = buttonBRect.anchoredPosition;
                buttonBPivot = buttonBRect.pivot;
            }
        }
        buttonALayoutSaved = true;
    }

    private void SetSingleButtonLayout(bool centered)
    {
        if (!buttonALayoutSaved) return;

        if (centered)
        {
            // 按钮 A：居中显示
            if (buttonARect != null)
            {
                buttonARect.anchorMin = new Vector2(0.5f, 0f);
                buttonARect.anchorMax = new Vector2(0.5f, 0f);
                buttonARect.pivot = new Vector2(0.5f, 0f);
                buttonARect.anchoredPosition = Vector2.zero;
                buttonARect.sizeDelta = new Vector2(buttonARect.sizeDelta.x, buttonHeight);
            }
            // 隐藏按钮 B
            if (buttonB != null) buttonB.gameObject.SetActive(false);
        }
        else
        {
            // 恢复双按钮布局
            if (buttonARect != null)
            {
                buttonARect.anchorMin = buttonAAnchorMin;
                buttonARect.anchorMax = buttonAAnchorMax;
                buttonARect.pivot = buttonAPivot;
                buttonARect.anchoredPosition = buttonAAnchoredPosition;
                buttonARect.sizeDelta = new Vector2(buttonARect.sizeDelta.x, buttonHeight);
            }
            if (buttonBRect != null)
            {
                buttonBRect.anchorMin = buttonBAnchorMin;
                buttonBRect.anchorMax = buttonBAnchorMax;
                buttonBRect.pivot = buttonBPivot;
                buttonBRect.anchoredPosition = buttonBAnchoredPosition;
                buttonBRect.sizeDelta = new Vector2(buttonBRect.sizeDelta.x, buttonHeight);
            }
        }
    }

    public void DisplayEvent(EventData eventData, System.Action onComplete = null)
    {
        if (eventData == null) return;
        currentEvent = eventData;
        onNoticeClosed = null;
        onEventClosed = onComplete;

        descriptionText.text = eventData.eventDescription;
        textA.text = eventData.optionAText;
        textB.text = eventData.optionBText;

        SetSingleButtonLayout(false);
        if (buttonB != null) buttonB.gameObject.SetActive(true);

        buttonA.onClick.RemoveAllListeners();
        buttonA.onClick.AddListener(() => OnOptionSelected(true));

        buttonB.onClick.RemoveAllListeners();
        buttonB.onClick.AddListener(() => OnOptionSelected(false));

        PushSortingOrder();
        gameObject.SetActive(true);
        if (maskBackground != null) maskBackground.SetActive(true);
    }

    public void DisplayNotice(string message, string confirmText = "知道了", System.Action onClose = null)
    {
        currentEvent = null;
        onNoticeClosed = onClose;
        onChoiceA = null;
        onChoiceB = null;

        descriptionText.text = message;
        textA.text = confirmText;
        if (textB != null) textB.text = "";
        if (buttonB != null) buttonB.gameObject.SetActive(false);

        SetSingleButtonLayout(true);

        buttonA.onClick.RemoveAllListeners();
        buttonA.onClick.AddListener(CloseNotice);

        PushSortingOrder();
        gameObject.SetActive(true);
        if (maskBackground != null) maskBackground.SetActive(true);
    }

    /// <summary>
    /// 通用双按钮选择弹窗（带回调，不自动修改属性）
    /// </summary>
    public void DisplayChoice(string message, string optionAText, string optionBText, System.Action onA, System.Action onB)
    {
        currentEvent = null;
        onNoticeClosed = null;
        onChoiceA = onA;
        onChoiceB = onB;

        descriptionText.text = message;
        textA.text = optionAText;
        textB.text = optionBText;

        SetSingleButtonLayout(false);
        if (buttonB != null) buttonB.gameObject.SetActive(true);

        buttonA.onClick.RemoveAllListeners();
        buttonA.onClick.AddListener(() => OnChoiceSelected(true));

        buttonB.onClick.RemoveAllListeners();
        buttonB.onClick.AddListener(() => OnChoiceSelected(false));

        PushSortingOrder();
        gameObject.SetActive(true);
        if (maskBackground != null) maskBackground.SetActive(true);
    }

    private void OnChoiceSelected(bool isOptionA)
    {
        System.Action callback = isOptionA ? onChoiceA : onChoiceB;
        onChoiceA = null;
        onChoiceB = null;

        PopSortingOrder();
        gameObject.SetActive(false);
        if (maskBackground != null) maskBackground.SetActive(false);

        callback?.Invoke();
    }

    public void ForceClose()
    {
        onNoticeClosed = null;
        onChoiceA = null;
        onChoiceB = null;
        onEventClosed = null;
        currentEvent = null;

        SetSingleButtonLayout(false);
        if (buttonB != null) buttonB.gameObject.SetActive(true);

        PopSortingOrder();
        gameObject.SetActive(false);
        if (maskBackground != null) maskBackground.SetActive(false);
    }

    private void CloseNotice()
    {
        SetSingleButtonLayout(false);
        if (buttonB != null) buttonB.gameObject.SetActive(true);

        System.Action callback = onNoticeClosed;
        onNoticeClosed = null;

        PopSortingOrder();
        gameObject.SetActive(false);
        if (maskBackground != null) maskBackground.SetActive(false);

        callback?.Invoke();
    }

    private void OnOptionSelected(bool isOptionA)
    {
        if (currentEvent == null) return;

        if (isOptionA)
        {
            PlayerDataManager.Instance.ModifyStats(
                currentEvent.sugarDeltaA, currentEvent.healthDeltaA, currentEvent.moodDeltaA, currentEvent.moneyDeltaA);
        }
        else
        {
            PlayerDataManager.Instance.ModifyStats(
                currentEvent.sugarDeltaB, currentEvent.healthDeltaB, currentEvent.moodDeltaB, currentEvent.moneyDeltaB);
        }

        PopSortingOrder();
        gameObject.SetActive(false);
        if (maskBackground != null) maskBackground.SetActive(false);

        var cb = onEventClosed;
        onEventClosed = null;
        cb?.Invoke();
    }
}
