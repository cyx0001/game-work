using UnityEngine;
using UnityEngine.UI;

public class SimpleGifPlayer : MonoBehaviour
{
    [Header("=== GIF 序列帧配置 ===")]
    public Sprite[] gifFrames;

    [Header("=== 帧率速度调节 ===")]
    public float baseFramesPerSecond = 16f;

    [Tooltip("参考的基础下落速度（用于按比例缩放播放速度）")]
    public float referenceMoveSpeed = 500f;

    private Image uiImage;
    private int currentFrameIndex;
    private float timer;
    
    // 缓存主控制器的引用，避免每次 Update 都去找
    private TreadmillSceneController sceneController;

    void Start()
    {
        uiImage = GetComponent<Image>();
        
        // 自动去场景中寻找主控制器
        sceneController = Object.FindAnyObjectByType<TreadmillSceneController>();

        // 【防呆检查 1】：检查物体上到底有没有 Image 组件
        if (uiImage == null)
        {
            Debug.LogError($"【错误】{gameObject.name} 物体上缺少 'Image' 组件！SimpleGifPlayer 无法运行。");
            enabled = false;
            return;
        }

        // 【防呆检查 2】：检查有没有拖图片进去
        if (gifFrames == null || gifFrames.Length == 0)
        {
            Debug.LogError($"【错误】{gameObject.name} 的 'Gif Frames' 数组是空的！请拖入 1-16 张序列帧图片。");
            enabled = false;
            return;
        }
    }

    void Update()
    {
        if (uiImage == null || gifFrames == null || gifFrames.Length == 0) return;

        // --- 核心逻辑：根据主控制器的实时下落速度，动态计算当前的播放帧率 ---
        float currentFps = baseFramesPerSecond;
        
        if (sceneController != null)
        {
            // 实时获取主控制器上的 noteMoveSpeed
            float currentMoveSpeed = sceneController.noteMoveSpeed;
            
            // 计算速度比例。例如：当前速度 1000 / 基础速度 500 = 2倍速
            float speedRatio = currentMoveSpeed / referenceMoveSpeed * 1.5f;
            
            // 最终播放帧率 = 基础帧率 * 速度比例
            currentFps = baseFramesPerSecond * speedRatio;
            
            // 限制一个最大最小帧率，防止极端情况下画面闪瞎眼或完全静止
            currentFps = Mathf.Clamp(currentFps, 5f, 60f);
        }

        timer += Time.deltaTime;

        // 根据动态计算出的 currentFps 来控制切图频率
        if (timer >= 1f / currentFps)
        {
            timer = 0f;
            currentFrameIndex = (currentFrameIndex + 1) % gifFrames.Length; // 循环索引
            
            if (gifFrames[currentFrameIndex] != null)
            {
                uiImage.sprite = gifFrames[currentFrameIndex];
            }
        }
    }
}