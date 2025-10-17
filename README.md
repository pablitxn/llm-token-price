# LLM Token Price Comparison Platform

[![Backend CI](https://github.com/pablitxn/llm-token-price/actions/workflows/backend-ci.yml/badge.svg)](https://github.com/pablitxn/llm-token-price/actions/workflows/backend-ci.yml)
[![Frontend CI](https://github.com/pablitxn/llm-token-price/actions/workflows/frontend-ci.yml/badge.svg)](https://github.com/pablitxn/llm-token-price/actions/workflows/frontend-ci.yml)

A modern web application for comparing and analyzing pricing across Large Language Model (LLM) providers. This platform helps developers and organizations make data-driven decisions about model selection by providing real-time pricing comparisons, cost calculations, and performance benchmarks.

## 📋 Project Overview

This platform enables users to:
- Compare pricing across 50+ LLM models from various providers
- Calculate estimated monthly costs based on usage patterns
- Analyze model performance using standardized benchmarks
- Filter and discover models using smart algorithms
- Visualize cost and performance metrics

**Project Level:** 4 (Enterprise Scale)  
**Target Scale:** 5,000+ monthly active users by month 6

## 🏗️ Architecture

This is a monorepo project using:

### Backend (`services/backend/`)
- **.NET 9** with **Hexagonal Architecture** (Clean Architecture)
- **PostgreSQL 16** for persistent storage
- **Redis 7.2** for caching
- **ASP.NET Core Web API** for REST endpoints

**Project Structure:**
```
services/backend/
├── LlmTokenPrice.Domain/         # Domain entities and business rules
├── LlmTokenPrice.Application/    # Use cases and application logic
├── LlmTokenPrice.Infrastructure/ # Data access and external services
└── LlmTokenPrice.API/            # REST API controllers and endpoints
```

### Frontend (`apps/web/`)
- **React 19** with **TypeScript** (strict mode)
- **Vite** as build tool with Rolldown
- **TailwindCSS 4** for styling
- **Zustand** for client state management
- **TanStack Query** for server state (API caching)
- **TanStack Table** for data tables
- **Chart.js** for data visualization
- **React Router 7** for client-side routing

**Component Structure:**
```
apps/web/src/
├── main.tsx                  # React entry point with providers
├── App.tsx                   # Route configuration
├── /components/
│   └── /layout/              # Layout components
│       ├── Layout.tsx        # Main layout wrapper
│       ├── Header.tsx        # Navigation header
│       └── Footer.tsx        # Page footer
├── /pages/                   # Route components
│   ├── HomePage.tsx          # Landing page (/)
│   ├── CalculatorPage.tsx   # Cost calculator (/calculator)
│   ├── ComparisonPage.tsx   # Model comparison (/compare)
│   └── NotFoundPage.tsx     # 404 handler
├── /api/                     # API client utilities
│   ├── client.ts             # Axios instance with interceptors
│   └── health.ts             # Health check function
└── /styles/
    └── globals.css           # TailwindCSS + custom styles
```

**State Architecture:**
- **Server State (TanStack Query):** API data with 5-minute stale time, 1 retry
- **Client State (Zustand):** Comparison basket, filters, view preferences (future)
- **Local State (useState):** Form inputs, modals, pagination

**Routing:**
- `/` → HomePage (landing page)
- `/calculator` → CalculatorPage (cost calculator)
- `/compare` → ComparisonPage (model comparison)
- `/*` → NotFoundPage (404 handler)

## 🚀 Prerequisites

Before you begin, ensure you have the following installed:

- **Node.js** 20+ ([Download](https://nodejs.org/))
- **.NET 9 SDK** ([Download](https://dotnet.microsoft.com/download))
- **PostgreSQL 16** ([Download](https://www.postgresql.org/download/))
- **Redis 7.2** ([Download](https://redis.io/download))
- **pnpm** (Package manager): `npm install -g pnpm`
- **Git** ([Download](https://git-scm.com/downloads))

Optional:
- **Docker** & **Docker Compose** (for containerized PostgreSQL + Redis)

## 📦 Installation

### 1. Clone the Repository

```bash
git clone <repository-url>
cd llm-token-price
```

### 2. Backend Setup

```bash
cd services/backend

# Restore .NET dependencies
dotnet restore

# Build the solution
dotnet build

# Verify build success
dotnet build --configuration Release
```

### 3. Frontend Setup

```bash
cd apps/web

# Install dependencies
pnpm install

# Verify installation
pnpm run build
```

### 4. Database Setup

**Option A: Using Docker Compose (Recommended)**

From the project root directory:

```bash
# Start PostgreSQL 16 + TimescaleDB and Redis 7.2
docker compose up -d

# Verify containers are running
docker compose ps

# Check PostgreSQL health
docker exec llmpricing_postgres pg_isready -U llmpricing
```

**Services:**
- **PostgreSQL + TimescaleDB:** `localhost:5434` (username: `llmpricing`, password: `dev_password`, database: `llmpricing_dev`)
- **Redis:** `localhost:6379` (no authentication for local development)

**Reset Database (if needed):**
```bash
# Stop containers and remove volumes (deletes all data)
docker compose down -v

# Restart containers
docker compose up -d
```

**Option B: Manual Setup**

1. Install PostgreSQL 16 with TimescaleDB extension
2. Create database and user:
   ```sql
   CREATE USER llmpricing WITH PASSWORD 'your_password';
   CREATE DATABASE llmpricing_dev OWNER llmpricing;
   ```
3. Install and start Redis 7.2
4. Update connection strings in `appsettings.Development.json` accordingly

### 5. Configuration

The `appsettings.Development.json` file is automatically excluded from git (.gitignore).

**Location:** `services/backend/LlmTokenPrice.API/appsettings.Development.json`

**Default configuration (Docker Compose):**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5434;Database=llmpricing_dev;Username=llmpricing;Password=dev_password",
    "Redis": "localhost:6379,abortConnect=false"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft.AspNetCore": "Information",
      "Microsoft.EntityFrameworkCore": "Information"
    }
  }
}
```

**⚠️ Security Note:** The `dev_password` is only for local development. Production credentials are managed via environment variables.

**🔒 Shared Development Environments:** If you are working in a shared development environment (e.g., cloud VM, shared container, or team workspace), you should **immediately change the default password** to prevent unauthorized access to your development database. Update both the `docker-compose.yml` file and the connection string in `appsettings.Development.json` with a unique password.

### 6. Database Seeding

The application **automatically seeds** the database with sample data on first startup in development environments. This provides realistic test data for API endpoints and frontend components without manual configuration.

**Sample Data Includes:**
- **10 LLM Models** from 5 providers:
  - **OpenAI:** GPT-4, GPT-3.5 Turbo
  - **Anthropic:** Claude 3 Opus, Claude 3 Sonnet, Claude 3 Haiku
  - **Google:** Gemini 1.5 Pro, Gemini 1.5 Flash
  - **Meta:** Llama 3 70B, Llama 3 8B (open source, free)
  - **Mistral:** Mistral Large

- **5 Benchmark Definitions:**
  - **MMLU** (Massive Multitask Language Understanding) - Reasoning
  - **HumanEval** - Code Generation
  - **GSM8K** - Math Reasoning
  - **HELM** - Language Understanding
  - **MT-Bench** - Multi-turn Dialogue

- **34+ Benchmark Scores** linking models to performance metrics
- **Model Capabilities** (context windows, feature flags, pricing)

**How It Works:**
1. On first startup, the backend checks if the database is empty
2. If no models exist, it seeds benchmarks → models → capabilities → scores
3. On subsequent startups, seeding is **skipped automatically** (idempotent)
4. All data is timestamped and ready for querying immediately

**Verify Seeding:**
```bash
# Check seeding logs when starting the backend
cd services/backend
ASPNETCORE_ENVIRONMENT=Development dotnet run --project LlmTokenPrice.API

# Look for these log messages:
# ✅ "Seeding database with sample data..."
# ✅ "Seeded 5 benchmarks"
# ✅ "Seeded 10 models with capabilities and benchmark scores"
# ✅ "Database seeded successfully"

# Or on subsequent runs:
# ℹ️ "Database already contains model data. Skipping seed."
```

**Query Sample Data:**
```bash
# Using Docker (PostgreSQL via docker exec)
docker exec llmpricing_postgres psql -U llmpricing -d llmpricing_dev -c \
  'SELECT "Name", "Provider", "InputPricePer1M", "OutputPricePer1M" FROM models ORDER BY "Provider";'

# Expected output: 10 models with pricing from various providers
```

**Reset Database & Re-seed:**
```bash
# Stop containers and remove volumes (deletes ALL data)
docker compose down -v

# Restart containers with fresh database
docker compose up -d

# Start backend - automatic seeding will run
cd services/backend
ASPNETCORE_ENVIRONMENT=Development dotnet run --project LlmTokenPrice.API
```

**Production Note:**
Sample data seeding runs **only in Development environment**. Production databases use manual CSV imports via the admin panel (Epic 4).

## 🏃 Running the Application

### Development Mode

**Terminal 1 - Backend API:**
```bash
cd services/backend/LlmTokenPrice.API
ASPNETCORE_ENVIRONMENT=Development dotnet run
# API will be available at: http://localhost:5000
# Swagger UI: http://localhost:5000/swagger
```

**Terminal 2 - Frontend Dev Server:**
```bash
cd apps/web
pnpm run dev
# Frontend will be available at: http://localhost:5173
```

### Verification Steps

1. **Health Check:** Visit `http://localhost:5000/api/health`
   - Should return `200 OK` with service status
   - Verifies database and Redis connections

2. **Frontend:** Visit `http://localhost:5173`
   - Should load React application
   - Hot Module Replacement (HMR) should be active

## 💻 Development Workflow

### Build Commands

**Backend:**
```bash
cd services/backend

# Build all projects
dotnet build

# Build in Release mode
dotnet build --configuration Release

# Restore dependencies
dotnet restore

# Run the API
dotnet run --project LlmTokenPrice.API
```

**Frontend:**
```bash
cd apps/web

# Start development server (with HMR)
pnpm run dev

# Build for production
pnpm run build

# Type check (TypeScript)
pnpm run type-check

# Lint code
pnpm run lint

# Preview production build
pnpm run preview
```

### Quality Gates

- **Backend Build Time:** < 30 seconds
- **Frontend Build Time:** < 15 seconds
- **Frontend Bundle Size (gzipped):** < 500KB
- **TypeScript:** Zero `any` types in strict mode
- **Build Status:** 0 errors, 0 warnings

### Concurrent Development

Run both backend and frontend simultaneously:

```bash
# Terminal 1
cd services/backend && dotnet run --project LlmTokenPrice.API

# Terminal 2
cd apps/web && pnpm run dev
```

The frontend dev server proxies `/api/*` requests to `http://localhost:5000` automatically.

## 🧪 Testing

### Backend Tests
```bash
cd services/backend
dotnet test
```

### Frontend Tests
```bash
cd apps/web
pnpm run test
```

## 🚦 CI/CD Pipeline

This project uses **GitHub Actions** for automated continuous integration and deployment. All code changes are automatically validated on every push and pull request.

### Pipeline Status

- **Backend CI:** [![Backend CI](https://github.com/pablitxn/llm-token-price/actions/workflows/backend-ci.yml/badge.svg)](https://github.com/pablitxn/llm-token-price/actions/workflows/backend-ci.yml)
- **Frontend CI:** [![Frontend CI](https://github.com/pablitxn/llm-token-price/actions/workflows/frontend-ci.yml/badge.svg)](https://github.com/pablitxn/llm-token-price/actions/workflows/frontend-ci.yml)

### Backend CI Pipeline

**Workflow:** `.github/workflows/backend-ci.yml`

**Triggers:**
- Push to `main` branch (backend files only)
- Pull requests to `main` branch (backend files only)

**Steps:**
1. Checkout code
2. Setup .NET 9 SDK
3. Start PostgreSQL 16 + TimescaleDB service container
4. Start Redis 7.2 service container
5. Restore dependencies (`dotnet restore`)
6. Build solution (`dotnet build --configuration Release`)
7. Run tests (`dotnet test --configuration Release`)

**Service Containers:**
- **PostgreSQL:** `timescale/timescaledb:2.13.0-pg16` on port 5432
- **Redis:** `redis:7-alpine` on port 6379

**Test Database:**
- Database: `llmpricing_test`
- Username: `postgres`
- Password: `test`

### Frontend CI Pipeline

**Workflow:** `.github/workflows/frontend-ci.yml`

**Triggers:**
- Push to `main` branch (frontend files only)
- Pull requests to `main` branch (frontend files only)

**Steps:**
1. Checkout code
2. Setup Node.js 20
3. Setup pnpm 10
4. Cache pnpm store (speeds up subsequent runs)
5. Install dependencies (`pnpm install --frozen-lockfile`)
6. Run type check (`pnpm run type-check`)
7. Run linter (`pnpm run lint`)
8. Build application (`pnpm run build`)

### Running CI Checks Locally

Before pushing changes, verify your code passes all CI checks:

**Backend:**
```bash
cd services/backend

# Run the full CI workflow locally
dotnet restore
dotnet build --configuration Release
dotnet test --configuration Release
```

**Frontend:**
```bash
cd apps/web

# Run the full CI workflow locally
pnpm install --frozen-lockfile
pnpm run type-check
pnpm run lint
pnpm run build
```

### CI/CD Troubleshooting

**Backend Pipeline Failures:**

1. **Service container health check timeout:**
   - PostgreSQL or Redis container failed to start
   - Check GitHub Actions logs for service container errors
   - Verify health check commands in workflow file

2. **Build failures:**
   - Missing dependencies: Run `dotnet restore` locally
   - Version mismatch: Ensure .NET 9 is specified in workflow
   - Check for compilation errors in changed files

3. **Test failures:**
   - Database connection issues: Verify connection string in workflow
   - Flaky tests: Check test logs for timing issues
   - Missing test data: Ensure tests set up required data

**Frontend Pipeline Failures:**

1. **Type check failures:**
   - TypeScript errors: Run `pnpm run type-check` locally
   - Strict mode violations: Fix `any` types and null checks
   - Missing type definitions: Install missing `@types/*` packages

2. **Lint failures:**
   - ESLint errors: Run `pnpm run lint` locally
   - Fix automatically: `pnpm run lint --fix`
   - Check `.eslintrc.json` for rule configurations

3. **Build failures:**
   - Dependency issues: Delete `node_modules` and run `pnpm install`
   - Bundle size exceeded: Check for large dependencies
   - Syntax errors: Run build locally to see detailed errors

4. **Cache issues:**
   - Stale dependencies: Clear GitHub Actions cache
   - Lockfile mismatch: Commit updated `pnpm-lock.yaml`

**Branch Protection (when configured):**
- Pull requests cannot be merged until all CI checks pass
- Required checks: Backend CI and Frontend CI
- Branches must be up to date before merging

## 📦 Caching Architecture

This platform uses **Redis** as a multi-layer caching strategy to optimize performance and reduce database load.

### Caching Strategy

```
Client (TanStack Query, 5min)
  → Redis (1hr TTL)
    → PostgreSQL (source of truth)
```

### Cache Key Naming Conventions

All cache keys follow the pattern: `{InstanceName}:{entity}:{id}:v1`

Examples:
- **Model list**: `llmpricing:models:list:v1`
- **Model detail**: `llmpricing:model:{guid}:v1`
- **QAPS scores**: `llmpricing:qaps:bestvalue:v1`

The `v1` suffix enables cache invalidation on schema changes.

### Time-To-Live (TTL) Strategy

| Cache Type | TTL | Use Case |
|------------|-----|----------|
| API Responses (GET /api/models) | 1 hour | Model lists, benchmark lists |
| Model Detail (GET /api/models/{id}) | 30 minutes | Individual model data |
| Computed Values (QAPS scores) | 1 hour | Smart filter results |

### Redis Testing Commands

**Verify Redis connection:**
```bash
# Using Docker
docker exec llmpricing_redis redis-cli PING
# Expected: PONG

# Check health endpoint
curl http://localhost:5000/api/health
# Expected: {"status":"healthy","services":{"database":{"status":"ok"},"redis":{"status":"ok"}}}
```

**Manual cache operations:**
```bash
# Set a test key with 60-second expiry
docker exec llmpricing_redis redis-cli SET "llmpricing:test:manual" "test-value" EX 60

# Get a key
docker exec llmpricing_redis redis-cli GET "llmpricing:test:manual"

# Check if key exists
docker exec llmpricing_redis redis-cli EXISTS "llmpricing:test:manual"

# Check TTL (time remaining)
docker exec llmpricing_redis redis-cli TTL "llmpricing:test:manual"

# Delete a key
docker exec llmpricing_redis redis-cli DEL "llmpricing:test:manual"

# List all keys (development only)
docker exec llmpricing_redis redis-cli KEYS "llmpricing:*"

# Flush all keys (development only - WARNING: deletes everything!)
docker exec llmpricing_redis redis-cli FLUSHALL
```

### Graceful Degradation

The application is designed to **function without Redis**:
- If Redis is unavailable, cache operations return null/false
- Application logs warnings but continues serving requests
- Database serves as fallback (slower but functional)
- Health endpoint returns `degraded` status (not `unhealthy`)

**Test graceful degradation:**
```bash
# Stop Redis container
docker stop llmpricing_redis

# Verify app still runs (check health endpoint)
curl http://localhost:5000/api/health
# Expected: {"status":"degraded","services":{"database":{"status":"ok"},"redis":{"status":"error"}}}

# Restart Redis
docker start llmpricing_redis
```

### Redis Troubleshooting

**Redis connection refused:**
```bash
# Verify container is running
docker ps --filter "name=llmpricing_redis"

# Check logs
docker logs llmpricing_redis

# Restart container
docker restart llmpricing_redis
```

**Cache not working (keys not persisting):**
- Check TTL hasn't expired: `docker exec llmpricing_redis redis-cli TTL "your-key"`
- Verify key name matches pattern: `llmpricing:{entity}:{id}:v1`
- Check backend logs for serialization errors

**High memory usage:**
```bash
# Check memory stats
docker exec llmpricing_redis redis-cli INFO memory

# Set max memory limit (docker-compose.yml)
# maxmemory: 256mb
# maxmemory-policy: allkeys-lru (evict least recently used)
```

## 🗄️ Database Management

### Entity Framework Migrations

**Generate a new migration:**
```bash
cd services/backend
dotnet ef migrations add MigrationName --project LlmTokenPrice.Infrastructure --startup-project LlmTokenPrice.API
```

**Apply migrations:**
```bash
# Applied automatically on application startup in Development environment
# Or apply manually:
dotnet ef database update --project LlmTokenPrice.Infrastructure --startup-project LlmTokenPrice.API
```

**List migrations:**
```bash
dotnet ef migrations list --project LlmTokenPrice.Infrastructure --startup-project LlmTokenPrice.API
```

**Remove last migration:**
```bash
dotnet ef migrations remove --project LlmTokenPrice.Infrastructure --startup-project LlmTokenPrice.API
```

### Database Connection Verification

**Check health endpoint:**
```bash
curl http://localhost:5000/api/health
# Expected response (when both DB and Redis are healthy):
# {"status":"healthy","services":{"database":{"status":"ok","latencyMs":38.07},"redis":{"status":"ok","latencyMs":3.39}},"timestamp":"..."}
```

**Connect to PostgreSQL (via Docker):**
```bash
docker exec -it llmpricing_postgres psql -U llmpricing -d llmpricing_dev
```

**Common PostgreSQL commands:**
```sql
\dt              -- List all tables
\d table_name    -- Describe table structure
\q               -- Quit psql
```

### Troubleshooting

**Port 5434 already in use:**
```bash
# Check what's using the port
docker ps --filter "publish=5434"
# Stop conflicting container or change port in docker-compose.yml
```

**Database connection refused:**
```bash
# Wait for PostgreSQL health check to pass
docker logs llmpricing_postgres
# Verify container is healthy:
docker compose ps
```

**Password authentication failed:**
- Verify `POSTGRES_PASSWORD` in `docker-compose.yml` matches `Password` in connection string
- Reset: `docker compose down -v && docker compose up -d`

## 🌐 API Documentation

The backend API is built with ASP.NET Core and provides RESTful endpoints for all platform functionality.

### Base URL

**Development:** `http://localhost:5000/api`

### API Endpoints

#### Health Check
```
GET /api/health
```

**Description:** Check the health status of the backend services (database and Redis).

**Response Status Codes:**
- `200 OK` - At least database is healthy (service is functional)
- `503 Service Unavailable` - Database is down (service cannot function)

**Response States:**
- **healthy**: Both database and Redis are connected
- **degraded**: Only database is connected (cache unavailable but functional)
- **unhealthy**: Database is down (service cannot function)

**Response Example (Healthy):**
```json
{
  "status": "healthy",
  "services": {
    "database": {
      "status": "ok",
      "latencyMs": 38.54
    },
    "redis": {
      "status": "ok",
      "latencyMs": 3.42
    }
  },
  "timestamp": "2025-10-16T23:58:22.0614556Z"
}
```

**Response Example (Degraded - Redis Down):**
```json
{
  "status": "degraded",
  "services": {
    "database": {
      "status": "ok",
      "latencyMs": 56.17
    },
    "redis": {
      "status": "error",
      "latencyMs": 0
    }
  },
  "timestamp": "2025-10-16T23:59:11.0303781Z"
}
```

**Response Example (Unhealthy - Database Down):**
```json
{
  "status": "unhealthy",
  "services": {
    "database": {
      "status": "error",
      "latencyMs": 1.46
    },
    "redis": {
      "status": "ok",
      "latencyMs": 0.27
    }
  },
  "timestamp": "2025-10-17T00:01:45.2909397Z"
}
```

### Swagger/OpenAPI Documentation

Interactive API documentation is available via Swagger UI (development environment only).

**Swagger UI:** `http://localhost:5000/swagger`

**Features:**
- Complete API endpoint documentation
- Request/response schema definitions
- "Try it out" functionality for testing endpoints
- Download OpenAPI specification: `http://localhost:5000/swagger/v1/swagger.json`

### CORS Configuration

The API is configured to accept requests from the frontend development server.

**Allowed Origins (Development):**
- `http://localhost:5173` (Vite dev server)

**Production Note:** CORS origins should be configured via environment variables for production deployments.

### JSON Response Format

All API responses use **camelCase** property naming and System.Text.Json serialization.

**Configuration:**
- Property naming: camelCase (e.g., `"latencyMs"` not `"LatencyMs"`)
- Null handling: Null values are excluded from responses
- Enums: Serialized as strings (not integers)
- Timestamps: ISO 8601 format in UTC

### API Troubleshooting

**CORS Errors in Browser Console:**
- Verify the frontend is running on `http://localhost:5173`
- Check that the origin matches exactly (no trailing slash)
- Ensure `app.UseCors()` middleware is placed before `app.UseAuthorization()` in Program.cs

**503 Service Unavailable on /api/health:**
- Database connection is down
- Check Docker Compose: `docker compose ps`
- Verify PostgreSQL is healthy: `docker logs llmpricing_postgres`

**Swagger UI Not Loading (404 Error):**
- Swagger is only available in Development environment
- Verify `ASPNETCORE_ENVIRONMENT=Development` is set
- Check middleware order in Program.cs (Swagger before UseRouting)

**API Not Responding:**
- Verify API is running: `curl http://localhost:5000/api/health`
- Check port 5000 is not in use: `lsof -i:5000`
- Review API logs for startup errors

## 🔨 Building for Production

### Backend
```bash
cd services/backend
dotnet publish -c Release -o ./publish
```

### Frontend
```bash
cd apps/web
pnpm run build
# Output will be in: dist/
```

## 📚 Documentation

- [Product Requirements Document (PRD)](./docs/PRD.md)
- [Epic Breakdown](./docs/epics.md)
- [Solution Architecture](./docs/solution-architecture.md)
- [Technical Specifications](./docs/tech-spec-epic-1.md)
- [UX Specification](./docs/ux-specification.md)

## 🛠️ Tech Stack Summary

| Layer | Technology | Version |
|-------|-----------|---------|
| Frontend Framework | React | 19.x |
| Build Tool | Vite (Rolldown) | 7.x |
| Language | TypeScript | 5.9.x |
| Styling | TailwindCSS | 4.x |
| State Management | Zustand | 5.x |
| Data Fetching | TanStack Query | 5.x |
| Tables | TanStack Table | 8.x |
| Charts | Chart.js | 4.x |
| Backend Framework | ASP.NET Core | .NET 9 |
| Database | PostgreSQL | 16 |
| Cache | Redis | 7.2 |
| Package Manager | pnpm | 10.x |

## 🤝 Contributing

Please read our contributing guidelines before submitting pull requests.

## 📄 License

[License information here]

## 📞 Support

For questions or issues, please contact [support contact].

---

**Last Updated:** 2025-10-16  
**Current Phase:** Epic 1 - Project Foundation & Data Infrastructure
