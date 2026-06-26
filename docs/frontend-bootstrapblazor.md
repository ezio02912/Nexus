# Frontend BootstrapBlazor Strategy

## Source location

BootstrapBlazor được đặt tại:

```text
external/bootstrap-blazor
```

Repo này đang checkout branch:

```text
vendor/bootstrapblazor-latest
```

## Update flow

```bash
git -C external/bootstrap-blazor fetch origin
git -C external/bootstrap-blazor checkout vendor/bootstrapblazor-latest
git -C external/bootstrap-blazor merge origin/main
```

Sau khi update source UI framework, kiểm tra lại `apps/web-admin` và `apps/web-tenant`.

## Tích hợp phase sau

Phase hiện tại chỉ dựng structure. Khi bắt đầu code UI, chọn một trong hai hướng:

- Dùng BootstrapBlazor qua NuGet để đơn giản hóa build.
- Dùng project reference/source reference từ `external/bootstrap-blazor` nếu cần tùy biến framework.
