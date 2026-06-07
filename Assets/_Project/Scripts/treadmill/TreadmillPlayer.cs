using UnityEngine;

/// <summary>
/// 跑步机迷你游戏——玩家控制器
/// 处理 A/D 换道、Space/Ctrl 加速减速
/// </summary>
public class TreadmillPlayer : MonoBehaviour
{
    private int currentLane = 1;
    private float targetX;

    public TreadmillGameManager gameManager;

    void Start()
    {
        if (gameManager == null) gameManager = FindObjectOfType<TreadmillGameManager>();
        if (gameManager != null)
        {
            currentLane = gameManager.currentLane;
            targetX = gameManager.laneX[currentLane];
        }
    }

    void Update()
    {
        if (gameManager == null) return;
        if (gameManager.state != TreadmillState.Playing) return;

        // 换道输入
        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
            MoveToLane(currentLane - 1);
        if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
            MoveToLane(currentLane + 1);

        // 平滑移动
        if (gameManager.playerIcon != null)
        {
            Vector2 pos = gameManager.playerIcon.anchoredPosition;
            pos.x = Mathf.Lerp(pos.x, targetX, 12f * Time.deltaTime);
            gameManager.playerIcon.anchoredPosition = pos;

            // 更新游戏管理器的当前赛道
            gameManager.currentLane = currentLane;
        }
    }

    public void MoveToLane(int lane)
    {
        currentLane = Mathf.Clamp(lane, 0, 2);
        targetX = gameManager.laneX[currentLane];
    }

    public void ResetToCenter()
    {
        currentLane = 1;
        targetX = gameManager.laneX[1];
        if (gameManager != null && gameManager.playerIcon != null)
            gameManager.playerIcon.anchoredPosition = new Vector2(targetX, gameManager.playerY);
    }
}
