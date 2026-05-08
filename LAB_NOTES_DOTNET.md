---
name: .NET 8 Build & NuGet Issues
description: Common build failures, dependency resolution, framework compatibility
type: technical
---

### 1. NuGet source configuration can block all package resolution

**Cause:** When a custom NuGet source (e.g., internal Nexus) is configured globally or in `nuget.config` but is inaccessible, `dotnet restore` retries indefinitely and fails on all packages, even public ones from nuget.org.

**Rule:** Always create a local `nuget.config` in the repository root that explicitly sets `packageSources` to only `https://api.nuget.org/v3/index.json`. Never rely on machine-global NuGet sources. If a private feed is needed, add it explicitly in the file and document the access requirement.

### 2. Package version mismatches can surface at compile time, not restore time

**Cause:** When a dependency declares a range (e.g., `Microsoft.NET.Test.Sdk >= 17.9.1`), NuGet may resolve to a different version than what you intended. This is silent unless the version introduces a breaking change in an API you use.

**Rule:** Pin exact versions in `.csproj` files using `Version="X.Y.Z"` notation, not ranges. If a range is required (rare), document why and test against both the minimum and maximum resolved versions locally.

### 3. Implicit using statements can hide namespace conflicts

**Cause:** With `<ImplicitUsings>enable</ImplicitUsings>` in the `.csproj`, certain namespaces are automatically imported (e.g., `System`, `System.Linq`). If you use a third-party namespace with the same name (rare), the implicit import shadows it and causes confusing `CS0246` "type not found" errors.

**Rule:** If you encounter a "type not found" error for a namespace you know is in scope, check that no implicit using is shadowing it. You can disable implicit usings with `<ImplicitUsings>disable</ImplicitUsings>` and manage usings explicitly, but the default is safe for most projects.

### 4. Dapper command definitions must match SQL parameter names exactly

**Cause:** Dapper maps method parameters to SQL `@ParameterName` placeholders by name. If the C# parameter name doesn't match the SQL placeholder (e.g., C# `userId` but SQL `@user_id`), Dapper silently ignores the parameter and passes `null` to SQL, causing runtime errors or unexpected query behavior.

**Rule:** Always use consistent naming: either SCREAMING_SNAKE_CASE in both C# and SQL, or camelCase in both. The SQL `@ParameterName` must match the C# property name exactly. Use explicit parameter objects (not anonymous types) for complex queries so the mapping is visible.
