using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// 开始界面：全屏显示 start.png，按任意键进入主场景 _Scene_0。
/// 场景结构与 ComputerQuiz 一致：Main Camera + Canvas/Background + 运行时 Prompt。
/// </summary>
public class StartSceneController : MonoBehaviour
{
    private const string BackgroundAssetPath = "Assets/_Project/Material/start.png";
    private const string BgmAssetPath = "Assets/_Project/bgm/bgm4.mp3";

    public const string MainSceneName = "_Scene_0";

    [Header("资源")]
    public Sprite backgroundSprite;
    public TMP_FontAsset promptFont;
    public AudioClip bgmClip;

    [Header("音效")]
    [Range(0f, 1f)]
    public float bgmVolume = 1f;

    [Header("文案")]
    public string promptMessage = "按任意键开始";

    [Header("行为")]
    [Tooltip("进入场景后忽略输入的秒数，避免误触")]
    public float inputDelay = 0.5f;
    [Tooltip("按键后淡出时长")]
    public float fadeOutDuration = 0.45f;

    [Header("提示样式")]
    public float promptFontSize = 38f;
    public Color promptColor = new Color(1f, 0.94f, 0.72f, 1f);
    public float blinkPeriod = 1.1f;
    public float promptBottomOffset = 72f;

    private CanvasGroup rootGroup;
    private TextMeshProUGUI promptText;
    private bool isLoading;
    private float readyTime;
    private Coroutine blinkRoutine;

    void Awake()
    {
        ResetPersistedGameState();
        ResolveAssets();
        SetupUI();
    }

    /// <summary>
    /// 进入开始界面 = 新开一局。Build 不会像 Editor 那样自动清 PlayerPrefs，
    /// 否则电脑「今日已用」、小游戏跳过记录等会跨次启动残留。
    /// </summary>
    private static void ResetPersistedGameState()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
    }

    void Start()
    {
        PlayBgm();
        readyTime = Time.unscaledTime + inputDelay;
        if (promptText != null && blinkRoutine == null)
            blinkRoutine = StartCoroutine(BlinkPrompt());
    }

    void Update()
    {
        if (isLoading || Time.unscaledTime < readyTime) return;

        if (Input.anyKeyDown)
            StartCoroutine(LoadMainSceneRoutine());
    }

    private void ResolveAssets()
    {
#if UNITY_EDITOR
        if (backgroundSprite == null)
        {
            Object[] assets = AssetDatabase.LoadAllAssetsAtPath(BackgroundAssetPath);
            foreach (Object asset in assets)
            {
                if (asset is Sprite sprite)
                {
                    backgroundSprite = sprite;
                    break;
                }
            }
        }

        if (bgmClip == null)
            bgmClip = AssetDatabase.LoadAssetAtPath<AudioClip>(BgmAssetPath);
#endif
    }

    private void PlayBgm()
    {
        if (bgmClip == null) return;

        AudioSource source = gameObject.GetComponent<AudioSource>();
        if (source == null)
            source = gameObject.AddComponent<AudioSource>();

        source.clip = bgmClip;
        source.loop = true;
        source.volume = bgmVolume;
        source.playOnAwake = false;
        source.Play();
    }

    private IEnumerator LoadMainSceneRoutine()
    {
        isLoading = true;

        if (blinkRoutine != null)
        {
            StopCoroutine(blinkRoutine);
            blinkRoutine = null;
        }

        if (promptText != null)
            promptText.alpha = 1f;

        if (rootGroup != null && fadeOutDuration > 0f)
        {
            float t = 0f;
            while (t < fadeOutDuration)
            {
                t += Time.unscaledDeltaTime;
                rootGroup.alpha = 1f - Mathf.Clamp01(t / fadeOutDuration);
                yield return null;
            }
            rootGroup.alpha = 0f;
        }

        SceneManager.LoadScene(MainSceneName);
    }

    private void SetupUI()
    {
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("[StartSceneController] 场景中缺少 Canvas，请与 ComputerQuiz 场景保持相同结构。");
            return;
        }

        rootGroup = canvas.GetComponent<CanvasGroup>();
        if (rootGroup == null)
            rootGroup = canvas.gameObject.AddComponent<CanvasGroup>();
        rootGroup.alpha = 1f;
        rootGroup.interactable = false;
        rootGroup.blocksRaycasts = false;

        RectTransform canvasRect = canvas.GetComponent<RectTransform>();
        ApplyBackgroundSprite(canvasRect);
        EnsurePrompt(canvasRect);
    }

    private void ApplyBackgroundSprite(RectTransform canvasRect)
    {
        Transform bgTransform = canvasRect.Find("Background");
        if (bgTransform == null) return;

        Image img = bgTransform.GetComponent<Image>();
        if (img == null) return;

        if (backgroundSprite != null)
            img.sprite = backgroundSprite;

        img.raycastTarget = false;
    }

    private void EnsurePrompt(RectTransform canvasRect)
    {
        Transform existing = canvasRect.Find("Prompt");
        GameObject promptObj;

        if (existing != null)
        {
            promptObj = existing.gameObject;
            promptText = promptObj.GetComponent<TextMeshProUGUI>();
        }
        else
        {
            promptObj = CreateUIObject("Prompt", canvasRect);
            RectTransform rect = promptObj.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0f);
            rect.anchorMax = new Vector2(0.5f, 0f);
            rect.pivot = new Vector2(0.5f, 0f);
            rect.anchoredPosition = new Vector2(0f, promptBottomOffset);
            rect.sizeDelta = new Vector2(900f, 64f);

            promptText = promptObj.AddComponent<TextMeshProUGUI>();
        }

        promptText.text = promptMessage;
        promptText.fontSize = promptFontSize;
        promptText.color = promptColor;
        promptText.alignment = TextAlignmentOptions.Center;
        promptText.raycastTarget = false;
        promptText.enableWordWrapping = false;

        if (promptFont != null)
            promptText.font = promptFont;
    }

    private IEnumerator BlinkPrompt()
    {
        if (promptText == null) yield break;

        while (!isLoading)
        {
            float t = 0f;
            while (t < blinkPeriod && !isLoading)
            {
                t += Time.unscaledDeltaTime;
                float phase = t / blinkPeriod;
                promptText.alpha = 0.35f + 0.65f * (0.5f + 0.5f * Mathf.Sin(phase * Mathf.PI * 2f));
                yield return null;
            }
        }
    }

    private static GameObject CreateUIObject(string name, RectTransform parent)
    {
        GameObject obj = new GameObject(name, typeof(RectTransform));
        obj.transform.SetParent(parent, false);
        obj.layer = LayerMask.NameToLayer("UI");
        return obj;
    }
}
