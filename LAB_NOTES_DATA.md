---
name: Dapper & PostgreSQL Data Access
description: SQL query patterns, connection pooling, N+1 problems, transaction handling
type: technical
---

### 1. Dapper queries must include projection mapping to avoid object bloat

**Cause:** When you query a table and map to a domain entity, Dapper tries to map *all* columns to *all* properties. If your SQL returns extra columns (e.g., `SELECT *` or joins with unnecessary columns), Dapper either fails or creates objects with uninitialized properties. This creates silent bugs where properties are null when they shouldn't be.

**Rule:** Always use explicit `SELECT column1, column2, column3` in queries, mapping only the columns needed for the domain entity. If you need extra data (e.g., a joined count), use a separate DTO with the extra properties, or map to a tuple and project into the domain entity afterward.

### 2. PostgreSQL connections must be explicitly returned to the pool

**Cause:** When you create an `IDbConnection` with `new NpgsqlConnection()` and forget to call `.Dispose()` or use a `using` statement, the connection stays open and consumes a slot in the connection pool. Once the pool is exhausted (default 20 connections), new queries time out waiting for an available connection.

**Rule:** Always use `using (var connection = new NpgsqlConnection(connectionString))` or dependency-inject a `DbConnectionFactory` that wraps this pattern. Never create a bare connection and forget to dispose it. Test with connection pool monitoring: `SELECT count(*) FROM pg_stat_activity WHERE datname='feedy_dev'`.

### 3. N+1 queries occur when repositories load related data inside a loop

**Cause:** When you fetch a list of recipes and then loop through each to load its ingredients (one query per recipe), you execute 1 + N queries instead of 1. This is hard to spot in code review because the loop is often in the application layer, not the repository.

**Rule:** Always batch-load related data in a single query using SQL joins, then project/group in-memory. If you must load related data separately, load it once with a `WHERE recipe_id IN (...)` clause for all recipes at once, then join in-memory. Never call a repository method inside a loop.

### 4. Dapper can't map to immutable records without a parameterless constructor

**Cause:** With `record User(Guid Id, string Name);`, Dapper requires a parameterless constructor or positional constructor that matches the property order. If properties are defined out of order or the record uses init-only properties, Dapper fails to map columns to properties.

**Rule:** For Dapper mapping, define records with properties in the same order as the SQL `SELECT` clause. Or use classes with parameterless constructors and public property setters. If using records, ensure the constructor parameter names match the column names from the SQL query.

### 5. Transaction isolation can cause phantom reads in high-concurrency scenarios

**Cause:** PostgreSQL's default isolation level (`READ COMMITTED`) allows phantom reads: a transaction can see newly-inserted rows that other transactions commit between its own reads. In the Smart Feed engine, this means a user's preference might change between when you fetch recipes and when you rank them, causing inconsistent results.

**Rule:** For use cases that require consistency across multiple reads, wrap all queries in a transaction with `SET TRANSACTION ISOLATION LEVEL SERIALIZABLE;` or use `REPEATABLE READ` if serializable is too strict. Document the isolation level choice in the repository method.
