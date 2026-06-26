namespace Nexus.Services.File.Api.Storage;

/// <summary>
/// Abstraction over the binary store. The default implementation writes to the local file
/// system; it can be swapped for an S3/MinIO-backed implementation without touching callers.
/// </summary>
public interface IFileStorage
{
    Task<string> SaveAsync(Guid fileId, string fileName, Stream content, CancellationToken cancellationToken = default);

    Task<Stream?> OpenAsync(string storagePath, CancellationToken cancellationToken = default);
}

public sealed class FileSystemFileStorage : IFileStorage
{
    private readonly string _root;

    public FileSystemFileStorage(IConfiguration configuration)
    {
        var configured = configuration["FileStorage:Root"];
        _root = string.IsNullOrWhiteSpace(configured)
            ? Path.Combine(AppContext.BaseDirectory, "file-storage")
            : configured;
        Directory.CreateDirectory(_root);
    }

    public async Task<string> SaveAsync(Guid fileId, string fileName, Stream content, CancellationToken cancellationToken = default)
    {
        var safeName = $"{fileId:N}_{Path.GetFileName(fileName)}";
        var fullPath = Path.Combine(_root, safeName);
        await using var target = System.IO.File.Create(fullPath);
        await content.CopyToAsync(target, cancellationToken);
        return safeName;
    }

    public Task<Stream?> OpenAsync(string storagePath, CancellationToken cancellationToken = default)
    {
        var fullPath = Path.Combine(_root, storagePath);
        if (!System.IO.File.Exists(fullPath))
        {
            return Task.FromResult<Stream?>(null);
        }

        return Task.FromResult<Stream?>(System.IO.File.OpenRead(fullPath));
    }
}
