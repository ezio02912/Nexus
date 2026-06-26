# Runbook: Update BootstrapBlazor

1. Fetch latest source:

```bash
git -C external/bootstrap-blazor fetch origin
```

2. Merge latest upstream:

```bash
git -C external/bootstrap-blazor checkout vendor/bootstrapblazor-latest
git -C external/bootstrap-blazor merge origin/main
```

3. Build affected frontend apps:

```bash
DOTNET_CLI_HOME=/private/tmp/dotnet-home dotnet build apps/web-admin/src/Nexus.Web.Admin/Nexus.Web.Admin.csproj
DOTNET_CLI_HOME=/private/tmp/dotnet-home dotnet build apps/web-tenant/src/Nexus.Web.Tenant/Nexus.Web.Tenant.csproj
```

4. Record breaking changes in `docs/frontend-bootstrapblazor.md`.
