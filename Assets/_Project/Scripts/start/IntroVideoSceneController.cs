using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityEngine.SceneManagement;
using TMPro;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// 开场动画：全屏播放 Resources/开场动画.mp4，播完或按键跳过进入 Start 场景。
/// </summary>
public class IntroVideoSceneController : MonoBehaviour
{
    private const string VideoAssetPath = "Assets/_Project/Resources/开场动画.mp4";
    private const string ResourcesVideoName = "开场动画";

    public const string NextSceneName = "Start";

    [Header("视频")]
    public VideoClip introClip;

    [Header("行为")]
    [Tooltip("进入场景后忽略输入的秒数，避免误触")]
    public float inputDelay = 0.5f;
    [Tooltip("跳过后淡出时长")]
    public float fadeOutDuration = 0.35f;
    public bool allowSkip = true;
    [Tooltip("等待视频就绪的最长秒数")]
    public float prepareTimeout = 8f;

    [Header("跳过提示")]
    public string skipMessage = "按任意键跳过";
    public TMP_FontAsset skipFont;
    public float skipFontSize = 28f;
    public Color skipColor = new Color(1f, 0.94f, 0.72f, 0.85f);
    public float skipBottomOffset = 48f;

    private VideoPlayer videoPlayer;
    private Camera targetCamera;
    private CanvasGroup skipGroup;
    private TextMeshProUGUI skipText;
    private bool isLeaving;
    private float readyTime;
    private string lastError;

    void Awake()
    {
        targetCamera = Camera.main;
        if (targetCamera == null)
        {
            Debug.LogError("[IntroVideo] 场景中缺少 Main Camera。");
            return;
        }

        targetCamera.clearFlags = CameraClearFlags.SolidColor;
        targetCamera.backgroundColor = Color.black;

        ResolveAssets();
        SetupSkipUI();
        SetupVideoPlayer();
    }

    void Start()
    {
        if (videoPlayer == null)
        {
            SceneManager.LoadScene(NextSceneName);
            return;
        }

        readyTime = Time.unscaledTime + inputDelay;
        StartCoroutine(PlayIntroRoutine());
    }

    void Update()
    {
        if (!allowSkip || isLeaving || Time.unscaledTime < readyTime) return;

        if (Input.anyKeyDown)
            StartCoroutine(LeaveRoutine());
    }

    private void ResolveAssets()
    {
#if UNITY_EDITOR
        if (introClip == null)
            introClip = AssetDatabase.LoadAssetAtPath<VideoClip>(VideoAssetPath);
#endif

        if (introClip == null)
            introClip = Resources.Load<VideoClip>(ResourcesVideoName);

        if (introClip == null)
            Debug.LogWarning("[IntroVideo] 未能加载 VideoClip，将尝试用文件路径播放。");
    }

    private void SetupVideoPlayer()
    {
        videoPlayer = gameObject.AddComponent<VideoPlayer>();
        videoPlayer.playOnAwake = false;
        videoPlayer.isLooping = false;
        videoPlayer.waitForFirstFrame = true;
        videoPlayer.skipOnDrop = true;
        videoPlayer.audioOutputMode = VideoAudioOutputMode.Direct;
        videoPlayer.renderMode = VideoRenderMode.CameraFarPlane;
        videoPlayer.targetCamera = targetCamera;
        videoPlayer.aspectRatio = VideoAspectRatio.FitInside;
        videoPlayer.loopPointReached += OnVideoFinished;
        videoPlayer.errorReceived += OnVideoError;
    }

    private void SetupSkipUI()
    {
        if (!allowSkip) return;

        GameObject canvasObj = new GameObject("SkipCanvas", typeof(RectTransform));
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        canvas.additionalShaderChannels |= AdditionalCanvasShaderChannels.TexCoord1
            | AdditionalCanvasShaderChannels.Normal
            | AdditionalCanvasShaderChannels.Tangent;

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;
        canvasObj.AddComponent<GraphicRaycaster>();

        skipGroup = canvasObj.AddComponent<CanvasGroup>();
        skipGroup.alpha = 1f;
        skipGroup.interactable = false;
        skipGroup.blocksRaycasts = false;

        RectTransform canvasRect = canvasObj.GetComponent<RectTransform>();

        GameObject promptObj = CreateUIObject("SkipPrompt", canvasRect);
        RectTransform rect = promptObj.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0f);
        rect.anchorMax = new Vector2(0.5f, 0f);
        rect.pivot = new Vector2(0.5f, 0f);
        rect.anchoredPosition = new Vector2(0f, skipBottomOffset);
        rect.sizeDelta = new Vector2(900f, 48f);

        skipText = promptObj.AddComponent<TextMeshProUGUI>();
        skipText.text = skipMessage;
        skipText.fontSize = skipFontSize;
        skipText.color = skipColor;
        skipText.alignment = TextAlignmentOptions.Center;
        skipText.raycastTarget = false;
        skipText.enableWordWrapping = false;

        if (skipFont != null)
            skipText.font = skipFont;
    }

    private IEnumerator PlayIntroRoutine()
    {
        if (!TryConfigureVideoSource())
        {
            Debug.LogError("[IntroVideo] 找不到视频文件，跳过开场动画。");
            SceneManager.LoadScene(NextSceneName);
            yield break;
        }

        yield return WaitUntilPrepared();

        if (!videoPlayer.isPrepared)
        {
            Debug.LogError($"[IntroVideo] 视频无法播放{(string.IsNullOrEmpty(lastError) ? "" : ": " + lastError)}，跳过开场动画。");
            SceneManager.LoadScene(NextSceneName);
            yield break;
        }

        videoPlayer.Play();
        Debug.Log("[IntroVideo] 开始播放开场动画。");
    }

    private bool TryConfigureVideoSource()
    {
        if (introClip != null)
        {
            videoPlayer.source = VideoSource.VideoClip;
            videoPlayer.clip = introClip;
            videoPlayer.url = string.Empty;
            return true;
        }

        string url = BuildVideoFileUrl();
        if (string.IsNullOrEmpty(url))
            return false;

        videoPlayer.source = VideoSource.Url;
        videoPlayer.clip = null;
        videoPlayer.url = url;
        Debug.Log($"[IntroVideo] 使用文件路径播放: {url}");
        return true;
    }

    private IEnumerator WaitUntilPrepared()
    {
        lastError = null;
        videoPlayer.Prepare();

        float elapsed = 0f;
        while (!videoPlayer.isPrepared && elapsed < prepareTimeout)
        {
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        if (videoPlayer.isPrepared)
            yield break;

        if (videoPlayer.source == VideoSource.VideoClip)
        {
            string url = BuildVideoFileUrl();
            if (!string.IsNullOrEmpty(url))
            {
                Debug.LogWarning("[IntroVideo] VideoClip 准备失败，改用文件 URL 重试。");
                videoPlayer.Stop();
                videoPlayer.source = VideoSource.Url;
                videoPlayer.clip = null;
                videoPlayer.url = url;

                lastError = null;
                videoPlayer.Prepare();
                elapsed = 0f;
                while (!videoPlayer.isPrepared && elapsed < prepareTimeout)
                {
                    elapsed += Time.unscaledDeltaTime;
                    yield return null;
                }
            }
        }
    }

    private string BuildVideoFileUrl()
    {
#if UNITY_EDITOR
        string assetPath = introClip != null
            ? AssetDatabase.GetAssetPath(introClip)
            : VideoAssetPath;

        if (!string.IsNullOrEmpty(assetPath) && File.Exists(assetPath))
            return ToFileUrl(Path.GetFullPath(assetPath));
#endif

        // 打包后 Resources 中的视频会随 VideoClip 一起提供；若 clip 为空则无法回退到原始 mp4。
        return null;
    }

    private static string ToFileUrl(string fullPath)
    {
        return "file:///" + fullPath.Replace("\\", "/");
    }

    private void OnVideoError(VideoPlayer player, string message)
    {
        lastError = message;
        Debug.LogError("[IntroVideo] " + message);
    }

    private void OnVideoFinished(VideoPlayer player)
    {
        if (isLeaving) return;
        StartCoroutine(LeaveRoutine());
    }

    private IEnumerator LeaveRoutine()
    {
        if (isLeaving) yield break;
        isLeaving = true;

        if (videoPlayer != null && videoPlayer.isPlaying)
            videoPlayer.Stop();

        if (skipGroup != null && fadeOutDuration > 0f)
        {
            float t = 0f;
            while (t < fadeOutDuration)
            {
                t += Time.unscaledDeltaTime;
                skipGroup.alpha = 1f - Mathf.Clamp01(t / fadeOutDuration);
                yield return null;
            }
            skipGroup.alpha = 0f;
        }

        SceneManager.LoadScene(NextSceneName);
    }

    void OnDestroy()
    {
        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached -= OnVideoFinished;
            videoPlayer.errorReceived -= OnVideoError;
        }
    }

    private static GameObject CreateUIObject(string name, RectTransform parent)
    {
        GameObject obj = new GameObject(name, typeof(RectTransform));
        obj.transform.SetParent(parent, false);
        int uiLayer = LayerMask.NameToLayer("UI");
        obj.layer = uiLayer >= 0 ? uiLayer : 5;
        return obj;
    }
}
