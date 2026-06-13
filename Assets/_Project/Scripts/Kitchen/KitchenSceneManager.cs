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
    
    // === 核心修改：在这里直接在物理面板公开一个图片库 ===
    [Header("食物美术图片库 (把你自定义目录的图片拖到这里)")]
    public List<Sprite> foodSprites = new List<Sprite>();

    [Header("HUD 看板面板")]
    public TextMeshProUGUI movesText;
    public TextMeshProUGUI sugarText;
    public TextMeshProUGUI healthText;
    public TextMeshProUGUI moodText;

    [Header("游戏结算面板组件")]
    public GameObject resultPanel;
    public TextMeshProUGUI resultContentText;
    public Button btnConfirm;

    private int kitchenLevel = 1;
    private int gridSize = 5;
    private int remainingMoves = 10;
    
    private float curSugar = 0f;
    private float curHealth = 0f;
    private float curMood = 0f;

    private FoodItem[,] board;
    private FoodItem firstSelected = null;

    void Start()
    {
        kitchenLevel = KitchenGameBridge.Input_KitchenLevel;
        
        remainingMoves = 10;
        curSugar = 0f; curHealth = 0f; curMood = 0f;
        
        resultPanel.SetActive(false);
        btnConfirm.onClick.AddListener(ExitAndReturnToMainScene);

        SetupGridByLevel();
        GenerateBoard();
        UpdateHUD();
    }

    private void SetupGridByLevel()
    {
        if (kitchenLevel == 1)
        {
            gridSize = 5;
            gridLayoutGroup.cellSize = new Vector2(140, 140);
            gridLayoutGroup.spacing = new Vector2(10, 10);
        }
        else if (kitchenLevel == 2)
        {
            gridSize = 6;
            gridLayoutGroup.cellSize = new Vector2(116, 116);
            gridLayoutGroup.spacing = new Vector2(8, 8);
        }
        else
        {
            gridSize = 7;
            gridLayoutGroup.cellSize = new Vector2(100, 100);
            gridLayoutGroup.spacing = new Vector2(6, 6);
        }
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

    // 提供给 FoodItem 调用，用来安全获取图片库里的图片
    public Sprite GetSpriteByIndex(int index)
    {
        if (index >= 0 && index < foodSprites.Count)
        {
            return foodSprites[index];
        }
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

    private void CheckAndProcessMatches()
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

        if (matchedItems.Count > 0)
        {
            float ratio = 1.0f;
            if (matchedItems.Count == 4) ratio = 1.5f;
            else if (matchedItems.Count >= 5) ratio = 2.0f;

            foreach (var item in matchedItems)
            {
                curSugar += item.info.sugarDelta * ratio;
                curHealth += item.info.healthDelta * ratio;
                curMood += item.info.moodDelta * ratio;

                List<FoodStaticInfo> pool = FoodTable.Infos.FindAll(f => f.unlockLevel <= kitchenLevel);
                item.Init(item.x, item.y, pool[Random.Range(0, pool.Count)], this);
            }
            
            CheckAndProcessMatches();
        }
    }

    private void UpdateHUD()
    {
        movesText.text = $"剩余步数: {remainingMoves}";
        sugarText.text = $"累计血糖: {(curSugar >= 0 ? "+" : "")}{curSugar:F1}";
        healthText.text = $"累计健康: {(curHealth >= 0 ? "+" : "")}{curHealth:F1}";
        moodText.text = $"累计心情: {(curMood >= 0 ? "+" : "")}{curMood:F1}";
    }

    private void OnGameFinished()
    {
        resultPanel.SetActive(true);
        resultContentText.text = $"<b>厨房烹饪成果：</b>\n\n" +
                                 $"最终血糖：{curSugar:F1}\n" +
                                 $"最终健康：{curHealth:F1}\n" +
                                 $"最终心情：{curMood:F1}";
        
        KitchenGameBridge.Output_SugarDelta = curSugar;
        KitchenGameBridge.Output_HealthDelta = curHealth;
        KitchenGameBridge.Output_MoodDelta = curMood;
        KitchenGameBridge.IsDataReady = true;
    }

    private void ExitAndReturnToMainScene()
    {
        SceneManager.LoadScene("_Scene_0"); 
    }
}