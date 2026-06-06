using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HUDController : MonoBehaviour
{
    [Header("顶部状态栏元素")]
    public TextMeshProUGUI dayText;
    public TextMeshProUGUI moneyText;
    public TextMeshProUGUI apText;

    [Header("底部状态栏进度条")]
    public Slider sugarSlider;
    public Slider healthSlider;
    public Slider moodSlider;

    [Header("进度条数值文本")]
    public TextMeshProUGUI sugarText;
    public TextMeshProUGUI healthText;
    public TextMeshProUGUI moodText;

    [Header("功能按钮")]
    public Button endDayButton;

    private void Start()
    {
        sugarSlider.maxValue = GameConstants.MAX_BLOOD_SUGAR;
        healthSlider.maxValue = GameConstants.MAX_HEALTH;
        moodSlider.maxValue = GameConstants.MAX_MOOD;

        if (endDayButton != null)
        {
            endDayButton.onClick.RemoveAllListeners(); // 防重复绑定
            endDayButton.onClick.AddListener(() => GameManager.Instance.EndDay());
        }

        if (PlayerDataManager.Instance != null)
        {
            PlayerDataManager.Instance.OnDataChanged.RemoveAllListeners();
            PlayerDataManager.Instance.OnDataChanged.AddListener(UpdatePlayerStatsUI);
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnAPChanged.RemoveAllListeners();
            GameManager.Instance.OnAPChanged.AddListener(UpdatePhaseUI);
        }

        // 延迟一帧或者确保安全地刷新
        UpdatePlayerStatsUI();
        UpdatePhaseUI();
    }

    // 刷新三大属性、金钱的 UI 显示
    private void UpdatePlayerStatsUI()
    {
        PlayerDataManager pd = PlayerDataManager.Instance;
        if (pd == null) return;

        // 更新 Slider 进度条的值
        sugarSlider.value = pd.bloodSugar;
        healthSlider.value = pd.health;
        moodSlider.value = pd.mood;

        // 更新文本数字（保留 0 位小数）
        sugarText.text = "血糖:"+$"{pd.bloodSugar:F0}/{GameConstants.MAX_BLOOD_SUGAR}";
        healthText.text = "健康:" + $"{pd.health:F0}/{GameConstants.MAX_HEALTH}";
        moodText.text = "心情:" + $"{pd.mood:F0}/{GameConstants.MAX_MOOD}";

        // 更新金钱
        moneyText.text = $"¥ {pd.money}";
    }

    // 刷新天数、阶段和行动点 (AP) 的 UI 显示
    private void UpdatePhaseUI()
    {
        GameManager gm = GameManager.Instance;
        if (gm == null) return;

        apText.text = $"剩余行动点: {gm.remainingAP} / 5";

        // 简易的天数阶段划分
        string phaseStr = "适应期";
        if (gm.currentDay > 10) phaseStr = "冲刺期";
        else if (gm.currentDay > 5) phaseStr = "危机期";

        dayText.text = $"第 {gm.currentDay} 天 ({phaseStr})";
    }
}