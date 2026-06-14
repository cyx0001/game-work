using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("��ƵԴ���")]
    public AudioSource bgmSource; // ���𲥷� BGM �����

    [Header("��Ƶ��Դ")]
    public AudioClip gameplayBGM; // ��ĺ�����Ϸ��������

    private void Awake()
    {
        // === ������ƣ�������Ϸֻ����Ƶ��������Ҫ�糡�������� ===
        // ���������������ս����������ʱ��BGM �Ų��Ῠ�١��жϻ��ͷ����
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // �Զ�������� AudioSource ���
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
        // ��Ϸ����ʱ���Զ����� BGM
        PlayBGM(gameplayBGM);
    }

    // ���ű������ֵķ���
    public void PlayBGM(AudioClip clip)
    {
        if (clip == null || bgmSource == null) return;

        // �����ǰ�Ѿ��ڲ������������ˣ��Ͳ�Ҫ����ز�
        if (bgmSource.clip == clip && bgmSource.isPlaying) return;

        bgmSource.clip = clip;
        bgmSource.loop = true;        // ���뿪��ѭ������
        bgmSource.volume = 0.4f;      // Ĭ�������趨Ϊ 40%����ܰ���̶���
        bgmSource.playOnAwake = false;
        bgmSource.Play();
    }

    // ֹͣ�������֣����ã��������ĳЩ�������飩
    public void PlayDefaultBGM()
    {
        PlayBGM(gameplayBGM);
    }

    public void StopBGM()
    {
        if (bgmSource != null && bgmSource.isPlaying)
        {
            bgmSource.Stop();
        }
    }
}