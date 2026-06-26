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

- `POST /files`
- `GET /files/{id}`
- `POST /files/{id}/versions`
- `POST /file-links`

## Events

- `FileUploaded`
- `FileLinked`
- `FileVersionCreated`
