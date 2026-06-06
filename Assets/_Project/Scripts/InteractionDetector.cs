using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractionDetector : MonoBehaviour
{
    void Update()
    {
        // 核心安全伞：只有当游戏没有被暂停（没有弹出弹窗、没有进ICU或通关）时才允许任何点击
        if (Time.timeScale == 0f) return;

        // 1. 左键点击：执行正常物件功能（使用床铺、跑步机、厨房等）
        if (Input.GetMouseButtonDown(0))
        {
            InteractableObject clickableObj = GetInteractableObjectAtMouse();
            if (clickableObj != null)
            {
                clickableObj.ExecuteAction();
            }
        }

        // 2. 右键点击：弹出该物件的升级面板！
        if (Input.GetMouseButtonDown(1))
        {
            InteractableObject clickableObj = GetInteractableObjectAtMouse();
            if (clickableObj != null && UpgradePopupController.Instance != null)
            {
                // 打开升级面板，并把这个物体传过去
                UpgradePopupController.Instance.OpenUpgradePanel(clickableObj);
            }
        }
    }

    // 封装一个复用的提取鼠标下物件的方法，让代码更干净
    private InteractableObject GetInteractableObjectAtMouse()
    {
        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(mousePosition, Vector2.zero);

        if (hit.collider != null)
        {
            return hit.collider.GetComponent<InteractableObject>();
        }
        return null;
    }
}