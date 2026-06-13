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

    private EventData currentEvent;
    private System.Action onNoticeClosed;

    private RectTransform buttonARect;
    private Vector2 buttonAAnchorMin;
    private Vector2 buttonAAnchorMax;
    private Vector2 buttonAAnchoredPosition;
    private Vector2 buttonAPivot;
    private bool buttonALayoutSaved;

    private void Awake()
    {
        Instance = this;
        SaveButtonALayout();

        gameObject.SetActive(false);
        if (maskBackground != null)
            maskBackground.SetActive(false);
    }

    private void SaveButtonALayout()
    {
        if (buttonA == null) return;

        buttonARect = buttonA.GetComponent<RectTransform>();
        if (buttonARect == null) return;

        buttonAAnchorMin = buttonARect.anchorMin;
        buttonAAnchorMax = buttonARect.anchorMax;
        buttonAAnchoredPosition = buttonARect.anchoredPosition;
        buttonAPivot = buttonARect.pivot;
        buttonALayoutSaved = true;
    }

    private void SetSingleButtonLayout(bool centered)
    {
        if (!buttonALayoutSaved || buttonARect == null) return;

        if (centered)
        {
            buttonARect.anchorMin = new Vector2(0.5f, 0f);
            buttonARect.anchorMax = new Vector2(0.5f, 0f);
            buttonARect.pivot = new Vector2(0.5f, 0f);
            buttonARect.anchoredPosition = Vector2.zero;
        }
        else
        {
            buttonARect.anchorMin = buttonAAnchorMin;
            buttonARect.anchorMax = buttonAAnchorMax;
            buttonARect.pivot = buttonAPivot;
            buttonARect.anchoredPosition = buttonAAnchoredPosition;
        }
    }

    public void DisplayEvent(EventData eventData)
    {
        if (eventData == null) return;
        currentEvent = eventData;
        onNoticeClosed = null;

        descriptionText.text = eventData.eventDescription;
        textA.text = eventData.optionAText;
        textB.text = eventData.optionBText;

        SetSingleButtonLayout(false);
        if (buttonB != null) buttonB.gameObject.SetActive(true);

        buttonA.onClick.RemoveAllListeners();
        buttonA.onClick.AddListener(() => OnOptionSelected(true));

        buttonB.onClick.RemoveAllListeners();
        buttonB.onClick.AddListener(() => OnOptionSelected(false));

        gameObject.SetActive(true);
        if (maskBackground != null) maskBackground.SetActive(true);
    }

    public void DisplayNotice(string message, string confirmText = "知道了", System.Action onClose = null)
    {
        currentEvent = null;
        onNoticeClosed = onClose;

        descriptionText.text = message;
        textA.text = confirmText;
        if (textB != null) textB.text = "";
        if (buttonB != null) buttonB.gameObject.SetActive(false);

        SetSingleButtonLayout(true);

        buttonA.onClick.RemoveAllListeners();
        buttonA.onClick.AddListener(CloseNotice);

        gameObject.SetActive(true);
        if (maskBackground != null) maskBackground.SetActive(true);
    }

    private void CloseNotice()
    {
        SetSingleButtonLayout(false);
        if (buttonB != null) buttonB.gameObject.SetActive(true);

        System.Action callback = onNoticeClosed;
        onNoticeClosed = null;
        callback?.Invoke();

        gameObject.SetActive(false);
        if (maskBackground != null) maskBackground.SetActive(false);
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

        gameObject.SetActive(false);
        if (maskBackground != null) maskBackground.SetActive(false);
    }
}
