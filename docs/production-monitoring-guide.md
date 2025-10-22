# Production Monitoring Guide

**Version:** 1.0
**Last Updated:** 2025-10-21
**Status:** Ready for Production Deployment
**Target Audience:** DevOps Engineers, System Administrators

---

## Table of Contents

1. [Introduction](#1-introduction)
2. [Monitoring Architecture](#2-monitoring-architecture)
3. [PostgreSQL Connection Pool Monitoring](#3-postgresql-connection-pool-monitoring)
4. [Application Performance Monitoring](#4-application-performance-monitoring)
5. [Redis Cache Monitoring](#5-redis-cache-monitoring)
6. [API Rate Limiting Metrics](#6-api-rate-limiting-metrics)
7. [Dashboard Setup](#7-dashboard-setup)
8. [Alerting Configuration](#8-alerting-configuration)
9. [Troubleshooting](#9-troubleshooting)
10. [Appendix](#10-appendix)

---

## 1. Introduction

This guide provides comprehensive instructions for setting up production monitoring for the LLM Token Price Platform. Monitoring is critical for:

- **Proactive Issue Detection**: Identify problems before users are affected
- **Performance Optimization**: Track resource utilization and bottlenecks
- **Capacity Planning**: Forecast infrastructure needs based on growth trends
- **SLA Compliance**: Ensure 99.9% uptime target is met

### Key Metrics to Monitor

| Category | Metrics | Alert Threshold |
|----------|---------|----------------|
| **Database** | Connection pool utilization, query latency, deadlocks | Pool >90%, Latency >500ms |
| **Cache** | Cache hit ratio, Redis memory usage, eviction rate | Hit ratio <80%, Memory >85% |
| **API** | Request rate, error rate, response time, rate limit violations | Error rate >1%, P95 >1s |
| **Infrastructure** | CPU, Memory, Disk I/O, Network throughput | CPU >80%, Memory >90% |

---

## 2. Monitoring Architecture

### Stack Overview

```
┌──────────────────────────────────────────────────────────┐
│                    Monitoring Stack                      │
├──────────────────────────────────────────────────────────┤
│  Metrics Collection:                                     │
│    - Prometheus (PostgreSQL Exporter, Redis Exporter)    │
│    - .NET OpenTelemetry (Application metrics)            │
│    - ASP.NET Core Health Checks                          │
│                                                          │
│  Visualization:                                          │
│    - Grafana (Dashboards)                                │
│    - pgAdmin (PostgreSQL-specific)                       │
│                                                          │
│  Alerting:                                               │
│    - Prometheus Alertmanager                             │
│    - Email/Slack notifications                           │
└──────────────────────────────────────────────────────────┘
```

### Component Deployment

**Option A: Docker Compose (Recommended for Small-Medium Scale)**

```yaml
# docker-compose.monitoring.yml
version: '3.8'

services:
  prometheus:
    image: prom/prometheus:latest
    ports:
      - "9090:9090"
    volumes:
      - ./prometheus.yml:/etc/prometheus/prometheus.yml
      - prometheus_data:/prometheus
    command:
      - '--config.file=/etc/prometheus/prometheus.yml'
      - '--storage.tsdb.retention.time=30d'

  grafana:
    image: grafana/grafana:latest
    ports:
      - "3000:3000"
    environment:
      - GF_SECURITY_ADMIN_PASSWORD=changeme
    volumes:
      - grafana_data:/var/lib/grafana
      - ./grafana/dashboards:/etc/grafana/provisioning/dashboards

  postgres-exporter:
    image: prometheuscommunity/postgres-exporter:latest
    ports:
      - "9187:9187"
    environment:
      - DATA_SOURCE_NAME=postgresql://user:password@postgres:5432/llm_token_price?sslmode=disable

  redis-exporter:
    image: oliver006/redis_exporter:latest
    ports:
      - "9121:9121"
    environment:
      - REDIS_ADDR=redis:6379

volumes:
  prometheus_data:
  grafana_data:
```

**Option B: Cloud-Managed Services (Recommended for Production)**

- **Azure**: Application Insights + Azure Monitor
- **AWS**: CloudWatch + Managed Prometheus
- **GCP**: Cloud Monitoring + Cloud Trace

---

## 3. PostgreSQL Connection Pool Monitoring

### 3.1 Connection Pool Metrics

**Critical Metrics:**

1. **Active Connections**: Number of connections currently in use
2. **Idle Connections**: Connections in pool but not executing queries
3. **Waiting Connections**: Requests waiting for an available connection
4. **Connection Pool Size**: Total connections allocated
5. **Connection Lifetime**: Average time a connection is held

### 3.2 Configuration in .NET

Update `appsettings.Production.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=db.example.com;Database=llm_token_price;Username=app_user;Password=***;Maximum Pool Size=100;Minimum Pool Size=10;Connection Lifetime=300;Connection Idle Lifetime=60;Connection Pruning Interval=10"
  },
  "Monitoring": {
    "ConnectionPoolMetrics": {
      "Enabled": true,
      "SamplingIntervalSeconds": 10,
      "ExportToPrometheus": true
    }
  }
}
```

**Connection String Parameters Explained:**

- `Maximum Pool Size=100`: Max connections (adjust based on load testing)
- `Minimum Pool Size=10`: Connections kept warm (reduces latency)
- `Connection Lifetime=300`: Max seconds before connection recycled (prevents stale connections)
- `Connection Idle Lifetime=60`: Idle connections closed after 60s
- `Connection Pruning Interval=10`: Check for idle connections every 10s

### 3.3 Prometheus Metrics

**PostgreSQL Exporter Queries** (`postgres-exporter-queries.yml`):

```yaml
pg_stat_activity:
  query: |
    SELECT
      state,
      COUNT(*) as count
    FROM pg_stat_activity
    WHERE datname = 'llm_token_price'
    GROUP BY state
  metrics:
    - state:
        usage: "LABEL"
        description: "Connection state (active, idle, idle in transaction)"
    - count:
        usage: "GAUGE"
        description: "Number of connections in this state"

pg_stat_database:
  query: |
    SELECT
      numbackends,
      xact_commit,
      xact_rollback,
      blks_read,
      blks_hit,
      tup_returned,
      tup_fetched,
      tup_inserted,
      tup_updated,
      tup_deleted
    FROM pg_stat_database
    WHERE datname = 'llm_token_price'
  metrics:
    - numbackends:
        usage: "GAUGE"
        description: "Number of backends currently connected to this database"
    - xact_commit:
        usage: "COUNTER"
        description: "Number of transactions committed"
    - xact_rollback:
        usage: "COUNTER"
        description: "Number of transactions rolled back"
    - blks_read:
        usage: "COUNTER"
        description: "Number of disk blocks read"
    - blks_hit:
        usage: "COUNTER"
        description: "Number of disk blocks found in buffer cache"

pg_settings_max_connections:
  query: "SELECT setting::numeric as max_connections FROM pg_settings WHERE name = 'max_connections'"
  metrics:
    - max_connections:
        usage: "GAUGE"
        description: "Maximum number of concurrent connections allowed"
```

### 3.4 Grafana Dashboard Panel (Connection Pool)

```json
{
  "title": "PostgreSQL Connection Pool Utilization",
  "targets": [
    {
      "expr": "(pg_stat_activity_count{state='active'} / pg_settings_max_connections) * 100",
      "legendFormat": "Active Connection %"
    }
  ],
  "thresholds": [
    { "value": 80, "color": "yellow" },
    { "value": 90, "color": "red" }
  ],
  "alert": {
    "name": "High Connection Pool Utilization",
    "conditions": [
      {
        "evaluator": { "type": "gt", "params": [90] },
        "query": { "datasource": "Prometheus", "model": "pg_stat_activity_count" }
      }
    ]
  }
}
```

### 3.5 Alert Rules

**Prometheus Alert Rules** (`alerts/postgres.yml`):

```yaml
groups:
  - name: postgres_connection_pool
    interval: 30s
    rules:
      - alert: PostgreSQLConnectionPoolNearExhaustion
        expr: (pg_stat_activity_count{state='active'} / pg_settings_max_connections) > 0.90
        for: 5m
        labels:
          severity: critical
          component: database
        annotations:
          summary: "PostgreSQL connection pool >90% utilized"
          description: "Active connections: {{ $value }}% of maximum. Consider increasing pool size or investigating connection leaks."

      - alert: PostgreSQLHighWaitingConnections
        expr: pg_stat_activity_count{state='idle in transaction'} > 10
        for: 2m
        labels:
          severity: warning
          component: database
        annotations:
          summary: "High number of idle-in-transaction connections"
          description: "{{ $value }} connections are idle but holding transactions. This may indicate application issues."

      - alert: PostgreSQLDeadlockDetected
        expr: rate(pg_stat_database_deadlocks[5m]) > 0
        for: 1m
        labels:
          severity: warning
          component: database
        annotations:
          summary: "PostgreSQL deadlock detected"
          description: "Deadlock rate: {{ $value }} per second. Review application transaction logic."
```

---

## 4. Application Performance Monitoring

### 4.1 OpenTelemetry Integration

Install NuGet packages:

```bash
dotnet add package OpenTelemetry.Exporter.Prometheus.AspNetCore
dotnet add package OpenTelemetry.Extensions.Hosting
dotnet add package OpenTelemetry.Instrumentation.AspNetCore
dotnet add package OpenTelemetry.Instrumentation.EntityFrameworkCore
dotnet add package OpenTelemetry.Instrumentation.Http
```

**Update `Program.cs`:**

```csharp
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;

var builder = WebApplication.CreateBuilder(args);

// Configure OpenTelemetry
builder.Services.AddOpenTelemetry()
    .WithMetrics(metrics =>
    {
        metrics
            .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("llm-token-price-api"))
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddRuntimeInstrumentation()
            .AddProcessInstrumentation()
            .AddPrometheusExporter();
    });

var app = builder.Build();

// Expose Prometheus metrics endpoint
app.MapPrometheusScrapingEndpoint(); // Available at /metrics

app.Run();
```

### 4.2 Custom Metrics

**Track QAPS Calculation Performance:**

```csharp
// services/backend/LlmTokenPrice.Application/Services/BestValueService.cs
using System.Diagnostics.Metrics;

public class BestValueService
{
    private static readonly Meter _meter = new("LlmTokenPrice.BestValue");
    private static readonly Histogram<double> _qapsCalculationDuration = _meter.CreateHistogram<double>(
        "qaps_calculation_duration_ms",
        unit: "ms",
        description: "Duration of QAPS calculation in milliseconds"
    );

    public async Task<List<BestValueDto>> CalculateBestValueAsync(...)
    {
        var stopwatch = Stopwatch.StartNew();
        try
        {
            // ... QAPS calculation logic ...
            return results;
        }
        finally
        {
            stopwatch.Stop();
            _qapsCalculationDuration.Record(stopwatch.Elapsed.TotalMilliseconds);
        }
    }
}
```

### 4.3 Health Checks Endpoint

Already implemented in `Program.cs` (Story 2.13). Monitor at `/api/health`:

```json
{
  "status": "Healthy",
  "checks": [
    { "name": "database", "status": "Healthy", "duration": "00:00:00.0234" },
    { "name": "redis", "status": "Healthy", "duration": "00:00:00.0045" }
  ],
  "totalDuration": "00:00:00.0279"
}
```

**Prometheus Blackbox Exporter Configuration** (`blackbox.yml`):

```yaml
modules:
  http_2xx:
    prober: http
    timeout: 5s
    http:
      valid_http_versions: ["HTTP/1.1", "HTTP/2.0"]
      valid_status_codes: [200]
      method: GET
      headers:
        Accept: application/json
      fail_if_not_matches_regexp:
        - '"status":"Healthy"'
```

---

## 5. Redis Cache Monitoring

### 5.1 Redis Exporter Metrics

**Key Metrics from Redis Exporter:**

| Metric | Description | Alert Threshold |
|--------|-------------|-----------------|
| `redis_memory_used_bytes` | Total memory used by Redis | >85% of `maxmemory` |
| `redis_keyspace_hits_total` | Cache hits | N/A (calculate hit ratio) |
| `redis_keyspace_misses_total` | Cache misses | Hit ratio <80% |
| `redis_evicted_keys_total` | Keys evicted due to memory limit | >100/min |
| `redis_connected_clients` | Active client connections | >1000 |
| `redis_commands_processed_total` | Total commands processed | N/A (rate metric) |

### 5.2 Cache Hit Ratio Calculation

**Prometheus Query:**

```promql
# Cache hit ratio (percentage)
(rate(redis_keyspace_hits_total[5m]) / (rate(redis_keyspace_hits_total[5m]) + rate(redis_keyspace_misses_total[5m]))) * 100
```

**Target**: ≥80% cache hit ratio

### 5.3 Alert Rules

```yaml
groups:
  - name: redis_cache
    interval: 30s
    rules:
      - alert: RedisCacheHitRatioLow
        expr: (rate(redis_keyspace_hits_total[5m]) / (rate(redis_keyspace_hits_total[5m]) + rate(redis_keyspace_misses_total[5m]))) < 0.80
        for: 10m
        labels:
          severity: warning
          component: cache
        annotations:
          summary: "Redis cache hit ratio below 80%"
          description: "Current hit ratio: {{ $value | humanizePercentage }}. Consider increasing cache TTL or reviewing cache keys."

      - alert: RedisMemoryHigh
        expr: (redis_memory_used_bytes / redis_memory_max_bytes) > 0.85
        for: 5m
        labels:
          severity: critical
          component: cache
        annotations:
          summary: "Redis memory usage >85%"
          description: "Memory used: {{ $value | humanizePercentage }}. Increase `maxmemory` or review eviction policy."

      - alert: RedisHighEvictionRate
        expr: rate(redis_evicted_keys_total[5m]) > 100
        for: 5m
        labels:
          severity: warning
          component: cache
        annotations:
          summary: "High Redis key eviction rate"
          description: "Evicting {{ $value }} keys/min. Memory pressure detected."
```

---

## 6. API Rate Limiting Metrics

### 6.1 AspNetCoreRateLimit Metrics

**Custom Middleware to Export Metrics** (`Middleware/RateLimitMetricsMiddleware.cs`):

```csharp
public class RateLimitMetricsMiddleware
{
    private static readonly Meter _meter = new("LlmTokenPrice.RateLimit");
    private static readonly Counter<long> _rateLimitViolations = _meter.CreateCounter<long>(
        "rate_limit_violations_total",
        description: "Total number of rate limit violations"
    );

    private readonly RequestDelegate _next;

    public RateLimitMetricsMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        await _next(context);

        // Track 429 Too Many Requests responses
        if (context.Response.StatusCode == 429)
        {
            _rateLimitViolations.Add(1, new KeyValuePair<string, object?>("endpoint", context.Request.Path.Value));
        }
    }
}
```

### 6.2 Grafana Dashboard Panel

```json
{
  "title": "Rate Limit Violations",
  "targets": [
    {
      "expr": "rate(rate_limit_violations_total[5m])",
      "legendFormat": "Violations/sec - {{ endpoint }}"
    }
  ],
  "yaxis": {
    "label": "Violations per second"
  }
}
```

---

## 7. Dashboard Setup

### 7.1 Grafana Provisioning

**Directory Structure:**

```
grafana/
├── dashboards/
│   ├── dashboard-provider.yml
│   ├── postgres-dashboard.json
│   ├── redis-dashboard.json
│   ├── api-dashboard.json
│   └── overview-dashboard.json
└── datasources/
    └── prometheus.yml
```

**`grafana/datasources/prometheus.yml`:**

```yaml
apiVersion: 1

datasources:
  - name: Prometheus
    type: prometheus
    access: proxy
    url: http://prometheus:9090
    isDefault: true
    editable: true
```

**`grafana/dashboards/dashboard-provider.yml`:**

```yaml
apiVersion: 1

providers:
  - name: 'LLM Token Price Dashboards'
    orgId: 1
    folder: 'Production'
    type: file
    disableDeletion: false
    updateIntervalSeconds: 10
    allowUiUpdates: true
    options:
      path: /etc/grafana/provisioning/dashboards
```

### 7.2 Pre-Built Dashboard Templates

**Import Grafana Dashboard IDs** (from https://grafana.com/grafana/dashboards/):

- **PostgreSQL**: Dashboard ID `9628` (PostgreSQL Database)
- **Redis**: Dashboard ID `11835` (Redis Dashboard for Prometheus)
- **.NET**: Dashboard ID `10915` (ASP.NET Core Metrics)

**Import via Grafana UI:**
1. Login to Grafana (`http://localhost:3000`)
2. Click **"+"** → **"Import"**
3. Enter Dashboard ID → **"Load"**
4. Select Prometheus datasource → **"Import"**

---

## 8. Alerting Configuration

### 8.1 Prometheus Alertmanager

**`alertmanager.yml`:**

```yaml
global:
  resolve_timeout: 5m
  smtp_smarthost: 'smtp.gmail.com:587'
  smtp_from: 'alerts@llmpricing.com'
  smtp_auth_username: 'alerts@llmpricing.com'
  smtp_auth_password: 'your-app-password'

route:
  group_by: ['alertname', 'cluster', 'service']
  group_wait: 10s
  group_interval: 10s
  repeat_interval: 12h
  receiver: 'team-email'
  routes:
    - match:
        severity: critical
      receiver: 'team-pagerduty'

receivers:
  - name: 'team-email'
    email_configs:
      - to: 'team@llmpricing.com'
        headers:
          Subject: '[{{ .Status }}] {{ .GroupLabels.alertname }} - LLM Token Price Platform'

  - name: 'team-pagerduty'
    pagerduty_configs:
      - service_key: 'your-pagerduty-service-key'
        description: '{{ .GroupLabels.alertname }} - {{ .CommonAnnotations.summary }}'

  - name: 'slack'
    slack_configs:
      - api_url: 'https://hooks.slack.com/services/YOUR/WEBHOOK/URL'
        channel: '#alerts'
        title: '{{ .GroupLabels.alertname }}'
        text: '{{ .CommonAnnotations.description }}'
```

### 8.2 Alert Severity Levels

| Severity | Response Time | Example Alerts |
|----------|---------------|----------------|
| **Critical** | Immediate (wake up on-call engineer) | Connection pool >90%, API downtime, database unreachable |
| **Warning** | Within 1 hour (during business hours) | Cache hit ratio <80%, high eviction rate, slow queries |
| **Info** | Review during daily standup | Deployment successful, new user signup spike |

---

## 9. Troubleshooting

### 9.1 High Connection Pool Utilization

**Symptoms**: Connection pool consistently >80% utilized, requests timing out

**Diagnostic Steps:**

1. **Check long-running queries:**
   ```sql
   SELECT pid, now() - pg_stat_activity.query_start AS duration, query
   FROM pg_stat_activity
   WHERE state = 'active'
   ORDER BY duration DESC;
   ```

2. **Identify connection leaks:**
   ```sql
   SELECT state, COUNT(*)
   FROM pg_stat_activity
   GROUP BY state;
   ```
   - Look for high counts of "idle in transaction" (indicates uncommitted transactions)

3. **Review application logs** for unclosed `DbContext` instances

**Resolutions:**

- **Short-term**: Increase `Maximum Pool Size` (e.g., from 100 to 150)
- **Long-term**:
  - Ensure all `DbContext` instances are properly disposed (use `using` statements)
  - Reduce transaction scope (commit more frequently)
  - Add indexes to slow queries (reduce query duration)

### 9.2 Low Cache Hit Ratio

**Symptoms**: Redis cache hit ratio <80%, high database load

**Diagnostic Steps:**

1. **Check cache key distribution:**
   ```bash
   redis-cli --scan --pattern 'cache:*' | wc -l
   ```

2. **Inspect TTL distribution:**
   ```bash
   redis-cli --scan --pattern 'cache:*' | while read key; do echo "$key: $(redis-cli TTL $key)"; done
   ```

3. **Review eviction policy:**
   ```bash
   redis-cli INFO stats | grep evicted_keys
   ```

**Resolutions:**

- **Increase cache TTL** for stable data (e.g., benchmark definitions: 1hr → 24hr)
- **Increase Redis `maxmemory`** if eviction rate is high
- **Optimize cache keys** (avoid storing large objects, use pagination for lists)

### 9.3 Slow API Response Times

**Symptoms**: P95 latency >1 second, Grafana dashboard shows red

**Diagnostic Steps:**

1. **Trace slow requests** with OpenTelemetry distributed tracing
2. **Profile QAPS calculation** (check if normalization is slow)
3. **Review database query plans:**
   ```sql
   EXPLAIN ANALYZE SELECT * FROM models WHERE is_active = true;
   ```

**Resolutions:**

- Add database indexes for frequently filtered columns
- Optimize LINQ queries (avoid `N+1` queries)
- Increase Redis cache coverage (cache more expensive queries)

---

## 10. Appendix

### 10.1 Prometheus Configuration

**`prometheus.yml`:**

```yaml
global:
  scrape_interval: 15s
  evaluation_interval: 15s

alerting:
  alertmanagers:
    - static_configs:
        - targets: ['alertmanager:9093']

rule_files:
  - "alerts/postgres.yml"
  - "alerts/redis.yml"
  - "alerts/api.yml"

scrape_configs:
  # PostgreSQL Exporter
  - job_name: 'postgres'
    static_configs:
      - targets: ['postgres-exporter:9187']
    relabel_configs:
      - source_labels: [__address__]
        target_label: instance
        replacement: 'postgres-db'

  # Redis Exporter
  - job_name: 'redis'
    static_configs:
      - targets: ['redis-exporter:9121']

  # ASP.NET Core API
  - job_name: 'api'
    metrics_path: '/metrics'
    static_configs:
      - targets: ['api:5000']

  # Node Exporter (Server metrics)
  - job_name: 'node'
    static_configs:
      - targets: ['node-exporter:9100']

  # Blackbox Exporter (Health checks)
  - job_name: 'blackbox'
    metrics_path: /probe
    params:
      module: [http_2xx]
    static_configs:
      - targets:
          - https://api.llmpricing.com/api/health
    relabel_configs:
      - source_labels: [__address__]
        target_label: __param_target
      - source_labels: [__param_target]
        target_label: instance
      - target_label: __address__
        replacement: blackbox-exporter:9115
```

### 10.2 Deployment Checklist

Before enabling production monitoring:

- [ ] **Docker Compose deployed** or cloud monitoring services configured
- [ ] **Prometheus scraping all targets** (check `/targets` page)
- [ ] **Grafana dashboards imported** and displaying data
- [ ] **Alert rules loaded** (`/alerts` page in Prometheus)
- [ ] **Alertmanager configured** with email/Slack/PagerDuty
- [ ] **Test alerts** by triggering a threshold (e.g., temporarily reduce connection pool)
- [ ] **Document runbooks** for each alert type
- [ ] **Set up on-call rotation** for critical alerts
- [ ] **Schedule weekly review** of dashboards and metrics trends

### 10.3 Maintenance Schedule

| Task | Frequency | Owner |
|------|-----------|-------|
| Review Grafana dashboards for anomalies | Daily | DevOps |
| Analyze alert trends (false positives) | Weekly | DevOps + Dev Team |
| Update alert thresholds based on actual load | Monthly | DevOps |
| Review and prune old Prometheus data | Quarterly | DevOps |
| Update monitoring stack (Prometheus, Grafana, exporters) | Quarterly | DevOps |

### 10.4 Useful Resources

- **Prometheus Documentation**: https://prometheus.io/docs/
- **Grafana Dashboards**: https://grafana.com/grafana/dashboards/
- **PostgreSQL Exporter**: https://github.com/prometheus-community/postgres_exporter
- **Redis Exporter**: https://github.com/oliver006/redis_exporter
- **OpenTelemetry .NET**: https://opentelemetry.io/docs/instrumentation/net/

---

**End of Production Monitoring Guide**

*This guide is part of Story 3.1b (Subtask 5.3). Last updated: 2025-10-21*
