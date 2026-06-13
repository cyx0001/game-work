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
    public bool launchTreadmillMinigame = false;

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
    public TMPro.TMP_FontAsset promptFont; // 拖入 zpix SDF，支持中文

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

        // 挂一个辅助脚本来通知父物体
        var relay = trig.AddComponent<TriggerRelay>();
        relay.parent = this;
    }

    /// <summary>
    /// 由 TriggerRelay 调用
    /// </summary>
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
        // 查找或创建全局提示Canvas（Screen Space Overlay）
        Canvas promptCanvas = FindObjectOfType<Canvas>(false);
        if (promptCanvas == null)
        {
            GameObject canvasGO = new GameObject("_PromptCanvas");
            promptCanvas = canvasGO.AddComponent<Canvas>();
            promptCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            promptCanvas.sortingOrder = 100;
            DontDestroyOnLoad(canvasGO);
        }

        promptChild = new GameObject($"Prompt_{gameObject.GetInstanceID()}");
        promptChild.transform.SetParent(promptCanvas.transform);
        promptChild.transform.localPosition = Vector3.zero;
        promptChild.transform.localScale = Vector3.one;

        promptChild.AddComponent<RectTransform>();

        // 背景
        GameObject bg = new GameObject("Bg");
        bg.transform.SetParent(promptChild.transform);
        RectTransform bgRect = bg.AddComponent<RectTransform>();
        bgRect.anchorMin = new Vector2(0.5f, 0.5f);
        bgRect.anchorMax = new Vector2(0.5f, 0.5f);
        bgRect.sizeDelta = new Vector2(300, 60);
        bgRect.anchoredPosition = Vector2.zero;

        UnityEngine.UI.Image bgImg = bg.AddComponent<UnityEngine.UI.Image>();
        bgImg.color = new Color(0, 0, 0, 0.7f);

        // 文本
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
            // 创建独立材质实例，避免影响其他文字
            promptTMP.fontMaterial = new Material(promptFont.material);
            // 收紧面膨胀，减少模糊
            promptTMP.fontMaterial.SetFloat("_FaceDilate", 0f);
            // 降低轮廓柔化
            promptTMP.fontMaterial.SetFloat("_OutlineSoftness", 0f);
            // 确保像素对齐
            promptTMP.fontMaterial.SetFloat("_GradientScale", 10f);
        }

        promptChild.SetActive(false);
    }

    // ==================== 更新 ====================

    private void Update()
    {
        // 跟踪屏幕位置
        if (promptChild != null && promptChild.activeSelf)
        {
            Vector3 screenPos = Camera.main != null 
                ? Camera.main.WorldToScreenPoint(transform.position + Vector3.up * 0.8f) 
                : Vector3.zero;
            promptChild.transform.position = screenPos;
        }

        // 按键触发
        if (playerInRange && Input.GetKeyDown(interactKey))
        {
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

    public void ExecuteAction()
    {
        if (launchTreadmillMinigame)
        {
            TreadmillSceneLauncher.Launch(this);
            return;
        }

        if (GameManager.Instance == null || !GameManager.Instance.UseAP(1))
            return;

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

/// <summary>
/// Trigger 中继脚本 —— 因为 Trigger 在子物体上，需要通知父 InteractableObject
/// </summary>
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
