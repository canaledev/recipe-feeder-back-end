# Lab Notes: Build, Lint & Test Configuration

Errors encountered in previous sessions. Rule + root cause. No narrative.
Load this file when hitting **build failures, lint errors, or test discovery issues**.

---

### 1. Tests included in `tsconfig.app.json` break the build

**Cause:** `"include": ["src"]` catches the tests. `tsc -b` fails on Node-only types (`global`, `process`) that appear in tests but don't exist in the browser environment.

**Rules:**
- `tsconfig.app.json` must include only `src/app` and exclude `src/tests/**`
- Mock globals with `vi.stubGlobal('fetch', vi.fn())`, never `global.fetch = vi.fn()`

```json
{ "include": ["src/app"], "exclude": ["src/tests/**"] }
```

---

### 2. ESLint: do not mix components with non-components in the same `.tsx` file

**Cause:** `react-refresh/only-export-components` fails if a `.tsx` exports objects (context, constants) alongside React components.

**Rule:** Separate: `AppContext.tsx` → Provider component only. `useAppContext.ts` → `createContext` and the hook.

---

### 3. `@types/node` is required when `vite.config.ts` uses `path` or `__dirname`

**Cause:** Without `@types/node`, `tsc -b` cannot find `'path'` or `__dirname`.

```bash
npm install -D @types/node
```
```json
// tsconfig.node.json
{ "compilerOptions": { "types": ["node"] } }
```

---

### 4. Vitest inherits Vite's `root` — tests are not found

**Cause:** If `vite.config.ts` sets `root: 'src/app'`, Vitest adopts it too and cannot find `src/tests/**`.

**Rule:** Set Vitest root explicitly:
```ts
test: {
  root: resolve(__dirname, '.'),
  include: ['src/tests/**/*.test.{ts,tsx}'],
  setupFiles: [resolve(__dirname, 'src/tests/setup.ts')],
}
```
