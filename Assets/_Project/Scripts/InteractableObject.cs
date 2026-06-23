using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InteractableObject : MonoBehaviour
{
    [Header("绑定资源")]
    public ObjectData objectData;

    [Header("小游戏模式")]
    public bool launchMinigame = false;
    [Tooltip("Treadmill / Kitchen / ComputerQuiz 等")]
    public string minigameSceneName = "";

    [Header("当前等级")]
    public int currentLevel = 1;

    [Header("联动升级")]
    [Tooltip("升级本物件时，会同步升级列表中的所有物件")]
    public InteractableObject[] linkedObjects;

    [Header("高亮设置")]
    [Tooltip("高亮光圈颜色")]
    public Color highlightColor = new Color(1f, 0.95f, 0.4f, 0.5f);
    [Tooltip("高亮光圈排序层级(需大于背景)")]
    public int highlightSortingOrder = 10;

    [Header("按键交互")]
    public KeyCode interactKey = KeyCode.Space;
    public float interactRange = 2.5f;
    [TextArea]
    public string promptText = "按空格键 睡觉";
    public TMPro.TMP_FontAsset promptFont;

    [Header("跳过设置")]
    [Tooltip("是否允许该物件在当前等级已通关后跳过小游戏")]
    public bool allowSkip = true;

    // 内部引用
    private SpriteRenderer spriteRenderer;
    private GameObject highlightChild;
    private SpriteRenderer highlightRenderer;
    private Color originalColor;
    private Vector3 originalScale;
    private bool isHighlighted = false;

    // 按键提示
    private GameObject promptChild;
    private TextMeshProUGUI promptTMP;
    private bool playerInRange = false;

    // 交互状态锁（防止弹窗期间重复触发）
    private bool isWaitingForPopup = false;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;
        originalScale = transform.localScale;

        if (spriteRenderer == null)
            CreateHighlightChild();

        CreateProximityTrigger();
        CreatePromptUI();
    }

    // ==================== 近距离触发 ====================

    private void CreateProximityTrigger()
    {
        GameObject trig = new GameObject("_ProximityTrigger");
        trig.transform.SetParent(transform);
        trig.transform.localPosition = Vector3.zero;
        trig.layer = gameObject.layer;

        CircleCollider2D col = trig.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = interactRange;

        var relay = trig.AddComponent<TriggerRelay>();
        relay.parent = this;
    }

    public void OnPlayerEnterRange()
    {
        if (playerInRange) return;
        playerInRange = true;
        if (promptChild != null)
            promptChild.SetActive(true);
    }

    public void OnPlayerExitRange()
    {
        if (!playerInRange) return;
        playerInRange = false;
        if (promptChild != null)
            promptChild.SetActive(false);
    }

    // ==================== 按键提示 UI ====================

    private void CreatePromptUI()
    {
        // 按名称精确查找已有的提示画布，避免 FindObjectOfType<Canvas> 在 DontDestroyOnLoad
        // 残留画布（如 SleepFadeCanvas）存在时返回错误的父画布，导致提示渲染异常
        GameObject existingCanvas = GameObject.Find("_PromptCanvas");
        Canvas promptCanvas = existingCanvas != null ? existingCanvas.GetComponent<Canvas>() : null;

        if (promptCanvas == null)
        {
            GameObject canvasGO = new GameObject("_PromptCanvas");
            promptCanvas = canvasGO.AddComponent<Canvas>();
            promptCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            promptCanvas.sortingOrder = 100;

            CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            DontDestroyOnLoad(canvasGO);
        }

        promptChild = new GameObject($"Prompt_{gameObject.GetInstanceID()}");
        promptChild.transform.SetParent(promptCanvas.transform);
        promptChild.transform.localPosition = Vector3.zero;
        promptChild.transform.localScale = Vector3.one;
        promptChild.AddComponent<RectTransform>();

        GameObject bg = new GameObject("Bg");
        bg.transform.SetParent(promptChild.transform);
        RectTransform bgRect = bg.AddComponent<RectTransform>();
        bgRect.anchorMin = new Vector2(0.5f, 0.5f);
        bgRect.anchorMax = new Vector2(0.5f, 0.5f);
        bgRect.anchoredPosition = Vector2.zero;

        UnityEngine.UI.Image bgImg = bg.AddComponent<UnityEngine.UI.Image>();
        bgImg.color = new Color(0, 0, 0, 0.7f);

        GameObject txt = new GameObject("Text");
        txt.transform.SetParent(bg.transform);
        RectTransform txtRect = txt.AddComponent<RectTransform>();
        txtRect.anchorMin = Vector2.zero;
        txtRect.anchorMax = Vector2.one;
        txtRect.offsetMin = new Vector2(4, 2);
        txtRect.offsetMax = new Vector2(-4, -2);

        promptTMP = txt.AddComponent<TextMeshProUGUI>();
        promptTMP.text = promptText;
        promptTMP.fontSize = 32;
        promptTMP.color = Color.white;
        promptTMP.alignment = TextAlignmentOptions.Center;
        promptTMP.overflowMode = TextOverflowModes.Overflow;
        promptTMP.enableWordWrapping = false;

        if (promptFont != null)
        {
            promptTMP.font = promptFont;
            promptTMP.fontMaterial = new Material(promptFont.material);
            promptTMP.fontMaterial.SetFloat("_FaceDilate", 0.15f);
            promptTMP.fontMaterial.SetFloat("_OutlineSoftness", 0f);
            promptTMP.fontMaterial.SetFloat("_GradientScale", 10f);
        }
        promptTMP.ForceMeshUpdate();

        // 背景框自动适配文字实际尺寸
        Vector2 textSize = promptTMP.GetRenderedValues(false);
        bgRect.sizeDelta = new Vector2(textSize.x + 24, textSize.y + 14);

        promptChild.SetActive(false);
    }

    // ==================== 更新 ====================

    private void Update()
    {
        bool inMinigame = GameManager.Instance != null && GameManager.Instance.isInMinigame;

        if (promptChild != null)
        {
            if (inMinigame)
            {
                if (promptChild.activeSelf)
                    promptChild.SetActive(false);
            }
            else if (playerInRange && !promptChild.activeSelf)
            {
                promptChild.SetActive(true);
            }
            else if (!playerInRange && promptChild.activeSelf)
            {
                promptChild.SetActive(false);
            }

            if (promptChild.activeSelf)
            {
                Vector3 screenPos = Camera.main != null
                    ? Camera.main.WorldToScreenPoint(transform.position + Vector3.up * 0.8f)
                    : Vector3.zero;
                promptChild.transform.position = screenPos;
            }
        }

        if (playerInRange && Input.GetKeyDown(interactKey))
        {
            if (inMinigame)
                return;
            ExecuteAction();
        }
    }

    // ==================== 高亮效果 ====================

    private void CreateHighlightChild()
    {
        highlightChild = new GameObject("_Highlight");
        highlightChild.transform.SetParent(transform);
        highlightChild.transform.localPosition = Vector3.zero;

        highlightRenderer = highlightChild.AddComponent<SpriteRenderer>();

        int size = 64;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        Color[] px = new Color[size * size];
        float half = size * 0.5f;
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float d = Mathf.Sqrt((x - half) * (x - half) + (y - half) * (y - half));
                float a = 1f - Mathf.Clamp01(d / half);
                a = a * a * a;
                px[y * size + x] = new Color(1f, 1f, 1f, a);
            }
        }
        tex.SetPixels(px);
        tex.Apply();

        Sprite spr = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 64f);
        highlightRenderer.sprite = spr;
        highlightRenderer.color = highlightColor;
        highlightRenderer.sortingOrder = highlightSortingOrder;

        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            float s = Mathf.Max(col.bounds.size.x, col.bounds.size.y) * 1.3f;
            highlightChild.transform.localScale = Vector3.one * Mathf.Max(s, 0.8f);
        }
        else
        {
            highlightChild.transform.localScale = Vector3.one * 2f;
        }

        highlightChild.SetActive(false);
    }

    public LevelData GetCurrentLevelData()
    {
        if (objectData == null || objectData.levels.Length < currentLevel)
        {
            Debug.LogError($"{gameObject.name} 缺少数据或者等级超范围！");
            return new LevelData();
        }
        return objectData.levels[currentLevel - 1];
    }

    public void SetHighlight(bool highlight)
    {
        if (isHighlighted == highlight) return;
        isHighlighted = highlight;

        if (highlight)
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.color = originalColor * 1.2f;
                transform.localScale = originalScale * 1.08f;
            }
            if (highlightChild != null)
                highlightChild.SetActive(true);
        }
        else
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.color = originalColor;
                transform.localScale = originalScale;
            }
            if (highlightChild != null)
                highlightChild.SetActive(false);
        }
    }

    // ==================== 交互逻辑 ====================

    /// <summary>新手指南内容（仅基础操作）</summary>
    public const string TUTORIAL_MESSAGE =
        "<b>欢迎来到《血糖控制专家》！</b>\n\n" +
        "<b>[移动]</b>  WASD 或 方向键\n\n" +
        "<b>[交互]</b>\n" +
        "  走近物体按 <b>空格键</b> 或 <b>鼠标左键</b>\n" +
        "  <b>鼠标右键</b>点击物体查看升级面板\n\n" +
        "<b>[行动点]</b>  每天 5 点 AP，合理安排！\n\n" +
        "<b>[目标]</b>  14 天内将血糖控制在 70~120 的安全范围。\n\n" +
        "<color=#C08860>温馨提示：</color>如果忘记目标，可以移动鼠标到屏幕左边查看哦！";

    /// <summary>新手指南第二页：可交互物件说明</summary>
    public const string TUTORIAL_OBJECTS_MESSAGE =
        "<b>可交互物件说明</b>\n\n" +
        "<b>跑步机</b>  降低血糖，提升健康\n\n" +
        "<b>厨房</b>  做饭影响血糖，提升健康与心情\n\n" +
        "<b>电脑</b>  知识问答获得金钱，健康会略微下降\n\n" +
        "<b>床</b>  休息恢复血糖、健康与心情\n\n" +
        "走到物件旁边按 <b>空格键</b> 即可交互！\n\n" +
        "<b>[结束按钮]</b>  屏幕右下角点击\"结束今天\"进入当天结算，推进到下一天";

    /// <summary>获取该物件当前等级的通关记录 PlayerPrefs Key</summary>
    private string GetClearKey()
    {
        string objName = objectData != null ? objectData.objectName : gameObject.name;
        return $"Skip_{objName}_Lv{currentLevel}";
    }

    /// <summary>当前等级是否已通关（可跳过）</summary>
    public bool IsLevelCleared()
    {
        return PlayerPrefs.GetInt(GetClearKey(), 0) == 1;
    }

    /// <summary>标记当前等级已通关（由小游戏完成时调用）</summary>
    public void MarkLevelCleared()
    {
        PlayerPrefs.SetInt(GetClearKey(), 1);
        PlayerPrefs.Save();
    }

#if UNITY_EDITOR
    [ContextMenu("清除本物件所有跳过记录")]
    private void ClearAllSkipRecords()
    {
        for (int lv = 1; lv <= 3; lv++)
        {
            string objName = objectData != null ? objectData.objectName : gameObject.name;
            PlayerPrefs.DeleteKey($"Skip_{objName}_Lv{lv}");
        }
        PlayerPrefs.Save();
        Debug.Log($"[{gameObject.name}] 已清除全部跳过记录");
    }

    [ContextMenu("清除所有教程/跳过记录（全局）")]
    private void ClearAllGlobalRecords()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        Debug.Log("[全局] 已清除所有 PlayerPrefs 记录");
    }
#endif

    /// <summary>
    /// 直接执行属性应用（跳过小游戏时使用）
    /// </summary>
    private void ExecuteActionDirect()
    {
        if (GameManager.Instance == null || !GameManager.Instance.UseAP(1))
        {
            if (EventPopupController.Instance != null)
                EventPopupController.Instance.DisplayNotice("行动点不足！", "确定");
            return;
        }

        if (IsBed())
        {
            EnsureSleepFadeController();
            SleepFadeController.Instance.PlaySleepTransition(() =>
            {
                ApplyStats();
            });
        }
        else
        {
            ApplyStats();
            StartCoroutine(ClickFeedbackAnimation());
        }
    }

    public void ExecuteAction()
    {
        // 弹窗等待中 → 拒绝重复交互
        if (isWaitingForPopup) return;

        ExecuteActionInternal();
    }

    /// <summary>实际交互逻辑（含跳过判断）</summary>
    private void ExecuteActionInternal()
    {
        // 小游戏跳转
        if (launchMinigame)
        {
            // 检查是否可跳过（已通关 + 允许跳过）
            if (allowSkip && IsLevelCleared())
            {
                ShowSkipPopup();
                return;
            }

            // 未通关 → 强制进入小游戏
            LaunchMinigame();
            return;
        }

        // 非小游戏物件 → 直接扣AP并应用属性
        ExecuteActionDirect();
    }

    /// <summary>显示「跳过 / 再次挑战」选择弹窗</summary>
    private void ShowSkipPopup()
    {
        isWaitingForPopup = true;

        if (EventPopupController.Instance == null)
        {
            // 无弹窗系统时直接走跳过逻辑
            isWaitingForPopup = false;
            ExecuteActionDirect();
            return;
        }

        string objDisplayName = objectData != null ? objectData.displayName : gameObject.name;
        string message = $"<b>{objDisplayName} Lv.{currentLevel}</b> 小游戏已通关！\n\n你可以选择再次挑战或直接跳过以获得属性加成。\n\n<color=#C08860>提示：</color>升级后跳过可获得的属性更高，但每次升级需要再玩一次小游戏后才可跳过哦！";

        EventPopupController.Instance.DisplayChoice(
            message,
            "再次挑战",
            "跳过（直接应用属性）",
            onA: () => { isWaitingForPopup = false; LaunchMinigame(); },
            onB: () => { isWaitingForPopup = false; ExecuteActionDirect(); }
        );
    }

    /// <summary>启动小游戏</summary>
    private void LaunchMinigame()
    {
        // 电脑小游戏每日限制：如果今天已完成，不扣AP，直接提示并返回
        if (minigameSceneName == "ComputerQuiz" && ComputerSceneLauncher.HasCompletedToday())
        {
            if (EventPopupController.Instance != null)
                EventPopupController.Instance.DisplayNotice("今天已经使用过电脑了，明天再来吧。", "知道了");
            return;
        }

        if (GameManager.Instance == null || !GameManager.Instance.UseAP(1))
        {
            if (EventPopupController.Instance != null)
                EventPopupController.Instance.DisplayNotice("行动点不足！无法进行小游戏。", "确定");
            return;
        }

        // 每日目标检查
        if (DailyGoalManager.Instance != null)
            DailyGoalManager.Instance.CheckGoal(minigameSceneName);

        if (minigameSceneName == "Treadmill")
            TreadmillSceneLauncher.Launch(this);
        else if (minigameSceneName == "Kitchen")
            KitchenSceneLauncher.Launch(this);
        else if (minigameSceneName == "ComputerQuiz")
            ComputerSceneLauncher.Launch(this);
    }

    private bool IsBed()
    {
        return objectData != null && objectData.objectName == "Bed";
    }

    private void ApplyStats()
    {
        LevelData data = GetCurrentLevelData();
        if (PlayerDataManager.Instance != null)
        {
            PlayerDataManager.Instance.ModifyStats(
                data.bloodSugarDelta,
                data.healthDelta,
                data.moodDelta,
                data.moneyDelta
            );
        }
    }

    private void EnsureSleepFadeController()
    {
        if (SleepFadeController.Instance == null)
        {
            GameObject go = new GameObject("SleepFadeController");
            go.AddComponent<SleepFadeController>();
        }
    }

    private System.Collections.IEnumerator ClickFeedbackAnimation()
    {
        Vector3 orig = transform.localScale;
        transform.localScale = orig * 1.05f;
        yield return new WaitForSeconds(0.1f);
        transform.localScale = orig;
    }
}

public class TriggerRelay : MonoBehaviour
{
    public InteractableObject parent;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            parent.OnPlayerEnterRange();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            parent.OnPlayerExitRange();
    }
}
