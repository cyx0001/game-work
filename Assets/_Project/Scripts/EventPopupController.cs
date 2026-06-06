using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EventPopupController : MonoBehaviour
{
    [Header("遮罩物体")]
    public GameObject maskBackground; // 拖入你的 Mask_Background
    public static EventPopupController Instance { get; private set; }

    [Header("UI 元素关联")]
    public TextMeshProUGUI descriptionText;
    public Button buttonA;
    public TextMeshProUGUI textA;
    public Button buttonB;
    public TextMeshProUGUI textB;

    private EventData currentEvent;

    private void Awake()
    {
        Instance = this;

        // 确保游戏一运行，弹窗和遮罩都先隐形，把舞台留给主场景
        gameObject.SetActive(false);
        if (maskBackground != null)
        {
            maskBackground.SetActive(false);
        }
    }

    // 核心方法：外部调用这个方法来弹出并显示一个特定的事件
    public void DisplayEvent(EventData eventData)
    {
        if (eventData == null) return;
        currentEvent = eventData;

        descriptionText.text = eventData.eventDescription;
        textA.text = eventData.optionAText;
        textB.text = eventData.optionBText;

        buttonA.onClick.RemoveAllListeners();
        buttonA.onClick.AddListener(() => OnOptionSelected(true));

        buttonB.onClick.RemoveAllListeners();
        buttonB.onClick.AddListener(() => OnOptionSelected(false));

        // === 同时显示弹窗和遮罩 ===
        gameObject.SetActive(true);
        if (maskBackground != null) maskBackground.SetActive(true);
    }

    // 当玩家做出抉择时
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

        // === 同时隐藏弹窗和遮罩 ===
        gameObject.SetActive(false);
        if (maskBackground != null) maskBackground.SetActive(false);
    }
}