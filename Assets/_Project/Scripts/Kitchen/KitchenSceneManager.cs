using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class KitchenSceneManager : MonoBehaviour
{
    [Header("棋盘布局组件")]
    public GridLayoutGroup gridLayoutGroup;
    public GameObject foodPrefab;

    [Header("食物美术图片库")]
    public List<Sprite> foodSprites = new List<Sprite>();

    [Header("HUD 看板面板")]
    public TextMeshProUGUI movesText;
    public TextMeshProUGUI sugarText;   // 复用为"当前得分"
    public TextMeshProUGUI healthText;  // 复用为"评分阈值提示"
    public TextMeshProUGUI moodText;    // 复用为"连消倍率提示"

    [Header("游戏结算面板组件")]
    public GameObject resultPanel;
    public TextMeshProUGUI resultContentText;
    public Button btnConfirm;

    [Header("音频")]
    public AudioClip bgmClip;

    private int kitchenLevel = 1;
    private int gridSize = 5;
    private int remainingMoves = 10;

    private int totalScore = 0;
    private float lastComboMultiplier = 1.0f;

    private FoodItem[,] board;
    private FoodItem firstSelected = null;

    private const string KITCHEN_TUTORIAL_KEY = "Tutorial_Kitchen_Shown";
    public const string KITCHEN_TUTORIAL_MSG_PAGE1 =
        "<b>厨房小游戏</b>\n\n" +
        "<b>[操作]</b>\n" +
        "  点击一块食物选中它\n" +
        "  再点击上下左右相邻的食物交换位置\n" +
        "  横向或竖向 >=3 个相同食物即可消除\n\n" +
        "<b>[计分规则]</b>\n" +
        "  绿色食物 = 健康高分  |  红色食物 = 扣分\n" +
        "  3连消 1x  |  4连消 1.2x  |  5+连消 1.4x";
    public const string KITCHEN_TUTORIAL_MSG_PAGE2 =
        "<b>[评分]</b>  S≥200  |  A≥120  |  B≥50  |  C<50\n" +
        "  评分越高，烹饪效果越好！\n\n" +
        "<b>[步数]</b>  随厨房等级增加 (Lv1=10 / Lv2=13 / Lv3=16)";

    void Start()
    {
        kitchenLevel = KitchenGameBridge.Input_KitchenLevel;
        if (kitchenLevel < 1) kitchenLevel = 1;

        // 等级越高步数越多：Lv1=10, Lv2=13, Lv3=16
        remainingMoves = 7 + kitchenLevel * 3;
        totalScore = 0;

        if (resultPanel != null)
            resultPanel.SetActive(false);
        if (btnConfirm != null)
            btnConfirm.onClick.AddListener(OnResultConfirm);

        if (bgmClip != null && AudioManager.Instance != null)
            AudioManager.Instance.PlayBGM(bgmClip);

        StartCoroutine(ShowTutorialIfNeeded());
    }

    private IEnumerator ShowTutorialIfNeeded()
    {
        yield return null;

        if (PlayerPrefs.GetInt(KITCHEN_TUTORIAL_KEY, 0) == 0)
        {
            if (EventPopupController.Instance != null)
            {
                EventPopupController.Instance.DisplayNotice(KITCHEN_TUTORIAL_MSG_PAGE1, "继续", () =>
                {
                    EventPopupController.Instance.DisplayNotice(KITCHEN_TUTORIAL_MSG_PAGE2, "开始烹饪！", () =>
                    {
                        PlayerPrefs.SetInt(KITCHEN_TUTORIAL_KEY, 1);
                        PlayerPrefs.Save();
                        InitKitchen();
                    });
                });
                yield break;
            }
            PlayerPrefs.SetInt(KITCHEN_TUTORIAL_KEY, 1);
            PlayerPrefs.Save();
        }

        InitKitchen();
    }

    private void InitKitchen()
    {
        SetupGridByLevel();
        GenerateBoard();
        ClearInitialMatches();
        UpdateHUD();
    }

    private void ClearInitialMatches()
    {
        int safety = 0;
        while (safety < 100)
        {
            HashSet<FoodItem> matched = FindMatches();
            if (matched.Count == 0) break;

            List<FoodStaticInfo> pool = FoodTable.Infos.FindAll(f => f.unlockLevel <= kitchenLevel);
            foreach (var item in matched)
            {
                FoodStaticInfo randInfo = pool[Random.Range(0, pool.Count)];
                item.Init(item.x, item.y, randInfo, this);
            }
            safety++;
        }
    }

    private void SetupGridByLevel()
    {
        // 所有等级固定 5x5 棋盘，升级增加步数而非棋盘大小
        gridSize = 5;
        gridLayoutGroup.cellSize = new Vector2(140, 140);
        gridLayoutGroup.spacing = new Vector2(10, 10);
        board = new FoodItem[gridSize, gridSize];
    }

    private void GenerateBoard()
    {
        foreach (Transform child in gridLayoutGroup.transform) { Destroy(child.gameObject); }

        List<FoodStaticInfo> pool = FoodTable.Infos.FindAll(f => f.unlockLevel <= kitchenLevel);

        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                GameObject obj = Instantiate(foodPrefab, gridLayoutGroup.transform);
                FoodItem item = obj.GetComponent<FoodItem>();
                FoodStaticInfo randInfo = pool[Random.Range(0, pool.Count)];

                item.Init(x, y, randInfo, this);
                board[x, y] = item;
            }
        }
    }

    public Sprite GetSpriteByIndex(int index)
    {
        if (index >= 0 && index < foodSprites.Count)
            return foodSprites[index];
        return null;
    }

    public void SelectCard(FoodItem clickedItem)
    {
        if (remainingMoves <= 0) return;

        if (firstSelected == null)
        {
            firstSelected = clickedItem;
        }
        else
        {
            if (firstSelected == clickedItem)
            {
                firstSelected = null;
                return;
            }

            if ((Mathf.Abs(firstSelected.x - clickedItem.x) == 1 && firstSelected.y == clickedItem.y) ||
                (Mathf.Abs(firstSelected.y - clickedItem.y) == 1 && firstSelected.x == clickedItem.x))
            {
                SwapItems(firstSelected, clickedItem);
                remainingMoves--;

                CheckAndProcessMatches();
                UpdateHUD();
                firstSelected = null;

                if (remainingMoves <= 0) { OnGameFinished(); }
            }
            else
            {
                firstSelected = clickedItem;
            }
        }
    }

    private void SwapItems(FoodItem a, FoodItem b)
    {
        int tempX = a.x; int tempY = a.y;
        a.x = b.x; a.y = b.y;
        b.x = tempX; b.y = tempY;

        board[a.x, a.y] = a;
        board[b.x, b.y] = b;

        int indexA = a.transform.GetSiblingIndex();
        int indexB = b.transform.GetSiblingIndex();
        a.transform.SetSiblingIndex(indexB);
        b.transform.SetSiblingIndex(indexA);
    }

    private HashSet<FoodItem> FindMatches()
    {
        HashSet<FoodItem> matchedItems = new HashSet<FoodItem>();

        for (int y = 0; y < gridSize; y++)
        {
            int matchCount = 1;
            for (int x = 0; x < gridSize - 1; x++)
            {
                if (board[x, y].info.name == board[x + 1, y].info.name) matchCount++;
                else
                {
                    if (matchCount >= 3)
                    {
                        for (int i = x; i > x - matchCount; i--) matchedItems.Add(board[i, y]);
                    }
                    matchCount = 1;
                }
            }
            if (matchCount >= 3)
            {
                for (int i = gridSize - 1; i > gridSize - 1 - matchCount; i--) matchedItems.Add(board[i, y]);
            }
        }

        for (int x = 0; x < gridSize; x++)
        {
            int matchCount = 1;
            for (int y = 0; y < gridSize - 1; y++)
            {
                if (board[x, y].info.name == board[x, y + 1].info.name) matchCount++;
                else
                {
                    if (matchCount >= 3)
                    {
                        for (int i = y; i > y - matchCount; i--) matchedItems.Add(board[x, i]);
                    }
                    matchCount = 1;
                }
            }
            if (matchCount >= 3)
            {
                for (int i = gridSize - 1; i > gridSize - 1 - matchCount; i--) matchedItems.Add(board[x, i]);
            }
        }

        return matchedItems;
    }

    private void CheckAndProcessMatches()
    {
        HashSet<FoodItem> matchedItems = FindMatches();

        if (matchedItems.Count > 0)
        {
            float ratio = 1.0f;
            if (matchedItems.Count == 4) ratio = 1.2f;
            else if (matchedItems.Count >= 5) ratio = 1.4f;

            lastComboMultiplier = ratio;

            foreach (var item in matchedItems)
            {
                totalScore += Mathf.RoundToInt(item.info.scoreValue * ratio);

                List<FoodStaticInfo> pool = FoodTable.Infos.FindAll(f => f.unlockLevel <= kitchenLevel);
                item.Init(item.x, item.y, pool[Random.Range(0, pool.Count)], this);
            }

            CheckAndProcessMatches();
        }
    }

    private void UpdateHUD()
    {
        if (movesText != null)
            movesText.text = $"剩余步数: {remainingMoves}";

        if (sugarText != null)
            sugarText.text = $"当前得分: <color=#{(totalScore >= 0 ? "2ECC71" : "E74C3C")}>{totalScore}</color>";

        if (healthText != null)
            healthText.text = "";

        if (moodText != null)
        {
            if (lastComboMultiplier > 1.0f)
                moodText.text = $"上次连消: {lastComboMultiplier:F1}x";
            else
                moodText.text = "";
        }
    }

    private void OnGameFinished()
    {
        string rating;
        float multiplier;
        Color ratingColor;

        if (totalScore >= FoodTable.SCORE_S)
        {
            rating = "完美烹饪 (S)";
            multiplier = 1.5f;
            ratingColor = new Color(0.18f, 0.80f, 0.44f); // green
        }
        else if (totalScore >= FoodTable.SCORE_A)
        {
            rating = "优秀烹饪 (A)";
            multiplier = 1.0f;
            ratingColor = Color.yellow;
        }
        else if (totalScore >= FoodTable.SCORE_B)
        {
            rating = "一般烹饪 (B)";
            multiplier = 0.7f;
            ratingColor = Color.white;
        }
        else
        {
            rating = "糟糕烹饪 (C)";
            multiplier = 0.4f;
            ratingColor = Color.red;
        }

        // 预览将要应用的属性
        InteractableObject src = KitchenSceneLauncher.SourceObject;
        LevelData data = src != null ? src.GetCurrentLevelData() : new LevelData();
        float previewSugar = data.bloodSugarDelta * multiplier;
        float previewHealth = data.healthDelta * multiplier;
        float previewMood = data.moodDelta * multiplier;

        if (resultPanel != null)
            resultPanel.SetActive(true);
        if (resultContentText != null)
            resultContentText.text =
            $"<b>厨房烹饪成果</b>\n\n" +
            $"最终得分: {totalScore}\n" +
            $"评级: <color=#{ColorUtility.ToHtmlStringRGB(ratingColor)}>{rating}</color>\n" +
            $"评分倍率: {multiplier:F1}x\n\n" +
            $"<b>属性变化预览:</b>\n" +
            $"血糖: {(previewSugar >= 0 ? "+" : "")}{previewSugar:F1}\n" +
            $"健康: {(previewHealth >= 0 ? "+" : "")}{previewHealth:F1}\n" +
            $"心情: {(previewMood >= 0 ? "+" : "")}{previewMood:F1}";

        // 储存倍率供确认时使用
        KitchenGameBridge.Output_Multiplier = multiplier;
        KitchenGameBridge.Output_Score = totalScore;
        KitchenGameBridge.IsDataReady = true;
    }

    private void OnResultConfirm()
    {
        if (resultPanel != null)
            resultPanel.SetActive(false);

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayDefaultBGM();

        float multiplier = KitchenGameBridge.IsDataReady ? KitchenGameBridge.Output_Multiplier : 1.0f;

        InteractableObject src = KitchenSceneLauncher.SourceObject;
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

            src.MarkLevelCleared();
        }

        KitchenGameBridge.IsDataReady = false;
        KitchenSceneLauncher.ReturnToMain();
    }
}
