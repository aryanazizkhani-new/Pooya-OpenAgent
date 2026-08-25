using System.Diagnostics;

namespace OpenAgent.Runtime;

public sealed class WhisperStream : IDisposable
{
    //--------------------------------------------------
    // Fields
    //--------------------------------------------------

    private readonly object voiceLock = new();

    private Process? ffmpegProcess;

    private bool recording;

    private string ffmpegExe = "";

    private string whisperExe = "";

    private string whisperModel = "";

    private string microphoneName = "";

    private readonly string outputFile =
        Path.Combine(
            Path.GetTempPath(),
            "openagent_input.wav");

    //--------------------------------------------------
    // Event
    //--------------------------------------------------

    public event Action<string>? OnText;

    //--------------------------------------------------
    // Property
    //--------------------------------------------------

    public bool IsRunning
    {
        get
        {
            return recording;
        }
    }

    //--------------------------------------------------
    // Constructor
    //--------------------------------------------------

    public WhisperStream()
    {
        recording = false;
    }
    //--------------------------------------------------
// Auto Search
//--------------------------------------------------

private void EnsureTools()
{
    if (string.IsNullOrWhiteSpace(ffmpegExe))
    {
        ffmpegExe = FindFFmpeg()
            ?? throw new FileNotFoundException(
                "FFmpeg not found.");
    }

    if (string.IsNullOrWhiteSpace(whisperExe))
    {
        whisperExe = FindWhisper()
            ?? throw new FileNotFoundException(
                "whisper-cli.exe not found.");
    }

    if (string.IsNullOrWhiteSpace(whisperModel))
    {
        whisperModel = FindModel()
            ?? throw new FileNotFoundException(
                "Whisper model (*.bin) not found.");
    }
}

private static string? FindFFmpeg()
{
    string[] paths =
    {
        Path.Combine(
            AppContext.BaseDirectory,
            "ffmpeg",
            "ffmpeg.exe"),

        Path.Combine(
            AppContext.BaseDirectory,
            "whisper",
            "ffmpeg.exe"),

        Path.Combine(
            AppContext.BaseDirectory,
            "bin",
            "ffmpeg.exe")
    };

    foreach (string file in paths)
    {
        if (File.Exists(file))
            return file;
    }

    return null;
}

private static string? FindWhisper()
{
    string[] paths =
    {
        Path.Combine(
            AppContext.BaseDirectory,
            "whisper",
            "whisper-cli.exe"),

        Path.Combine(
            AppContext.BaseDirectory,
            "whisper-cli.exe")
    };

    foreach (string file in paths)
    {
        if (File.Exists(file))
            return file;
    }

    return null;
}

private static string? FindModel()
{
    string[] folders =
    {
        Path.Combine(
            AppContext.BaseDirectory,
            "models"),

        Path.Combine(
            AppContext.BaseDirectory,
            "whisper"),

        AppContext.BaseDirectory
    };

    foreach (string folder in folders)
    {
        if (!Directory.Exists(folder))
            continue;

        string[] files =
            Directory.GetFiles(
                folder,
                "*.bin");

        if (files.Length > 0)
            return files[0];
    }

    return null;
}
//--------------------------------------------------
// Start Recording
//--------------------------------------------------

public void Start(
    string ffmpegPath,
    string whisperPath,
    string modelPath,
    string microphone)
{
    lock (voiceLock)
    {
        //----------------------------------------------
        // Already Running
        //----------------------------------------------

        if (recording)
        {
            Console.WriteLine(
                "[VOICE] Already Running");

            return;
        }

        //----------------------------------------------
        // Use Manual Config (if provided)
        //----------------------------------------------

        if (!string.IsNullOrWhiteSpace(ffmpegPath))
            ffmpegExe = ffmpegPath;

        if (!string.IsNullOrWhiteSpace(whisperPath))
            whisperExe = whisperPath;

        if (!string.IsNullOrWhiteSpace(modelPath))
            whisperModel = modelPath;

        microphoneName = microphone;

        //----------------------------------------------
        // Auto Search Missing Files
        //----------------------------------------------

        EnsureTools();

        //----------------------------------------------
        // Delete Previous Recording
        //----------------------------------------------

        try
        {
            if (File.Exists(outputFile))
                File.Delete(outputFile);
        }
        catch
        {
        }

        //----------------------------------------------
        // Build FFmpeg Command
        //----------------------------------------------

        ProcessStartInfo psi = new()
        {
            FileName = ffmpegExe,

            Arguments =
                $"-y " +
                $"-f dshow " +
                $"-i audio=\"{microphoneName}\" " +
                $"-ar 16000 " +
                $"-ac 1 " +
                $"\"{outputFile}\"",

            RedirectStandardInput = true,

            UseShellExecute = false,

            CreateNoWindow = true
        };

        //----------------------------------------------
        // Start FFmpeg
        //----------------------------------------------

        ffmpegProcess =
            Process.Start(psi);

        if (ffmpegProcess == null)
            throw new Exception(
                "Cannot start FFmpeg.");

        recording = true;

        Console.WriteLine(
            "[VOICE] Recording Started");
    }
}
//--------------------------------------------------
// Stop Recording
//--------------------------------------------------

public async Task<string?> StopAsync()
{
    lock (voiceLock)
    {
        if (!recording)
            return null;

        recording = false;
    }

    Console.WriteLine(
        "[VOICE] Stopping...");

    //--------------------------------------------------
    // Stop FFmpeg
    //--------------------------------------------------

    if (ffmpegProcess != null)
    {
        try
        {
            if (!ffmpegProcess.HasExited)
            {
                try
                {
                    ffmpegProcess.StandardInput.WriteLine("q");
                    ffmpegProcess.StandardInput.Flush();
                }
                catch
                {
                }

                if (!ffmpegProcess.WaitForExit(3000))
                {
                    Console.WriteLine(
                        "[VOICE] Force Kill FFmpeg");

                    ffmpegProcess.Kill(true);

                    ffmpegProcess.WaitForExit();
                }
            }
        }
        catch
        {
        }

        try
        {
            ffmpegProcess.Dispose();
        }
        catch
        {
        }

        ffmpegProcess = null;
    }

    //--------------------------------------------------
    // Wait File Flush
    //--------------------------------------------------

    await Task.Delay(500);

    //--------------------------------------------------
    // Validate File
    //--------------------------------------------------

    if (!File.Exists(outputFile))
    {
        Console.WriteLine(
            "[VOICE] WAV Not Found");

        return null;
    }

    FileInfo info =
        new(outputFile);

    if (info.Length < 1024)
    {
        Console.WriteLine(
            "[VOICE] WAV Empty");

        return null;
    }

    Console.WriteLine(
        $"[VOICE] WAV Size : {info.Length}");

    //--------------------------------------------------
    // Run Whisper
    //--------------------------------------------------

    ProcessStartInfo psi =
        new()
        {
            FileName = whisperExe,

            Arguments =
                $"-m \"{whisperModel}\" " +
                $"-f \"{outputFile}\" " +
                $"--no-gpu",

            RedirectStandardOutput = true,
            RedirectStandardError = true,

            UseShellExecute = false,

            CreateNoWindow = true
        };

    using Process whisper =
        Process.Start(psi)!;

    string stdout =
        await whisper.StandardOutput.ReadToEndAsync();

    string stderr =
        await whisper.StandardError.ReadToEndAsync();

    await whisper.WaitForExitAsync();

    //--------------------------------------------------
    // Debug
    //--------------------------------------------------

    Console.WriteLine();
    Console.WriteLine("===== WHISPER =====");
    Console.WriteLine(psi.FileName);
    Console.WriteLine(psi.Arguments);
    Console.WriteLine("===================");

    if (!string.IsNullOrWhiteSpace(stderr))
    {
        Console.WriteLine();
        Console.WriteLine(stderr);
    }

    //--------------------------------------------------
    // Extract Text
    //--------------------------------------------------

    string result = "";

    foreach (string line in stdout.Split(
                 Environment.NewLine,
                 StringSplitOptions.RemoveEmptyEntries))
    {
        int idx =
            line.IndexOf(']');

        if (idx >= 0 &&
            idx + 1 < line.Length)
        {
            result +=
                line[(idx + 1)..].Trim() + " ";
        }
    }

    result =
        result.Trim();

    if (!string.IsNullOrWhiteSpace(result))
    {
        OnText?.Invoke(result);

        Console.WriteLine(
            "[VOICE] Text : " + result);

        return result;
    }

    Console.WriteLine(
        "[VOICE] Nothing Recognized");

    return null;
}
//--------------------------------------------------
// Stop FFmpeg
//--------------------------------------------------

private void StopFFmpeg()
{
    if (ffmpegProcess == null)
        return;

    try
    {
        if (!ffmpegProcess.HasExited)
        {
            try
            {
                ffmpegProcess.StandardInput.WriteLine("q");
                ffmpegProcess.StandardInput.Flush();
            }
            catch
            {
            }

            if (!ffmpegProcess.WaitForExit(2000))
            {
                ffmpegProcess.Kill(true);

                ffmpegProcess.WaitForExit();
            }
        }
    }
    catch
    {
    }

    try
    {
        ffmpegProcess.Dispose();
    }
    catch
    {
    }

    ffmpegProcess = null;

    recording = false;
}

//
//--------------------------------------------------
// Delete Temp File
//--------------------------------------------------

private void DeleteTempFile()
{
    try
    {
        if (File.Exists(outputFile))
            File.Delete(outputFile);
    }
    catch
    {
    }
}

//
//--------------------------------------------------
// Dispose
//--------------------------------------------------

public void Dispose()
{
    StopFFmpeg();

    DeleteTempFile();

    OnText = null;

    GC.SuppressFinalize(this);
}}