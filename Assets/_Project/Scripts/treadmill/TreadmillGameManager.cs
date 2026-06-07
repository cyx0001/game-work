using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 跑步机迷你游戏——《履带极限狂飙》 主控制器
/// 状态机: Idle -> Config -> Playing -> Result -> Idle
/// 注意：此脚本存在于独立场景 TreadmillScene 中，通过 TreadmillSceneLauncher 桥接主场景
/// </summary>
public enum TreadmillState { Idle, Config, Playing, Result }
public enum TreadmillMode { Warmup = 0, Aerobic = 1, Sprint = 2 }
public enum ItemType { Cola, Fries, Burger, Water, Coffee }

public class TreadmillGameManager : MonoBehaviour
{
    // ==================== 模式配置 ====================
    [System.Serializable]
    public class ModeConfig
    {
        public string modeName = "热身慢跑 (25min)";
        [TextArea] public string description = "垃圾食品极少，心率安全";
        public int apCost = 1;
        public float gameDuration = 25f;
        public float baseSpeed = 5f;
        public float maxSpeed = 10f;
        public float spawnInterval = 1.8f;
        public float itemSpeed = 3f;
        public float calorieMultiplier = 0.7f;
    }

    [Header("运动模式配置")]
    public ModeConfig[] modes = new ModeConfig[3];

    // ==================== 物品类型配置 ====================
    [System.Serializable]
    public class ItemTypeConfig
    {
        public ItemType type;
        public string displayName = "可乐";
        public int caloriePenalty = 50;
        public Color color = Color.white;
        public float widthMultiplier = 1f;
        public string specialEffect = ""; // slow / coffee / water
    }

    [Header("物品类型配置")]
    public ItemTypeConfig[] itemConfigs = new ItemTypeConfig[5];

    // ==================== 赛道参数 ====================
    [Header("赛道布局（相对 playArea 的 anchoredPosition）")]
    public float[] laneX = { -150f, 0f, 150f };
    public float spawnY = 300f;
    public float playerY = -220f;
    public float destroyY = -320f;
    public float collisionRadius = 40f;

    // ==================== 心率参数 ====================
    [Header("心率参数")]
    public float initialHR = 80f;
    public float spaceHRRise = 20f;
    public float ctrlHRDrop = 30f;
    public float inertiaHRRise = 3f;
    public float hrMin = 70f;
    public float hrMax = 250f;
    public float hrDriftTarget = 90f;
    public float hrWobbleAmp = 2f;

    [Header("危险边界")]
    public float maxSafeHR = 170f;
    public float criticalHR = 210f;
    public float criticalDuration = 2.5f;

    // ==================== 卡路里 ====================
    [Header("卡路里")]
    public float distancePerCalMeter = 10f;
    public int caloriesPerSegment = -25;

    // ==================== 运行时状态 ====================
    [HideInInspector] public TreadmillState state;
    [HideInInspector] public TreadmillMode currentMode;
    [HideInInspector] public float heartRate;
    [HideInInspector] public float distance;
    [HideInInspector] public int calories;
    [HideInInspector] public float timeRemaining;
    [HideInInspector] public float currentSpeed;
    [HideInInspector] public int currentLane = 1;

    // ==================== 组件引用 ====================
    [Header("组件引用")]
    public TreadmillPlayer player;
    public TreadmillUI ui;
    public RectTransform playArea;
    public RectTransform playerIcon;

    // ==================== 内部状态 ====================
    private float spawnTimer;
    private List<TreadmillItem> activeItems = new List<TreadmillItem>();
    private float coffeeTimer;
    private float slowTimer;
    private float hrDangerTimer;
    private float timeSinceLastCalorie;
    private float hrPhase;

    // ==================== 初始化 ====================
    void Awake()
    {
        if (itemConfigs == null || itemConfigs.Length == 0) InitDefaultItemConfigs();
        if (modes == null || modes.Length == 0) InitDefaultModeConfigs();
    }

    void Start()
    {
        // 从 Launcher 获取跑步机等级，调整难度
        int treadmillLevel = TreadmillSceneLauncher.TreadmillLevel;
        ApplyLevelScaling(treadmillLevel);

        state = TreadmillState.Config;
        ui?.ShowConfigPanel();
    }

    /// <summary>根据跑步机等级调整游戏参数</summary>
    private void ApplyLevelScaling(int level)
    {
        // 等级越高，物品移动越慢（更好躲），绿区（spawn间隔）更友好
        float scale = 1f - (level - 1) * 0.15f;
        for (int i = 0; i < modes.Length; i++)
        {
            modes[i].itemSpeed *= scale;
            modes[i].spawnInterval *= (1f / scale); // 间隔更大=物品更少
        }
        Debug.Log($"[跑步机] Lv.{level} 难度缩放: {scale:F2}");
    }

    private void InitDefaultModeConfigs()
    {
        modes = new ModeConfig[3];
        modes[0] = new ModeConfig { modeName = "热身慢跑 (25min)", description = "垃圾食品极少，心率安全", apCost = 1, gameDuration = 25f, baseSpeed = 5f, maxSpeed = 10f, spawnInterval = 1.8f, itemSpeed = 3f, calorieMultiplier = 0.7f };
        modes[1] = new ModeConfig { modeName = "有氧燃脂 (25min)", description = "标准难度，零食密度中等", apCost = 2, gameDuration = 25f, baseSpeed = 8f, maxSpeed = 16f, spawnInterval = 1.2f, itemSpeed = 5f, calorieMultiplier = 1.0f };
        modes[2] = new ModeConfig { modeName = "极限冲刺 (25min)", description = "零食铺天盖地，卡路里效率双倍", apCost = 3, gameDuration = 25f, baseSpeed = 12f, maxSpeed = 22f, spawnInterval = 0.8f, itemSpeed = 7f, calorieMultiplier = 1.8f };
    }

    private void InitDefaultItemConfigs()
    {
        itemConfigs = new ItemTypeConfig[5];
        itemConfigs[0] = new ItemTypeConfig { type = ItemType.Cola,  displayName = "可乐", caloriePenalty = 50,  color = new Color(1f, 0.2f, 0.2f), widthMultiplier = 1f,   specialEffect = "" };
        itemConfigs[1] = new ItemTypeConfig { type = ItemType.Fries, displayName = "薯条", caloriePenalty = 100, color = new Color(1f, 0.8f, 0f),   widthMultiplier = 1f,   specialEffect = "slow" };
        itemConfigs[2] = new ItemTypeConfig { type = ItemType.Burger,displayName = "汉堡", caloriePenalty = 250, color = new Color(0.54f, 0.27f, 0.07f), widthMultiplier = 1.5f, specialEffect = "" };
        itemConfigs[3] = new ItemTypeConfig { type = ItemType.Water, displayName = "矿泉水", caloriePenalty = 0,   color = new Color(0.27f, 0.53f, 1f), widthMultiplier = 1f,   specialEffect = "water" };
        itemConfigs[4] = new ItemTypeConfig { type = ItemType.Coffee,displayName = "咖啡", caloriePenalty = 0,   color = new Color(0.29f, 0.17f, 0.16f), widthMultiplier = 1f,   specialEffect = "coffee" };
    }

    // ==================== 游戏流程 ====================
    public void StartGame(TreadmillMode mode)
    {
        currentMode = mode;
        ModeConfig cfg = modes[(int)mode];

        // 扣AP（主场景的 GameManager 还存在，可以直接访问）
        if (GameManager.Instance != null && !GameManager.Instance.UseAP(cfg.apCost))
        {
            // AP不足，直接返回主场景
            ReturnToMainScene();
            return;
        }

        // 重置所有运行时状态
        heartRate = initialHR;
        distance = 0f;
        calories = 0;
        timeRemaining = cfg.gameDuration;
        currentSpeed = cfg.baseSpeed;
        currentLane = 1;
        spawnTimer = 0f;
        coffeeTimer = 0f;
        slowTimer = 0f;
        hrDangerTimer = 0f;
        timeSinceLastCalorie = 0f;
        hrPhase = 0f;

        ClearAllItems();
        if (player != null) player.ResetToCenter();
        if (playerIcon != null)
            playerIcon.anchoredPosition = new Vector2(laneX[1], playerY);

        state = TreadmillState.Playing;
        ui?.ShowHUD();

        Debug.Log($"[跑步机] 开始 {cfg.modeName}，{cfg.gameDuration}s，基础速度 {cfg.baseSpeed}m/s");
    }

    void Update()
    {
        if (state == TreadmillState.Playing)
            UpdatePlaying();
    }

    // ==================== 游戏主循环 ====================
    private void UpdatePlaying()
    {
        ModeConfig cfg = modes[(int)currentMode];
        float dt = Time.deltaTime;

        bool spaceHeld = Input.GetKey(KeyCode.Space);
        bool ctrlHeld = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);

        // 速度
        float targetSpeed;
        if (spaceHeld) targetSpeed = cfg.maxSpeed;
        else if (ctrlHeld) targetSpeed = 2f;
        else targetSpeed = cfg.baseSpeed;

        if (slowTimer > 0f) targetSpeed *= 0.5f;
        currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, 3f * dt);

        // 物品速度随 Space/Ctrl 变化（视觉反馈）
        float itemSpeedMult;
        if (spaceHeld) itemSpeedMult = 2f;
        else if (ctrlHeld) itemSpeedMult = 0.3f;
        else itemSpeedMult = 1f;
        if (slowTimer > 0f) itemSpeedMult *= 0.5f;
        float baseItemSpeed = cfg.itemSpeed;
        foreach (var item in activeItems)
        {
            if (item != null)
                item.speed = baseItemSpeed * itemSpeedMult;
        }

        // 心率
        hrPhase += dt;
        if (spaceHeld)
            heartRate += spaceHRRise * dt;
        else if (ctrlHeld)
            heartRate -= ctrlHRDrop * dt;
        else
        {
            if (heartRate > 140f)
                heartRate += inertiaHRRise * dt;
            else
            {
                float drift = (hrDriftTarget - heartRate) * 0.5f * dt;
                float wobble = Mathf.Sin(hrPhase * 3f) * hrWobbleAmp * dt;
                heartRate += drift + wobble;
            }
        }
        heartRate = Mathf.Clamp(heartRate, hrMin, hrMax);

        // 距离
        float speedMS = currentSpeed * (coffeeTimer > 0f ? 1.5f : 1f);
        distance += speedMS * dt;

        // 卡路里（基于距离 × 模式效率）
        timeSinceLastCalorie += speedMS * dt;
        if (timeSinceLastCalorie >= distancePerCalMeter)
        {
            int reduction = Mathf.RoundToInt(caloriesPerSegment * cfg.calorieMultiplier);
            if (spaceHeld) reduction = Mathf.RoundToInt(reduction * 2f);
            if (coffeeTimer > 0f) reduction = Mathf.RoundToInt(reduction * 2f);
            calories += reduction;
            timeSinceLastCalorie -= distancePerCalMeter;
        }

        // 计时
        timeRemaining -= dt;

        // 物品生成
        spawnTimer -= dt;
        if (spawnTimer <= 0f)
        {
            SpawnRandomItem();
            spawnTimer = cfg.spawnInterval;
        }

        UpdateItems();

        if (slowTimer > 0f) slowTimer -= dt;
        if (coffeeTimer > 0f) coffeeTimer -= dt;

        // 终极命运
        if (heartRate >= criticalHR)
        {
            hrDangerTimer += dt;
            if (hrDangerTimer >= criticalDuration)
            {
                CheckUltimateFate();
                return;
            }
        }
        else hrDangerTimer = 0f;

        // 结束
        if (timeRemaining <= 0f)
        {
            EndGame(false);
            return;
        }

        ui?.UpdateHUD(this);
    }

    // ==================== 物品系统 ====================
    private void SpawnRandomItem()
    {
        float roll = Random.value;
        ItemType type;
        if (roll < 0.35f) type = ItemType.Cola;
        else if (roll < 0.55f) type = ItemType.Fries;
        else if (roll < 0.70f) type = ItemType.Burger;
        else if (roll < 0.85f) type = ItemType.Water;
        else type = ItemType.Coffee;

        int lane = Random.Range(0, 3);
        CreateItem(type, lane);
    }

    private GameObject CreateItem(ItemType type, int lane)
    {
        ItemTypeConfig cfg = GetItemConfig(type);
        if (cfg == null) return null;

        GameObject go = new GameObject($"Item_{cfg.displayName}_{lane}");
        go.transform.SetParent(playArea, false);

        UnityEngine.UI.Image img = go.AddComponent<UnityEngine.UI.Image>();
        img.color = cfg.color;

        RectTransform rt = go.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(50f * cfg.widthMultiplier, 50f);
        rt.anchoredPosition = new Vector2(laneX[lane], spawnY);

        TreadmillItem itemComp = go.AddComponent<TreadmillItem>();
        itemComp.type = type;
        itemComp.lane = lane;
        itemComp.speed = modes[(int)currentMode].itemSpeed;
        itemComp.widthMultiplier = cfg.widthMultiplier;

        activeItems.Add(itemComp);
        return go;
    }

    public ItemTypeConfig GetItemConfig(ItemType type)
    {
        foreach (var c in itemConfigs) if (c.type == type) return c;
        return null;
    }

    private void UpdateItems()
    {
        for (int i = activeItems.Count - 1; i >= 0; i--)
        {
            TreadmillItem item = activeItems[i];
            if (item == null) { activeItems.RemoveAt(i); continue; }

            item.MoveDown();

            if (!item.collected)
            {
                float itemY = item.GetY();
                if (Mathf.Abs(itemY - playerY) < collisionRadius && CheckLaneCollision(item))
                {
                    OnPlayerHitItem(item);
                    item.collected = true;
                    Destroy(item.gameObject);
                    activeItems.RemoveAt(i);
                    continue;
                }
            }

            if (item.GetY() < destroyY)
            {
                Destroy(item.gameObject);
                activeItems.RemoveAt(i);
            }
        }
    }

    private void ClearAllItems()
    {
        foreach (var item in activeItems)
            if (item != null && item.gameObject != null)
                Destroy(item.gameObject);
        activeItems.Clear();
    }

    private bool CheckLaneCollision(TreadmillItem item)
    {
        if (item.widthMultiplier > 1f)
        {
            for (int offset = -1; offset <= 1; offset++)
            {
                int checkLane = item.lane + offset;
                if (checkLane >= 0 && checkLane < 3 && checkLane == currentLane)
                    return true;
            }
            return false;
        }
        return item.lane == currentLane;
    }

    // ==================== 碰撞处理 ====================
    private void OnPlayerHitItem(TreadmillItem item)
    {
        ItemTypeConfig cfg = GetItemConfig(item.type);
        if (cfg == null) return;

        switch (cfg.specialEffect)
        {
            case "water": heartRate = Mathf.Max(hrMin, heartRate - 40f); break;
            case "coffee": coffeeTimer = 2f; break;
            case "slow": slowTimer = 1f; break;
        }

        calories += cfg.caloriePenalty;
        Debug.Log($"[跑步机] 撞到 {cfg.displayName} +{cfg.caloriePenalty}cal");
    }

    // ==================== 终极命运 ====================
    private void CheckUltimateFate()
    {
        float health = PlayerDataManager.Instance != null ? PlayerDataManager.Instance.health : 50f;
        if (Random.value < Mathf.Clamp01(health / 100f))
        {
            Debug.Log("[跑步机] ★ 打破极限！");
            EndGame(true);
        }
        else
        {
            Debug.Log("[跑步机] 💀 心肌梗塞！");
            state = TreadmillState.Idle;
            ui?.ShowHeartAttack();
        }
    }

    // ==================== 结算 ====================
    private void EndGame(bool perfect)
    {
        state = TreadmillState.Result;
        if (perfect)
        {
            ui?.ShowResult("S", "完美！打破极限！", calories, distance);
            return;
        }

        string grade, gradeName;
        if (calories < -600) { grade = "S"; gradeName = "控糖超人"; }
        else if (calories < 0) { grade = "A"; gradeName = "健康达人"; }
        else if (calories < 300) { grade = "B"; gradeName = "带伤上阵"; }
        else { grade = "C"; gradeName = "无效运动"; }

        ui?.ShowResult(grade, gradeName, calories, distance);
    }

    // ==================== 返回主场景 ====================
    public void ConfirmResult()
    {
        int cal = calories;
        if (cal < -600) PlayerDataManager.Instance?.ModifyStats(-25f, 0f, 5f, 0);
        else if (cal < 0) PlayerDataManager.Instance?.ModifyStats(-15f, 0f, 2f, 0);
        else if (cal < 300) PlayerDataManager.Instance?.ModifyStats(-5f, 5f, 0f, 0);
        else PlayerDataManager.Instance?.ModifyStats(10f, 0f, -2f, 0);

        ReturnToMainScene();
    }

    public void CancelMinigame()
    {
        ReturnToMainScene();
    }

    public void HeartAttackReturn()
    {
        PlayerDataManager.Instance?.ModifyStats(50f, -999f, -50f, 0);
        ReturnToMainScene();
    }

    private void ReturnToMainScene()
    {
        ClearAllItems();
        state = TreadmillState.Idle;
        TreadmillSceneLauncher.ReturnToMain();
    }
}
