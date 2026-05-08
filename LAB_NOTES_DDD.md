---
name: Domain-Driven Design Patterns
description: Aggregate roots, value objects, bounded contexts, domain events, ubiquitous language
type: technical
---

### 1. Aggregate Roots must encapsulate all state changes via public methods, never direct property assignment

**Cause:** When aggregate root properties are mutable (settable), external code can bypass business rules and leave the aggregate in an inconsistent state. For example, updating `User.Email` directly bypasses email validation and duplicate-check logic.

**Rule:** Aggregate root entities must have immutable public properties (records with `init-only` or private setters). All state changes go through public methods that enforce invariants. Example: `user.ChangeEmail(newEmail)` validates and checks for duplicates before updating.

### 2. Value Objects must be immutable and compared by value, not identity

**Cause:** A `Money` or `Email` value object that is mutable can be changed unexpectedly, corrupting dependent aggregates. For example, if `Email` is a mutable class, changes to a single `Email` instance affect all aggregates that reference it.

**Rule:** All value objects are immutable records with no setters. Comparison uses `Equals()` and `GetHashCode()` to compare by value, not reference. When a value object needs to change, replace it entirely: `user = user.WithEmail(newEmail)`.

### 3. Repository queries must load full Aggregate Roots, not partial projections

**Cause:** Loading partial data (e.g., only `User.Id` and `User.Email`, omitting `User.Profile`) leaves the aggregate incomplete and violates the Aggregate Root boundary. Logic in Application handlers may assume the full aggregate is loaded.

**Rule:** Repositories load entire aggregate roots with all owned entities. If you need only a projection (e.g., `UserId` and `Email` for a dropdown), create a separate read model or query object that does not pretend to return an aggregate.

### 4. Domain Services encapsulate logic that spans multiple aggregates but has no natural home in either

**Cause:** When business logic that involves two aggregates (e.g., "Check if a user's preferred ingredient conflicts with a recipe") is scattered across Application handlers or repositories, it becomes hard to maintain and re-use.

**Rule:** Create Domain Services in the Domain layer for cross-aggregate logic. Domain Services take aggregate roots as parameters and enforce invariants across them. They are stateless and never persist state themselves.

### 5. Bounded Contexts must have separate models and repositories; no sharing of aggregates across contexts

**Cause:** When a `Recipe` aggregate in the "Catalog" context is used directly in the "Feed" context, changes to one context's requirements force changes to the shared model, creating hidden coupling.

**Rule:** Each Bounded Context owns its own aggregate models, even if they represent the same real-world entity. A `Recipe` in Catalog and a `Recipe` in Feed are separate aggregates with different responsibilities. Cross-context communication happens via Anti-Corruption Layers (mappers, value objects, or integration events).

### 6. Domain Events record what happened and are published to other aggregates and external systems

**Cause:** When aggregates silently change state without notifying the system, downstream logic (e.g., sending an email when a user registers) must poll or use external triggers.

**Rule:** Aggregates record immutable `DomainEvent` records when business events occur (e.g., `UserRegisteredEvent`, `RecipeAddedToFeedEvent`). These events are persisted alongside the aggregate and published to Application handlers or external systems via an event bus.

### 7. Ubiquitous Language must be enforced in code: domain terms are class names, property names, and method names

**Cause:** When code uses generic names (`Data1`, `Update()`, `Validate()`), developers unfamiliar with the domain cannot read the code or know what rules apply.

**Rule:** Every class, method, and property name reflects the domain language. Instead of `Update()`, use `ChangeEmail()`, `SubscribeToPlaylist()`, `RankRecipes()`. Domain experts should recognize the code as a direct expression of domain rules.
