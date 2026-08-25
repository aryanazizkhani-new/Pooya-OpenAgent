namespace OpenAgent.Runtime;

public static class AnimationEngine
{
    public static List<string> Analyze(string text)
    {
        List<string> list = new();

        string t = text.ToLower();

        if (t.Contains("book"))
            list.Add("Read");

        if (t.Contains("think"))
            list.Add("Think");

        if (t.Contains("left"))
            list.Add("GestureLeft");

        if (t.Contains("right"))
            list.Add("LookRight");

        if (t.Contains("happy"))
            list.Add("Congratulate");

        if (t.Contains("sad"))
            list.Add("Sad");

        if (t.Contains("warning"))
            list.Add("Alert");

        if (list.Count == 0 && text.Length > 20)
            list.Add("Explain");

        return list;
    }
}