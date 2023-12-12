namespace SimpleBackup.App;

public class ApplicationOptions
{
    public string DirectoryToBackup { get; set; } = string.Empty;
    public string BucketName { get; set; } = string.Empty;
}