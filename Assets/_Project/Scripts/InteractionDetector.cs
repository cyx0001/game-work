using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractionDetector : MonoBehaviour
{
    void Update()
    {
        // ���İ�ȫɡ��ֻ�е���Ϸû�б���ͣ��û�е���������û�н�ICU��ͨ�أ�ʱ�������κε��
        if (Time.timeScale == 0f) return;
        if (GameManager.Instance != null && GameManager.Instance.isInMinigame) return;

        // 1. ��������ִ������������ܣ�ʹ�ô��̡��ܲ����������ȣ�
        if (Input.GetMouseButtonDown(0))
        {
            InteractableObject clickableObj = GetInteractableObjectAtMouse();
            if (clickableObj != null)
            {
                clickableObj.ExecuteAction();
            }
        }

        // 2. �Ҽ�����������������������壡
        if (Input.GetMouseButtonDown(1))
        {
            InteractableObject clickableObj = GetInteractableObjectAtMouse();
            if (clickableObj != null && UpgradePopupController.Instance != null)
            {
                // ��������壬����������崫��ȥ
                UpgradePopupController.Instance.OpenUpgradePanel(clickableObj);
            }
        }
    }

    // ��װһ�����õ���ȡ���������ķ������ô�����ɾ�
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