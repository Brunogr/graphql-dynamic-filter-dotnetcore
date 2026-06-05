# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [2.0.0] - 2026-06-05

### Added

- New package identity: `DynamicQuery.AspNetCore` and `DynamicQuery.NetFramework`
- Minimal API support via `DynamicQuery<T>.BindAsync`
- Shared `QueryBindingEngine` for consistent binding across hosts
- `Apply` extension methods for `IEnumerable<T>` and `IQueryable<T>`
- Multi-targeting for ASP.NET Core: `net8.0`, `net9.0`, `net10.0`
- GitHub Actions CI with coverage reporting
- Tag-based NuGet publishing workflow

### Changed

- Renamed `DynamicFilter<T>` to `DynamicQuery<T>`
- Renamed `DynamicFilterBinder` to `DynamicQueryBinder`
- Renamed namespaces from `Graphql.*` to `DynamicQuery.*`
- URL query parameter contract unchanged (`query`, `filter`, `order`, `page`, `pagesize`, `select`)
- NetFramework package now targets `net472`

### Removed

- GraphQL-related naming and legacy `netcoreapp2.1` / ASP.NET Core 2.1 dependencies

### Migration from Graphql.DynamicFilter 1.x

1. Replace the NuGet package:
   - `Graphql.DynamicFilter` → `DynamicQuery.AspNetCore`
   - `Graphql.DynamicFilter.NetFramework` → `DynamicQuery.NetFramework`
2. Update namespaces:
   - `Graphql.DynamicFiltering` → `DynamicQuery.AspNetCore`
   - `Graphql.NetFramework.DynamicFiltering` → `DynamicQuery.NetFramework`
3. Rename types in your code:
   - `DynamicFilter<T>` → `DynamicQuery<T>`
4. Query string syntax remains the same.

## [1.0.11] and earlier

See the legacy [Graphql.DynamicFilter](https://www.nuget.org/packages/Graphql.DynamicFilter) package on NuGet.
