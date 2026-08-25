using OpenAgent.Interop;

namespace OpenAgent.Runtime;

public sealed class AgentLoader : IDisposable
{
    public event Action<CrashInfo>? OnCharacterDead;
    private AgentHost host;

    public IDaSvrCharacter? Character { get; private set; }

    public int CharacterId { get; private set; }

    public string CharacterName { get; private set; } = "";

    public bool Loaded => Character != null;

    public AgentLoader()
    {
        host = new AgentHost();
    }
    private static IEnumerable<string> SplitText(
    string text,
    int maxLength = 120)
{
    if (string.IsNullOrWhiteSpace(text))
        yield break;

    while (text.Length > maxLength)
    {
        int cut = text.LastIndexOf(' ', maxLength);

        if (cut < 40)
            cut = maxLength;

        yield return text[..cut].Trim();

        text = text[(cut)..].Trim();
    }

    if (text.Length > 0)
        yield return text;
}
public void MoveToCenter(
    int screenWidth,
    int screenHeight)
{
    if (Character == null)
        return;

    try
    {
        var size = GetSize();

        int x =
            (screenWidth - size.width) / 2;

        int y =
            (screenHeight - size.height) / 2;

        int request;

        Character.MoveTo(
            (short)x,
            (short)y,
            1000,
            out request);

        Console.WriteLine(
            $"[MOVE] Center -> {x},{y}");
    }
    catch(Exception ex)
    {
        Console.WriteLine(ex.Message);
    }
}
    //--------------------------------------------------
    // Load
    //--------------------------------------------------

    public void Load(string file)
    {
        if (!File.Exists(file))
            throw new FileNotFoundException(file);

        int request;

        host.Server.Load(
            file,
            out int id,
            out request);

        CharacterId = id;

        host.Server.GetCharacterEx(
            id,
            out IDaSvrCharacter character);

        Character = character;
        Console.ForegroundColor = ConsoleColor.Cyan;
Console.WriteLine($"LOAD THREAD = {Environment.CurrentManagedThreadId}");
Console.ResetColor();

        Character.Show(
            0,
            out request);

        Character.GetName(out string name);
        

        CharacterName = name;

    }
public void LoadRecovery(string file)
{
    try
{
    host.Dispose();
}
catch
{
}

host = new AgentHost();
    if (!File.Exists(file))
        throw new FileNotFoundException(file);

    int request;

    host.Server.Load(
        file,
        out int id,
        out request);

    CharacterId = id;

    host.Server.GetCharacterEx(
        id,
        out IDaSvrCharacter character);

    Character = character;

    Character.GetName(out string name);

    CharacterName = name;

    // نمایش سریع (بدون استفاده از ShowRecovery)
    Character.Show(
        1,
        out request);
}

    //--------------------------------------------------
    // Async Load
    //--------------------------------------------------

    public Task<Agent> LoadAsync(string file)
    {
        Load(file);

        Agent agent = new Agent(
            CharacterName,
            this);

        return Task.FromResult(agent);
    }

    //--------------------------------------------------
    // Create Agent
    //--------------------------------------------------

    public Agent CreateAgent()
    {
        if (!Loaded)
            throw new InvalidOperationException(
                "Character not loaded.");

        return new Agent(
            CharacterName,
            this);
    }
public bool IsCharacterAlive()
{
    if (Character == null)
        return false;

    try
    {

        Character.GetName(out _);
        return true;
    }
    catch (Exception ex)
    {
        Console.ForegroundColor = ConsoleColor.Red;

        Console.WriteLine();
        Console.WriteLine("========== CHARACTER DEAD ==========");
        Console.WriteLine($"Type    : {ex.GetType().FullName}");
        Console.WriteLine($"Message : {ex.Message}");
        Console.WriteLine($"HRESULT : 0x{ex.HResult:X8}");
        Console.WriteLine("StackTrace:");
        Console.WriteLine(ex.StackTrace);
        Console.WriteLine("====================================");
        Console.WriteLine();

        Console.ResetColor();

Character = null;
CharacterId = 0;
CharacterName = "";

OnCharacterDead?.Invoke(
    new CrashInfo
    {
        Reason = CrashReason.ComException,
        Exception = ex,
        Method = "Speak"
    });

return false;


    }
}
//--------------------------------------------------
// Wait Speak
//--------------------------------------------------

private async Task WaitSpeakFinished()
{
    await Task.Delay(1200);
}

//--------------------------------------------------
// Animation Detection
//--------------------------------------------------

private void PlayAnimationForText(string text)
{
    string lower = text.ToLower();

    if (lower.Contains("book") ||
        lower.Contains("read") ||
        lower.Contains("magazine"))
    {
        Play("Read");
    }
    else if (lower.Contains("think") ||
             lower.Contains("thought") ||
             lower.Contains("thoughts"))
    {
        Play("Think");
    }
    else if (lower.Contains("left"))
    {
        Play("GestureLeft");
    }
    else if (lower.Contains("right"))
    {
        Play("GestureRight");
    }
    else if (lower.Contains("congratulate") ||
             lower.Contains("congratulation"))
    {
        Play("Congratulate");
    }
    else if (lower.Contains("confuse") ||
             lower.Contains("confused"))
    {
        Play("Confused");
    }
    else if (lower.Contains("alert") ||
             lower.Contains("warning") ||
             lower.Contains("danger"))
    {
        Play("Alert");
    }
    else if (lower.Contains("process") ||
             lower.Contains("calculator"))
    {
        Play("Process");
    }
    else if (lower.Contains("idea") ||
             lower.Contains("innovation") ||
             lower.Contains("suggest"))
    {
        Play("Suggest");
    }
    else if (lower.Contains("surprise"))
    {
        Play("Surprised");
    }
else if (lower.Contains("i don't know") ||
         lower.Contains("idk") ||
         lower.Contains("don't know") ||
         lower.Contains("do not know") ||
         lower.Contains("not sure") ||
         lower.Contains("perhaps") ||
         lower.Contains("maybe"))
{
    Play("Think");
}
    else if (lower.Contains("write") ||
             lower.Contains("writing"))
    {
        Play("Writing");
    }
    else if (text.Length > 20 ||
             lower.Contains("explain"))
    {
        Play("Explain");
    }
}
public async Task SmartSpeak(string text)
{
    foreach (string part in SplitText(text))
    {
        // اول انیمیشن
        PlayAnimationForText(part);

        Console.WriteLine("[TEST] Animation started");

        // بلافاصله صحبت
        Speak(part);

        Console.WriteLine("[TEST] Speak started");

        await WaitSpeakFinished();
    }

    RestPose();
}
    //--------------------------------------------------
    // Speak
    //--------------------------------------------------

public void Speak(string text)
{
    if (Character == null)
    {
        Console.WriteLine(
            "[SPEAK BLOCKED] Character is null");

        return;
    }


    try
    {
        int request;

        Character.Speak(
            text,
            "",
            out request);

        Console.WriteLine(
            "[DEBUG] Speak OK");
    }
    catch(Exception ex)
    {
        Console.WriteLine();
        Console.WriteLine("========== SPEAK ERROR ==========");
        Console.WriteLine(ex);
        Console.WriteLine("=================================");
    }
}

    //--------------------------------------------------
    // Think
    //--------------------------------------------------

public void Think(string text)
{
    if (!IsCharacterAlive())
        return;

    Console.ForegroundColor = ConsoleColor.DarkYellow;
    Console.WriteLine($"[DEBUG] Think() : \"{text}\"");
    Console.ResetColor();

    try
    {
        int request;

        Character!.Think(
            text,
            out request);

        Console.ForegroundColor = ConsoleColor.DarkGreen;
        Console.WriteLine($"[DEBUG] Think OK   Request={request}");
        Console.ResetColor();
    }
    catch (Exception ex)
    {
        Console.ForegroundColor = ConsoleColor.Red;

        Console.WriteLine();
        Console.WriteLine("========== COM ERROR ==========");
        Console.WriteLine("Method : Think()");
        Console.WriteLine($"Character : {CharacterName}");
        Console.WriteLine(ex.GetType().FullName);
        Console.WriteLine(ex.Message);
        Console.WriteLine("===============================");
        Console.WriteLine();

        Console.ResetColor();

        Character = null;
        CharacterId = 0;
        CharacterName = "";
        OnCharacterDead?.Invoke(
    new CrashInfo
    {
        Reason = CrashReason.ComException,
        Exception = ex,
        Method = "think"
    });
    }
}

    //--------------------------------------------------
    // Animation
    //--------------------------------------------------
public void Play(string animation)
{
    if (!IsCharacterAlive())
        return;

    Console.ForegroundColor = ConsoleColor.DarkYellow;
    Console.WriteLine($"[DEBUG] Play({animation})");
    Console.ResetColor();

    try
    {
        int request;

        Character!.Play(
            animation,
            out request);

        Console.ForegroundColor = ConsoleColor.DarkGreen;
        Console.WriteLine($"[DEBUG] Play OK    Request={request}");
        Console.ResetColor();
    }
catch (Exception ex)
{
    Console.ForegroundColor = ConsoleColor.Red;

    Console.WriteLine();
    Console.WriteLine("========== COM ERROR ==========");
    Console.WriteLine($"Method : Play({animation})");
    Console.WriteLine($"Character : {CharacterName}");
    Console.WriteLine(ex.GetType().FullName);
    Console.WriteLine(ex.Message);
    Console.WriteLine("===============================");
    Console.WriteLine();

    Console.ResetColor();
    Character = null;
CharacterId = 0;
CharacterName = "";

OnCharacterDead?.Invoke(
    new CrashInfo
    {
        Reason = CrashReason.ComException,
        Exception = ex,
        Method = $"Play({animation})"
    });
}
}
    //--------------------------------------------------
    // Move
    //--------------------------------------------------
public void MoveTo(short x, short y, int speed = 100)
{
    if (Character == null)
        return;

    try
    {
        int request;

        Console.WriteLine(
            $"[DEBUG] MoveTo {x},{y}");

        Character.MoveTo(
            x,
            y,
            speed,
            out request);

        Console.WriteLine(
            $"[DEBUG] Move Request={request}");
    }
    catch(Exception ex)
    {
        Console.WriteLine(
            "[MOVE ERROR] " + ex.Message);
    }
}


public void SetPosition(int x, int y)
{
    if (Character == null)
        return;

    try
    {
        Character.SetPosition(
            x,
            y);

        Console.WriteLine(
            $"[DEBUG] SetPosition {x},{y}");
    }
    catch(Exception ex)
    {
        Console.WriteLine(
            "[POSITION ERROR] " + ex.Message);
    }
}


public (int width,int height) GetSize()
{
    if(Character == null)
        return (0,0);

    int w;
    int h;

    Character.GetSize(
        out w,
        out h);

    return (w,h);
}


public (int x,int y) GetPosition()
{
    if(Character == null)
        return (0,0);

    int x;
    int y;

    Character.GetPosition(
        out x,
        out y);

    return (x,y);
}
public void Move(short x, short y)
{
    if (!IsCharacterAlive())
        return;

    try
    {
        int request;

        Character!.MoveTo(
            x,
            y,
            1000,
            out request);
    }
catch (Exception ex)
{
    OnCharacterDead?.Invoke(
        new CrashInfo
        {
            Reason = CrashReason.ComException,
            Exception = ex,
            Method = "Move"
        });

    Console.ForegroundColor = ConsoleColor.Red;

    Console.WriteLine();
    Console.WriteLine("========== COM ERROR ==========");
    Console.WriteLine("Method : Move()");
    Console.WriteLine($"Character : {CharacterName}");
    Console.WriteLine(ex.GetType().FullName);
    Console.WriteLine(ex.Message);
    Console.WriteLine("===============================");
    Console.WriteLine();

    Console.ResetColor();

    Character = null;
    CharacterId = 0;
    CharacterName = "";
}
}

    //--------------------------------------------------
    // Show
    //--------------------------------------------------

public void Show()
{
    if (!IsCharacterAlive())
        return;

    try
    {
        int request;

        Character!.Show(
            0,
            out request);
    }
 catch (Exception ex)
{
    OnCharacterDead?.Invoke(
        new CrashInfo
        {
            Reason = CrashReason.ComException,
            Exception = ex,
            Method = "Show"
        });

    Console.ForegroundColor = ConsoleColor.Red;

    Console.WriteLine();
    Console.WriteLine("========== COM ERROR ==========");
    Console.WriteLine("Method : Show()");
    Console.WriteLine($"Character : {CharacterName}");
    Console.WriteLine(ex.GetType().FullName);
    Console.WriteLine(ex.Message);
    Console.WriteLine("===============================");
    Console.WriteLine();

    Console.ResetColor();

    Character = null;
    CharacterId = 0;
    CharacterName = "";
}
}
public void ShowRecovery()
{
    if (Character == null)
        return;

    int request;

    Character.Show(
        1,
        out request);
}
public void ForceShow()
{
    if (!Loaded)
        return;

    int request;

    Console.WriteLine("[DEBUG] ForceShow()");

    Character!.Show(
        0,
        out request);

    Console.WriteLine(
        $"[DEBUG] ForceShow OK Request={request}");
}

    //--------------------------------------------------
    // Hide
    //--------------------------------------------------

public void Hide()
{
    Console.WriteLine("[DEBUG] Hide()");
    if (!IsCharacterAlive())
        return;

    try
    {
        int request;

        Character!.Hide(
            0,
            out request);
    }
catch (Exception ex)
{
    OnCharacterDead?.Invoke(
        new CrashInfo
        {
            Reason = CrashReason.ComException,
            Exception = ex,
            Method = "Hide"
        });

    Console.ForegroundColor = ConsoleColor.Red;

    Console.WriteLine();
    Console.WriteLine("========== COM ERROR ==========");
    Console.WriteLine("Method : Hide()");
    Console.WriteLine($"Character : {CharacterName}");
    Console.WriteLine(ex.GetType().FullName);
    Console.WriteLine(ex.Message);
    Console.WriteLine("===============================");
    Console.WriteLine();

    Console.ResetColor();

    Character = null;
    CharacterId = 0;
    CharacterName = "";
}
}
    //--------------------------------------------------
    // Stop
    //--------------------------------------------------

public void Stop()
{
    if (!IsCharacterAlive())
        return;

    Console.ForegroundColor = ConsoleColor.DarkYellow;
    Console.WriteLine("[DEBUG] Stop()");
    Console.ResetColor();

    try
    {
        Character!.StopAll(
            (int)StopAllFlags.StopAll_Play |
            (int)StopAllFlags.StopAll_Speak);

        Console.ForegroundColor = ConsoleColor.DarkGreen;
        Console.WriteLine("[DEBUG] Stop OK");
        Console.ResetColor();
    }
    catch (Exception ex)
    {
        Console.ForegroundColor = ConsoleColor.Red;

        Console.WriteLine();
        Console.WriteLine("========== COM ERROR ==========");
        Console.WriteLine("Method : Stop()");
        Console.WriteLine($"Character : {CharacterName}");
        Console.WriteLine(ex.GetType().FullName);
        Console.WriteLine(ex.Message);
        Console.WriteLine("===============================");
        Console.WriteLine();

        Console.ResetColor();

        Character = null;
        CharacterId = 0;
        CharacterName = "";
        OnCharacterDead?.Invoke(
    new CrashInfo
    {
        Reason = CrashReason.ComException,
        Exception = ex,
        Method = "stop"
    });
    }
}

    //--------------------------------------------------
    // Helper Animations
    //--------------------------------------------------

    public void Greet() => Play("Greet");
    public void MoveLeft() => Play("MoveLeft");

    public void Explain() => Play("Explain");

    public void RestPose() => Play("RestPose");

    public void Congratulate() => Play("Congratulate");

    public void Sad() => Play("Sad");

    public void Alert() => Play("Alert");

    //--------------------------------------------------
    // Unload
    //--------------------------------------------------

public void Unload()
{
    if (!Loaded)
        return;

    try
    {
        host.Server.Unload(CharacterId);
    }
    catch
    {
    }

    Character = null;
    CharacterId = 0;
    CharacterName = "";
}
    //--------------------------------------------------
    // Dispose
    //--------------------------------------------------

public void Dispose()
{
    try
    {
        Unload();
    }
    catch
    {
    }

    try
    {
        host.Dispose();
    }
    catch
    {
    }
}
}