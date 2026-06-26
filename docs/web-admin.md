# Web Admin

Web Admin là Blazor Server app dùng Bootstrap mặc định của template .NET.

## Run

```bash
cd /Users/user/Documents/Nexus
./tools/scripts/run-core-services.zsh
dotnet run --project apps/web-admin/src/Nexus.Web.Admin/Nexus.Web.Admin.csproj --launch-profile http
```

Mở:

```text
http://localhost:7100
```

## Login flow

1. Vào `Tenants`, tạo tenant.
2. Copy `Tenant Id` từ bảng tenant.
3. Vào `Users`, tạo user với `Tenant Id`.
4. Vào `Login`, nhập `Tenant Id`, user name, password.

Login hiện dùng endpoint:

```text
POST http://localhost:7202/api/auth/login
```

Token hiện là placeholder in-memory để phục vụ phase core. Phase sau sẽ thay bằng JWT/OIDC thật.
