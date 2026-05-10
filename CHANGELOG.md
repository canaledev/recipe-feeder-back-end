# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added
- Generic translation infrastructure: `translations` table, `ITranslationRepository`, `TranslationService` with three-tier fallback chain (requested language → source language → English) (#1)
- `RequestLocalizationMiddleware` configured for supported cultures: `en`, `es`, `pt`, `fr`, `hi`; unsupported codes fall back to `en` silently (#1)
- `.resx` resource files for all 5 supported languages; `IStringLocalizer<TranslationService>` injected for error message localization (#1)
- Unified error response shape `{ "errorCode": "USER_ALREADY_EXISTS", "message": "..." }` with stable `ErrorCodes` constants (#1)
- `source_language` column on `recipes` table to track content authoring language (#1)
- SQL migrations: `001_add_translations_table.sql`, `002_add_source_language_to_recipes.sql` (#1)
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
