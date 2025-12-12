namespace TegWallet.Infrastructure.Photos;

public class CloudinarySettings
{
    public string CloudName { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public string ApiSecret { get; set; } = string.Empty;
    public string Folder { get; set; } = "fintech-documents";

    // Optional: Video-specific settings
    public VideoSettings Videos { get; set; } = new();

    // Optional: Image-specific settings
    public ImageSettings Images { get; set; } = new();

    // Optional: PDF-specific settings
    public PdfSettings Pdfs { get; set; } = new();
}

public class VideoSettings
{
    public int MaxDurationSeconds { get; set; } = 300; // 5 minutes max
    public string[] AllowedCodecs { get; set; } = { "h264", "h265", "vp9" };
    public string DefaultFormat { get; set; } = "mp4";
}

public class ImageSettings
{
    public int MaxWidth { get; set; } = 4096;
    public int MaxHeight { get; set; } = 4096;
    public string DefaultFormat { get; set; } = "auto";
}

public class PdfSettings
{
    public int MaxPages { get; set; } = 50;
    public bool AllowTextExtraction { get; set; } = true;
}
