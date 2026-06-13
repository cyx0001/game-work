using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractionDetector : MonoBehaviour
{
    private InteractableObject lastHoveredObject;

    void Update()
    {
        // 安全伞：只有当游戏没有被暂停、没有进入跑步机、没有进ICU/通关时，才处理任何点击
        if (Time.timeScale == 0f) return;
        if (GameManager.Instance != null && GameManager.Instance.isInMinigame) return;

        // ===== 悬停高亮检测 =====
        InteractableObject currentHovered = GetInteractableObjectAtMouse();

        // 鼠标移入新物体 → 旧物体取消高亮，新物体高亮
        if (currentHovered != lastHoveredObject)
        {
            if (lastHoveredObject != null)
                lastHoveredObject.SetHighlight(false);

            if (currentHovered != null)
                currentHovered.SetHighlight(true);

            lastHoveredObject = currentHovered;
        }

        // 1. 左键点击 → 执行目标物体的功能（使用键盘、跑步机、睡觉等）
        if (Input.GetMouseButtonDown(0))
        {
            InteractableObject clickableObj = GetInteractableObjectAtMouse();
            if (clickableObj != null)
            {
                clickableObj.ExecuteAction();
            }
        }

        // 2. 右键 → 弹出升级面板
        if (Input.GetMouseButtonDown(1))
        {
            InteractableObject clickableObj = GetInteractableObjectAtMouse();
            if (clickableObj != null && UpgradePopupController.Instance != null)
            {
                // 打开升级面板，把具体物件传进去
                UpgradePopupController.Instance.OpenUpgradePanel(clickableObj);
            }
        }
    }

    // 封装一个复用型的获取鼠标下物体的方法，供各处调用保持干净
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
