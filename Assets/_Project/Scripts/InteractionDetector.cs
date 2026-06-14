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
                UpgradePopupController.Instance.OpenUpgradePanel(clickableObj);
            }
        }
    }

    private InteractableObject GetInteractableObjectAtMouse()
    {
        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        // 使用 RaycastAll 穿透父物体碰撞体，找到子物体上的 InteractableObject
        RaycastHit2D[] hits = Physics2D.RaycastAll(mousePosition, Vector2.zero);
        foreach (var hit in hits)
        {
            if (hit.collider == null) continue;
            InteractableObject obj = hit.collider.GetComponent<InteractableObject>();
            if (obj != null) return obj;
        }
        return null;
    }
}
