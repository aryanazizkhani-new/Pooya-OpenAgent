namespace OpenAgent.Runtime;

public sealed class Agent
{
    private readonly AgentLoader loader;

    public string Name { get; }

    internal Agent(string name, AgentLoader loader)
    {
        Name = name;
        this.loader = loader;
    }

    //--------------------------------------------------
    // Visibility
    //--------------------------------------------------

    public void Show()
    {
        loader.Show();
    }
    public void ForceShow()
{
    loader.ForceShow();
}

    public void Hide()
    {
        loader.Hide();
    }

    //--------------------------------------------------
    // Speech
    //--------------------------------------------------

    public void Say(string text)
    {
        loader.Speak(text);
    }

    public void Speak(string text)
    {
        loader.Speak(text);
    }

    public void Think(string text)
    {
        loader.Think(text);
    }

    //--------------------------------------------------
    // Animation
    //--------------------------------------------------

    public void Play(string animation)
    {
        loader.Play(animation);
    }

    public void Greet()
    {
        loader.Greet();
    }

    public void Explain()
    {
        loader.Explain();
    }

    public void RestPose()
    {
        loader.RestPose();
    }

    public void Congratulate()
    {
        loader.Congratulate();
    }

    public void Sad()
    {
        loader.Sad();
    }

    public void Alert()
    {
        loader.Alert();
    }

    //--------------------------------------------------
    // Move
    //--------------------------------------------------

    public void Move(short x, short y)
    {
        loader.Move(x, y);
    }

    //--------------------------------------------------
    // Stop
    //--------------------------------------------------

    public void Stop()
    {
        loader.Stop();
    }

    //--------------------------------------------------
    // Close
    //--------------------------------------------------

    public void Close()
    {
        loader.Hide();
    }

    //--------------------------------------------------
    // ToString
    //--------------------------------------------------

    public override string ToString()
    {
        return Name;
    }
}