---
name: Docker & Container Configuration
description: Dockerfile layers, environment variables, PostgreSQL setup, image size optimization
type: technical
---

### 1. Multi-stage Dockerfile reduces image size and attack surface

**Cause:** If you build the .NET app in the same stage where you run it, the final image includes the SDK (~1GB), build artifacts, and intermediate files. This bloats the image and increases the attack surface for security scanning.

**Rule:** Always use a three-stage Dockerfile: (1) `build` stage with SDK to compile, (2) `publish` stage to create the release bundle, (3) `final` stage with only the runtime to run the app. This reduces image size from ~1GB to ~200MB.

### 2. Environment variables in Docker must be set at container start, not baked into the image

**Cause:** If you run `ENV ASPNETCORE_ENVIRONMENT=Production` in the Dockerfile, the environment is baked into the image. To use a different environment (dev vs. prod), you'd need to rebuild the image, which is inefficient and error-prone.

**Rule:** Set environment variables via `docker run -e VAR=value` or in `docker-compose.yml` under `environment:`. The Dockerfile should only set variables that are truly immutable (e.g., `ASPNETCORE_URLS=http://+:5000`).

### 3. PostgreSQL connection strings in Docker must use the service name, not localhost

**Cause:** When running multiple containers with Docker Compose, `localhost` or `127.0.0.1` refers to the container's own network interface, not the PostgreSQL container. The connection string `Host=localhost` will fail with "connection refused."

**Rule:** In a docker-compose network, use the service name as the hostname: `Host=postgres;Port=5432;`. Docker's internal DNS automatically resolves the service name to the container's IP. Only use `localhost` when connecting from the host machine *outside* Docker.

### 4. Docker Compose healthchecks must verify actual database readiness, not just port availability

**Cause:** When you define a health check as `tcp://postgres:5432`, the port may be open but the database may not have finished initializing. A dependent service (API) may start before the database is ready, causing connection errors.

**Rule:** Use application-level health checks: `pg_isready -U postgres` for PostgreSQL. For the API, use `curl http://localhost:5000/health`. Define `depends_on` with a `condition: service_healthy` to ensure the service is truly ready before starting dependents.

### 5. .dockerignore must exclude large directories and version control metadata

**Cause:** If you build a Docker image without a `.dockerignore`, the build context includes `node_modules/`, `.git/`, `bin/`, `obj/`, and other large directories. This slows the build and increases upload time to registries.

**Rule:** Create a `.dockerignore` file that excludes: `**/.git`, `**/.vs`, `**/bin`, `**/obj`, `**/*.user`, `.env.local`, and any large directories. This reduces build context from 500MB+ to 50MB.
