using System.Collections.Generic;

/// <summary>
/// 糖尿病科普题库（后续可继续扩展）
/// 注意：文本请使用字体图集内常见字符，避免 ～、≥、弯引号等缺字符号
/// </summary>
public static class QuizQuestionBank
{
    public static readonly List<QuizQuestion> AllQuestions = new List<QuizQuestion>
    {
        new QuizQuestion(
            "根据我国常用诊断标准，空腹静脉血浆血糖达到多少可诊断为糖尿病？",
            new[] { ">=6.1 mmol/L", ">=7.0 mmol/L", ">=5.6 mmol/L", ">=11.1 mmol/L" },
            1,
            "空腹血糖 >=7.0 mmol/L 是糖尿病的重要诊断标准之一；>=11.1 mmol/L 多用于随机血糖或 OGTT 2 小时血糖。"
        ),
        new QuizQuestion(
            "糖尿病患者出现三多一少症状时，一少通常指的是什么？",
            new[] { "尿量减少", "体重减轻", "食欲减少", "皮肤弹性减少" },
            1,
            "三多一少指多饮、多尿、多食和体重减轻，是糖尿病典型表现。"
        ),
        new QuizQuestion(
            "糖化血红蛋白(HbA1c)主要反映的是哪段时间的平均血糖水平？",
            new[] { "近 1-2 周", "近 2-3 个月", "近 6 个月", "当天血糖" },
            1,
            "HbA1c 反映近 2-3 个月的平均血糖，是评估长期控糖效果的重要指标。"
        ),
        new QuizQuestion(
            "糖尿病综合管理的五驾马车包括以下哪一组？",
            new[] { "饮食、运动、药物、监测、教育", "饮食、手术、药物、监测、教育", "饮食、运动、保健品、监测、教育", "禁食、运动、药物、监测、教育" },
            0,
            "五驾马车指饮食控制、运动治疗、药物治疗、血糖监测和糖尿病教育，需协同进行。"
        ),
        new QuizQuestion(
            "以下哪种口服降糖药主要通过抑制肝脏葡萄糖输出发挥作用？",
            new[] { "二甲双胍", "阿卡波糖", "格列齐特", "瑞格列奈" },
            0,
            "二甲双胍是 2 型糖尿病的一线用药，主要机制是减少肝糖输出、改善胰岛素敏感性。"
        ),
        new QuizQuestion(
            "糖尿病酮症酸中毒(DKA)最常见的诱因是什么？",
            new[] { "胰岛素使用过量", "感染", "饮食控制过严", "运动过度" },
            1,
            "感染是 DKA 最常见诱因，常因胰岛素相对不足导致酮体大量生成。"
        ),
        new QuizQuestion(
            "对于 2 型糖尿病患者，以下哪种运动方式更合适？",
            new[] { "高强度间歇训练", "缓和的有氧运动(如快走、游泳)", "重度负重训练", "长期完全不运动" },
            1,
            "规律、中等强度的有氧运动有助于改善血糖和心血管健康，运动方案应个体化。"
        ),
        new QuizQuestion(
            "关于胰岛素治疗，以下说法正确的是？",
            new[] { "使用胰岛素会产生依赖性，应尽量避免", "是否需长期注射取决于病情和胰岛功能", "只有 1 型糖尿病才需要胰岛素", "血糖正常后即可立即停用胰岛素" },
            1,
            "胰岛素是正常生理激素，是否长期使用取决于疾病类型和胰岛功能，并非上瘾。"
        )
    };

    /// <summary>
    /// 随机抽取指定数量的题目（不重复）
    /// </summary>
    public static List<QuizQuestion> DrawRandomQuestions(int count)
    {
        count = UnityEngine.Mathf.Clamp(count, 1, AllQuestions.Count);

        List<QuizQuestion> pool = new List<QuizQuestion>(AllQuestions);
        List<QuizQuestion> result = new List<QuizQuestion>(count);

        for (int i = 0; i < count; i++)
        {
            int index = UnityEngine.Random.Range(0, pool.Count);
            result.Add(pool[index]);
            pool.RemoveAt(index);
        }

        return result;
    }
}
