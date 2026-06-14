using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// 电脑糖尿病知识问答——场景控制器
/// </summary>
public class ComputerQuizSceneManager : MonoBehaviour
{
    private const string BackgroundAssetPath =
        "Assets/_Project/Material/Gemini_Generated_Image_vc1p8vvc1p8vvc1p.png";
    private const string ChineseFontAssetPath =
        "Assets/_Project/Art/Chinese_Font_Asset.asset";
    [Header("游戏设置")]
    [Tooltip("每局随机抽取的题目数量")]
    public int questionsPerRound = 5;

    [Header("答对一题属性变化(参照厨房绿色食物消除)")]
    public float correctSugarDelta = -1f;
    public float correctHealthDelta = 2f;
    public float correctMoodDelta = 1f;

    [Header("字体（拖入 Chinese_Font_Asset）")]
    public TMP_FontAsset chineseFont;

    [Header("背景")]
    public Sprite backgroundSprite;

    [Header("布局")]
    public Vector2 contentRootPosition = new Vector2(0f, 85f);
    public Vector2 contentRootSize = new Vector2(500f, 460f);

    [Header("颜色")]
    public Color panelColor = new Color(0f, 0f, 0f, 0.28f);
    public Color optionNormalColor = new Color(0f, 0f, 0f, 0.35f);
    public Color optionCorrectColor = new Color(0.18f, 0.55f, 0.32f, 1f);
    public Color optionWrongColor = new Color(0.65f, 0.18f, 0.18f, 1f);
    public Color hudTextColor = new Color(0.12f, 0.08f, 0.05f, 1f);

    private List<QuizQuestion> roundQuestions;
    private int currentIndex;
    private int correctCount;
    private bool answered;

    private float curSugar;
    private float curHealth;
    private float curMood;

    private TextMeshProUGUI progressText;
    private TextMeshProUGUI questionText;
    private TextMeshProUGUI feedbackText;
    private TextMeshProUGUI remainingText;
    private TextMeshProUGUI sugarText;
    private TextMeshProUGUI healthText;
    private TextMeshProUGUI moodText;
    private Button[] optionButtons = new Button[4];
    private TextMeshProUGUI[] optionLabels = new TextMeshProUGUI[4];
    private Button nextButton;
    private GameObject quizPanel;
    private GameObject resultPanel;
    private TextMeshProUGUI resultText;
    private Button resultCloseButton;

    private const string TUTORIAL_KEY = "Tutorial_ComputerQuiz_Shown";
    public const string TUTORIAL_MSG =
        "<b>糖尿病知识问答</b>\n\n" +
        "<b>[操作]</b>\n" +
        "  每题四个选项，点击选择答案\n" +
        "  答对会累计血糖/健康/心情变化\n\n" +
        "<b>[属性]</b>\n" +
        "  答对一题: 血糖-1 / 健康+2 / 心情+1\n" +
        "  (与厨房绿色食物消除量级相近)";

    void Awake()
    {
        ResolveAssets();
    }

    void Start()
    {
        BuildUI();
        StartCoroutine(ShowTutorialIfNeeded());
    }

    private void ResolveAssets()
    {
#if UNITY_EDITOR
        if (backgroundSprite == null)
            backgroundSprite = AssetDatabase.LoadAssetAtPath<Sprite>(BackgroundAssetPath);
        if (chineseFont == null)
            chineseFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(ChineseFontAssetPath);
#endif
    }

    private IEnumerator ShowTutorialIfNeeded()
    {
        yield return null;

        if (PlayerPrefs.GetInt(TUTORIAL_KEY, 0) == 0 && EventPopupController.Instance != null)
        {
            EventPopupController.Instance.DisplayNotice(TUTORIAL_MSG, "开始答题！", () =>
            {
                PlayerPrefs.SetInt(TUTORIAL_KEY, 1);
                PlayerPrefs.Save();
                BeginRound();
            });
            yield break;
        }

        BeginRound();
    }

    private void BeginRound()
    {
        roundQuestions = QuizQuestionBank.DrawRandomQuestions(questionsPerRound);
        currentIndex = 0;
        correctCount = 0;
        answered = false;
        curSugar = 0f;
        curHealth = 0f;
        curMood = 0f;

        quizPanel.SetActive(true);
        resultPanel.SetActive(false);
        nextButton.gameObject.SetActive(false);
        feedbackText.text = "";

        UpdateHUD();
        ShowQuestion();
    }

    private void ShowQuestion()
    {
        if (currentIndex >= roundQuestions.Count)
        {
            ShowResult();
            return;
        }

        answered = false;
        QuizQuestion q = roundQuestions[currentIndex];

        progressText.text = $"第 {currentIndex + 1} / {roundQuestions.Count} 题";
        questionText.text = q.question;
        feedbackText.text = "";
        nextButton.gameObject.SetActive(false);

        string[] prefixes = { "A.", "B.", "C.", "D." };
        for (int i = 0; i < 4; i++)
        {
            optionLabels[i].text = $"{prefixes[i]} {q.options[i]}";
            optionButtons[i].interactable = true;
            SetOptionColor(optionButtons[i], optionNormalColor);
        }

        TextMeshProUGUI nextLabel = nextButton.GetComponentInChildren<TextMeshProUGUI>();
        if (nextLabel != null)
            nextLabel.text = currentIndex >= roundQuestions.Count - 1 ? "查看结果" : "下一题";

        UpdateHUD();
    }

    private void OnOptionClicked(int optionIndex)
    {
        if (answered) return;
        answered = true;

        QuizQuestion q = roundQuestions[currentIndex];
        bool isCorrect = optionIndex == q.correctIndex;

        if (isCorrect)
        {
            correctCount++;
            curSugar += correctSugarDelta;
            curHealth += correctHealthDelta;
            curMood += correctMoodDelta;

            feedbackText.text =
                $"<color=#7CFC98>回答正确！</color> " +
                $"(血糖{(correctSugarDelta >= 0 ? "+" : "")}{correctSugarDelta:F0} " +
                $"健康+{correctHealthDelta:F0} 心情+{correctMoodDelta:F0})\n{q.explanation}";
        }
        else
        {
            feedbackText.text =
                $"<color=#FF8080>回答错误。</color> 正确答案是 <b>{(char)('A' + q.correctIndex)}</b>\n{q.explanation}";
        }

        for (int i = 0; i < 4; i++)
        {
            optionButtons[i].interactable = false;
            if (i == q.correctIndex)
                SetOptionColor(optionButtons[i], optionCorrectColor);
            else if (i == optionIndex)
                SetOptionColor(optionButtons[i], optionWrongColor);
        }

        UpdateHUD();
        nextButton.gameObject.SetActive(true);
    }

    private void OnNextClicked()
    {
        currentIndex++;
        ShowQuestion();
    }

    private void UpdateHUD()
    {
        int remaining = Mathf.Max(0, roundQuestions.Count - currentIndex);
        remainingText.text = $"剩余题数: {remaining}";
        sugarText.text = $"累计血糖: {(curSugar >= 0 ? "+" : "")}{curSugar:F1}";
        healthText.text = $"累计健康: {(curHealth >= 0 ? "+" : "")}{curHealth:F1}";
        moodText.text = $"累计心情: {(curMood >= 0 ? "+" : "")}{curMood:F1}";
    }

    private void ShowResult()
    {
        quizPanel.SetActive(false);
        resultPanel.SetActive(true);

        int total = roundQuestions.Count;
        float rate = total > 0 ? (float)correctCount / total * 100f : 0f;
        string grade;

        if (rate >= 90f) grade = "控糖小达人";
        else if (rate >= 70f) grade = "知识达人";
        else if (rate >= 50f) grade = "继续加油";
        else grade = "需要多学习";

        resultText.text =
            $"<b>答题成果</b>\n\n" +
            $"正确：{correctCount} / {total}  ({rate:F0}%)\n" +
            $"评价：{grade}\n\n" +
            $"最终血糖：{curSugar:F1}\n" +
            $"最终健康：{curHealth:F1}\n" +
            $"最终心情：{curMood:F1}";

        ComputerGameBridge.Output_CorrectCount = correctCount;
        ComputerGameBridge.Output_TotalCount = total;
        ComputerGameBridge.Output_SugarDelta = curSugar;
        ComputerGameBridge.Output_HealthDelta = curHealth;
        ComputerGameBridge.Output_MoodDelta = curMood;
        ComputerGameBridge.IsDataReady = true;
    }

    private void OnResultClose()
    {
        if (ComputerSceneLauncher.SourceObject != null)
        {
            if (PlayerDataManager.Instance != null)
            {
                PlayerDataManager.Instance.ModifyStats(
                    ComputerGameBridge.Output_SugarDelta,
                    ComputerGameBridge.Output_HealthDelta,
                    ComputerGameBridge.Output_MoodDelta,
                    0
                );
            }

            if (GameManager.Instance != null)
                GameManager.Instance.UseAP(1);

            InteractableObject source = ComputerSceneLauncher.SourceObject;
            if (source != null)
                source.MarkLevelCleared();

            ComputerSceneLauncher.ReturnToMain();
        }
        // 独立场景：确认按钮暂不处理，后续再接逻辑
    }

    private void SetOptionColor(Button btn, Color color)
    {
        Image img = btn.GetComponent<Image>();
        if (img != null) img.color = color;
    }

    #region UI 构建

    private void BuildUI()
    {
        EnsureEventSystem();

        RectTransform canvasRect = GetOrCreateCanvasRoot();
        EnsureSceneBackground(canvasRect);

        if (quizPanel != null) return;

        CreateHudPanel(canvasRect);
        quizPanel = CreateQuizPanel(canvasRect);
        resultPanel = CreateResultPanel(canvasRect);
        quizPanel.SetActive(false);
        resultPanel.SetActive(false);
    }

    private RectTransform GetOrCreateCanvasRoot()
    {
        Canvas existing = GetCanvasInThisScene();
        if (existing != null)
            return PrepareCanvas(existing);

        GameObject canvasObj = new GameObject("Canvas");
        SceneManager.MoveGameObjectToScene(canvasObj, gameObject.scene);

        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvasObj.AddComponent<CanvasScaler>().ApplyPreset(CanvasScalerPreset.FullHD);
        canvasObj.AddComponent<GraphicRaycaster>();
        return PrepareCanvas(canvas);
    }

    private Canvas GetCanvasInThisScene()
    {
        Scene scene = gameObject.scene;
        Canvas[] canvases = FindObjectsOfType<Canvas>(true);
        foreach (Canvas canvas in canvases)
        {
            if (canvas.gameObject.scene == scene)
                return canvas;
        }
        return null;
    }

    private RectTransform PrepareCanvas(Canvas canvas)
    {
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
        if (scaler == null)
            scaler = canvas.gameObject.AddComponent<CanvasScaler>();
        scaler.ApplyPreset(CanvasScalerPreset.FullHD);

        if (canvas.GetComponent<GraphicRaycaster>() == null)
            canvas.gameObject.AddComponent<GraphicRaycaster>();

        canvas.additionalShaderChannels |= AdditionalCanvasShaderChannels.TexCoord1
            | AdditionalCanvasShaderChannels.Normal
            | AdditionalCanvasShaderChannels.Tangent;

        return canvas.GetComponent<RectTransform>();
    }

    private void EnsureSceneBackground(RectTransform canvasRect)
    {
        Transform existing = canvasRect.Find("Background");
        if (existing != null)
        {
            Image img = existing.GetComponent<Image>();
            if (img != null && backgroundSprite != null)
            {
                img.sprite = backgroundSprite;
                img.type = Image.Type.Simple;
                img.preserveAspect = false;
                img.color = Color.white;
            }
            return;
        }

        CreateBackground(canvasRect);
    }

    private void EnsureEventSystem()
    {
        if (FindObjectOfType<UnityEngine.EventSystems.EventSystem>() != null) return;

        GameObject es = new GameObject("EventSystem");
        es.AddComponent<UnityEngine.EventSystems.EventSystem>();
        es.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
    }

    private void CreateBackground(RectTransform parent)
    {
        GameObject bg = CreateUIObject("Background", parent);
        StretchFull(bg.GetComponent<RectTransform>());
        Image img = bg.AddComponent<Image>();
        img.raycastTarget = false;

        if (backgroundSprite != null)
        {
            img.sprite = backgroundSprite;
            img.type = Image.Type.Simple;
            img.preserveAspect = false;
            img.color = Color.white;
        }
        else
        {
            img.color = new Color(0.72f, 0.55f, 0.38f, 1f);
        }
    }

    private void CreateHudPanel(RectTransform parent)
    {
        GameObject hud = CreateUIObject("HUD_Panel", parent);
        RectTransform hudRect = hud.GetComponent<RectTransform>();
        hudRect.anchorMin = new Vector2(0f, 0.5f);
        hudRect.anchorMax = new Vector2(0f, 0.5f);
        hudRect.pivot = new Vector2(0f, 0.5f);
        hudRect.anchoredPosition = new Vector2(40f, 0f);
        hudRect.sizeDelta = new Vector2(360f, 320f);

        Image hudBg = hud.AddComponent<Image>();
        hudBg.color = new Color(1f, 1f, 1f, 0.18f);
        hudBg.raycastTarget = false;

        VerticalLayoutGroup layout = hud.AddComponent<VerticalLayoutGroup>();
        layout.childAlignment = TextAnchor.UpperLeft;
        layout.spacing = 36f;
        layout.padding = new RectOffset(24, 10, 20, 10);
        layout.childControlWidth = true;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        remainingText = CreateHudLabel(hudRect, "Remaining");
        sugarText = CreateHudLabel(hudRect, "Sugar");
        healthText = CreateHudLabel(hudRect, "Health");
        moodText = CreateHudLabel(hudRect, "Mood");
    }

    private TextMeshProUGUI CreateHudLabel(RectTransform parent, string name)
    {
        GameObject obj = CreateUIObject(name, parent);
        LayoutElement le = obj.AddComponent<LayoutElement>();
        le.preferredHeight = 36f;

        TextMeshProUGUI tmp = obj.AddComponent<TextMeshProUGUI>();
        tmp.fontSize = 28f;
        tmp.color = hudTextColor;
        tmp.alignment = TextAlignmentOptions.MidlineLeft;
        if (chineseFont != null) tmp.font = chineseFont;
        return tmp;
    }

    private GameObject CreateQuizPanel(RectTransform parent)
    {
        GameObject panel = CreateUIObject("QuizPanel", parent);
        StretchFull(panel.GetComponent<RectTransform>());

        GameObject contentRoot = CreateUIObject("ContentRoot", panel.GetComponent<RectTransform>());
        RectTransform contentRect = contentRoot.GetComponent<RectTransform>();
        contentRect.anchorMin = contentRect.anchorMax = new Vector2(0.5f, 0.5f);
        contentRect.anchoredPosition = contentRootPosition;
        contentRect.sizeDelta = contentRootSize;

        progressText = CreateTMP("Progress", contentRect, new Vector2(0, 190), new Vector2(460, 34), 22);
        progressText.alignment = TextAlignmentOptions.Center;

        GameObject questionBox = CreateUIObject("QuestionBox", contentRect);
        RectTransform qBoxRect = questionBox.GetComponent<RectTransform>();
        qBoxRect.anchorMin = qBoxRect.anchorMax = new Vector2(0.5f, 0.5f);
        qBoxRect.anchoredPosition = new Vector2(0, 110);
        qBoxRect.sizeDelta = new Vector2(470, 110);
        questionBox.AddComponent<Image>().color = panelColor;

        questionText = CreateTMP("QuestionText", qBoxRect, Vector2.zero, new Vector2(450, 100), 22);
        questionText.alignment = TextAlignmentOptions.TopLeft;

        float optionStartY = 20f;
        float optionSpacing = 58f;
        for (int i = 0; i < 4; i++)
        {
            int captured = i;
            optionButtons[i] = CreateOptionButton(contentRect, new Vector2(0, optionStartY - i * optionSpacing), captured);
        }

        feedbackText = CreateTMP("Feedback", contentRect, new Vector2(0, -205), new Vector2(470, 70), 18);
        feedbackText.alignment = TextAlignmentOptions.TopLeft;

        nextButton = CreateButton("Btn_Next", contentRect, new Vector2(0, -255), new Vector2(180, 46), "下一题");
        nextButton.onClick.AddListener(OnNextClicked);
        nextButton.gameObject.SetActive(false);

        return panel;
    }

    private GameObject CreateResultPanel(RectTransform parent)
    {
        GameObject panel = CreateUIObject("ResultPanel", parent);
        StretchFull(panel.GetComponent<RectTransform>());
        Image overlay = panel.AddComponent<Image>();
        overlay.color = new Color(0, 0, 0, 0.65f);

        GameObject box = CreateUIObject("ResultBox", panel.GetComponent<RectTransform>());
        RectTransform boxRect = box.GetComponent<RectTransform>();
        boxRect.anchorMin = boxRect.anchorMax = new Vector2(0.5f, 0.5f);
        boxRect.anchoredPosition = Vector2.zero;
        boxRect.sizeDelta = new Vector2(560, 460);
        box.AddComponent<Image>().color = panelColor;

        resultText = CreateTMP("ResultText", boxRect, new Vector2(0, 30), new Vector2(520, 300), 28);
        resultText.alignment = TextAlignmentOptions.Center;

        resultCloseButton = CreateButton("Btn_Close", boxRect, new Vector2(0, -170), new Vector2(220, 56), "确认");
        resultCloseButton.onClick.AddListener(OnResultClose);

        return panel;
    }

    private Button CreateOptionButton(RectTransform parent, Vector2 pos, int index)
    {
        Button btn = CreateButton($"Option_{index}", parent, pos, new Vector2(470, 50), "");
        SetOptionColor(btn, optionNormalColor);

        RectTransform btnRect = btn.GetComponent<RectTransform>();
        optionLabels[index] = CreateTMP("Label", btnRect, Vector2.zero, new Vector2(450, 44), 20);
        optionLabels[index].alignment = TextAlignmentOptions.MidlineLeft;
        optionLabels[index].raycastTarget = false;

        btn.onClick.AddListener(() => OnOptionClicked(index));
        return btn;
    }

    private Button CreateButton(string name, RectTransform parent, Vector2 pos, Vector2 size, string label)
    {
        GameObject obj = CreateUIObject(name, parent);
        RectTransform rect = obj.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = pos;
        rect.sizeDelta = size;

        Image img = obj.AddComponent<Image>();
        img.color = new Color(0.22f, 0.45f, 0.75f, 0.9f);

        Button btn = obj.AddComponent<Button>();
        btn.targetGraphic = img;

        if (!string.IsNullOrEmpty(label))
        {
            TextMeshProUGUI tmp = CreateTMP("Text", rect, Vector2.zero, size - new Vector2(10, 10), 24);
            tmp.text = label;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.raycastTarget = false;
        }

        return btn;
    }

    private TextMeshProUGUI CreateTMP(string name, RectTransform parent, Vector2 pos, Vector2 size, float fontSize)
    {
        GameObject obj = CreateUIObject(name, parent);
        RectTransform rect = obj.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = pos;
        rect.sizeDelta = size;

        TextMeshProUGUI tmp = obj.AddComponent<TextMeshProUGUI>();
        tmp.fontSize = fontSize;
        tmp.color = Color.white;
        tmp.enableWordWrapping = true;
        if (chineseFont != null) tmp.font = chineseFont;

        return tmp;
    }

    private GameObject CreateUIObject(string name, RectTransform parent)
    {
        GameObject obj = new GameObject(name, typeof(RectTransform));
        obj.transform.SetParent(parent, false);
        obj.layer = LayerMask.NameToLayer("UI");
        return obj;
    }

    private void StretchFull(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    #endregion
}

internal static class CanvasScalerPresetExtensions
{
    public static void ApplyPreset(this CanvasScaler scaler, CanvasScalerPreset preset)
    {
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = preset == CanvasScalerPreset.FullHD
            ? new Vector2(1920, 1080)
            : new Vector2(1280, 720);
        scaler.matchWidthOrHeight = 0.5f;
    }
}

internal enum CanvasScalerPreset
{
    FullHD,
    HD
}
