using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UpgradePopupController : MonoBehaviour
{
    public static UpgradePopupController Instance { get; private set; }

    [Header("UI 元素关联")]
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI detailsText;
    public TextMeshProUGUI costText;
    public Button upgradeButton;
    public Button closeButton;

    private InteractableObject targetObject; // 当前正在准备升级的目标物件

    private void Awake()
    {
        Instance = this;
        gameObject.SetActive(false); // 游戏启动时默认隐藏

        // 绑定关闭按钮
        if (closeButton != null) closeButton.onClick.AddListener(ClosePanel);
    }

    // 核心方法：外部物件点击“升级”时调用，传入物件组件
    public void OpenUpgradePanel(InteractableObject obj)
    {
        if (obj == null || obj.objectData == null) return;
        targetObject = obj;

        RefreshUI();

        // 重新绑定升级按钮的点击事件
        upgradeButton.onClick.RemoveAllListeners();
        upgradeButton.onClick.AddListener(ExecuteUpgrade);

        gameObject.SetActive(true);
    }

    // 刷新面板文本和按钮状态
    private void RefreshUI()
    {
        if (targetObject == null) return;

        ObjectData data = targetObject.objectData;
        int currentLvl = targetObject.currentLevel;
        titleText.text = $"{data.displayName} (当前 Lv.{currentLvl})";

        // 如果已经满级（假设上限 3 级）
        if (currentLvl >= data.levels.Length)
        {
            detailsText.text = "该物件已升至最高等级，属性已达极限！";
            costText.text = "消耗金钱: --";
            upgradeButton.interactable = false; // 禁用按钮
            return;
        }

        // 获取当前级和下一级的数据进行对比
        LevelData currentData = data.levels[currentLvl - 1];
        LevelData nextData = data.levels[currentLvl]; // 数组索引 currentLvl 刚好对应下一级

        // 拼接对比文案
        string details = $"【行动效果对比】\n" +
                         $"血糖变化: {currentData.bloodSugarDelta} -> <color=#2ECC71>{nextData.bloodSugarDelta}</color>\n" +
                         $"健康变化: {currentData.healthDelta} -> <color=#2ECC71>{nextData.healthDelta}</color>\n" +
                         $"心情变化: {currentData.moodDelta} -> <color=#2ECC71>{nextData.moodDelta}</color>\n" +
                         $"金钱变化: {currentData.moneyDelta} -> <color=#2ECC71>{nextData.moneyDelta}</color>";

        detailsText.text = details;
        costText.text = $"升级消耗: ¥ {currentData.upgradeCost}";

        // 检查玩家钱包钱够不够，不够就让按钮变灰
        if (PlayerDataManager.Instance != null && PlayerDataManager.Instance.money >= currentData.upgradeCost)
        {
            upgradeButton.interactable = true;
        }
        else
        {
            upgradeButton.interactable = false; // 没钱变灰
            costText.text += " <color=red>(余额不足)</color>";
        }
    }

    // 执行升级
    private void ExecuteUpgrade()
    {
        if (targetObject == null) return;

        int currentLvl = targetObject.currentLevel;
        LevelData currentData = targetObject.objectData.levels[currentLvl - 1];

        // 1. 扣钱
        if (PlayerDataManager.Instance != null)
        {
            // 传入负数代表扣钱
            PlayerDataManager.Instance.ModifyStats(0, 0, 0, -currentData.upgradeCost);
        }

        // 2. 物件等级 +1
        targetObject.currentLevel++;
        Debug.Log($"【系统升级】{targetObject.objectData.displayName} 成功升级至 Lv.{targetObject.currentLevel}！");

        // 3. 刷新面板（或者直接关闭面板）
        RefreshUI();
    }

    public void ClosePanel()
    {
        gameObject.SetActive(false);
    }
}