// Triggers a browser download from in-memory bytes (base64), because the file-service
// content endpoint requires a Bearer token that a plain anchor link cannot send.
export function download(fileName, contentType, base64) {
    const link = document.createElement("a");
    link.href = `data:${contentType || "application/octet-stream"};base64,${base64}`;
    link.download = fileName || "download";
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
}
