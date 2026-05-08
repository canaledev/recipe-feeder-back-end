---
name: xUnit, Moq, and TestContainers Patterns
description: Test project structure, mocking strategy, integration test setup, assertion style
type: technical
---

### 1. Test class naming must match the class being tested

**Cause:** If you test a class named `GetRecipeFeedService` but name the test class `RecipeFeedTests`, future developers looking for tests will search for `GetRecipeFeedServiceTests` and find nothing, assuming there are no tests.

**Rule:** Name test classes `<ClassBeingTested>Tests`. If a class has multiple concerns, split it into multiple focused classes and test each one. The test project folder structure must mirror `src/` structure exactly (e.g., `src/Application/UseCases/GetRecipeFeed/` maps to `tests/Application.Tests/UseCases/GetRecipeFeed/`).

### 2. Arrange-Act-Assert sections must be separated by blank lines

**Cause:** When test methods are dense and lack visual separation, it's hard to see what is setup, what is being tested, and what is verified. This leads to unclear test intent and bugs going unnoticed.

**Rule:** Every test method must follow the three-section pattern with blank lines between them:
```csharp
// Arrange
var mockRepo = new Mock<IRecipeRepository>();
// Act
var result = handler.Handle(command, CancellationToken.None);
// Assert
result.Should().NotBeNull();
```

### 3. Moq setup calls must match the exact parameter types and values being passed

**Cause:** If you mock `repository.GetRecipe(It.IsAny<Guid>())` but the handler calls `GetRecipe(recipeId)` with a specific ID, Moq might fail to match if the parameter type doesn't match exactly (e.g., `int` vs `long`). This causes silent test failures where the mock is not invoked.

**Rule:** Use exact parameter matchers (`It.Is<Guid>(g => g == expectedId)`) for critical parameters; use `It.IsAny<T>()` only for parameters that are truly irrelevant to the test. Always verify the mock was called: `mockRepo.Verify(r => r.GetRecipe(...), Times.Once);`

### 4. Integration tests must use TestContainers for PostgreSQL, never a shared database

**Cause:** When multiple developers or CI pipelines run tests against a shared PostgreSQL database, tests interfere with each other (one test deletes data another test depends on), causing flaky, non-deterministic failures.

**Rule:** Use `Testcontainers.PostgreSql` to spin up an isolated PostgreSQL container per test run. Define a base class `PostgresSqlTestBase` that creates a container in `SetUp`, migrates the schema, and tears down in `TearDown`. Each integration test inherits from this and runs in isolation.

### 5. Assertion libraries must use FluentAssertions for readability

**Cause:** With raw xUnit assertions (`Assert.NotNull(result)`), error messages are cryptic and don't explain what went wrong. FluentAssertions produces readable error messages like `Expected result to be null, but found instance of X.`

**Rule:** Use FluentAssertions for all assertions: `result.Should().NotBeNull();`, `items.Should().HaveCount(5);`, `value.Should().BeGreaterThan(0);`. Never mix xUnit assertions with FluentAssertions in the same project. This applies to all test projects.
