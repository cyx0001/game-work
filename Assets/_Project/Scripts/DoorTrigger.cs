using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// 门触发器 - 玩家靠近时，Tilemap 上的门 tile 消失
/// 放在门前方，需要 BoxCollider2D (isTrigger = true)
/// 
/// 设置方式：把 Door Area 的 Gizmo 线框对准门的位置即可
/// </summary>
public class DoorTrigger : MonoBehaviour
{
    [Header("目标 Tilemap")]
    public Tilemap doorTilemap;

    [Header("门的世界区域")]
    [Tooltip("门覆盖的矩形中心（世界坐标）")]
    public Vector2 doorCenter = Vector2.zero;
    [Tooltip("门的宽高（单位：世界单位，通常是 cellSize 的倍数）")]
    public Vector2 doorSize = new Vector2(2f, 2f);

    private Vector3Int[] doorTilePositions;
    private TileBase[] originalTiles;
    private bool isOpen = false;

    private void Awake()
    {
        if (doorTilemap == null)
        {
            Debug.LogError($"{name}: 请拖入 Tilemap！");
            return;
        }
        CalculateTilePositions();
        SaveOriginalTiles();
    }

    /// <summary>
    /// 从世界坐标矩形反算 Tile 位置
    /// </summary>
    private void CalculateTilePositions()
    {
        List<Vector3Int> positions = new List<Vector3Int>();

        float halfW = doorSize.x * 0.5f;
        float halfH = doorSize.y * 0.5f;

        // 用 grid cell 尺寸作为步长
        Vector3 cellSize = doorTilemap.cellSize;
        float stepX = Mathf.Max(cellSize.x, 0.1f);
        float stepY = Mathf.Max(cellSize.y, 0.1f);

        // 遍历矩形区域，收集所有格子里有 Tile 的位置
        for (float x = doorCenter.x - halfW; x < doorCenter.x + halfW; x += stepX * 0.5f)
        {
            for (float y = doorCenter.y - halfH; y < doorCenter.y + halfH; y += stepY * 0.5f)
            {
                Vector3Int cellPos = doorTilemap.WorldToCell(new Vector3(x, y, 0));
                if (doorTilemap.GetTile(cellPos) != null && !positions.Contains(cellPos))
                    positions.Add(cellPos);
            }
        }

        doorTilePositions = positions.ToArray();
        Debug.Log($"{name}: 找到 {doorTilePositions.Length} 个门 Tile");
    }

    private void SaveOriginalTiles()
    {
        originalTiles = new TileBase[doorTilePositions.Length];
        for (int i = 0; i < doorTilePositions.Length; i++)
            originalTiles[i] = doorTilemap.GetTile(doorTilePositions[i]);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isOpen && other.CompareTag("Player"))
            OpenDoor();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (isOpen && other.CompareTag("Player"))
            CloseDoor();
    }

    private void OpenDoor()
    {
        isOpen = true;
        foreach (var pos in doorTilePositions)
            doorTilemap.SetTile(pos, null);
    }

    private void CloseDoor()
    {
        isOpen = false;
        for (int i = 0; i < doorTilePositions.Length; i++)
            doorTilemap.SetTile(doorTilePositions[i], originalTiles[i]);
    }

    // ====== 编辑器可视化 ======
#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        // 门区域（黄色虚线框）
        Gizmos.color = new Color(1f, 1f, 0f, 0.3f);
        Gizmos.DrawCube(doorCenter, doorSize);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(doorCenter, doorSize);

        if (doorTilemap != null && doorTilePositions != null)
        {
            Gizmos.color = Color.red;
            foreach (var pos in doorTilePositions)
            {
                Vector3 center = doorTilemap.CellToWorld(pos) + doorTilemap.cellSize * 0.5f;
                Gizmos.DrawWireCube(center, doorTilemap.cellSize * 0.85f);
            }
        }
    }
#endif
}
