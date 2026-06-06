using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractionDetector : MonoBehaviour
{
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(mousePosition, Vector2.zero);

            if (hit.collider != null)
            {
                // 尝试获取被点击物体身上的 InteractableObject 组件
                InteractableObject clickableObj = hit.collider.GetComponent<InteractableObject>();

                if (clickableObj != null)
                {
                    // 成功获取，直接执行该物体的行动
                    clickableObj.ExecuteAction();
                }
            }
        }
    }
}
