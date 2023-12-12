using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace SimpleBackup.App;

public class Daemon(ILogger<Daemon> logger, IOptions<ApplicationOptions> options, IAmazonS3 s3Client) : IDisposable
{
    private readonly FileSystemWatcher _fileSystemWatcher = new (options.Value.DirectoryToBackup);
    private readonly object _disposedLock = new ();
    private bool _disposed = false;
    private CancellationTokenSource _cts;
    
    public void DoWork(CancellationToken cancellationToken)
    {
        _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _fileSystemWatcher.Created += async (sender, args) => await UploadFile(sender, args);
        _fileSystemWatcher.Changed += async (sender, args) => await UploadFile(sender, args);
        _fileSystemWatcher.EnableRaisingEvents = true;
    }

    public void Stop()
    {
        _fileSystemWatcher.EnableRaisingEvents = false;
    }

    private async Task UploadFile(object sender, FileSystemEventArgs args)
    {
        logger.LogInformation("File {FileName} created", args.Name);
        if (!Path.HasExtension(args.FullPath)) return;
        if (string.IsNullOrEmpty(args.Name))
        {
            logger.LogError("File name is null or empty");
            return;
        }
            
        var fileStream = File.OpenRead(args.FullPath);
        var putRequest = new PutObjectRequest()
        {
            // Needed for Cloudflare R2
            DisablePayloadSigning = true,
            BucketName = options.Value.BucketName,
            Key = args.Name,
            InputStream = fileStream,
            ContentType = MimeTypes.GetMimeType(args.Name) ?? "application/octet-stream"
        };
        logger.LogInformation("Uploading {FileName} to S3", args.Name);
        await s3Client.PutObjectAsync(putRequest, _cts.Token);
        logger.LogInformation("Uploaded {FileName} to S3", args.Name);
    }
    
    public void Dispose()
    {
        lock (_disposedLock)
        {
            if (_disposed) return;
            _disposed = true;
        }
        _fileSystemWatcher.Dispose();
    }
}