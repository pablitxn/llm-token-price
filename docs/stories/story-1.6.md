# Story 1.6: Create Basic API Structure with Health Endpoint

Status: Ready

## Story

As a developer,
I want a functional REST API with health check endpoint and CORS configuration,
So that I can verify backend/frontend connectivity and begin building API endpoints.

## Acceptance Criteria

1. HealthController created with GET /api/health endpoint returning database and Redis connection status
2. CORS configured to allow requests from frontend development server (http://localhost:5173)
3. Swagger/OpenAPI documentation auto-generated and accessible at /swagger endpoint
4. API returns proper JSON responses with content-type: application/json headers
5. Health endpoint returns 200 OK when both database and Redis are healthy, 503 Service Unavailable when database fails
6. Backend API accessible from frontend with successful CORS preflight requests

## Tasks / Subtasks

- [ ] Create HealthController in API layer (AC: 1, 5)
  - [ ] Create `Backend.API/Controllers/HealthController.cs` with `[ApiController]` and `[Route("api/[controller]")]` attributes
  - [ ] Inject `AppDbContext` and `IConnectionMultiplexer` via constructor dependency injection
  - [ ] Implement GET endpoint with `[HttpGet]` attribute
  - [ ] Check database health: `var dbHealth = await _context.Database.CanConnectAsync();`
  - [ ] Check Redis health: `var redisHealth = _redis.IsConnected;`
  - [ ] Return JSON response with status ("healthy", "degraded", "unhealthy"), services object (database, redis status), and timestamp
  - [ ] Return 200 OK if database healthy (regardless of Redis), 503 Service Unavailable if database fails
  - [ ] Add XML documentation comments explaining health check logic

- [ ] Configure CORS in Program.cs (AC: 2, 6)
  - [ ] Open `Backend.API/Program.cs` and locate service registration section (before `var app = builder.Build();`)
  - [ ] Add CORS services: `builder.Services.AddCors(options => { ... });`
  - [ ] Configure default policy: allow origin `http://localhost:5173`, allow any header, allow any method
  - [ ] Add CORS middleware: `app.UseCors();` (must be before `app.UseAuthorization()`)
  - [ ] Test CORS: use browser dev tools to verify OPTIONS preflight requests succeed
  - [ ] Document CORS configuration in README.md with note about production origin configuration

- [ ] Configure Swagger/OpenAPI documentation (AC: 3)
  - [ ] Verify Swashbuckle.AspNetCore package installed (should be from Story 1.2)
  - [ ] Add Swagger services in Program.cs: `builder.Services.AddEndpointsApiExplorer(); builder.Services.AddSwaggerGen();`
  - [ ] Configure Swagger metadata: set API title "LLM Pricing Comparison API", version "v1", description
  - [ ] Add Swagger middleware (development only): `if (app.Environment.IsDevelopment()) { app.UseSwagger(); app.UseSwaggerUI(); }`
  - [ ] Test Swagger UI: navigate to `http://localhost:5000/swagger` and verify API documentation loads
  - [ ] Verify health endpoint appears in Swagger UI with GET operation

- [ ] Configure JSON serialization options (AC: 4)
  - [ ] Update Program.cs to configure JSON options: `builder.Services.AddControllers().AddJsonOptions(options => { ... });`
  - [ ] Set camelCase property naming: `options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;`
  - [ ] Ignore null values: `options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;`
  - [ ] Configure datetime format: `options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());`
  - [ ] Test JSON response format: call health endpoint and verify camelCase response (e.g., `"timestamp"` not `"Timestamp"`)

- [ ] Test health endpoint functionality (AC: 1, 5)
  - [ ] Start backend API: `dotnet run --project Backend.API`
  - [ ] Test healthy state: `curl http://localhost:5000/api/health` with database and Redis running, verify 200 OK and `"status": "healthy"`
  - [ ] Test degraded state: stop Redis, call health endpoint, verify 200 OK but `"status": "degraded"`, `"redis": "error"`
  - [ ] Test unhealthy state: stop PostgreSQL, call health endpoint, verify 503 Service Unavailable
  - [ ] Verify JSON structure matches: `{ "status": "...", "services": { "database": "...", "redis": "..." }, "timestamp": "..." }`
  - [ ] Test from browser: open `http://localhost:5000/api/health` and verify JSON renders correctly

- [ ] Test frontend-to-backend connectivity via CORS (AC: 6)
  - [ ] Start backend API: `dotnet run --project Backend.API` (port 5000)
  - [ ] Start frontend dev server: `npm run dev` in /frontend directory (port 5173)
  - [ ] Open browser dev tools Network tab
  - [ ] From frontend, make test API call: `fetch('http://localhost:5000/api/health').then(r => r.json()).then(console.log)`
  - [ ] Verify OPTIONS preflight request succeeds (status 204) with Access-Control-Allow-Origin header
  - [ ] Verify GET request succeeds (status 200) with health data returned
  - [ ] Check for CORS errors in browser console (should be none)

- [ ] Document API structure and verify all components (AC: 1-6)
  - [ ] Update README.md with "API Documentation" section explaining health endpoint, Swagger URL
  - [ ] Document API base URL: `http://localhost:5000/api` for development
  - [ ] Document health check response format with example JSON
  - [ ] Create troubleshooting section: CORS errors (check origin whitelist), 503 errors (database connection), Swagger not loading (check environment)
  - [ ] Create API verification checklist: backend starts on port 5000, health endpoint returns 200, Swagger accessible, CORS works from frontend
  - [ ] Verify all acceptance criteria: run through checklist and confirm all 6 criteria met

## Dev Notes

### Architecture Constraints

**From solution-architecture.md Section 2.4 - API Structure:**
- **RESTful endpoints pattern:** `/api/{controller}` for all endpoints
- **Health check endpoint:** GET /api/health returns service status (database, Redis, timestamp)
- **Response format:** `{ "data": {...}, "meta": {...} }` for success, `{ "error": {...} }` for errors (implement in Epic 2)
- **CORS:** Development allows `http://localhost:5173`, production configured via environment variables

**From solution-architecture.md Section 1.1 - Technology Stack:**
- **Swagger/OpenAPI:** Swashbuckle.AspNetCore 6.5.0 for auto-generated API documentation
- **JSON serialization:** System.Text.Json (not Newtonsoft.Json) for .NET 8 consistency
- **API documentation:** Swagger UI with "try it out" functionality for manual testing

**From tech-spec-epic-1.md Story 1.6:**
- **HealthController structure:** Inject AppDbContext and IConnectionMultiplexer, check connectivity, return status JSON
- **CORS policy:** `WithOrigins("http://localhost:5173").AllowAnyHeader().AllowAnyMethod()`
- **Swagger configuration:** Development-only middleware (`if (app.Environment.IsDevelopment())`)

### Project Structure Notes

**API layer files:**
```
/backend/
└── Backend.API/
    ├── Program.cs (updated with CORS, Swagger, JSON options)
    ├── Controllers/
    │   └── HealthController.cs (health check endpoint)
    └── appsettings.json (API configuration)
```

**HealthController structure:**
```csharp
namespace Backend.API.Controllers
{
    /// <summary>
    /// Health check endpoint for monitoring service availability.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class HealthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConnectionMultiplexer _redis;
        private readonly ILogger<HealthController> _logger;

        public HealthController(
            AppDbContext context,
            IConnectionMultiplexer redis,
            ILogger<HealthController> logger)
        {
            _context = context;
            _redis = redis;
            _logger = logger;
        }

        /// <summary>
        /// Check health of database and cache services.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
        public async Task<IActionResult> Get()
        {
            var dbHealth = await _context.Database.CanConnectAsync();
            var redisHealth = _redis.IsConnected;

            var status = dbHealth && redisHealth ? "healthy"
                       : dbHealth ? "degraded"
                       : "unhealthy";

            var response = new
            {
                status,
                services = new
                {
                    database = dbHealth ? "ok" : "error",
                    redis = redisHealth ? "ok" : "error"
                },
                timestamp = DateTime.UtcNow
            };

            return dbHealth
                ? Ok(response)
                : StatusCode(StatusCodes.Status503ServiceUnavailable, response);
        }
    }
}
```

**Program.cs CORS and Swagger configuration:**
```csharp
// CORS (before builder.Build())
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod());
});

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "LLM Pricing Comparison API",
        Version = "v1",
        Description = "REST API for LLM model pricing and benchmark data"
    });
});

// JSON options
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
});

var app = builder.Build();

// Middleware (order matters!)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors(); // Before UseAuthorization
app.UseAuthorization();
app.MapControllers();
```

### Testing Standards Summary

**Health endpoint validation:**
1. Healthy state test: Both services OK → 200 OK, `"status": "healthy"`
2. Degraded state test: Redis down → 200 OK, `"status": "degraded"`
3. Unhealthy state test: Database down → 503 Service Unavailable, `"status": "unhealthy"`
4. JSON format test: Verify camelCase, no null values, timestamp in ISO 8601 format

**CORS validation:**
1. Preflight test: OPTIONS request from frontend → 204 No Content
2. GET request test: Actual request from frontend → 200 OK with data
3. Headers test: Verify `Access-Control-Allow-Origin: http://localhost:5173`
4. No errors in browser console

**Swagger validation:**
1. Swagger UI loads: Navigate to http://localhost:5000/swagger
2. Health endpoint documented: GET /api/health visible with description
3. Try it out works: Execute request from Swagger UI, verify response

**Quality gates:**
- API startup time: <5 seconds
- Health check response time: <200ms
- Swagger UI load time: <2 seconds

### Dependencies and Prerequisites

**Prerequisites from previous stories:**
- Story 1.2: Swashbuckle.AspNetCore package installed
- Story 1.3: AppDbContext available for injection
- Story 1.5: IConnectionMultiplexer available for injection

**Common issues and solutions:**
1. **CORS error in browser**: Verify origin matches exactly (http://localhost:5173, no trailing slash)
2. **503 on health check**: Database not running, check Docker Compose
3. **Swagger 404**: Verify development environment, check middleware order
4. **JSON not camelCase**: Verify JSON options configured before `builder.Build()`

### References

- [Source: docs/solution-architecture.md#Section 2.4 - API Structure and RESTful endpoints]
- [Source: docs/solution-architecture.md#Section 2.4 - Response format standards]
- [Source: docs/solution-architecture.md#Section 1.1 - Swashbuckle.AspNetCore 6.5.0]
- [Source: docs/tech-spec-epic-1.md#Story 1.6 - HealthController implementation]
- [Source: docs/epics.md#Epic 1, Story 1.6 - Acceptance Criteria]
- [Source: docs/PRD.md#NFR004 - Availability: graceful degradation with health checks]

## Dev Agent Record

### Context Reference

<!-- Path(s) to story context XML will be added here by context workflow -->

### Agent Model Used

<!-- Agent model information will be populated during development -->

### Debug Log References

<!-- Debug logs will be added during development -->

### Completion Notes List

<!-- Completion notes will be added after story implementation -->

### File List

<!-- Modified/created files will be listed here after implementation -->
