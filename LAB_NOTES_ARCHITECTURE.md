---
name: Clean Architecture & Dependency Injection
description: Layer boundaries, SOLID violations, service composition, vertical slices
type: technical
---

### 1. Application layer must not import Infrastructure types

**Cause:** If an Application service imports an `IRecipeRepository` from the `Feedy.Infrastructure` namespace, the dependency points outward, violating Clean Architecture. This couples the business logic to infrastructure choices and makes the application layer untestable without infrastructure.

**Rule:** Define all service interfaces in `Feedy.Application.Interfaces` or `Feedy.Domain.Interfaces`, never import Infrastructure types into Application. The infrastructure layer implements these interfaces. Dependency injection wires them together at the API layer (`Program.cs`).

### 2. Vertical slices must not share DTOs or Application services across use cases

**Cause:** When two use cases (e.g., `GetRecipeFeed` and `GetFavorites`) both use a shared `RecipeDto`, a change to one use case's needs (e.g., adding `matchPercentage` to the feed) forces a change to the shared DTO, breaking the other use case's contract. This creates implicit coupling that is hard to reason about.

**Rule:** Each vertical slice (use case folder) owns its own DTOs, query/command objects, and Application service. If two use cases need the same data shape, duplicate the DTO definition. Sharing is allowed only for Domain entities (immutable value objects or records). Duplication is preferable to coupling across use cases.

### 3. Dependency injection must happen at the boundary, not in business logic

**Cause:** If a use case constructor receives an `IServiceProvider` and calls `GetService<IRecipeRepository>()` inside the handler, you've hidden the dependencies and made the handler harder to test and understand. The dependency graph becomes invisible.

**Rule:** Only the Minimal API endpoint (in `Feedy.Api/Endpoints/`) is allowed to receive injected dependencies from the container. The Application layer constructor should declare all dependencies explicitly. The API endpoint passes them to the Application service, which never touches the container.

### 4. Domain entities must not reference services or repositories

**Cause:** If a `Recipe` entity has a method `GetComplementaryRecipes(IRecipeRepository repo)`, the entity is no longer a pure data container—it's mixed with infrastructure concerns. This violates the Domain-Driven Design principle that entities are stateless knowledge carriers.

**Rule:** Domain entities contain only data and pure business rules (e.g., `bool IsExpired() => CreatedAt.AddDays(30) < DateTime.UtcNow;`). All orchestration and repository calls belong in the Application layer. If an entity needs to know about related objects, use an `IEnumerable<Guid>` of IDs, not lazy-loaded collections.

### 5. Repository interfaces must match the queries that use cases need, not the database schema

**Cause:** If you create `IRecipeRepository.GetAll()` and then in a use case you need only titles and IDs, you're loading full recipe objects into memory unnecessarily. Over time, repositories accumulate 20+ methods to satisfy different use cases, becoming a "god object."

**Rule:** Repositories should return only the data a use case needs. If multiple use cases need different projections of recipes, either create use-case-specific methods (`GetRecipeTitlesForFeed`, `GetRecipeDetailsForDisplay`) or use a query parameter object (`GetRecipes(RecipeQuery query)` with selectors). Never load full entities if you only need a few fields.
