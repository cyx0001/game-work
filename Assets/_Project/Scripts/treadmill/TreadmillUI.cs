using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 跑步机迷你游戏——全部 UI 管理
/// 配置面板 / HUD / 结果面板 / 危险效果
/// </summary>
public class TreadmillUI : MonoBehaviour
{
    // ==================== 根面板引用 ====================
    [Header("根面板")]
    public GameObject rootPanel;           // 整个小游戏的根 Canvas/Panel
    public GameObject configPanel;         // 局前配置面板
    public GameObject hudPanel;            // 局内 HUD
    public GameObject resultPanel;         // 结果面板

    // ==================== 配置面板 ====================
    [Header("配置面板")]
    public TextMeshProUGUI configTitle;
    public Button[] modeButtons = new Button[3];  // 3个模式按钮
    public TextMeshProUGUI[] modeButtonLabels = new TextMeshProUGUI[3];
    public Button configCancelButton;

    // ==================== HUD ====================
    [Header("HUD")]
    public TextMeshProUGUI hudTimer;
    public TextMeshProUGUI hudCalories;
    public TextMeshProUGUI hudHR;
    public TextMeshProUGUI hudSpeed;
    public TextMeshProUGUI hudDistance;
    public Image hudHRFill;               // 心率进度条（填充）
    public Image hudHRDangerFlash;        // 危险闪烁

    // ==================== 危险覆盖层 ====================
    [Header("危险效果（PostFX 降级方案）")]
    public Image vignetteOverlay;         // 四周变暗覆盖层
    public Image redOverlay;              // 全屏红色半透明
    public float vignetteMaxAlpha = 0.6f;
    public float redMaxAlpha = 0.3f;

    // ==================== 结果面板 ====================
    [Header("结果面板")]
    public TextMeshProUGUI resultGrade;
    public TextMeshProUGUI resultTitle;
    public TextMeshProUGUI resultCalories;
    public TextMeshProUGUI resultDistance;
    public TextMeshProUGUI resultEffect;
    public Button resultConfirmButton;

    // ==================== 心梗面板 ====================
    [Header("心梗面板")]
    public GameObject heartAttackPanel;
    public TextMeshProUGUI heartAttackText;
    public Button heartAttackButton;

    // ==================== 心跳音效 ====================
    [Header("心跳音效（可选）")]
    public AudioSource heartbeatAudio;    // 播放心跳声

    // ==================== 内部状态 ====================
    private TreadmillGameManager gm;
    private float dangerFactor;

    void Start()
    {
        gm = FindObjectOfType<TreadmillGameManager>();

        // 根面板背景设为半透明黑色，遮挡主场景
        Image bg = rootPanel?.GetComponent<Image>();
        if (bg != null) bg.color = new Color(0f, 0f, 0f, 0.78f);

        // 绑定按钮事件
        for (int i = 0; i < 3 && i < modeButtons.Length; i++)
        {
            int modeIndex = i; // 闭包捕获
            if (modeButtons[i] != null)
                modeButtons[i].onClick.AddListener(() => OnModeSelected((TreadmillMode)modeIndex));
        }

        if (configCancelButton != null)
            configCancelButton.onClick.AddListener(() => gm?.CancelMinigame());

        if (resultConfirmButton != null)
            resultConfirmButton.onClick.AddListener(() => gm?.ConfirmResult());

        if (heartAttackButton != null)
            heartAttackButton.onClick.AddListener(() => gm?.HeartAttackReturn());

        // 初始隐藏所有
        ShowIdle();
    }

    void Update()
    {
        if (gm == null || gm.state != TreadmillState.Playing) return;

        // 危险效果更新
        UpdateDangerEffects();
    }

    // ==================== 面板切换 ====================
    public void ShowIdle()
    {
        rootPanel?.SetActive(false);
        configPanel?.SetActive(false);
        hudPanel?.SetActive(false);
        resultPanel?.SetActive(false);
        heartAttackPanel?.SetActive(false);
        SetDangerEffects(0f);
    }

    public void ShowConfigPanel()
    {
        rootPanel?.SetActive(true);
        configPanel?.SetActive(true);
        hudPanel?.SetActive(false);
        resultPanel?.SetActive(false);
        SetDangerEffects(0f);

        // 更新模式按钮信息
        if (gm != null)
        {
            for (int i = 0; i < 3 && i < modeButtonLabels.Length; i++)
            {
                if (modeButtonLabels[i] != null && i < gm.modes.Length)
                {
                    var mode = gm.modes[i];
                    modeButtonLabels[i].text = $"{mode.modeName}\nAP: {mode.apCost} | {mode.gameDuration:F0}s\n<size=80%>{mode.description}</size>";
                }
            }
        }
    }

    public void ShowHUD()
    {
        rootPanel?.SetActive(true);
        configPanel?.SetActive(false);
        hudPanel?.SetActive(true);
        resultPanel?.SetActive(false);
        SetDangerEffects(0f);
    }

    public void ShowResult(string grade, string gradeName, int calories, float distance)
    {
        rootPanel?.SetActive(true);
        configPanel?.SetActive(false);
        hudPanel?.SetActive(false);
        resultPanel?.SetActive(true);
        SetDangerEffects(0f);

        // 填充结果
        if (resultGrade != null) resultGrade.text = grade;
        if (resultTitle != null) resultTitle.text = gradeName;
        if (resultCalories != null)
        {
            string sign = calories <= 0 ? "" : "+";
            string color = calories <= 0 ? "#2ECC71" : "#E74C3C";
            resultCalories.text = $"消耗卡路里: <color={color}>{sign}{calories}</color>";
        }
        if (resultDistance != null)
            resultDistance.text = $"跑步距离: {distance:F1} m";

        // 主游戏效果预览
        if (resultEffect != null)
        {
            string effect = calories < -600 ? "血糖 <color=#2ECC71>-25</color>，心情 <color=#2ECC71>+5</color>" :
                             calories < 0    ? "血糖 <color=#2ECC71>-15</color>，心情 <color=#2ECC71>+2</color>" :
                             calories < 300  ? "血糖 <color=#2ECC71>-5</color>，健康 <color=#2ECC71>+5</color>" :
                                               "血糖 <color=#E74C3C>+10</color>，心情 <color=#E74C3C>-2</color>";
            resultEffect.text = $"主系统效果:\n{effect}";
        }
    }

    public void ShowHeartAttack()
    {
        // 心肌梗塞特殊画面
        rootPanel?.SetActive(true);
        configPanel?.SetActive(false);
        hudPanel?.SetActive(false);
        resultPanel?.SetActive(false);
        heartAttackPanel?.SetActive(true);

        if (heartAttackText != null)
            heartAttackText.text = "💀 急性心肌梗塞！\n\n病人由于在跑步机上盲目卷速度，\n导致急性心肌梗塞。\n\n达成结局：【跑步机上的光速飞仙】";

        // 全屏红色覆盖
        if (redOverlay != null)
        {
            redOverlay.color = new Color(0.5f, 0f, 0f, 0.8f);
            redOverlay.gameObject.SetActive(true);
        }
        if (vignetteOverlay != null)
        {
            vignetteOverlay.color = new Color(0f, 0f, 0f, 1f);
            vignetteOverlay.gameObject.SetActive(true);
        }
    }

    // ==================== HUD 更新 ====================
    public void UpdateHUD(TreadmillGameManager gm)
    {
        if (!hudPanel.activeSelf) return;

        if (hudTimer != null)
            hudTimer.text = $"{gm.timeRemaining:F1}s";

        if (hudCalories != null)
        {
            string color = gm.calories <= 0 ? "#2ECC71" : "#E74C3C";
            hudCalories.text = $"卡路里: <color={color}>{gm.calories}</color>";
        }

        if (hudHR != null)
            hudHR.text = $"❤️ {gm.heartRate:F0} bpm";

        if (hudSpeed != null)
            hudSpeed.text = $"速度: {gm.currentSpeed:F1} m/s";

        if (hudDistance != null)
            hudDistance.text = $"距离: {gm.distance:F1} m";

        // 心率进度条（0-250映射）
        if (hudHRFill != null)
            hudHRFill.fillAmount = gm.heartRate / 250f;
    }

    // ==================== 危险效果 ====================
    private void UpdateDangerEffects()
    {
        if (gm == null) return;

        dangerFactor = Mathf.Clamp01((gm.heartRate - 150f) / 60f);

        SetDangerEffects(dangerFactor);

        // 心跳音效
        if (heartbeatAudio != null)
        {
            heartbeatAudio.volume = dangerFactor * 0.8f;
            heartbeatAudio.pitch = Mathf.Lerp(1f, 1.8f, dangerFactor);
        }

        // 危险闪烁（HR > 170 时）
        if (hudHRDangerFlash != null)
        {
            bool flashing = gm.heartRate > maxSafeHR;
            hudHRDangerFlash.gameObject.SetActive(flashing);
            if (flashing)
            {
                float alpha = Mathf.PingPong(Time.time * 4f, 0.5f) + 0.3f;
                hudHRDangerFlash.color = new Color(1f, 0f, 0f, alpha);
            }
        }
    }

    private float maxSafeHR = 170f;

    private void SetDangerEffects(float factor)
    {
        if (vignetteOverlay != null)
        {
            Color c = vignetteOverlay.color;
            c.a = factor * vignetteMaxAlpha;
            vignetteOverlay.color = c;
            vignetteOverlay.gameObject.SetActive(factor > 0.01f);
        }

        if (redOverlay != null)
        {
            Color c = redOverlay.color;
            c.a = factor * redMaxAlpha;
            redOverlay.color = c;
            redOverlay.gameObject.SetActive(factor > 0.01f);
        }
    }

    // ==================== 按钮回调 ====================
    private void OnModeSelected(TreadmillMode mode)
    {
        gm?.StartGame(mode);
    }
}
