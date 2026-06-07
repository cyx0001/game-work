using UnityEngine;

/// <summary>
/// 跑步机迷你游戏——物品组件
/// 每个掉落的食物/道具挂载此脚本，控制下落
/// </summary>
public class TreadmillItem : MonoBehaviour
{
    [HideInInspector] public ItemType type;
    [HideInInspector] public int lane;
    [HideInInspector] public float speed = 2f;
    [HideInInspector] public float widthMultiplier = 1f;
    [HideInInspector] public bool collected;

    private RectTransform rt;

    void Awake()
    {
        rt = GetComponent<RectTransform>();
    }

    /// <summary>每帧调用：向下移动</summary>
    public void MoveDown()
    {
        if (rt == null) return;
        Vector2 pos = rt.anchoredPosition;
        pos.y -= speed * Time.deltaTime * 100f; // 像素坐标速度
        rt.anchoredPosition = pos;
    }

    /// <summary>获取当前 Y 坐标</summary>
    public float GetY()
    {
        return rt != null ? rt.anchoredPosition.y : 0f;
    }

    /// <summary>获取当前 X 坐标</summary>
    public float GetX()
    {
        return rt != null ? rt.anchoredPosition.x : 0f;
    }
}
