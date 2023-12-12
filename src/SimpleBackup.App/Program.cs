using System.Data;
using Amazon.Runtime;
using Amazon.S3;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SimpleBackup.App;

var host = Host.CreateDefaultBuilder(args)
    .UseSystemd()
    .ConfigureAppConfiguration(builder =>
    {
        builder.AddEnvironmentVariables()
            .AddUserSecrets<Daemon>()
            .AddCommandLine(args);
    })
    .ConfigureServices((ctx, services) =>
    {
        services.AddSystemd();
        services.AddLogging(opt =>
        {
            opt.AddConsole();
        });

        var awsAccessKey = ctx.Configuration.GetValue<string>("AWS_ACCESS_KEY_ID");
        var awsSecretKey = ctx.Configuration.GetValue<string>("AWS_SECRET_ACCESS_KEY");
        var awsServiceUrl = ctx.Configuration.GetValue<string>("AWS_SERVICE_URL");
        if (string.IsNullOrEmpty(awsServiceUrl))
        {
            throw new NoNullAllowedException("AWS_SERVICE_URL is not set");
        }
        services.AddSingleton<IAmazonS3>(opt =>
            new AmazonS3Client(new BasicAWSCredentials(awsAccessKey, awsSecretKey),  new AmazonS3Config()
            {
                ServiceURL = awsServiceUrl
            }));

        var directoryToBackup = ctx.Configuration.GetValue<string>("Dir");
        var bucketName = ctx.Configuration.GetValue<string>("Bucket");
        if (string.IsNullOrEmpty(directoryToBackup) || string.IsNullOrEmpty(bucketName))
        {
            throw new NoNullAllowedException(
                "You must specify a directory to watch with --Dir <directory> and a bucket name with --Bucket <bucket>");
        }

        services.AddOptions<ApplicationOptions>()
            .Configure(opt =>
            {
                opt.DirectoryToBackup = directoryToBackup;
                opt.BucketName = bucketName;
            });

        services.AddSingleton<Daemon>();
    }).Build();

try
{
    var daemon = host.Services.GetRequiredService<Daemon>();
    daemon.DoWork(host.Services.GetRequiredService<IHostApplicationLifetime>().ApplicationStopping);
    await host.RunAsync();
}
catch (TaskCanceledException ex)
{
    // Ignore
}