using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TreadmillSceneController : MonoBehaviour
{
    [Header("=== UI 元素 ===")]
    public TextMeshProUGUI countdownText;
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI speedText;
    public TextMeshProUGUI feedbackText;

    [Header("=== 结算面板 ===")]
    public GameObject resultPanel;
    public TextMeshProUGUI resultScoreText;
    public TextMeshProUGUI ratingText;
    public Button resultCloseButton;

    [Header("=== 核心位置与预制体 ===")]
    public GameObject notePrefab;
    public RectTransform spawnPoint;
    public RectTransform judgementLine;
    public RectTransform noteContainer;

    [Header("=== 游戏参数调节 ===")]
    public float noteMoveSpeed = 500f;
    public float spawnInterval = 0.7f;
    public float totalGameTime = 15f;
    public float missBoundaryY = -550f;

    private int score = 0;
    private float currentTimer;
    private bool isPlaying = false;

    private float[] laneXPositions = new float[] { -300f, 0f, 300f };
    private List<SimpleNote>[] laneNotes = new List<SimpleNote>[3];

    private const string TREADMILL_TUTORIAL_KEY = "Tutorial_Treadmill_Shown";
    public const string TREADMILL_TUTORIAL_MSG =
        "<b>跑步机小游戏</b>\n\n" +
        "<b>[操作]</b>\n" +
        "  A/S/D 或 左/下/右方向键 -- 击打三个轨道音符\n" +
        "  1 / 2 -- 减速 / 加速音符下落\n\n" +
        "<b>[判定]</b>\n" +
        "  音符到达底部判定线时按键，越近分越高\n" +
        "  完美 +10  |  一般 +5  |  失误 +0\n\n" +
        "<b>[评分]</b>  S>=120  |  A>=70  |  B>=30  |  C<30\n" +
        "  评分越高，运动效果越好！";

    void Awake()
    {
        for (int i = 0; i < 3; i++)
        {
            laneNotes[i] = new List<SimpleNote>();
        }
    }

    void Start()
    {
        resultPanel.SetActive(false);
        feedbackText.text = "";
        scoreText.text = "得分: 0";
        timerText.text = $"时间: {totalGameTime:F1}s";
        UpdateSpeedUI();

        resultCloseButton.onClick.AddListener(OnResultClose);

        StartCoroutine(ShowTutorialIfNeeded());
    }

    private IEnumerator ShowTutorialIfNeeded()
    {
        yield return null;

        if (PlayerPrefs.GetInt(TREADMILL_TUTORIAL_KEY, 0) == 0)
        {
            if (EventPopupController.Instance != null)
            {
                EventPopupController.Instance.DisplayNotice(TREADMILL_TUTORIAL_MSG, "准备好了！", () =>
                {
                    PlayerPrefs.SetInt(TREADMILL_TUTORIAL_KEY, 1);
                    PlayerPrefs.Save();
                    StartCoroutine(GameFlowRoutine());
                });
                yield break;
            }
            PlayerPrefs.SetInt(TREADMILL_TUTORIAL_KEY, 1);
            PlayerPrefs.Save();
        }

        StartCoroutine(GameFlowRoutine());
    }

    private void OnResultClose()
    {
        resultPanel.SetActive(false);

        float multiplier;
        if (score >= 120) multiplier = 1.5f;
        else if (score >= 70) multiplier = 1.0f;
        else if (score >= 30) multiplier = 0.7f;
        else multiplier = 0.4f;

        InteractableObject src = TreadmillSceneLauncher.SourceObject;
        if (src != null)
        {
            LevelData data = src.GetCurrentLevelData();
            if (PlayerDataManager.Instance != null)
            {
                PlayerDataManager.Instance.ModifyStats(
                    data.bloodSugarDelta * multiplier,
                    data.healthDelta * multiplier,
                    data.moodDelta * multiplier,
                    Mathf.RoundToInt(data.moneyDelta * multiplier)
                );
            }

            if (GameManager.Instance != null)
                GameManager.Instance.UseAP(1);

            src.MarkLevelCleared();

            TreadmillGameBridge.Output_Score = score;
            TreadmillGameBridge.IsDataReady = true;
        }

        TreadmillSceneLauncher.ReturnToMain();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1))
        {
            noteMoveSpeed = Mathf.Max(200f, noteMoveSpeed - 50f);
            UpdateSpeedUI();
        }
        if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2))
        {
            noteMoveSpeed = Mathf.Min(1200f, noteMoveSpeed + 50f);
            UpdateSpeedUI();
        }

        if (!isPlaying) return;

        currentTimer -= Time.deltaTime;
        if (currentTimer <= 0)
        {
            currentTimer = 0;
            isPlaying = false;
        }
        timerText.text = $"时间: {currentTimer:F1}s";

        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
        {
            ProcessHit(0);
        }
        if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
        {
            ProcessHit(1);
        }
        if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
        {
            ProcessHit(2);
        }
    }

    void UpdateSpeedUI()
    {
        if (speedText != null)
        {
            speedText.text = $"下落速度: {noteMoveSpeed} (按[1]减速 / [2]加速)";
        }
    }

    IEnumerator GameFlowRoutine()
    {
        countdownText.gameObject.SetActive(true);
        for (int i = 3; i > 0; i--)
        {
            countdownText.text = i.ToString();
            yield return new WaitForSeconds(1f);
        }
        countdownText.text = "START!";
        yield return new WaitForSeconds(0.6f);
        countdownText.gameObject.SetActive(false);

        currentTimer = totalGameTime;
        isPlaying = true;
        StartCoroutine(SpawnNotesRoutine());

        yield return new WaitUntil(() => !isPlaying);

        StopAllCoroutines();
        ClearAllNotes();

        ShowResult();
    }

    IEnumerator SpawnNotesRoutine()
    {
        while (isPlaying && currentTimer > 1.0f)
        {
            int randomLane = Random.Range(0, 3);

            GameObject go = Instantiate(notePrefab, noteContainer);
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.anchoredPosition = new Vector2(laneXPositions[randomLane], spawnPoint.anchoredPosition.y);

            SimpleNote note = go.GetComponent<SimpleNote>();
            note.missYBoundary = missBoundaryY;
            note.laneIndex = randomLane;
            note.controller = this;

            laneNotes[randomLane].Add(note);

            yield return new WaitForSeconds(spawnInterval);
        }
    }

    void ProcessHit(int lane)
    {
        laneNotes[lane].RemoveAll(n => n == null);

        if (laneNotes[lane].Count == 0)
        {
            ShowFeedback("失误", Color.red);
            return;
        }

        SimpleNote targetNote = laneNotes[lane][0];

        if (targetNote != null)
        {
            float dist = Mathf.Abs(targetNote.GetComponent<RectTransform>().anchoredPosition.y - judgementLine.anchoredPosition.y);

            if (dist < 30f)
            {
                score += 10;
                scoreText.text = $"得分: {score}";
                ShowFeedback("完美！", Color.green);

                laneNotes[lane].Remove(targetNote);
                targetNote.DestroyNote();
            }
            else if (dist >= 30f && dist < 65f)
            {
                score += 5;
                scoreText.text = $"得分: {score}";
                ShowFeedback("一般", Color.yellow);

                laneNotes[lane].Remove(targetNote);
                targetNote.DestroyNote();
            }
            else
            {
                ShowFeedback("失误", Color.red);
            }
        }
    }

    public void OnNoteMiss(SimpleNote note)
    {
        int lane = note.laneIndex;
        if (laneNotes[lane].Contains(note))
        {
            laneNotes[lane].Remove(note);
        }
        ShowFeedback("失误", Color.red);
    }

    void ShowFeedback(string text, Color color)
    {
        feedbackText.text = text;
        feedbackText.color = color;
        feedbackText.transform.localScale = Vector3.one * 1.2f;
        StartCoroutine(FadeFeedbackRoutine());
    }

    IEnumerator FadeFeedbackRoutine()
    {
        yield return new WaitForSeconds(0.4f);
        if (feedbackText.text == "完美！" || feedbackText.text == "一般" || feedbackText.text == "失误")
        {
            feedbackText.text = "";
        }
    }

    void ShowResult()
    {
        resultPanel.SetActive(true);
        resultScoreText.text = $"最终得分: {score}";

        if (score >= 120)
        {
            ratingText.text = "最终评分: 完美控糖达人 (S)";
            ratingText.color = Color.green;
        }
        else if (score >= 70)
        {
            ratingText.text = "最终评分: 稳健运动者 (A)";
            ratingText.color = Color.yellow;
        }
        else if (score >= 30)
        {
            ratingText.text = "最终评分: 热身运动 (B)";
            ratingText.color = Color.white;
        }
        else
        {
            ratingText.text = "最终评分: 需要多加练习 (C)";
            ratingText.color = Color.red;
        }
    }

    void ClearAllNotes()
    {
        for (int i = 0; i < 3; i++)
        {
            foreach (var note in laneNotes[i])
            {
                if (note != null) Destroy(note.gameObject);
            }
            laneNotes[i].Clear();
        }
    }
}
