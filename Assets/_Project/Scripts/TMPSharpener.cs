using UnityEngine;
using TMPro;

/// <summary>
/// 全局 TMP 字体锐化 —— 游戏启动时自动对所有 TextMeshProUGUI 应用清晰材质设置
/// </summary>
public static class TMPSharpener
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void SharpenAllTMPText()
    {
        var allTMP = Object.FindObjectsOfType<TextMeshProUGUI>(true);
        foreach (var tmp in allTMP)
        {
            if (tmp.font == null) continue;

            // 创建独立材质实例
            tmp.fontMaterial = new Material(tmp.font.material);
            var mat = tmp.fontMaterial;

            // 收紧面膨胀（0.15 = 微粗，0 = 最细）
            mat.SetFloat("_FaceDilate", 0.15f);
            // 消除轮廓柔化
            mat.SetFloat("_OutlineSoftness", 0f);
            // 提高采样精度
            mat.SetFloat("_GradientScale", 10f);
            // 去除加粗（加粗=模糊的主要来源）
            if ((tmp.fontStyle & FontStyles.Bold) != 0)
                tmp.fontStyle &= ~FontStyles.Bold;
        }
        Debug.Log($"[TMPSharpener] 已锐化 {allTMP.Length} 个 TextMeshProUGUI 组件");
    }
}
