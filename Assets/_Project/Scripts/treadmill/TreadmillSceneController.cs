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
    public TextMeshProUGUI speedText; // 用于显示当前速度提示
    public TextMeshProUGUI feedbackText;
    
    [Header("=== 结算面板 ===")]
    public GameObject resultPanel;
    public TextMeshProUGUI resultScoreText;
    public TextMeshProUGUI ratingText;     // 显示最后的评分等级（优秀/良好/及格）
    public Button resultCloseButton;

    [Header("=== 核心位置与预制体 ===")]
    public GameObject notePrefab;
    public RectTransform spawnPoint;       // 上方生成参考点
    public RectTransform judgementLine;    // 下方判定线参考点
    public RectTransform noteContainer;     // 音符生成的父层级

    [Header("=== 游戏参数调节 ===")]
    public float noteMoveSpeed = 500f;      // 初始音符下落速度(像素/秒)
    public float spawnInterval = 0.7f;      // 生成间隔
    public float totalGameTime = 15f;       // 15秒单局
    public float missBoundaryY = -550f;     // 音符掉落到多少Y坐标算Miss（需低于判定线Y坐标）

    private int score = 0;
    private float currentTimer;
    private bool isPlaying = false;
    
    // 3个轨道在X轴上的相对坐标：左轨道、中轨道、右轨道
    private float[] laneXPositions = new float[] { -300f, 0f, 300f };
    
    // 分别存储 3 个轨道里当前存活的音符列表
    private List<SimpleNote>[] laneNotes = new List<SimpleNote>[3];

    void Awake()
    {
        // 初始化3个轨道的列表容器
        for (int i = 0; i < 3; i++)
        {
            laneNotes[i] = new List<SimpleNote>();
        }
    }

    void Start()
    {
        // 界面状态初始化
        resultPanel.SetActive(false);
        feedbackText.text = "";
        scoreText.text = "得分: 0";
        timerText.text = $"时间: {totalGameTime:F1}s";
        UpdateSpeedUI();
        
        resultCloseButton.onClick.AddListener(() => { resultPanel.SetActive(false); });

        // 启动整局游戏流程
        StartCoroutine(GameFlowRoutine());
    }

    void Update()
    {
        // 1. 实时调速检测：无论游戏是否开始，都可以按 1 减速，按 2 加速
        if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1))
        {
            noteMoveSpeed = Mathf.Max(200f, noteMoveSpeed - 50f); // 最慢不低于200
            UpdateSpeedUI();
        }
        if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2))
        {
            noteMoveSpeed = Mathf.Min(1200f, noteMoveSpeed + 50f); // 最快不超过1200
            UpdateSpeedUI();
        }

        if (!isPlaying) return;

        // 2. 倒计时时钟计时
        currentTimer -= Time.deltaTime;
        if (currentTimer <= 0)
        {
            currentTimer = 0;
            isPlaying = false;
        }
        timerText.text = $"时间: {currentTimer:F1}s";

        // 3. 监听三个轨道的物理按键：左(←或A)、中(↓或S)、右(→或D)
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
        // 开局 3 2 1 倒计时
        countdownText.gameObject.SetActive(true);
        for (int i = 3; i > 0; i--)
        {
            countdownText.text = i.ToString();
            yield return new WaitForSeconds(1f);
        }
        countdownText.text = "START!";
        yield return new WaitForSeconds(0.6f);
        countdownText.gameObject.SetActive(false);

        // 正式开玩
        currentTimer = totalGameTime;
        isPlaying = true;
        StartCoroutine(SpawnNotesRoutine());

        // 等待限时结束
        yield return new WaitUntil(() => !isPlaying);

        // 游戏结束清场
        StopAllCoroutines();
        ClearAllNotes();

        // 触发最终评分并弹出结算
        ShowResult();
    }

    // 随机轨道生成音符
    IEnumerator SpawnNotesRoutine()
    {
        while (isPlaying && currentTimer > 1.0f) // 最后一秒停止生成
        {
            int randomLane = Random.Range(0, 3);

            GameObject go = Instantiate(notePrefab, noteContainer);
            RectTransform rect = go.GetComponent<RectTransform>();
            
            // 设定初始位置（使用对应的轨道X坐标，顶部的Y坐标）
            rect.anchoredPosition = new Vector2(laneXPositions[randomLane], spawnPoint.anchoredPosition.y);

            SimpleNote note = go.GetComponent<SimpleNote>();
            note.missYBoundary = missBoundaryY;
            note.laneIndex = randomLane;
            note.controller = this;

            laneNotes[randomLane].Add(note);

            yield return new WaitForSeconds(spawnInterval);
        }
    }

    // 核心点击判定
    void ProcessHit(int lane)
    {
        // 自动清理已被滑出销毁的残留 null 引用
        laneNotes[lane].RemoveAll(n => n == null);

        if (laneNotes[lane].Count == 0)
        {
            // 当前轨道空无一人玩家瞎按，直接判定为失误
            ShowFeedback("失误", Color.red);
            return;
        }

        // 下落式音游中，离底线最近的永远是列表中最早生成的第一个音符
        SimpleNote targetNote = laneNotes[lane][0];
        
        if (targetNote != null)
        {
            // 计算音符当前 Y 坐标同判定线 Y 坐标的绝对距离（垂直像素差）
            float dist = Mathf.Abs(targetNote.GetComponent<RectTransform>().anchoredPosition.y - judgementLine.anchoredPosition.y);

            // 【完美判定】 距离判定线小于 30 像素
            if (dist < 30f)
            {
                score += 10;
                scoreText.text = $"得分: {score}";
                ShowFeedback("完美！", Color.green);

                laneNotes[lane].Remove(targetNote);
                targetNote.DestroyNote();
            }
            // 【一般判定】 30像素 <= 距离 < 65像素
            else if (dist >= 30f && dist < 65f)
            {
                score += 5;
                scoreText.text = $"得分: {score}";
                ShowFeedback("一般", Color.yellow);

                laneNotes[lane].Remove(targetNote);
                targetNote.DestroyNote();
            }
            // 离得太远（按太早或太晚），算失误
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

    // 根据总分进行最后的评分计算
    void ShowResult()
    {
        resultPanel.SetActive(true);
        resultScoreText.text = $"最终得分: {score}";

        // 根据游戏期间的得分进行多档评分展示
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