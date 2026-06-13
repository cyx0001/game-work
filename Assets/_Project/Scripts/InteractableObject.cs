﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractableObject : MonoBehaviour
{
    [Header("对象数据资产")]
    public ObjectData objectData; // 引用创建好的 ScriptableObject

    [Header("小游戏")]
    //public TreadmillMinigame minigameToLaunch; // 不为空时点击会启动小游戏

    [Header("当前等级")]
    public int currentLevel = 1;  // 游戏内默认为 1 级

    // 获取当前等级对应的数值配置
    public LevelData GetCurrentLevelData()
    {
        if (objectData == null || objectData.levels.Length < currentLevel)
        {
            Debug.LogError($"{gameObject.name} 缺少数据配置或等级超出范围！");
            return new LevelData();
        }
        // 因为数组从 0 开始，比如 level 1 对应 levels[0]
        return objectData.levels[currentLevel - 1];
    }

    // 玩家左键确认执行该行动时调用
    public void ExecuteAction()
    {
        // 0. 如果配置了小游戏 → 启动小游戏
        // if (minigameToLaunch != null)
        // {
        //     if (GameManager.Instance != null && GameManager.Instance.UseAP(1))
        //     {
        //         minigameToLaunch.StartMinigame();
        //     }
        //     return;
        // }

        // 1. 检查并且尝试扣除 1 点 AP
        if (GameManager.Instance != null && GameManager.Instance.UseAP(1))
        {
            // 2. 获取当前等级对应的数值
            LevelData data = GetCurrentLevelData();

            // 3. 相应地修改玩家的属性状态
            if (PlayerDataManager.Instance != null)
            {
                PlayerDataManager.Instance.ModifyStats(
                    data.bloodSugarDelta,
                    data.healthDelta,
                    data.moodDelta,
                    data.moneyDelta
                );
            }

            // 4. 做一个微小的点击反馈动画（简单放大再缩小）
            StartCoroutine(ClickFeedbackAnimation());
        }
    }

    // 一个简单的点击反馈动画
    private System.Collections.IEnumerator ClickFeedbackAnimation()
    {
        Vector3 originalScale = transform.localScale;
        transform.localScale = originalScale * 1.05f; // 微微放大
        yield return new WaitForSeconds(0.1f);
        transform.localScale = originalScale;         // 恢复原状
    }
}
