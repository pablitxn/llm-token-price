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
- **React 19** with **TypeScript**
- **Vite** as build tool with Rolldown
- **TailwindCSS** for styling
- **Zustand** for state management
- **TanStack Query** for data fetching
- **TanStack Table** for data tables
- **Chart.js** for data visualization

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

```bash
cd services/backend/LlmTokenPrice.API
docker-compose up -d
```

**Option B: Manual Setup**

1. Start PostgreSQL server
2. Create database:
   ```sql
   CREATE DATABASE llm_token_price;
   ```
3. Start Redis server:
   ```bash
   redis-server
   ```

### 5. Configuration

Create `appsettings.Development.json` in `services/backend/LlmTokenPrice.API/`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=llm_token_price;Username=your_user;Password=your_password",
    "Redis": "localhost:6379"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

## 🏃 Running the Application

### Development Mode

**Terminal 1 - Backend API:**
```bash
cd services/backend/LlmTokenPrice.API
dotnet run
# API will be available at: http://localhost:5000
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
