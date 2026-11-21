---
apply: always
---

---
apply: always
---

# UsageData Architecture Rules

## 1. Purpose and Status
`UsageData` is a **tested, working reference implementation** of an EF Core + MySQL Minimal API that tracks anonymous site usage.

The current goal is to **extract the entities into a reusable class library** without changing runtime behavior, database schema, routes, or JSON response shapes.

## 2. Target Project Structure

```text
UsageData.Domain        (new class library)
└── Entities/
    ├── UserAgent.cs
    ├── AnonymousUser.cs
    └── AccessRecord.cs

UsageData               (ASP.NET Core application)
├── Data/
│   ├── UserDataContext.cs
│   └── Configurations/
├── Exceptions/
├── Features/
└── Program.cs
```

Dependency direction:

```text
UsageData ──▶ UsageData.Domain
```

`UsageData.Domain` must never reference `UsageData`, ASP.NET Core, or `UserDataContext`.

## 3. Namespace Migration

| Current namespace | Target namespace |
| :--- | :--- |
| `TrafficRecord.Models` | `UsageData.Domain.Entities` |
| `TrafficRecord.Data` | `UsageData.Data` |
| `UsageData.Exceptions` | Keep unchanged |

When moving types, update `using` directives and fully qualified references such as `Models.UserAgent`.

## 4. Entity Model Contract

These mappings are **database contracts**. Preserve them exactly.

| Entity | Table | Primary key | Relationships by primitive ID |
| :--- | :--- | :--- | :--- |
| `UserAgent` | `User_Agent` | `AgentId` | None |
| `AnonymousUser` | `Anonymous_User` | `AnonymousUserId` | `UserAgentId` → `UserAgent.AgentId` |
| `AccessRecord` | `Accessed_By` | (`AnonymousUserId`, `UserAgentId`) | References both entities |

Rules:
- Do **not** rename entity properties. Property names are used as MySQL column names and JSON property names.
- Do **not** add navigation properties unless explicitly requested.
- Preserve `private set` on database-generated keys.
- Preserve existing nullability, `required` modifiers, and default values unless explicitly requested.
- Entities must remain instantiable without `UserDataContext`, HTTP context, or application services.

## 5. Persistence Rules

Preferred extraction approach:
- `UsageData.Domain` contains entity classes only.
- Move EF Core mapping from entity attributes into `IEntityTypeConfiguration<T>` classes under `UsageData/Data/Configurations/`.
- Do not add EF Core packages to `UsageData.Domain` unless explicitly requested.
- `UserDataContext` should apply configurations from its own assembly.

Example:

```csharp
namespace UsageData.Data.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UsageData.Domain.Entities;

public sealed class AccessRecordConfiguration : IEntityTypeConfiguration<AccessRecord>
{
    public void Configure(EntityTypeBuilder<AccessRecord> builder)
    {
        builder.ToTable("Accessed_By");
        builder.HasKey(x => new { x.AnonymousUserId, x.UserAgentId });
    }
}
```

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.ApplyConfigurationsFromAssembly(typeof(UserDataContext).Assembly);
}
```

## 6. Endpoint Rules

Existing endpoints are tested contracts. Do not change their routes, HTTP methods, status codes, or response shapes.

| Route | Result |
| :--- | :--- |
| `GET /usage` | All `UserAgent` records |
| `GET /usage/{uid:int}` | One `UserAgent` or `404` |
| `GET /usage/anonymous-users` | All `AnonymousUser` records |
| `GET /usage/anonymous-users/{anonymousUserId:int}` | One `AnonymousUser` or `404` |
| `GET /usage/accessrecords` | All `AccessRecord` records |
| `GET /usage/accessrecords/{anonymousUserId:int}/{userAgentId:int}` | One `AccessRecord` or `404` |

For new or refactored endpoints:
- Put feature endpoint mappings under `Features/<FeatureName>/`.
- Use Minimal APIs with `MapGroup` and endpoint extension methods.
- Use `AsNoTracking()` for read-only queries.
- Pass `CancellationToken` to all async EF Core calls.
- Perform cross-entity joins and lookups in the application project, not in `UsageData.Domain`.
- Prefer response DTOs for new composed responses; leave existing entity responses unchanged.

## 7. Configuration Constraints
- Keep connection-string name `MYSQL_CONNECTIONSTRING`.
- Keep MySQL retry policy at 5 attempts and command timeout at 30 seconds.
- Keep `GlobalExceptionHandler` registered before endpoint execution.
- Never place real connection strings or secrets in suggested code; use placeholders.

## 8. Guardrails
1. No reverse dependency from `UsageData.Domain` to `UsageData`.
2. No database schema changes unless explicitly requested.
3. No migrations unless explicitly requested.
4. No changes to existing route or JSON contracts during extraction.
5. Suggest small, verifiable steps and identify affected files.
6. After extraction, verify the solution builds and existing `UsageData.http` requests still succeed.
