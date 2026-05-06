# Feedy — Recipe Feeder Frontend

A mobile-first Progressive Web App (PWA) for personalized recipe discovery. Feedy's core feature is a **Smart Feed** that surfaces algorithmically curated recipes based on each user's nutritional and psychographic profile.

> **Backend:** Separate .NET 8 service (not included in this repository).

## Tech Stack

| Layer | Technology |
|---|---|
| Framework | React 18 + Vite + TypeScript |
| Styling | Tailwind CSS (design tokens) |
| Server state | TanStack Query |
| Client state | React Context API |
| UI primitives | Headless UI / Radix UI |
| Icons | Lucide-React (functional) + Phosphor Icons (decorative) |
| Typography | Bitter (headings, via Google Fonts) · Inter / Montserrat (body) |

## Getting Started

### Prerequisites

- Node.js 20+
- npm 10+

### Installation

```bash
git clone https://github.com/canaledev/recipe-feeder-front-end.git
cd recipe-feeder-front-end
npm install
```

### Environment variables

Copy the example file and fill in the required values:

```bash
cp .env.example .env.local
```

| Variable | Description |
|---|---|
| `VITE_API_BASE_URL` | Base URL of the .NET 8 backend API |

## Available Scripts

```bash
npm run dev        # Start development server
npm run build      # Type-check and build for production
npm run preview    # Preview the production build locally
npm run lint       # Run ESLint
npm run test       # Run unit tests (Vitest)
```

## Project Structure

The codebase follows a **feature-based folder structure**:

```
src/
├── features/       # One folder per domain feature (feed, onboarding, recipe, ...)
├── components/     # Shared UI primitives
├── hooks/          # Shared custom hooks
├── lib/            # API client and third-party integrations
└── types/          # Global TypeScript types and interfaces
```

## PWA

The app is configured as a PWA with offline support via Service Workers and a `public/manifest.json`. It is optimized for mobile browsers and constrained to `max-w-md` on desktop to preserve the mobile layout.

## Contributing

See [CHANGELOG.md](./CHANGELOG.md) for the full history of changes.  
Issues and feature requests are tracked at [github.com/canaledev/recipe-feeder-front-end/issues](https://github.com/canaledev/recipe-feeder-front-end/issues).

## License

[MIT](./LICENSE)
