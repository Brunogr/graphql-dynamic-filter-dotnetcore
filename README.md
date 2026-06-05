# DynamicQuery.AspNetCore

[![NuGet](https://img.shields.io/nuget/v/DynamicQuery.AspNetCore.svg)](https://www.nuget.org/packages/DynamicQuery.AspNetCore/)
[![CI](https://github.com/Brunogr/dynamic-query-aspnetcore/actions/workflows/ci.yml/badge.svg)](https://github.com/Brunogr/dynamic-query-aspnetcore/actions/workflows/ci.yml)
[![codecov](https://codecov.io/gh/Brunogr/dynamic-query-aspnetcore/branch/master/graph/badge.svg)](https://codecov.io/gh/Brunogr/dynamic-query-aspnetcore)

OData-inspired URL query binding for ASP.NET Core. Parses query string parameters into LINQ expression trees for **filtering**, **sorting**, **paging**, and **projection**.

This library is **not GraphQL**. It binds REST query parameters such as `?query=name=Bruno&order=name&page=0&pagesize=10` to strongly typed expressions you can use with `IQueryable`, EF Core, or in-memory collections.

## Renamed from Graphql.DynamicFilter

This project was originally published as **[Graphql.DynamicFilter](https://www.nuget.org/packages/Graphql.DynamicFilter)** on NuGet (~41k downloads). The old name was misleading — the library provides OData-style URL query binding, not GraphQL.

Starting with **2.0**, development continues under new package IDs:

| Legacy package (1.x) | New package (2.x) |
|----------------------|-------------------|
| [Graphql.DynamicFilter](https://www.nuget.org/packages/Graphql.DynamicFilter) | [DynamicQuery.AspNetCore](https://www.nuget.org/packages/DynamicQuery.AspNetCore) |
| [Graphql.DynamicFilter.NetFramework](https://www.nuget.org/packages/Graphql.DynamicFilter.NetFramework) | [DynamicQuery.NetFramework](https://www.nuget.org/packages/DynamicQuery.NetFramework) |

The **query string format is unchanged**. Only the package name, namespaces, and type names changed (`DynamicFilter<T>` → `DynamicQuery<T>`).

## Packages

| Package | Description |
|---------|-------------|
| [DynamicQuery.AspNetCore](https://www.nuget.org/packages/DynamicQuery.AspNetCore) | ASP.NET Core (net8.0, net9.0, net10.0) — controllers and minimal APIs |
| [DynamicQuery.NetFramework](https://www.nuget.org/packages/DynamicQuery.NetFramework) | ASP.NET MVC 5 on .NET Framework 4.7.2 |

## Installation

```bash
dotnet add package DynamicQuery.AspNetCore
```

## Quick start — Controllers

```csharp
using DynamicQuery.AspNetCore;

[HttpGet]
public Task<List<User>> Get(DynamicQuery<User> query)
{
    var result = _users.Apply(query).ToList();
    return Task.FromResult(result);
}
```

Example request:

```http
GET /api/users?query=name=Bruno
```

Generated expression:

```csharp
x => x.Name == "Bruno"
```

## Quick start — Minimal APIs

```csharp
app.MapGet("/users", (DynamicQuery<User> query) =>
{
    return users.Apply(query).ToList();
});
```

`DynamicQuery<T>` implements the minimal API `BindAsync` convention automatically.

## Query syntax

Multiple filters are combined with **AND** (comma-separated). Use `|` within a value for **OR**.

| Operator | Example | Meaning |
|----------|---------|---------|
| Equals | `name=Bruno` | exact match |
| Contains (case-insensitive) | `name%b` | substring |
| Contains (case-sensitive) | `name%%B` | substring |
| Greater than | `age>15` | |
| Greater or equal | `age>=15` | |
| Less than | `age<15` | |
| Less or equal | `age<=15` | |
| Not equals | `age!=15` | |
| Nested property | `address.number=23` | dot notation |

Use `query=` or `filter=` for the filter parameter (both are supported).

### Ordering

```http
GET /api/users?query=name%b&order=name=Asc
GET /api/users?query=name%b&order=name=Desc
```

If direction is omitted, default is **Desc** (same as v1.x ASP.NET Core behavior).

### Paging

```http
GET /api/users?query=name%b&order=name&page=0&pagesize=10
```

### Projection (select)

```http
GET /api/users?select=name,age
```

Populates `query.Select` (`Expression<Func<T,T>>`) and `query.SelectText`.

## Apply helper

```csharp
using DynamicQuery.AspNetCore;

// In-memory collections (uses compiled expressions)
var items = users.Apply(query);

// IQueryable / EF Core (uses expression trees)
var items = dbContext.Users.Apply(query);
```

## MVC registration (optional)

Model binding works via the `[ModelBinder]` attribute on `DynamicQuery<T>`. To register the binder provider explicitly:

```csharp
builder.Services.AddControllers()
    .AddDynamicQuery();
```

## Migration from Graphql.DynamicFilter 1.x

If you installed the old package:

```bash
# Remove the legacy package
dotnet remove package Graphql.DynamicFilter

# Install the renamed package
dotnet add package DynamicQuery.AspNetCore
```

Update your code:

- `using Graphql.DynamicFiltering` → `using DynamicQuery.AspNetCore`
- `DynamicFilter<T>` → `DynamicQuery<T>`

Full details: [CHANGELOG.md](https://github.com/Brunogr/dynamic-query-aspnetcore/blob/master/CHANGELOG.md)

## Development

```bash
dotnet build DynamicQuery.sln
dotnet test DynamicQuery.sln
```

## License

MIT — see [LICENSE](https://github.com/Brunogr/dynamic-query-aspnetcore/blob/master/LICENSE).
