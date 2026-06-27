namespace Nexus.Web.Tenant.Services;

/// <summary>
/// A file the user picked but has not uploaded yet. The browser file bytes are read
/// eagerly when the file is selected and cached here, because the underlying
/// <c>IBrowserFile</c> reference becomes invalid after the input re-renders.
/// </summary>
public sealed class PendingFileAttachment
{
    public PendingFileAttachment(string fileName, string contentType, byte[] content)
    {
        FileName = fileName;
        ContentType = string.IsNullOrWhiteSpace(contentType) ? "application/octet-stream" : contentType;
        Content = content;
    }

    public string FileName { get; }
    public string ContentType { get; }
    public byte[] Content { get; }
    public long Size => Content.LongLength;
}
