using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractableObject : MonoBehaviour
{
    [Header("�������ʲ�")]
    public ObjectData objectData; // ����ղŴ����� ScriptableObject

    [Header("С��Ϸģʽ")]
    public bool launchTreadmillMinigame = false; // �Ƿ�������С��Ϸ

    [Header("��ǰ�ȼ�")]
    public int currentLevel = 1;  // ��Ϸ��Ĭ��Ϊ 1 ��

    // ��ȡ��ǰ�ȼ���Ӧ����ֵ����
    public LevelData GetCurrentLevelData()
    {
        if (objectData == null || objectData.levels.Length < currentLevel)
        {
            Debug.LogError($"{gameObject.name} ȱ���������û�ȼ�������Χ��");
            return new LevelData();
        }
        // ���������� 0 ��ʼ������ level 1 ��Ӧ levels[0]
        return objectData.levels[currentLevel - 1];
    }

    // ��ҵ��ȷ��ִ�и��ж�ʱ����
    public void ExecuteAction()
    {
        // 0. ���������С��Ϸģʽ→ �첽����С��Ϸ����
        if (launchTreadmillMinigame)
        {
            TreadmillSceneLauncher.Launch(this);
            return;
        }

        // 1. ��鲢���Կ۳� 1 �� AP
        if (GameManager.Instance != null && GameManager.Instance.UseAP(1))
        {
            // 2. ��ȡ��ǰ�ȼ���Ӧ����ֵ
            LevelData data = GetCurrentLevelData();

            // 3. �����޸���ҵ�����״̬
            if (PlayerDataManager.Instance != null)
            {
                PlayerDataManager.Instance.ModifyStats(
                    data.bloodSugarDelta,
                    data.healthDelta,
                    data.moodDelta,
                    data.moneyDelta
                );
            }

            // 4. ������΢�ĵ�������������Ŵ�����С��
            StartCoroutine(ClickFeedbackAnimation());
        }
    }

    // һ���򵥵ĵ�����Ŷ�������
    private System.Collections.IEnumerator ClickFeedbackAnimation()
    {
        Vector3 originalScale = transform.localScale;
        transform.localScale = originalScale * 1.05f; // ��΢�Ŵ�
        yield return new WaitForSeconds(0.1f);
        transform.localScale = originalScale;         // �ָ�ԭ��
    }
}
