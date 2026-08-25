using System.Diagnostics;

namespace OpenAgent.Runtime;

public sealed class AudioRecorder : IDisposable
{
    //--------------------------------------------------
    // Fields
    //--------------------------------------------------

    private Process? process;

    //--------------------------------------------------
    // Properties
    //--------------------------------------------------

    public bool IsRecording =>
        process != null &&
        !process.HasExited;

    //--------------------------------------------------
    // Start
    //--------------------------------------------------

 public bool Start(
    string ffmpegExe,
    string microphone,
    string outputFile)
{
    if (IsRecording)
        return false;

    process = new Process();

    process.StartInfo.FileName =
        ffmpegExe;

    process.StartInfo.Arguments =
        "-f dshow " +
        $"-i audio=\"{microphone}\" " +
        "-ac 1 " +
        "-ar 16000 " +
        "-acodec pcm_s16le " +
        "-y " +
        $"\"{outputFile}\"";

    process.StartInfo.UseShellExecute =
        false;

    process.StartInfo.CreateNoWindow =
        true;

    process.StartInfo.RedirectStandardInput =
        true;

    process.StartInfo.RedirectStandardOutput =
        true;

    process.StartInfo.RedirectStandardError =
        true;

    bool started =
        process.Start();

    if (!started)
        return false;

    // تخلیه خروجی بدون نمایش در CMD
    _ = Task.Run(async () =>
    {
        try
        {
            while (!process.StandardOutput.EndOfStream)
            {
                await process.StandardOutput.ReadLineAsync();
            }
        }
        catch
        {
        }
    });

    // تخلیه خطاها بدون نمایش در CMD
    _ = Task.Run(async () =>
    {
        try
        {
            while (!process.StandardError.EndOfStream)
            {
                await process.StandardError.ReadLineAsync();
            }
        }
        catch
        {
        }
    });

    return true;
}

    //--------------------------------------------------
    // Stop
    //--------------------------------------------------

    public void Stop()
    {
        if (process == null)
            return;

        try
        {
            if (!process.HasExited)
            {
                try
                {
                    process.StandardInput.WriteLine("q");
                    process.StandardInput.Flush();
                }
                catch
                {
                }

                if (!process.WaitForExit(5000))
                {
                    Console.WriteLine(
                        "[FFmpeg] Timeout -> Kill");

                    process.Kill(true);

                    process.WaitForExit();
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(
                "[FFmpeg Stop] " + ex);
        }
        finally
        {
            process.Dispose();
            process = null;
        }
    }

    //--------------------------------------------------
    // Dispose
    //--------------------------------------------------

    public void Dispose()
    {
        Stop();
    }
}