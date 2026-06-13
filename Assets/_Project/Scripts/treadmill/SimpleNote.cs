using UnityEngine;

public class SimpleNote : MonoBehaviour
{
    [HideInInspector] public float missYBoundary; // 低于这个Y坐标算漏接滑出屏幕
    [HideInInspector] public int laneIndex;       // 0:左(←), 1:中(↓), 2:右(→)
    [HideInInspector] public TreadmillSceneController controller;

    private RectTransform rectTransform;
    private bool isChecked = false;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    void Update()
    {
        // 核心：根据主控脚本实时提供的速度进行匀速下落
        float currentSpeed = controller.noteMoveSpeed;
        rectTransform.anchoredPosition += Vector2.down * currentSpeed * Time.deltaTime;

        // 超过底部边界未被点击，触发漏接 Miss
        if (rectTransform.anchoredPosition.y < missYBoundary && !isChecked)
        {
            isChecked = true;
            if (controller != null)
            {
                controller.OnNoteMiss(this);
            }
            Destroy(gameObject);
        }
    }

    public void DestroyNote()
    {
        isChecked = true;
        Destroy(gameObject);
    }
}