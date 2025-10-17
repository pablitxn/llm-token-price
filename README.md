# LLM Token Price Comparison Platform

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
