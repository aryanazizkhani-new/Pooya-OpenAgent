using System.Diagnostics;

namespace OpenAgent.AI;

public sealed class ModelManager : IDisposable
{
    //--------------------------------------------------
    // Fields
    //--------------------------------------------------

    private Process? currentProcess;

    //--------------------------------------------------
    // Properties
    //--------------------------------------------------

    public string CurrentModel { get; private set; }
        = "None";

    public bool IsRunning
    {
        get
        {
            return currentProcess != null &&
                   !currentProcess.HasExited;
        }
    }

    //--------------------------------------------------
    // Get Models
    //--------------------------------------------------

    public async Task<List<string>> GetModelsAsync()
    {
        List<string> models = new();

        using Process p = new();

        p.StartInfo.FileName = "ollama";
        p.StartInfo.Arguments = "list";

        p.StartInfo.UseShellExecute = false;
        p.StartInfo.RedirectStandardOutput = true;
        p.StartInfo.CreateNoWindow = true;

        p.Start();

        bool first = true;

        while (true)
        {
            string? line =
                await p.StandardOutput.ReadLineAsync();

            if (line == null)
                break;

            if (first)
            {
                first = false;
                continue;
            }

            if (string.IsNullOrWhiteSpace(line))
                continue;

            string[] parts =
                line.Split(
                    ' ',
                    StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length > 0)
                models.Add(parts[0]);
        }

        await p.WaitForExitAsync();

        return models;
    }

    //--------------------------------------------------
    // Check Model
    //--------------------------------------------------

    public async Task<bool> ModelExistsAsync(
        string model)
    {
        List<string> list =
            await GetModelsAsync();

        return list.Any(x =>
            x.Equals(
                model,
                StringComparison.OrdinalIgnoreCase));
    }

    //--------------------------------------------------
    // Alias
    //--------------------------------------------------

    public Task<bool> ExistsAsync(string model)
    {
        return ModelExistsAsync(model);
    }
        //--------------------------------------------------
    // Download Model
    //--------------------------------------------------

    public async Task<bool> DownloadModelAsync(
        string model,
        Action<string>? progress = null)
    {
        using Process p = new();

        p.StartInfo.FileName = "ollama";
        p.StartInfo.Arguments = $"pull {model}";

        p.StartInfo.UseShellExecute = false;
        p.StartInfo.RedirectStandardOutput = true;
        p.StartInfo.RedirectStandardError = true;
        p.StartInfo.CreateNoWindow = true;

        p.Start();

        while (true)
        {
            string? line =
                await p.StandardOutput.ReadLineAsync();

            if (line == null)
                break;

            if (!string.IsNullOrWhiteSpace(line))
                progress?.Invoke(line);
        }

        while (true)
        {
            string? line =
                await p.StandardError.ReadLineAsync();

            if (line == null)
                break;

            if (!string.IsNullOrWhiteSpace(line))
                progress?.Invoke(line);
        }

        await p.WaitForExitAsync();

        return p.ExitCode == 0;
    }

    //--------------------------------------------------
    // Run Model
    //--------------------------------------------------

    public bool RunModel(string model)
    {
        try
        {
            StopModel();

            currentProcess = new Process();

            currentProcess.StartInfo.FileName = "ollama";
            currentProcess.StartInfo.Arguments = $"run {model}";

            currentProcess.StartInfo.UseShellExecute = false;
            currentProcess.StartInfo.CreateNoWindow = true;

            currentProcess.Start();

            CurrentModel = model;

            return true;
        }
        catch
        {
            return false;
        }
    }
        //--------------------------------------------------
    // Stop Model
    //--------------------------------------------------

    public void StopModel()
    {
        try
        {
            if (currentProcess != null)
            {
                if (!currentProcess.HasExited)
                {
                    currentProcess.Kill(true);

                    currentProcess.WaitForExit();
                }

                currentProcess.Dispose();

                currentProcess = null;
            }

            CurrentModel = "None";
        }
        catch
        {
        }
    }

    //--------------------------------------------------
    // Restart Model
    //--------------------------------------------------

    public bool RestartModel()
    {
        if (CurrentModel == "None")
            return false;

        string model = CurrentModel;

        StopModel();

        return RunModel(model);
    }

    //--------------------------------------------------
    // Dispose
    //--------------------------------------------------

    public void Dispose()
    {
        StopModel();
    }
}