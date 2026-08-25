using System.Text;

namespace OpenAgent.AI;

public sealed class AttachmentManager
{
    //--------------------------------------------------
    // Fields
    //--------------------------------------------------

    private readonly List<string> attachments =
        new();

    //--------------------------------------------------
    // Properties
    //--------------------------------------------------

    public IReadOnlyList<string> Files =>
        attachments;

    public int Count =>
        attachments.Count;

    //--------------------------------------------------
    // Add File
    //--------------------------------------------------

    public bool Add(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return false;

        if (!File.Exists(path))
            return false;

        if (attachments.Contains(path))
            return false;

        attachments.Add(path);

        return true;
    }

    //--------------------------------------------------
    // Remove File
    //--------------------------------------------------

    public bool Remove(string path)
    {
        return attachments.Remove(path);
    }

    //--------------------------------------------------
    // Clear
    //--------------------------------------------------

    public void Clear()
    {
        attachments.Clear();
    }
        //--------------------------------------------------
    // Build Prompt
    //--------------------------------------------------

    public string BuildPrompt()
    {
        if (attachments.Count == 0)
            return string.Empty;

        StringBuilder builder = new();

        builder.AppendLine();
        builder.AppendLine("Attached Files:");
        builder.AppendLine("-------------------------");

        foreach (string file in attachments)
        {
            string ext =
                Path.GetExtension(file)
                    .ToLowerInvariant();

            string type = ext switch
            {
                ".png" => "Image",
                ".jpg" => "Image",
                ".jpeg" => "Image",
                ".bmp" => "Image",
                ".gif" => "Image",
                ".webp" => "Image",

                ".mp4" => "Video",
                ".avi" => "Video",
                ".mov" => "Video",
                ".mkv" => "Video",
                ".wmv" => "Video",

                ".pdf" => "PDF",

                ".txt" => "Text",

                ".doc" => "Word",
                ".docx" => "Word",

                ".xls" => "Excel",
                ".xlsx" => "Excel",

                ".ppt" => "PowerPoint",
                ".pptx" => "PowerPoint",

                ".cs" => "C# Source",
                ".cpp" => "C++ Source",
                ".h" => "Header",
                ".json" => "Json",
                ".xml" => "Xml",

                _ => "File"
            };

            builder.AppendLine(
                $"{type} : {Path.GetFileName(file)}");

            builder.AppendLine(
                file);

            builder.AppendLine();
        }

        builder.AppendLine("-------------------------");

        return builder.ToString();
    }

    //--------------------------------------------------
    // Get Images
    //--------------------------------------------------

    public IEnumerable<string> Images()
    {
        return attachments.Where(x =>
        {
            string ext =
                Path.GetExtension(x)
                    .ToLowerInvariant();

            return ext == ".png"
                || ext == ".jpg"
                || ext == ".jpeg"
                || ext == ".bmp"
                || ext == ".gif"
                || ext == ".webp";
        });
    }

    //--------------------------------------------------
    // Get Videos
    //--------------------------------------------------

    public IEnumerable<string> Videos()
    {
        return attachments.Where(x =>
        {
            string ext =
                Path.GetExtension(x)
                    .ToLowerInvariant();

            return ext == ".mp4"
                || ext == ".avi"
                || ext == ".mov"
                || ext == ".mkv"
                || ext == ".wmv";
        });
    }
}