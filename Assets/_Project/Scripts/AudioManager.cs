using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("音频源组件")]
    public AudioSource bgmSource; // 负责播放 BGM 的组件

    [Header("音频资源")]
    public AudioClip gameplayBGM; // 你的核心游戏背景音乐

    private void Awake()
    {
        // === 核心设计：整个游戏只有音频管理器需要跨场景不销毁 ===
        // 这样点击“重新挑战”重启场景时，BGM 才不会卡顿、切断或从头播放
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // 自动补齐或检查 AudioSource 组件
            if (bgmSource == null)
            {
                bgmSource = gameObject.GetComponent<AudioSource>();
                if (bgmSource == null)
                {
                    bgmSource = gameObject.AddComponent<AudioSource>();
                }
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // 游戏启动时，自动播放 BGM
        PlayBGM(gameplayBGM);
    }

    // 播放背景音乐的方法
    public void PlayBGM(AudioClip clip)
    {
        if (clip == null || bgmSource == null) return;

        // 如果当前已经在播放这首音乐了，就不要打断重播
        if (bgmSource.clip == clip && bgmSource.isPlaying) return;

        bgmSource.clip = clip;
        bgmSource.loop = true;        // 必须开启循环播放
        bgmSource.volume = 0.4f;      // 默认音量设定为 40%（温馨不刺耳）
        bgmSource.playOnAwake = false;
        bgmSource.Play();
    }

    // 停止背景音乐（备用，比如进入某些特殊大剧情）
    public void StopBGM()
    {
        if (bgmSource != null && bgmSource.isPlaying)
        {
            bgmSource.Stop();
        }
    }
}