namespace HidroRec.Backend.Configurations;

public sealed class FileStorageOptions
{
    public const string SectionName = "FileStorage";

    public string UploadsPath { get; init; } = "uploads";

    public int MaxImageSizeBytes { get; init; } = 5 * 1024 * 1024;
}
