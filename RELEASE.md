# Release checklist

Follow these steps when publishing **DynamicQuery.AspNetCore 2.0.0**.

## One-time setup

1. Rename the GitHub repository to `dynamic-query-aspnetcore` in **Settings → General → Repository name**.
2. Add GitHub repository secrets (**Settings → Secrets and variables → Actions → New repository secret**):
   - `NUGET_API_KEY` — required for release. Create at [nuget.org/account/apikeys](https://www.nuget.org/account/apikeys) with **Push** scope for `DynamicQuery.AspNetCore` and `DynamicQuery.NetFramework` (or a glob like `*` for your account).
   - `CODECOV_TOKEN` — optional, improves Codecov upload reliability for the coverage badge
3. Sign in to [Codecov](https://codecov.io/) with GitHub and enable the repository (required for the README coverage badge).

## Publish 2.0.0

1. Ensure `CHANGELOG.md` and version in `Directory.Build.props` are correct (`2.0.0`).
2. Merge changes to `main`.
3. Create and push the release tag:

```bash
git tag v2.0.0
git push origin v2.0.0
```

4. The **Release** workflow publishes `DynamicQuery.AspNetCore` and `DynamicQuery.NetFramework` to NuGet.
5. Create a GitHub Release from tag `v2.0.0` and paste the `2.0.0` section from `CHANGELOG.md`.

If a release workflow failed because `NUGET_API_KEY` was missing, add the secret and **Re-run all jobs** on the failed workflow run in the Actions tab (no need to create a new tag).

## Deprecate legacy packages

NuGet package IDs cannot be renamed. After the new packages are live:

1. Optionally publish a final **`Graphql.DynamicFilter` 1.0.12** with release notes pointing to `DynamicQuery.AspNetCore`.
2. Deprecate on [nuget.org](https://www.nuget.org/packages/Graphql.DynamicFilter/manage):
   - Alternative package: `DynamicQuery.AspNetCore`
   - Message: *Package renamed to DynamicQuery.AspNetCore. See https://github.com/Brunogr/dynamic-query-aspnetcore*
3. Repeat for **`Graphql.DynamicFilter.NetFramework`** → `DynamicQuery.NetFramework` if that legacy package was published.
