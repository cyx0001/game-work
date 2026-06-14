using System;

/// <summary>
/// 单道选择题数据结构
/// </summary>
[Serializable]
public class QuizQuestion
{
    public string question;
    public string[] options = new string[4];
    public int correctIndex;
    public string explanation;

    public QuizQuestion(string question, string[] options, int correctIndex, string explanation)
    {
        this.question = question;
        this.options = options;
        this.correctIndex = correctIndex;
        this.explanation = explanation;
    }
}
