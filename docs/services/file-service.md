# File Service

## Trách nhiệm

- Upload/download file.
- Version file.
- Liên kết file với chứng từ nghiệp vụ.
- Chuẩn bị tích hợp MinIO/S3 và virus scan ở phase sau.

## Data dự kiến

- Files
- FileVersions
- FileLinks

## API dự kiến

- `GET /api/files`
- `GET /api/files/{id}` requires `Nexus.Files.View`
- `GET /api/files/{id}/content` requires `Nexus.Files.View`
- `POST /api/files` requires `Nexus.Files.Upload`
- `POST /api/files/upload` requires `Nexus.Files.Upload`
- `DELETE /api/files/{id}` requires `Nexus.Files.Delete`
- `POST /files/{id}/versions`
- `GET /api/file-links` requires `Nexus.Files.View`
- `POST /api/file-links` requires `Nexus.Files.Upload`

## Ghi log upload

- Upload bị từ chối do sai `multipart/form-data` hoặc thiếu file sẽ ghi warning kèm content type / tenant.
- Lỗi lưu storage ghi error với `FileId`, `TenantId`, `FileName`, `Size`.
- Upload thành công ghi information với `FileId`, `TenantId`, `FileName`, `Size`.
- Xoá file cũng xoá binary storage; nếu storage delete lỗi, API trả `500` và ghi error với `FileId`, `TenantId`, `StoragePath`.
- Link file (`POST /api/file-links`) insert trực tiếp `FileLink` sau khi xác nhận file tồn tại, tránh cập nhật aggregate file gây optimistic concurrency khi upload rồi link ngay.

## Events

- `FileUploaded`
- `FileLinked`
- `FileVersionCreated`
