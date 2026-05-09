# Feedy Backend — Design Document

**Status:** Pre-design skeleton. Ready for implementation.

---

## Architecture Overview

### 4-Layer Clean Architecture
```
┌─────────────────────────────────────────┐
│ Presentation (Feedy.Api)                │ Minimal APIs, Endpoints, DI
├─────────────────────────────────────────┤
│ Application (Feedy.Application)         │ Use Cases, Commands/Queries, Handlers, DTOs
├─────────────────────────────────────────┤
│ Infrastructure (Feedy.Infrastructure)   │ Dapper Repositories, PostgreSQL, JWT
├─────────────────────────────────────────┤
│ Domain (Feedy.Domain)                   │ Aggregates, Value Objects, Events, Services
└─────────────────────────────────────────┘
```

### Vertical Slices (Use Cases)
Each use case is independent and owns its resources:
- Query/Command object
- Handler
- Use-case-specific DTOs
- Domain services
- **No sharing across use cases**

Example: `GetRecipeFeed` and `RegisterUser` have separate `RecipeDto` and `PasswordHasher`.

---

## Domain Layer (DDD)

### Aggregates (Aggregate Roots)
- **User** — owns Email and Password value objects; enforces registration/login invariants
- **Recipe** — immutable recipe data; owns tags and ingredients
- **UserProfile** — owns flavor preferences, dietary regime, rejected ingredients

### Value Objects
- **Email** — validated, immutable, compared by value
- **Password** — hashed, immutable, >= 8 chars
- **UserId** — strongly-typed identifier
- **RecipeId** — strongly-typed identifier
- **Tags[]** — domain-specific concept

### Domain Events
- `UserRegisteredEvent` — User aggregate records when registration succeeds
- `EmailChangedEvent` — User aggregate records email change
- `RecipeAddedToFeedEvent` — (future) triggered when recipe matches user
- Subscribers: Application handlers send emails, log metrics, publish to external systems

### Domain Services
- `DuplicateEmailChecker` — validates email uniqueness (cross-aggregate logic)
- `FeedRankingService` — (future) ranks recipes by user profile match
- Stateless, injected at Application/API boundary

### Repository Interfaces (Domain-owned)
- `IRecipeRepository` — loads Recipe aggregates
- `IUserRepository` — loads User aggregates, enforces email uniqueness

---

## Application Layer (Use Cases)

### Vertical Slice Structure
```
Application/UseCases/
├── GetRecipeFeed/
│   ├── GetRecipeFeedQuery.cs              ← Input contract
│   ├── GetRecipeFeedQueryHandler.cs       ← Orchestrates use case
│   ├── RecipeDto.cs                       ← Use-case-specific response
│   ├── GetRecipeFeedResult.cs             ← Result envelope
│   └── FeedRankingService.cs              ← Use-case-specific service
├── RegisterUser/
│   ├── RegisterUserCommand.cs             ← Input contract
│   ├── RegisterUserCommandHandler.cs      ← Orchestrates use case
│   ├── RegisterUserDto.cs                 ← Use-case-specific response
│   ├── PasswordHasher.cs                  ← Use-case-specific service
│   └── (no sharing with GetRecipeFeed)
├── LoginUser/
├── GetUserProfile/
├── UpdateUserProfile/
└── ... more use cases
```

**Key Rule:** No sharing of DTOs or services across use cases. If two use cases need similar logic, duplicate it.

### Command/Query Pattern
- **Query** — reads data, returns DTO, no side effects (GetRecipeFeedQuery)
- **Command** — writes data, returns result or void (RegisterUserCommand, LoginUserCommand)
- **Handler** — orchestrates the use case: load aggregates, apply logic, call repositories, record events

---

## Infrastructure Layer (Dapper + PostgreSQL)

### Repositories
- Implement `IRecipeRepository`, `IUserRepository` (defined in Domain)
- Use **Dapper** for explicit SQL queries
- Load full aggregates, not projections
- Handle transaction boundaries

Example:
```csharp
public async Task<RecipeData> GetRecipesForFeedAsync(...)
{
    const string sql = @"SELECT id, title, ... FROM recipes WHERE ...";
    using var connection = new NpgsqlConnection(_connectionString);
    var recipes = await connection.QueryAsync<RecipeData>(sql);
    return recipes;
}
```

### Data Models
- `RecipeData`, `UserData`, `UserProfileData` — data transfer objects from repository
- Mirror aggregate structure but optimized for query needs
- Mapped to domain aggregates in Application handlers

### Database
- **PostgreSQL 16** — relational store
- pgvector extension (future) — for Smart Feed embeddings
- Migrations via EF Core migrations (scaffolded, applied manually)

---

## API Layer (Minimal APIs)

### Endpoint Structure
```
Feedy.Api/Endpoints/
├── RecipeEndpoints.cs
│   └── MapRecipeEndpoints() → MapGet("/api/recipes/feed", GetFeed)
├── UserEndpoints.cs
│   └── MapUserEndpoints() → MapPost("/api/users/register", Register)
└── ...
```

### Dependency Injection (Program.cs)
```csharp
// Register repositories
builder.Services.AddScoped<IRecipeRepository>(...);
builder.Services.AddScoped<IUserRepository>(...);

// Register handlers
builder.Services.AddScoped<GetRecipeFeedQueryHandler>();
builder.Services.AddScoped<RegisterUserCommandHandler>();

// Register domain services
builder.Services.AddScoped<DuplicateEmailChecker>();
builder.Services.AddScoped<FeedRankingService>();
```

### Authentication
- JWT Bearer tokens
- `appsettings.json` configures issuer, audience, secret
- Endpoints protected with `RequireAuthorization()`

---

## Use Cases (Skeleton)

### 1. GetRecipeFeed
- **Input:** `GetRecipeFeedQuery(UserId, PageNumber, PageSize)`
- **Flow:**
  1. Load user profile from `IUserRepository`
  2. Load candidate recipes from `IRecipeRepository`
  3. Rank via `FeedRankingService` (match % by flavor tags)
  4. Map to `RecipeDto` (use-case-specific)
  5. Apply pagination
  6. Return `GetRecipeFeedResult`

### 2. RegisterUser
- **Input:** `RegisterUserCommand(Email, Password, FlavorTags)`
- **Flow:**
  1. Validate email uniqueness via `DuplicateEmailChecker`
  2. Create `User` aggregate (triggers `UserRegisteredEvent`)
  3. Save via `IUserRepository`
  4. Publish event → Application handler sends welcome email
  5. Return `RegisterUserDto(UserId, Email)`

### 3. LoginUser
- **Input:** `LoginUserCommand(Email, Password)`
- **Flow:**
  1. Load user via `IUserRepository.GetByEmailAsync()`
  2. Verify password
  3. Generate JWT token
  4. Return JWT

### 4. GetUserProfile
- **Input:** Query by `UserId`
- **Output:** Profile with flavor tags, dietary regime, rejected ingredients

### 5. UpdateUserProfile
- **Input:** Command with new preferences
- **Flow:**
  1. Load user profile
  2. Update preferences
  3. Save via `IUserRepository`
  4. (Future) Trigger `ProfileUpdatedEvent` to re-rank feed

---

## Data Model (SQL)

### Tables (Example)
```sql
-- Users
CREATE TABLE users (
    id UUID PRIMARY KEY,
    email VARCHAR(255) UNIQUE NOT NULL,
    password_hash VARCHAR(255) NOT NULL,
    created_at TIMESTAMP NOT NULL,
    last_login_at TIMESTAMP
);

-- User Profiles
CREATE TABLE user_profiles (
    user_id UUID PRIMARY KEY REFERENCES users(id),
    flavor_tags TEXT[] NOT NULL,
    dietary_regime TEXT[] NOT NULL,
    rejected_ingredients TEXT[] NOT NULL,
    updated_at TIMESTAMP NOT NULL
);

-- Recipes
CREATE TABLE recipes (
    id UUID PRIMARY KEY,
    title VARCHAR(255) NOT NULL,
    description TEXT,
    tags TEXT[] NOT NULL,
    difficulty VARCHAR(20) NOT NULL,
    prep_time_minutes INT NOT NULL,
    ingredients TEXT[] NOT NULL,
    main_image_url VARCHAR(512),
    is_active BOOLEAN DEFAULT true,
    created_at TIMESTAMP NOT NULL
);
```

---

## Testing Strategy

### Unit Tests (xUnit + FluentAssertions + Moq)
- **Domain:** Aggregates, Value Objects, Domain Services
- **Application:** Handlers (mock repositories)

### Integration Tests (xUnit + TestContainers)
- **Infrastructure:** Repositories (real PostgreSQL in container)
- **End-to-end:** Full use case with real DB

### Test Structure
```
tests/
├── Feedy.Domain.Tests/
│   ├── Entities/UserTests.cs
│   └── ValueObjects/EmailTests.cs
├── Feedy.Application.Tests/
│   └── UseCases/GetRecipeFeed/GetRecipeFeedQueryHandlerTests.cs
└── Feedy.Infrastructure.Tests/
    └── Persistence/RecipeRepositoryTests.cs
```

---

## Deployment (Docker + Kubernetes)

### Docker
```bash
docker-compose up -d                    # Local: PostgreSQL + API
docker build -t feedy-api:latest .      # Build multi-stage image
```

### Kubernetes
```bash
kubectl apply -f k8s/postgres-statefulset.yaml   # DB
kubectl apply -f k8s/deployment.yaml             # API (3 replicas)
```

---

## Configuration (appsettings.json)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=postgres;Port=5432;Database=feedy_dev;..."
  },
  "JwtSettings": {
    "Secret": "...",
    "Issuer": "feedy-api",
    "Audience": "feedy-frontend",
    "ExpirationMinutes": 1440
  },
  "CorsOrigins": [
    "http://localhost:5173",  // Frontend dev
    "http://localhost:3000"
  ]
}
```

---

## Technology Choices

| Component | Choice | Reason |
|-----------|--------|--------|
| Framework | .NET 8 Minimal APIs | Lightweight, type-safe, integrated |
| ORM | Dapper | SQL control, performance, simplicity |
| Database | PostgreSQL | Open source, JSON/array support, pgvector |
| Testing | xUnit + Moq + FluentAssertions | Industry standard |
| Containers | Docker Compose + Kubernetes | Local dev + production ready |
| Authentication | JWT Bearer | Stateless, scalable |

---

## Next Steps (Implementation)

1. **Database schema** — create migrations
2. **Repository implementations** — Dapper SQL queries
3. **Use case handlers** — business logic
4. **Endpoint implementations** — route to handlers
5. **Domain events publishing** — event bus (future)
6. **Tests** — unit + integration
7. **Documentation** — API specs (OpenAPI/Swagger)

---

## References

- **Clean Architecture:** `CLAUDE.md` Architecture section
- **DDD Patterns:** `LAB_NOTES_DDD.md`
- **Data Access:** `LAB_NOTES_DATA.md`
- **Testing:** `LAB_NOTES_TESTING.md`
- **Docker:** `LAB_NOTES_DOCKER.md`, `DOCKER.md`
