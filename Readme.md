# Simple Backup
Looking for a simple way to watch a directory for changes then upload them to an S3 compatible storage provider? This is the tool for you!

## Getting Started
Set the following environment variables:
- `AWS_ACCESS_KEY_ID`
- `AWS_SECRET_ACCESS_KEY`
- `AWS_SERVICE_URL`

Then run the following command:
```bash
dotnet run --project src/SimpleBackup/SimpleBackup.csproj --Dir /path/to/watch --Bucket bucket-name
```

## Notes:
I set this up to use Cloudflare R2 storage, but it should work with any S3 compatible storage provider. You just might have to change the `DisablePayloadSigning = true` in the `Deamon.cs` file.


It's also using `UseSystemd()` but you can change that to `UseWindowsService()` if you're on Windows.
https://learn.microsoft.com/en-us/dotnet/core/extensions/windows-service?pivots=dotnet-8-0