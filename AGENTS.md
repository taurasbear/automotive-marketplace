# Automotive Marketplace

Full-stack car buy/sell marketplace. ASP.NET Core 8 backend (Clean Architecture + CQRS) + React 19 + TypeScript frontend. Fully containerized with Docker Compose.

## Key Commands

```sh
# Backend
dotnet build ./Automotive.Marketplace.sln
dotnet test ./Automotive.Marketplace.sln
dotnet test --filter "FullyQualifiedName~MyHandlerTests" ./Automotive.Marketplace.sln
dotnet run --project Automotive.Marketplace.Server

# Frontend (cd automotive.marketplace.client first)
npm run dev
npm run build
npm run lint && npm run format:check

# Docker (dev)
docker compose --env-file .env up -d
```

## Architecture

**Backend layers:** `Domain` → `Application` (CQRS handlers, interfaces) → `Infrastructure` (EF Core, services) → `Server` (controllers, middleware)

**Frontend:** Feature-based under `src/features/`. Shared UI in `src/components/`. API calls via TanStack Query + Axios in `src/api/` or inside each feature's `api/` subfolder.

**Auth:** Short-lived JWT access token in Redux store + long-lived refresh token in HttpOnly cookie. Axios interceptor handles refresh on 401 automatically.

**Testing:** xUnit + TestContainers (real PostgreSQL per test class) + Respawn (state reset between tests) + Bogus builders + NSubstitute + FluentAssertions.

## Agent Preferences

When requesting input from the user, **always use dropdown select options** via the `ask_user` tool's `choices` parameter instead of asking the user to write a follow-up prompt. This provides a better UX and ensures responses are captured properly.

Prefer multiple choice (with choices array) over freeform input whenever possible.

## Project-Specific Skills

See `.agents/skills/` for patterns on:

- `be-cqrs-feature` — adding a new backend CQRS feature
- `be-handler-tests` — writing handler integration tests
- `fe-feature-structure` — frontend feature directory layout
- `fe-api-calls` — query options and mutation hooks
- `be-query-response-classes` - how to write query and response classes
