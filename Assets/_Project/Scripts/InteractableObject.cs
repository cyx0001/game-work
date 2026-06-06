using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractableObject : MonoBehaviour
{
    [Header("绑定数据资产")]
    public ObjectData objectData; // 拖入刚才创建的 ScriptableObject

    [Header("当前等级")]
    public int currentLevel = 1;  // 游戏内默认为 1 级

    // 获取当前等级对应的数值数据
    public LevelData GetCurrentLevelData()
    {
        if (objectData == null || objectData.levels.Length < currentLevel)
        {
            Debug.LogError($"{gameObject.name} 缺少数据配置或等级超出范围！");
            return new LevelData();
        }
        // 数组索引从 0 开始，所以 level 1 对应 levels[0]
        return objectData.levels[currentLevel - 1];
    }

    // 玩家点击确认执行该行动时调用
    public void ExecuteAction()
    {
        // 1. 检查并尝试扣除 1 点 AP
        if (GameManager.Instance != null && GameManager.Instance.UseAP(1))
        {
            // 2. 获取当前等级对应的数值
            LevelData data = GetCurrentLevelData();

            // 3. 真正修改玩家的属性状态
            if (PlayerDataManager.Instance != null)
            {
                PlayerDataManager.Instance.ModifyStats(
                    data.bloodSugarDelta,
                    data.healthDelta,
                    data.moodDelta,
                    data.moneyDelta
                );
            }

            // 4. 播放轻微的点击动画反馈（放大再缩小）
            StartCoroutine(ClickFeedbackAnimation());
        }
    }

    // 一个简单的点击缩放动画反馈
    private System.Collections.IEnumerator ClickFeedbackAnimation()
    {
        Vector3 originalScale = transform.localScale;
        transform.localScale = originalScale * 1.05f; // 稍微放大
        yield return new WaitForSeconds(0.1f);
        transform.localScale = originalScale;         // 恢复原样
    }
}
