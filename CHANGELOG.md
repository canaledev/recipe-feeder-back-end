# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added
- Playlist Catalog API — 5 new endpoints for browsing, subscribing, and viewing playlists (#18):
  - `GET /api/playlists` — paginated public browse with search, dietary regime, difficulty, and sort filters
  - `GET /api/playlists/subscribed` — authenticated user's active subscriptions with completion-status filter
  - `GET /api/playlists/{id}` — full playlist detail including ordered recipe list with per-item `matchPercentage` (computed by `PlaylistItemRankingService` against user profile) and `completionState`; `currentPlayheadIndex` derived from item completion states
  - `POST /api/playlists/{id}/subscription` — subscribe; seeds `user_playlist_items` rows with `never_done` in a single transaction; returns 409 if already subscribed
  - `DELETE /api/playlists/{id}/subscription` — soft-delete unsubscribe (sets `unsubscribed_at`); completion history preserved; returns 404 if not subscribed
- DB migrations `06_playlists.sql` (playlists + playlist_items) and `07_user_playlist_progress.sql` (user_playlist_subscriptions + user_playlist_items with completion state) (#18)
- 13 unit tests across 5 use case slices covering happy paths, guard-clause errors, and matchPercentage computation (#18)
- JWT refresh token rotation: `POST /auth/refresh` reads the `refresh_token` HttpOnly cookie, rotates it, and returns a new `{ accessToken }`; `POST /auth/logout` (requires Bearer token) revokes all active refresh tokens for the user and clears the cookie (#24)
- Access tokens expire after 15 minutes (`JwtSettings:AccessTokenExpirationMinutes`); refresh tokens expire after 30 days (`JwtSettings:RefreshTokenExpirationDays`) — both configurable (#24)
- Refresh token reuse detection: presenting a previously revoked token triggers immediate family-wide revocation of all sessions for that user (OWASP RFC 8725) (#24)
- `POST /auth/login` and `POST /auth/register` responses updated: `token` field renamed to `accessToken`; refresh token delivered exclusively via `HttpOnly; Secure; SameSite=Strict` cookie (#24)
- Auth endpoints: `POST /api/auth/register` returns `{ token, user: { id, email, fullName } }` (201) or 409 on duplicate email / 422 on validation failure; `POST /api/auth/login` returns the same shape (200) or 401 on invalid credentials — same error message for unknown email and wrong password to prevent account enumeration (#17)
- `IPasswordHasher` and `IJwtTokenProvider` interfaces in `Feedy.Application/Interfaces/`; implementations (`BcryptPasswordHasher`, `JwtTokenProvider`) in `Feedy.Infrastructure/Auth/`; BCrypt.Net-Next used for password hashing; tokens are HS256 JWTs with expiry from `JwtSettings:ExpirationMinutes` (#17)
- `users` DB table migration (`db/infrastructure/04_users.sql`): id, email, password_hash, full_name, created_at, last_login_at; unique constraint on email (#17)
- 10 unit tests covering `RegisterUserService` and `LoginUserService` including anti-enumeration verification (#17)
- Generic translation infrastructure: `translations` table, `ITranslationRepository`, `TranslationService` with three-tier fallback chain (requested language → source language → English) (#1)
- `RequestLocalizationMiddleware` configured for supported cultures: `en`, `es`, `pt`, `fr`, `hi`; unsupported codes fall back to `en` silently (#1)
- `.resx` resource files for all 5 supported languages; `IStringLocalizer<TranslationService>` injected for error message localization (#1)
- Unified error response shape `{ "errorCode": "USER_ALREADY_EXISTS", "message": "..." }` with stable `ErrorCodes` constants (#1)
- `source_language` column on `recipes` table to track content authoring language (#1)
- SQL migrations: `001_add_translations_table.sql`, `002_add_source_language_to_recipes.sql` (#1)
- CORS policy `AllowFrontend`: restricts allowed methods to GET, POST, PUT, DELETE, PATCH; reads allowed origins from `CorsOrigins` in `appsettings.json`; includes `https://feedy.app` as the production origin (#6)
- Unit tests for `CorsConfigurationTests` covering allowed origins, explicit method list, `AllowAnyMethod = false`, and `AllowAnyHeader = true` (#6)
- Global exception handler middleware (`GlobalExceptionHandlerMiddleware`): catches all unhandled exceptions, logs full stack trace via Serilog, and returns a RFC 7807 `ProblemDetails` 500 response; exception detail included only in Development environment (#2)
- `Feedy.Api.Tests` project: unit tests for `GlobalExceptionHandlerMiddleware` covering happy path, 500 status, `application/problem+json` content type, error logging, dev/prod detail visibility (#2)
- Health check endpoint at `GET /health`: returns JSON `{ status, checks }` with PostgreSQL reachability via `AspNetCore.HealthChecks.NpgSql`; responds 200 Healthy / 503 Unhealthy; anonymous, no auth required (#7)

### Added
- `Result<T>` discriminated union in `Feedy.Domain.Common`: application services return success or failure without throwing exceptions for business rule violations
- `Nothing` unit type for `Result<Nothing>` — used when an operation succeeds but returns no data
- `Error` record (`Code`, `Message`) carried by failed results; `Code` is machine-readable for HTTP status mapping
- Swashbuckle OpenAPI/Swagger UI at `/swagger` (development only); XML doc comments enabled so controller `<summary>` tags and `[ProducesResponseType]` attributes render in the UI (#5)
- FluentValidation auto-validation: request format and completeness checked before controllers run, producing `400 ValidationProblemDetails` automatically

### Changed
- API layer migrated from Minimal APIs to ASP.NET Core Controllers (`[ApiController]`, `ControllerBase`)
- Error handling: `try-catch` removed from controllers; format errors surface via FluentValidation, business rule violations via `Result<T>`; HTTP status codes mapped explicitly in controller actions (201, 400, 409, 422, 500)
- Command/Query objects replaced with plain `Request`/`Response` records per use case; handlers renamed to `Service` classes
- Each layer now owns its own DI registration via `AddDomain()`, `AddApplication()`, and `AddInfrastructure(IConfiguration)` extension methods — `Program.cs` no longer imports infrastructure or application namespaces

### Added
- Structured logging via Serilog: compact JSON in production, human-readable template in development (#3)
- Serilog enrichers: `MachineName`, `EnvironmentName`, `ThreadId` on every log event (#3)
- `UseSerilogRequestLogging()` middleware: one structured event per HTTP request, replacing verbose framework logs (#3)
- Local database infrastructure via Docker: `db/infrastructure/` (schema scripts) and `db/data/` (seed scripts) with incremental migration runner `db/migrate.ps1` (#9)
- `schema_migrations` tracking table: records each applied script by name so `migrate.ps1` is fully idempotent — re-running never re-applies an already-applied script (#9)
- Internationalization (i18n): auto-detects system language on first load; supports English, Spanish, Portuguese, French, and Hindi (#18)
- Settings screen accessible from the side menu with a language selector (native names + flag icons)
- Translation files lazy-loaded from `public/locales/{lng}/translation.json` to keep the bundle small
- All outgoing API requests include the `Accept-Language` header with the active language code
- Language preference persisted to `localStorage` and restored across sessions

### Changed
- Documentation: Incorporated Karpathy behavioral guidelines (anti-slop) into CLAUDE.md (#8)
- Design system: primary lime token updated to `#b4dc62`
- All card titles (Bitter font) set to `font-bold` across every screen for consistent typographic hierarchy
- Card shadow elevated to `shadow-md` across all screens (HomeScreen, FavoritesScreen, CollectionsScreen, CreatePlaylistScreen)
- Playlist tag chips styled with warm amber palette: `#fff8dd` background / `#ffa700` label

### Added
- Browse & Subscribe to Playlists screen (`BrowsePlaylistsScreen`): debounced search (300 ms), filter panel (dietary regime / difficulty / max duration), sort by relevance / popularity / newest / match percentage, infinite scroll via `IntersectionObserver`, skeleton loaders, empty state, and cover image per card
- Playlist service layer (`playlistService.ts`): `listPlaylists`, `subscribeTo`, `unsubscribeFrom` — REST contract documented inline for the .NET 8 backend
- `usePlaylistBrowse` hook: TanStack Query `useInfiniteQuery` with page size 10
- `usePlaylistSubscribe` hook: optimistic mutation that removes the subscribed playlist from all cached pages without a refetch
- Mock client extended to handle `GET /playlists` (filter, sort, paginate) and `POST/DELETE /playlists/{id}/subscribe` with in-memory subscription state
- 50 mock playlists with Unsplash cover images, varied dietary tags, difficulty levels, and match percentages

## [0.3.0] — 2026-04-30

### Added
- Dev-mode mock API client: simulates all backend responses (register, playlist list, subscribe) so the app runs fully without a configured `.NET 8` server
- `VITE_API_BASE_URL` environment variable: when set, the real backend is used; otherwise the mock client activates automatically

### Changed
- Auth screens aligned to design references: vibrant palette (emerald, terracotta, ochre tokens), relaxed password strength gate (Fair or above accepted)

## [0.2.0] — 2026-02-01

### Added
- All main app screens implemented from design handoff:
  - **HomeScreen** (Smart Feed): swipeable recipe cards with `matchPercentage` badge
  - **RecipeDetailScreen**: ingredient checklist, nutritional summary, difficulty/time stats
  - **BrowsePlaylistsScreen**: playlist grid (pre-integration shell)
  - **CollectionsScreen**: user's personal recipe collections
  - **CreatePlaylistScreen**: playlist builder with recipe selector, tag/difficulty/duration filters
  - **FavoritesScreen**: saved recipes organised by folder
  - **OnboardingScreen**: flavor preferences, ingredient exclusions, dietary regime selection
- Auth-based routing: `SplashScreen` gates the app and routes to onboarding or home based on `authStatus`
- `RegisterPage` with email/password form and social OAuth buttons (Google, Apple, Facebook)
- `PasswordStrengthIndicator` component
- `StarRating` shared component
- `AppLayout` with `AppHeader` and `SideMenu` (hamburger navigation)
- Side menu navigation wired to all screens

### Changed
- Project restructured into `src/app/` (deployable) and `src/tests/` (all test files)

## [0.1.0] — 2025-12-01

### Added
- Base project architecture: React 18 + Vite + TypeScript
- Tailwind CSS configured with organic design tokens (emerald greens, terracottas, ochres)
- TanStack Query `QueryClientProvider` wired at app root with 5-minute stale time
- React Context (`AppContext` + `useAppContext`) for global client-side state
- Centralized API client (`src/app/lib/api.ts`) for all .NET 8 backend communication
- PWA support via `vite-plugin-pwa`: `manifest.json` + Service Worker with offline caching strategy
- Feature-based folder structure (`features/feed`, `features/onboarding`, `features/recipe`, `features/playlists`, `features/favorites`, `features/auth`)
- Google Fonts: Bitter (serif, headings) + Inter (body)
- Vitest + Testing Library setup
- ESLint + Prettier configured
- Mobile-first layout shell constrained to `max-w-md` on desktop

[Unreleased]: https://github.com/canaledev/recipe-feeder-front-end/compare/v0.3.0...HEAD
[0.3.0]: https://github.com/canaledev/recipe-feeder-front-end/compare/v0.2.0...v0.3.0
[0.2.0]: https://github.com/canaledev/recipe-feeder-front-end/compare/v0.1.0...v0.2.0
[0.1.0]: https://github.com/canaledev/recipe-feeder-front-end/releases/tag/v0.1.0
