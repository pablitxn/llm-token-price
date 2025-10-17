# LLM Cost Comparison Platform - Epic Breakdown

**Author:** Pablo
**Date:** 2025-10-16
**Project Level:** 4 (Enterprise Scale)
**Target Scale:** 5,000+ monthly active users by month 6

---

## Overview

This document provides the detailed epic breakdown for LLM Cost Comparison Platform, expanding on the high-level epic list in the [PRD](./PRD.md).

Each epic includes:

- Expanded goal and value proposition
- Complete story breakdown with user stories
- Acceptance criteria for each story
- Story sequencing and dependencies

**Epic Sequencing Principles:**

- Epic 1 establishes foundational infrastructure and initial functionality
- Subsequent epics build progressively, each delivering significant end-to-end value
- Stories within epics are vertically sliced and sequentially ordered
- No forward dependencies - each story builds only on previous work

**Total Estimated Stories:** 76-93 stories across 8 epics

---

# Epic 1: Project Foundation & Data Infrastructure

**Goal:** Establish development environment, database schema, API skeleton, and CI/CD pipeline with initial deployable application containing foundational data models.

**Value Delivery:** Creates the technical foundation required for all subsequent work, including database schema for models/benchmarks/capabilities, basic API structure, deployment pipeline, and comprehensive test infrastructure.

**Estimated Stories:** 11 stories (includes comprehensive test infrastructure)

---

**Story 1.1: Initialize Project Repository and Development Environment**

As a developer,
I want a properly configured project repository with development environment setup,
So that I can begin building the application with consistent tooling.

**Acceptance Criteria:**
1. Repository initialized with .NET 8 backend and React 18+ frontend projects
2. Directory structure follows Hexagonal Architecture (domain, application, infrastructure, presentation)
3. Git repository configured with .gitignore for both .NET and Node/React
4. README.md with setup instructions created
5. Development environment configuration documented (required versions, dependencies)

**Prerequisites:** None (first story)

---

**Story 1.2: Configure Build Tools and Package Management**

As a developer,
I want build tools and package managers configured,
So that I can install dependencies and build the application.

**Acceptance Criteria:**
1. Backend: .NET 8 solution file created with project references
2. Frontend: Vite configured with TypeScript, React, TailwindCSS
3. Package.json includes all required dependencies (Zustand, React Query, TanStack Table, Chart.js)
4. Backend packages configured (Entity Framework Core, MassTransit, Redis client)
5. Both projects build successfully with `dotnet build` and `npm run build`

**Prerequisites:** Story 1.1

---

**Story 1.3: Setup PostgreSQL Database and Connection**

As a developer,
I want PostgreSQL database configured and connected,
So that I can persist application data.

**Acceptance Criteria:**
1. PostgreSQL 14+ installed locally or Docker container configured
2. Database connection string configured in backend appsettings
3. Entity Framework Core DbContext created with initial configuration
4. Database connection test passes successfully
5. Database migrations infrastructure configured

**Prerequisites:** Story 1.2

---

**Story 1.4: Create Core Data Models (Models, Capabilities, Benchmarks)**

As a developer,
I want database schema for LLM models, capabilities, and benchmarks,
So that I can store and retrieve model data.

**Acceptance Criteria:**
1. Models table entity created with fields: id, name, provider, version, release_date, status, input_price_per_1M, output_price_per_1M, currency, pricing_valid_from, pricing_valid_to, last_scraped_at, is_active, created_at, updated_at
2. ModelCapabilities table entity created with fields: id, model_id, context_window, max_output_tokens, supports_function_calling, supports_vision, supports_audio_input, supports_audio_output, supports_streaming, supports_json_mode
3. Benchmarks table entity created with fields: id, benchmark_name, full_name, description, category, interpretation, typical_range_min, typical_range_max
4. ModelBenchmarkScores table entity created with fields: id, model_id, benchmark_id, score, max_score, test_date, source_url, verified, notes
5. Entity relationships configured (one-to-one Models to Capabilities, one-to-many Models to BenchmarkScores)
6. Database migration generated and applied successfully

**Prerequisites:** Story 1.3

---

**Story 1.5: Setup Redis Cache Connection**

As a developer,
I want Redis cache configured and connected,
So that I can implement caching for performance optimization.

**Acceptance Criteria:**
1. Redis connection configured (Upstash or local Redis)
2. Redis client library integrated in backend
3. Cache service abstraction created with Get/Set/Delete operations
4. Connection health check passes
5. Basic cache test (set value, retrieve value) works successfully

**Prerequisites:** Story 1.2

---

**Story 1.6: Create Basic API Structure with Health Endpoint**

As a developer,
I want basic REST API structure with health check,
So that I can verify the backend is running and connected to dependencies.

**Acceptance Criteria:**
1. ASP.NET Core Web API configured with controllers
2. Health check endpoint `/api/health` created
3. Health check verifies database connection
4. Health check verifies Redis connection
5. API returns 200 OK with JSON response showing service status
6. CORS configured for frontend development

**Prerequisites:** Story 1.4, Story 1.5

---

**Story 1.7: Setup Frontend Application Shell**

As a developer,
I want basic React application shell,
So that I can begin building UI components.

**Acceptance Criteria:**
1. React app renders successfully at localhost:5173
2. TailwindCSS styling works (test with colored div)
3. React Router configured with placeholder routes (home, admin)
4. Basic layout component created (header, main content area, footer placeholder)
5. API client configured with axios or fetch wrapper pointing to backend

**Prerequisites:** Story 1.2

---

**Story 1.8: Configure CI/CD Pipeline**

As a developer,
I want automated build and deployment pipeline,
So that code changes are automatically tested and deployed.

**Acceptance Criteria:**
1. GitHub Actions (or similar) workflow created for backend build/test
2. GitHub Actions workflow created for frontend build
3. Automated tests run on pull requests
4. Build artifacts generated successfully
5. Deployment configuration documented (manual deploy for MVP, automated later)

**Prerequisites:** Story 1.6, Story 1.7

---

**Story 1.9: Seed Database with Sample Data**

As a developer,
I want sample model data in database,
So that I can develop and test features with realistic data.

**Acceptance Criteria:**
1. Database seed script created with 5-10 sample LLM models (GPT-4, Claude 3, Gemini, Llama, etc.)
2. Sample models include pricing, capabilities, and 3-5 benchmark scores each
3. Benchmark definitions seeded (MMLU, HumanEval, GSM8K, HellaSwag, MATH)
4. Seed script can be run via migration or CLI command
5. Sample data visible via direct database query

**Prerequisites:** Story 1.4

---

**Story 1.10: Create Basic GET API for Models List**

As a developer,
I want API endpoint to retrieve all models,
So that I can fetch model data in the frontend.

**Acceptance Criteria:**
1. GET `/api/models` endpoint created
2. Endpoint returns JSON array of all active models
3. Response includes model metadata, pricing, capabilities
4. Response includes basic benchmark scores (top 3-5 benchmarks)
5. API tested with Postman/curl and returns 200 OK with data
6. Basic error handling returns 500 if database fails

**Prerequisites:** Story 1.9

---

**Story 1.11: Establish Test Infrastructure and Validation Framework**

As a backend developer,
I want comprehensive test infrastructure with database isolation, architecture validation, and E2E testing capabilities,
So that I can write reliable tests that validate hexagonal architecture boundaries, database integrity, and critical infrastructure components.

**Acceptance Criteria:**
1. xUnit test framework configured with parallel test execution enabled
2. TestContainers setup for PostgreSQL 16 and Redis 7.2 with automatic container lifecycle management
3. Respawn database cleanup configured to reset database state between integration tests (<100ms cleanup)
4. ArchUnitNET tests enforce hexagonal architecture boundaries (domain layer has zero infrastructure dependencies)
5. FluentAssertions library integrated for readable test assertions
6. Playwright E2E framework configured with API request context for fast data seeding
7. DatabaseFixture (xUnit IClassFixture) provides shared test database instance across test classes
8. SampleDataSeeder factory creates valid test entities (Models, Capabilities, Benchmarks, BenchmarkScores)
9. Integration test validates PostgreSQL connection, migration execution, and entity creation
10. Integration test validates Redis cache Get/Set/Delete operations with connection resilience
11. Unit test validates hexagonal architecture boundaries (ArchUnit rules fail if domain depends on infrastructure)
12. E2E test validates API health endpoint returns 200 OK with database + Redis status checks
13. Test execution time meets targets: Unit tests <10s, Integration tests <30s, E2E smoke tests <5min
14. CI/CD pipeline executes all test levels successfully in GitHub Actions workflow

**Prerequisites:** Story 1.8 (CI/CD pipeline configured)

**Related:** See `/docs/test-design-epic-1.md` for comprehensive test coverage plan and risk mitigation strategies

---

# Epic 2: Model Data Management & Admin CRUD

**Goal:** Build admin panel for managing LLM models with full CRUD operations, enabling data entry and updates without code changes.

**Value Delivery:** Administrators can add new models, update pricing, manage capabilities, and enter benchmark scores through user-friendly forms, eliminating need for direct database access.

**Estimated Stories:** 10-12 stories

---

**Story 2.1: Create Admin Panel Authentication**

As an administrator,
I want secure login to admin panel,
So that only authorized users can manage model data.

**Acceptance Criteria:**
1. Admin login page created with username/password fields
2. Backend authentication endpoint `/api/admin/auth/login` created
3. Simple authentication mechanism (hardcoded credentials for MVP, or basic JWT)
4. Session/token stored in browser for authenticated requests
5. Protected routes redirect to login if not authenticated
6. Logout functionality clears session

**Prerequisites:** Story 1.7

---

**Story 2.2: Create Admin Dashboard Layout**

As an administrator,
I want admin dashboard with navigation,
So that I can access different management functions.

**Acceptance Criteria:**
1. Admin layout component created with sidebar navigation
2. Navigation menu includes: Models, Benchmarks, Dashboard (placeholder)
3. Main content area renders selected section
4. Header shows logged-in admin name and logout button
5. Responsive layout works on desktop and tablet

**Prerequisites:** Story 2.1

---

**Story 2.3: Build Models List View in Admin Panel**

As an administrator,
I want to view all models in a table,
So that I can see current data and select models to edit.

**Acceptance Criteria:**
1. Admin models list page displays all models in table
2. Table shows: name, provider, input price, output price, status, last updated
3. Search box filters models by name or provider
4. "Add New Model" button navigates to creation form
5. "Edit" button on each row navigates to edit form
6. "Delete" button on each row triggers confirmation dialog

**Prerequisites:** Story 2.2, Story 1.10

---

**Story 2.4: Create Add New Model Form**

As an administrator,
I want form to add new LLM model,
So that I can expand the model database.

**Acceptance Criteria:**
1. Add model form page created with all required fields:
   - Basic info: name, provider, version, release date, status
   - Pricing: input price/1M, output price/1M, currency, validity dates
2. Form validation ensures required fields completed
3. Form validation ensures prices are positive numbers
4. "Save" button posts data to backend API
5. Success redirects to models list with confirmation message
6. Error displays validation messages

**Prerequisites:** Story 2.3

---

**Story 2.5: Create Backend API for Adding Models**

As an administrator,
I want backend API to persist new models,
So that added models are saved to database.

**Acceptance Criteria:**
1. POST `/api/admin/models` endpoint created
2. Endpoint accepts JSON model data with validation
3. Model entity created and saved to database with timestamps
4. ModelCapabilities entity created with default values
5. API returns 201 Created with new model ID
6. Validation errors return 400 Bad Request with details

**Prerequisites:** Story 2.4

---

**Story 2.6: Add Capabilities Section to Model Form**

As an administrator,
I want to specify model capabilities in the form,
So that capability information is captured during model creation/editing.

**Acceptance Criteria:**
1. Capabilities section added to model form
2. Number inputs for context_window, max_output_tokens
3. Checkboxes for binary capabilities (function calling, vision, audio, streaming, JSON mode)
4. Form submission includes capabilities data
5. Backend API updated to save capabilities to ModelCapabilities table
6. Existing models display current capabilities when editing

**Prerequisites:** Story 2.5

---

**Story 2.7: Create Edit Model Functionality**

As an administrator,
I want to edit existing models,
So that I can update pricing or correct information.

**Acceptance Criteria:**
1. Edit model form loads existing model data
2. Form pre-populated with all current values (basic info, pricing, capabilities)
3. PUT `/api/admin/models/{id}` endpoint created
4. Endpoint updates model and capabilities in database
5. Updated_at timestamp refreshed on save
6. Success redirects to models list with confirmation

**Prerequisites:** Story 2.6

---

**Story 2.8: Create Delete Model Functionality**

As an administrator,
I want to delete models,
So that I can remove outdated or incorrect entries.

**Acceptance Criteria:**
1. Delete button triggers confirmation dialog
2. DELETE `/api/admin/models/{id}` endpoint created
3. Endpoint soft-deletes model (sets is_active=false) or hard-deletes
4. Associated capabilities and benchmark scores handled (cascade delete or set inactive)
5. Success refreshes models list with confirmation message

**Prerequisites:** Story 2.7

---

**Story 2.9: Create Benchmark Definitions Management**

As an administrator,
I want to manage benchmark definitions,
So that I can add new benchmarks for scoring models.

**Acceptance Criteria:**
1. Benchmarks management page created in admin panel
2. List view shows all benchmark definitions
3. Add benchmark form includes: name, full_name, description, category, interpretation, typical_range
4. POST `/api/admin/benchmarks` endpoint creates benchmark definition
5. Edit and delete functionality for benchmarks
6. Validation ensures benchmark names are unique

**Prerequisites:** Story 2.2

---

**Story 2.10: Create Benchmark Score Entry Form**

As an administrator,
I want to add benchmark scores for models,
So that performance data is captured for comparisons.

**Acceptance Criteria:**
1. Benchmark scores section added to model form (or separate page)
2. Form allows selecting model and benchmark from dropdowns
3. Score input field with validation (number within typical range)
4. Test date picker and source URL input
5. POST `/api/admin/models/{id}/benchmarks` endpoint saves score
6. Benchmark scores list displayed for each model

**Prerequisites:** Story 2.9

---

**Story 2.11: Add Bulk Benchmark Import via CSV**

As an administrator,
I want to import multiple benchmark scores via CSV,
So that I can efficiently add data for new models.

**Acceptance Criteria:**
1. CSV upload form created on benchmarks page
2. CSV template documented (model_id, benchmark_name, score, test_date, source_url)
3. File upload processed in backend
4. CSV parsed and validated (check model/benchmark exist, scores valid)
5. Valid rows imported to database
6. Import results shown (X successful, Y failed with reasons)

**Prerequisites:** Story 2.10

---

**Story 2.12: Add Timestamp Tracking and Display**

As an administrator,
I want to see when models were last updated,
So that I know data freshness.

**Acceptance Criteria:**
1. Models list table displays "Last Updated" column with formatted timestamp
2. Models with updates >7 days ago highlighted or flagged
3. Admin dashboard shows count of models needing updates
4. Public API includes pricing_updated_at timestamp
5. Frontend displays "Updated X days ago" on model cards

**Prerequisites:** Story 2.8

---

# Epic 3: Public Comparison Table Interface

**Goal:** Deliver core public-facing comparison table with sorting, filtering, and search capabilities, enabling users to browse and evaluate LLM models.

**Value Delivery:** Users can access comprehensive model comparison table, sort by any metric, filter by capabilities/provider/price, and search for specific models—delivering the primary platform value.

**Estimated Stories:** 12-15 stories

---

**Story 3.1: Create Public Homepage with Basic Layout**

As a user,
I want to access the platform homepage,
So that I can begin comparing LLM models.

**Acceptance Criteria:**
1. Public homepage route (`/`) created
2. Page layout includes header with platform name/logo
3. Main content area ready for comparison table
4. Footer with basic info (about, contact links placeholder)
5. Navigation bar includes search input placeholder
6. Responsive layout works on desktop, tablet, mobile

**Prerequisites:** Story 1.7

---

**Story 3.2: Fetch and Display Models in Basic Table**

As a user,
I want to see all available models in a table,
So that I can get overview of options.

**Acceptance Criteria:**
1. Frontend fetches models from GET `/api/models`
2. Basic HTML table displays models with columns: name, provider, input price, output price
3. Data loads on page mount
4. Loading spinner shown while fetching
5. Error message displayed if API fails
6. Table displays 10+ models with sample data

**Prerequisites:** Story 3.1, Story 1.10

---

**Story 3.3: Integrate TanStack Table for Advanced Features**

As a developer,
I want TanStack Table integrated,
So that I can implement sorting, filtering, and virtualization efficiently.

**Acceptance Criteria:**
1. TanStack Table library installed and configured
2. Models data rendered using TanStack Table component
3. Column definitions created for all model fields
4. Table replaces basic HTML table from Story 3.2
5. Performance verified with 50+ models (virtual scrolling if needed)

**Prerequisites:** Story 3.2

---

**Story 3.4: Implement Column Sorting**

As a user,
I want to sort models by any column,
So that I can find cheapest models or highest-scoring models.

**Acceptance Criteria:**
1. Clickable column headers enable sorting
2. Sort indicator (up/down arrow) shows sort direction
3. Sorting works for: name (alphabetical), provider (alphabetical), input price (numeric), output price (numeric), benchmark scores (numeric)
4. Default sort: alphabetical by name
5. Click toggles ascending/descending
6. Sort state persists during session

**Prerequisites:** Story 3.3

---

**Story 3.5: Add Provider Filter**

As a user,
I want to filter models by provider,
So that I can focus on specific vendors (OpenAI, Anthropic, Google, etc.).

**Acceptance Criteria:**
1. Filter sidebar created on left side of table
2. Provider filter section displays list of all providers with checkboxes
3. Checking/unchecking provider filters table in real-time
4. Multiple providers selectable (OR logic)
5. "Clear Filters" button resets all selections
6. Filter state shows count of active filters

**Prerequisites:** Story 3.4

---

**Story 3.6: Add Capabilities Filters**

As a user,
I want to filter models by capabilities,
So that I can find models supporting specific features (function calling, vision, etc.).

**Acceptance Criteria:**
1. Capabilities filter section added to sidebar
2. Checkboxes for each capability: function calling, vision support, audio support, streaming, JSON mode
3. Checking capability filters to only models with that capability
4. Multiple capabilities use AND logic (must have all selected)
5. Filters update table immediately
6. Tooltip explains what each capability means

**Prerequisites:** Story 3.5

---

**Story 3.7: Add Price Range Filter**

As a user,
I want to filter models by price range,
So that I can exclude models outside my budget.

**Acceptance Criteria:**
1. Price range filter section added to sidebar
2. Dual-range slider for min/max price (combined input+output price or separate)
3. Price range inputs display current selection
4. Adjusting slider filters table in real-time
5. Price range based on total cost per 1M tokens (input + output avg)
6. Range defaults to min/max of available models

**Prerequisites:** Story 3.6

---

**Story 3.8: Implement Search Functionality**

As a user,
I want to search for models by name or provider,
So that I can quickly find specific models.

**Acceptance Criteria:**
1. Search input box in header or above table
2. Typing filters table to matching models (name or provider contains search term)
3. Search is case-insensitive
4. Search updates table in real-time (debounced 300ms)
5. Clear search button (X icon) resets search
6. Search works alongside filters (combined filter logic)

**Prerequisites:** Story 3.5

---

**Story 3.9: Display Benchmark Scores in Table**

As a user,
I want to see key benchmark scores in the table,
So that I can compare model performance at a glance.

**Acceptance Criteria:**
1. Table columns added for 3-5 key benchmarks (MMLU, HumanEval, GSM8K)
2. Backend API updated to include benchmark scores in models response
3. Scores displayed with formatting (e.g., "85.2" or "85.2%")
4. Missing scores show as "N/A" or "-"
5. Benchmark columns sortable
6. Column headers show benchmark abbreviation with tooltip for full name

**Prerequisites:** Story 3.4

---

**Story 3.10: Add Checkbox Selection for Models**

As a user,
I want to select multiple models with checkboxes,
So that I can compare them side-by-side later.

**Acceptance Criteria:**
1. Checkbox column added as first column in table
2. Clicking checkbox selects/deselects model
3. Selected models highlighted with background color
4. Selected count shown above table ("3 models selected")
5. "Select All" checkbox in header selects/deselects all visible models
6. Selection state persists during filtering/sorting
7. Maximum 5 models selectable (6th click shows warning)

**Prerequisites:** Story 3.9

---

**Story 3.11: Create Comparison Basket UI**

As a user,
I want to see selected models in a comparison basket,
So that I can review my selections and proceed to comparison.

**Acceptance Criteria:**
1. Comparison basket component displayed when models selected (top of page or floating bottom bar)
2. Basket shows mini-cards of selected models (name, provider)
3. "X" button on each card removes from selection
4. "Compare Selected" button (disabled if <2 models selected)
5. "Clear All" button removes all selections
6. Basket collapses when empty

**Prerequisites:** Story 3.10

---

**Story 3.12: Implement Table Pagination or Virtual Scrolling**

As a user,
I want to browse large model lists efficiently,
So that performance remains fast with 50+ models.

**Acceptance Criteria:**
1. Choose pagination (10/25/50 per page) OR virtual scrolling (renders visible rows only)
2. If pagination: page controls (prev/next, page numbers) displayed below table
3. If virtual scrolling: smooth scroll performance with 100+ rows
4. Current approach works with filtering (shows correct subset)
5. User preference saved in session storage

**Prerequisites:** Story 3.9

---

**Story 3.13: Style and Polish Table Interface**

As a user,
I want visually appealing and professional table design,
So that the platform feels trustworthy and easy to use.

**Acceptance Criteria:**
1. TailwindCSS styling applied for clean, modern look
2. Table has alternating row colors for readability
3. Header sticky (remains visible during scroll)
4. Hover effects on rows
5. Responsive design: table scrolls horizontally on mobile, or switches to card layout
6. Loading states and empty states styled
7. Color scheme consistent with platform branding

**Prerequisites:** Story 3.11

---

**Story 3.14: Add Context Window and Capabilities Icons**

As a user,
I want to see capabilities as icons/badges,
So that I can quickly identify supported features.

**Acceptance Criteria:**
1. Capabilities displayed as icon badges (function icon, eye icon for vision, etc.)
2. Context window displayed with formatting (e.g., "128K tokens")
3. Tooltip on hover explains capability
4. Icons use consistent design system
5. Icons visible in both table and card views

**Prerequisites:** Story 3.13

---

**Story 3.15: Optimize API Response and Caching**

As a developer,
I want optimized API response with caching,
So that page loads are fast and reduce database load.

**Acceptance Criteria:**
1. Backend implements Redis caching for `/api/models` response (1 hour TTL)
2. API response includes only necessary fields (avoid over-fetching)
3. Cache key invalidated when model data updated in admin
4. API response compressed (gzip)
5. Page load time <2 seconds verified
6. Frontend implements React Query for client-side caching

**Prerequisites:** Story 3.9, Story 1.5

---

# Epic 4: Model Detail & Cost Calculator

**Goal:** Enable deep-dive model exploration and cost calculation for workload planning through detailed model modals and embedded cost calculator.

**Value Delivery:** Users can view complete model specifications, all benchmark scores, and calculate estimated monthly costs for their specific workloads—supporting informed decision-making.

**Estimated Stories:** 10-12 stories

---

**Story 4.1: Create Model Detail Modal Component**

As a user,
I want to click a model to see full details,
So that I can explore specifications beyond table summary.

**Acceptance Criteria:**
1. Clicking model name in table opens modal overlay
2. Modal component created with header (model name, provider), close button
3. Modal body displays placeholder content
4. Modal dismissable by clicking close button or clicking outside
5. URL updates with model ID (e.g., `/?model=gpt-4`) for shareable links
6. Browser back button closes modal

**Prerequisites:** Story 3.9

---

**Story 4.2: Add Overview Tab with Model Specifications**

As a user,
I want to see complete model specifications,
So that I understand all model details.

**Acceptance Criteria:**
1. Tabbed interface created in modal (Overview, Benchmarks, Pricing tabs)
2. Overview tab displays:
   - Full model name, provider, version, release date, status
   - Pricing: input price/1M, output price/1M, currency
   - Capabilities: context window, max output, all capability flags
   - Last updated timestamp
3. Data fetched from GET `/api/models/{id}` endpoint
4. Loading state shown while fetching
5. Information organized in clear sections with labels

**Prerequisites:** Story 4.1

---

**Story 4.3: Create Backend API for Model Detail**

As a developer,
I want dedicated API endpoint for model details,
So that modal can fetch complete model data efficiently.

**Acceptance Criteria:**
1. GET `/api/models/{id}` endpoint created
2. Endpoint returns single model with all fields
3. Response includes complete capabilities object
4. Response includes all benchmark scores (not just top 3)
5. Endpoint returns 404 if model not found
6. Response cached in Redis (30 min TTL)

**Prerequisites:** Story 4.2

---

**Story 4.4: Add Benchmarks Tab with All Scores**

As a user,
I want to see all benchmark scores for a model,
So that I can evaluate performance across multiple dimensions.

**Acceptance Criteria:**
1. Benchmarks tab displays all available benchmark scores for model
2. Scores organized by category (reasoning, math, code, language, multimodal)
3. Each benchmark shows: name, score, max_score, interpretation
4. Test date and source URL link displayed if available
5. Missing benchmarks show as "Not tested" or excluded
6. Scores sorted by category then alphabetically

**Prerequisites:** Story 4.3

---

**Story 4.5: Add Pricing Tab with Detailed Breakdown**

As a user,
I want detailed pricing information and calculator,
So that I can understand costs and estimate my usage.

**Acceptance Criteria:**
1. Pricing tab displays:
   - Input price per 1M tokens (with per-1K and per-token breakdown)
   - Output price per 1M tokens (with per-1K and per-token breakdown)
   - Currency
   - Pricing validity period
   - Last updated timestamp
2. Comparison note showing how price compares to similar models
3. Embedded cost calculator widget (next story)

**Prerequisites:** Story 4.4

---

**Story 4.6: Create Cost Calculator Component**

As a user,
I want to calculate estimated monthly cost for my workload,
So that I can budget and compare costs.

**Acceptance Criteria:**
1. Cost calculator component created with inputs:
   - Monthly token volume (slider or number input, default 1M)
   - Input/output ratio (slider, default 50/50)
2. Calculator displays:
   - Total monthly cost for current model
   - Cost breakdown (input tokens cost + output tokens cost)
3. Calculation updates in real-time as inputs change
4. Calculation formula: (volume * ratio * input_price) + (volume * (1-ratio) * output_price)
5. Cost displayed with currency symbol and formatting

**Prerequisites:** Story 4.5

---

**Story 4.7: Embed Calculator in Pricing Tab and Create Standalone Page**

As a user,
I want calculator embedded in model detail and available standalone,
So that I can calculate costs from anywhere.

**Acceptance Criteria:**
1. Calculator widget embedded in Pricing tab of model detail modal
2. Standalone calculator page created at `/calculator` route
3. Standalone page shows calculator for all models simultaneously
4. Standalone calculator shows results table sorted by cost
5. Navigation link to calculator in main header
6. Calculator state syncs when transitioning from embedded to standalone

**Prerequisites:** Story 4.6

---

**Story 4.8: Add Cost Comparison Table to Standalone Calculator**

As a user,
I want to see costs for all models in calculator,
So that I can compare which is cheapest for my workload.

**Acceptance Criteria:**
1. Calculator page displays results table below inputs
2. Table shows all models with columns: name, provider, monthly cost, cost breakdown
3. Table sorted by cost ascending (cheapest first)
4. Cost savings column shows % saved vs most expensive
5. Table updates in real-time as calculator inputs change
6. Highlight cheapest model with badge or color

**Prerequisites:** Story 4.7

---

**Story 4.9: Add Preset Workload Scenarios**

As a user,
I want preset scenarios for common workloads,
So that I can quickly see costs without manual input.

**Acceptance Criteria:**
1. Preset buttons above calculator: "Small Project" (1M tokens, 60/40), "Medium Project" (5M tokens, 50/50), "Large Project" (20M tokens, 70/30), "Custom"
2. Clicking preset sets calculator inputs automatically
3. Custom mode (default) allows manual input
4. Selected preset highlighted
5. Tooltip explains what each preset represents

**Prerequisites:** Story 4.8

---

**Story 4.10: Add Visualization to Calculator Results**

As a user,
I want visual cost comparison,
So that I can quickly see cost differences.

**Acceptance Criteria:**
1. Bar chart added below calculator results table
2. Chart shows top 10 models by cost (or all if <10)
3. X-axis: model names, Y-axis: monthly cost
4. Chart updates in real-time with calculator inputs
5. Clicking bar in chart highlights corresponding row in table
6. Chart uses Chart.js library

**Prerequisites:** Story 4.9

---

**Story 4.11: Add Export/Share Calculator Results**

As a user,
I want to share calculator results,
So that I can discuss costs with team.

**Acceptance Criteria:**
1. "Share" button generates shareable URL with calculator parameters (volume, ratio)
2. URL format: `/calculator?volume=5000000&ratio=60`
3. Opening shared URL loads calculator with those parameters
4. "Copy Link" button copies URL to clipboard
5. Success message confirms copy
6. (Future enhancement: Export as CSV or screenshot)

**Prerequisites:** Story 4.10

---

**Story 4.12: Optimize Calculator Performance**

As a developer,
I want calculator to perform efficiently,
So that calculations are instant even with many models.

**Acceptance Criteria:**
1. Calculation logic optimized to run in <100ms
2. Input changes debounced (100ms) to avoid excessive re-renders
3. Memoization used for expensive calculations
4. Chart rendering optimized (throttled updates)
5. Performance verified with 100+ models
6. No visible lag when adjusting sliders

**Prerequisites:** Story 4.8

---

# Epic 5: Multi-Model Comparison & Visualization

**Goal:** Implement side-by-side model comparison with bar chart visualization, enabling users to deeply analyze and contrast multiple models.

**Value Delivery:** Users can select 2-5 models and see detailed side-by-side comparison with synchronized charts, facilitating informed selection decisions.

**Estimated Stories:** 12-14 stories

---

**Story 5.1: Create Comparison Page Route and Layout**

As a user,
I want dedicated comparison page for selected models,
So that I can focus on comparing my shortlisted options.

**Acceptance Criteria:**
1. Comparison page route created at `/compare`
2. Page layout created with space for model cards and charts
3. "Compare Selected" button in comparison basket navigates to this page
4. URL includes selected model IDs (e.g., `/compare?models=1,2,3`)
5. Page displays message if no models selected ("Select at least 2 models to compare")
6. Back button or link returns to main table

**Prerequisites:** Story 3.11

---

**Story 5.2: Display Selected Models in Side-by-Side Cards**

As a user,
I want to see selected models side-by-side,
So that I can compare them visually.

**Acceptance Criteria:**
1. Comparison page displays 2-5 model cards horizontally (or stacked on mobile)
2. Each card shows: model name, provider, pricing, key capabilities
3. Cards have equal width for aligned comparison
4. Remove button (X) on each card removes model from comparison
5. Cards scroll horizontally if >3 models on smaller screens
6. Empty card placeholder prompts adding more models if <2

**Prerequisites:** Story 5.1

---

**Story 5.3: Create Comparison Table Component**

As a user,
I want detailed comparison table below cards,
So that I can see all attributes aligned.

**Acceptance Criteria:**
1. Comparison table created with rows for each attribute, columns for each model
2. Rows include: provider, pricing (input/output), context window, all capabilities, key benchmarks
3. Table cells aligned vertically for easy comparison
4. Differences highlighted (e.g., cheapest price in green, highest score in green)
5. Scrollable if many attributes
6. Toggle to show/hide certain attribute categories

**Prerequisites:** Story 5.2

---

**Story 5.4: Add Benchmark Comparison Section**

As a user,
I want to compare benchmark scores across models,
So that I can evaluate relative performance.

**Acceptance Criteria:**
1. Benchmark comparison section added below basic comparison table
2. Lists all benchmarks with scores for each selected model
3. Models shown in columns, benchmarks in rows
4. Highest score in each row highlighted
5. Missing scores shown as "N/A"
6. Benchmarks grouped by category with collapsible sections

**Prerequisites:** Story 5.3

---

**Story 5.5: Integrate Chart.js Library**

As a developer,
I want Chart.js configured and ready,
So that I can create visualizations.

**Acceptance Criteria:**
1. Chart.js library installed
2. Basic chart component wrapper created (reusable)
3. Chart configuration defaults set (colors, fonts, responsiveness)
4. Test chart renders successfully
5. Chart responsive and works on mobile

**Prerequisites:** Story 5.2

---

**Story 5.6: Create Bar Chart for Benchmark Comparison**

As a user,
I want bar chart showing benchmark scores,
So that I can visually compare model performance.

**Acceptance Criteria:**
1. Bar chart added to comparison page below comparison table
2. X-axis: benchmark names, Y-axis: scores
3. Grouped bars: one bar per model for each benchmark
4. Chart shows top 5-8 benchmarks by default
5. Legend identifies which bar color represents which model
6. Chart responsive and readable on all screen sizes

**Prerequisites:** Story 5.5

---

**Story 5.7: Add Metric Selector for Chart**

As a user,
I want to choose which benchmarks to visualize,
So that I can focus on relevant metrics.

**Acceptance Criteria:**
1. Metric selector dropdown or multi-select above chart
2. Lists all available benchmarks with categories
3. Default selection: top 5 key benchmarks
4. Selecting/deselecting updates chart in real-time
5. "Select All" / "Deselect All" buttons
6. Selection saved in session storage

**Prerequisites:** Story 5.6

---

**Story 5.8: Add Pricing Comparison Visualization**

As a user,
I want to visualize pricing differences,
So that I can quickly identify cheapest options.

**Acceptance Criteria:**
1. Pricing comparison chart added (bar chart or separate section)
2. Shows input price, output price, and total for each model
3. Bars color-coded (input vs output)
4. Cheapest model highlighted
5. Cost difference vs cheapest shown as percentage

**Prerequisites:** Story 5.6

---

**Story 5.9: Add Capabilities Comparison Matrix**

As a user,
I want visual comparison of capabilities,
So that I can see feature support at a glance.

**Acceptance Criteria:**
1. Capabilities matrix created as grid or table
2. Rows: capabilities (function calling, vision, streaming, etc.)
3. Columns: selected models
4. Checkmark icon if model supports capability, X or empty if not
5. Color coding: green for yes, gray for no
6. Visual makes it easy to spot gaps

**Prerequisites:** Story 5.4

---

**Story 5.10: Add Chart Type Switcher**

As a user,
I want to switch chart types,
So that I can view data in different formats.

**Acceptance Criteria:**
1. Chart type selector buttons: "Bar Chart" (default), "Grouped Bar" (if applicable)
2. (Future: Radar, Scatter - out of MVP scope)
3. Clicking button re-renders chart in selected type
4. Chart type persists during comparison session
5. Different chart types optimized for different data (pricing vs benchmarks)

**Prerequisites:** Story 5.7

---

**Story 5.11: Implement Chart Interactions (Hover, Click)**

As a user,
I want interactive charts,
So that I can explore data in detail.

**Acceptance Criteria:**
1. Hovering over chart elements shows tooltip with exact values
2. Tooltip displays model name, benchmark name, score
3. Clicking legend items toggles model visibility in chart
4. Smooth transitions when data changes
5. Chart zoom/pan (optional for MVP)

**Prerequisites:** Story 5.8

---

**Story 5.12: Add Export Comparison Feature**

As a user,
I want to export comparison data,
So that I can share with team or save for later.

**Acceptance Criteria:**
1. "Export" button on comparison page
2. Export as CSV: all models and attributes in table format
3. (Future: Export chart as PNG)
4. File download triggers with descriptive filename (comparison-YYYY-MM-DD.csv)
5. Success message confirms export

**Prerequisites:** Story 5.10

---

**Story 5.13: Add Comparison Page Navigation and State Management**

As a user,
I want to add/remove models without leaving comparison page,
So that I can refine my comparison dynamically.

**Acceptance Criteria:**
1. "Add Model" button on comparison page opens model selector
2. Model selector shows all available models (searchable)
3. Clicking model adds to comparison (max 5)
4. URL updates when models added/removed
5. Browser back/forward buttons work correctly
6. Comparison state syncs with main table selection

**Prerequisites:** Story 5.11

---

**Story 5.14: Optimize Comparison Page Performance**

As a developer,
I want comparison page optimized,
So that it loads quickly and responds smoothly.

**Acceptance Criteria:**
1. Comparison data fetched in single API call (batch fetch by IDs)
2. Chart rendering optimized (no unnecessary re-renders)
3. Lazy loading for below-fold content
4. Page load time <2 seconds for 5 models
5. Smooth interactions (no jank when adding/removing models)

**Prerequisites:** Story 5.13

---

# Epic 6: Smart Filter - Best Value Algorithm

**Goal:** Implement "Best Value" smart filter with QAPS (Quality-Adjusted Price per Score) calculation and ranking, providing algorithm-driven model recommendations.

**Value Delivery:** Users can apply intelligent filter to identify models offering best cost-performance ratio, receiving data-driven recommendations that go beyond simple price sorting.

**Estimated Stories:** 8-10 stories

---

**Story 6.1: Design QAPS Calculation Algorithm**

As a developer,
I want QAPS calculation algorithm designed,
So that I can implement value-based ranking.

**Acceptance Criteria:**
1. QAPS formula documented: QAPS = Composite_Quality_Score / Total_Price
2. Composite quality score calculation defined (weighted average of benchmarks)
3. Benchmark weights determined (e.g., 30% reasoning, 25% code, 20% math, 15% language, 10% multimodal)
4. Total price calculation defined (input + output price averaged or weighted)
5. Normalization approach for benchmarks with different scales documented
6. Edge cases handled (missing benchmarks, free models)

**Prerequisites:** None (design task)

---

**Story 6.2: Create Backend Service for QAPS Calculation**

As a developer,
I want backend service that calculates QAPS for all models,
So that I can rank models by value.

**Acceptance Criteria:**
1. QAPS calculation service created in backend domain layer
2. Service fetches all active models with benchmarks
3. Service computes composite quality score per model (weighted average of available benchmarks)
4. Service computes QAPS = quality score / total price
5. Service returns ranked list of models with QAPS values
6. Unit tests verify calculation correctness

**Prerequisites:** Story 6.1

---

**Story 6.3: Create API Endpoint for Best Value Filter**

As a developer,
I want API endpoint that returns Best Value ranked models,
So that frontend can display smart filter results.

**Acceptance Criteria:**
1. GET `/api/smart-filters/best-value` endpoint created
2. Endpoint calls QAPS calculation service
3. Returns JSON array of models sorted by QAPS descending
4. Response includes: model ID, name, provider, pricing, QAPS score, quality score
5. Optional query param: limit (default 10 models)
6. Endpoint cached (Redis, 1 hour TTL)

**Prerequisites:** Story 6.2

---

**Story 6.4: Add Best Value Filter Button to Main Table**

As a user,
I want Best Value filter button on main table,
So that I can quickly see recommended models.

**Acceptance Criteria:**
1. "Best Value" button added to filter sidebar or above table
2. Button has distinctive styling (badge, icon)
3. Clicking button fetches from best value endpoint
4. Table filtered to show only top 10 best value models
5. Table automatically sorted by QAPS descending
6. Badge shows "Best Value" label on filtered models

**Prerequisites:** Story 6.3, Story 3.7

---

**Story 6.5: Display QAPS Score and Value Indicator**

As a user,
I want to see QAPS score and value indicator,
So that I understand why models are recommended.

**Acceptance Criteria:**
1. QAPS score column added to table when Best Value filter active
2. Score displayed as number with 2 decimal places
3. Value badge/icon shown on models with high QAPS (top 20%)
4. Tooltip explains QAPS score on hover
5. Model detail modal shows QAPS score in overview tab

**Prerequisites:** Story 6.4

---

**Story 6.6: Add Explanation Panel for Best Value Filter**

As a user,
I want explanation of Best Value algorithm,
So that I understand the recommendations.

**Acceptance Criteria:**
1. Explanation panel appears when Best Value filter activated
2. Panel explains: "Best Value ranks models by Quality-Adjusted Price per Score (QAPS), combining performance across benchmarks with cost-effectiveness"
3. Shows formula: QAPS = Quality Score / Price
4. Lists benchmark weights used in calculation
5. Collapsible panel with "Learn More" link to detailed docs
6. Panel dismissible with "Got it" button (don't show again option)

**Prerequisites:** Story 6.5

---

**Story 6.7: Add Quality Score Breakdown in Model Detail**

As a user,
I want to see quality score breakdown,
So that I understand how model's score was calculated.

**Acceptance Criteria:**
1. Quality score section added to model detail modal (Overview or Benchmarks tab)
2. Shows composite quality score as number
3. Shows breakdown by benchmark category with weights
4. Example: "Reasoning (30%): 85, Code (25%): 90, Math (20%): 80, etc."
5. Visual progress bars show contribution of each category
6. Missing benchmarks noted as "Not included in score"

**Prerequisites:** Story 6.5

---

**Story 6.8: Handle Edge Cases in QAPS Calculation**

As a developer,
I want edge cases handled gracefully,
So that algorithm works for all models.

**Acceptance Criteria:**
1. Free/open-source models (price = $0) handled separately (avoid division by zero)
2. Models with missing benchmarks: score calculated from available benchmarks only
3. Models with <3 benchmarks excluded from Best Value ranking (insufficient data)
4. Very expensive models capped to avoid skewing (or flagged as "Premium" tier)
5. Error handling if benchmark data unavailable
6. Unit tests cover all edge cases

**Prerequisites:** Story 6.2

---

**Story 6.9: Add Filter Toggle and Clear Functionality**

As a user,
I want to toggle Best Value filter on/off,
So that I can compare filtered vs unfiltered views.

**Acceptance Criteria:**
1. Best Value button acts as toggle (active/inactive state)
2. Active state highlighted with different color/style
3. Clicking again deactivates filter and returns to full model list
4. "Clear All Filters" button clears Best Value along with other filters
5. Filter state shown in URL query params for shareability

**Prerequisites:** Story 6.6

---

**Story 6.10: Cache and Optimize Best Value Calculation**

As a developer,
I want Best Value calculation cached and optimized,
So that filter responds instantly.

**Acceptance Criteria:**
1. Best Value results cached in Redis (1 hour TTL)
2. Cache invalidated when model pricing or benchmarks updated
3. Background job pre-computes QAPS scores (optional for MVP)
4. API response time <500ms
5. Frontend caches results with React Query (5 min stale time)

**Prerequisites:** Story 6.8

---

# Epic 7: Data Quality & Admin Enhancements

**Goal:** Add validation, timestamp tracking, bulk import, and data freshness indicators to ensure platform maintains high data quality.

**Value Delivery:** Platform data remains accurate and current through validation workflows, admin tools for efficient data management, and user-visible freshness indicators.

**Estimated Stories:** 8-10 stories

---

**Story 7.1: Add Pricing Validation Rules**

As a developer,
I want pricing validation in backend,
So that invalid data is rejected.

**Acceptance Criteria:**
1. Validation ensures input/output prices are positive numbers
2. Validation checks that input price < output price * 10 (sanity check for typical patterns)
3. Currency validation (must be valid currency code)
4. Pricing validity dates: valid_from < valid_to
5. Validation errors return 400 with descriptive messages
6. Unit tests cover all validation rules

**Prerequisites:** Story 2.5

---

**Story 7.2: Add Benchmark Score Validation**

As a developer,
I want benchmark score validation,
So that scores are within expected ranges.

**Acceptance Criteria:**
1. Validation checks score against benchmark's typical_range_min/max
2. Scores outside range flagged but allowed (admin can override with note)
3. Warning message displayed in admin UI if score flagged
4. Max_score validation: score <= max_score
5. Admin can mark outlier scores as "verified" to suppress warnings

**Prerequisites:** Story 2.10

---

**Story 7.3: Add Timestamp Tracking for All Updates**

As an administrator,
I want to see when data was last updated,
So that I know what needs refreshing.

**Acceptance Criteria:**
1. All database entities have created_at and updated_at timestamps
2. Timestamps automatically set on insert/update
3. Admin panel displays last updated time for each model (human-readable format)
4. Models list sortable by last updated
5. Dashboard shows oldest models (need attention) at top

**Prerequisites:** Story 2.12

---

**Story 7.4: Create Data Freshness Indicator for Users**

As a user,
I want to see when pricing was last updated,
So that I know data currency.

**Acceptance Criteria:**
1. Pricing timestamp displayed on model cards in table ("Updated 3 days ago")
2. Model detail modal shows pricing valid from/to dates
3. Stale data (>14 days old) shown with warning icon and message
4. Color coding: green (<7 days), yellow (7-14 days), red (>14 days)
5. Platform-wide data freshness indicator in footer ("Last updated: date")

**Prerequisites:** Story 7.3

---

**Story 7.5: Add Bulk Operations in Admin Panel**

As an administrator,
I want to perform bulk operations,
So that I can efficiently update multiple models.

**Acceptance Criteria:**
1. Checkbox selection in admin models list (select multiple)
2. Bulk actions dropdown: "Update Status", "Update Currency", "Delete Selected"
3. Bulk update form shows fields to update with checkboxes (only update selected fields)
4. Confirmation dialog before bulk operations
5. Success/failure messages show count (e.g., "5 models updated, 2 failed")

**Prerequisites:** Story 2.8

---

**Story 7.6: Enhance CSV Import with Validation and Error Reporting**

As an administrator,
I want detailed error reporting for CSV imports,
So that I can fix data issues.

**Acceptance Criteria:**
1. CSV import validates each row before inserting
2. Invalid rows collected with specific error messages
3. Import results page shows:
   - Total rows processed
   - Successful imports (count)
   - Failed imports (count with error details table)
4. Failed rows can be downloaded as CSV for correction
5. Option to skip errors and import valid rows only

**Prerequisites:** Story 2.11

---

**Story 7.7: Add Admin Dashboard with Data Quality Metrics**

As an administrator,
I want dashboard showing data quality overview,
So that I can monitor platform health.

**Acceptance Criteria:**
1. Admin dashboard page created at `/admin/dashboard`
2. Metrics displayed:
   - Total models count
   - Models needing updates (>7 days old)
   - Models with incomplete benchmarks (<3 benchmarks)
   - Recent additions (last 7 days)
   - Most viewed models (if analytics available)
3. Quick action links: "Update stale models", "Add benchmarks"
4. Data quality score calculation (percentage of complete models)

**Prerequisites:** Story 7.3, Story 2.2

---

**Story 7.8: Add Model Duplicate Detection**

As an administrator,
I want duplicate detection when adding models,
So that I avoid creating duplicates.

**Acceptance Criteria:**
1. Before saving new model, system checks for existing model with same name+provider
2. If potential duplicate found, warning shown with link to existing model
3. Admin can choose: "Edit existing" or "Create anyway" (for versions)
4. Fuzzy matching catches similar names (e.g., "GPT-4" vs "GPT 4")
5. Duplicate check runs on add and edit

**Prerequisites:** Story 2.5

---

**Story 7.9: Add Audit Log for Admin Actions**

As an administrator,
I want audit log of all changes,
So that I can track who did what and when.

**Acceptance Criteria:**
1. Audit log table created (admin_user, action, entity_type, entity_id, changes_json, timestamp)
2. All create/update/delete operations logged
3. Admin audit log page displays recent changes (paginated)
4. Log filterable by admin user, action type, date range
5. Log shows before/after values for updates
6. Log export as CSV

**Prerequisites:** Story 2.1

---

**Story 7.10: Add Data Export Feature for Backups**

As an administrator,
I want to export all platform data,
So that I can create backups or migrate data.

**Acceptance Criteria:**
1. Export button in admin panel
2. Export generates JSON file with all models, capabilities, benchmarks
3. Export includes metadata: export date, version, record counts
4. Large exports streamed (don't load all in memory)
5. Import functionality created (restore from backup JSON)
6. Export/import tested with 100+ models

**Prerequisites:** Story 7.9

---

# Epic 8: Responsive Design & Mobile Optimization

**Goal:** Ensure platform works seamlessly on tablet and mobile devices through responsive layouts and mobile-friendly interactions.

**Value Delivery:** Users can access and use platform from any device, expanding reach to mobile users evaluating models on-the-go.

**Estimated Stories:** 8-10 stories

---

**Story 8.1: Audit Current Responsive Behavior**

As a developer,
I want to audit existing responsive issues,
So that I know what needs fixing.

**Acceptance Criteria:**
1. Test platform on multiple breakpoints (mobile 375px, tablet 768px, desktop 1280px, large 1920px)
2. Document issues: layout breaks, text overflow, buttons too small, etc.
3. Screenshot problematic views
4. Prioritize fixes: critical (broken), important (usability), nice-to-have (polish)
5. Create fix backlog from audit findings

**Prerequisites:** Story 3.13 (assumes some responsive design already exists)

---

**Story 8.2: Implement Mobile-First Table View**

As a user,
I want comparison table optimized for mobile,
So that I can browse models on my phone.

**Acceptance Criteria:**
1. On mobile (<768px), table switches to card-based layout
2. Each model displayed as vertical card with key info:
   - Model name, provider (header)
   - Pricing (prominent)
   - Top 2-3 capabilities (icons)
   - CTA button ("View Details", "Compare")
3. Cards stack vertically with scroll
4. Search and filters accessible in mobile drawer/overlay
5. Performance optimized (virtual scrolling if needed)

**Prerequisites:** Story 8.1, Story 3.13

---

**Story 8.3: Create Mobile-Friendly Filter Drawer**

As a user,
I want easy access to filters on mobile,
So that I can refine my search.

**Acceptance Criteria:**
1. Filter sidebar converts to bottom drawer or slide-in overlay on mobile
2. "Filters" button opens drawer (shows active filter count badge)
3. Drawer slides up from bottom with smooth animation
4. Close button and backdrop click dismisses drawer
5. Apply filters button closes drawer and applies selections
6. Drawer scrollable if filter content exceeds viewport

**Prerequisites:** Story 8.2, Story 3.7

---

**Story 8.4: Optimize Model Detail Modal for Mobile**

As a user,
I want model detail modal readable on mobile,
So that I can explore specifications on small screens.

**Acceptance Criteria:**
1. Modal takes full screen on mobile (<768px)
2. Tabs horizontal scrollable if needed
3. Text size and spacing optimized for mobile readability
4. Close button accessible (top right, large enough)
5. Swipe down to dismiss (optional enhancement)
6. Content scrollable within modal body

**Prerequisites:** Story 8.2, Story 4.2

---

**Story 8.5: Make Cost Calculator Touch-Friendly**

As a user,
I want calculator inputs optimized for touch,
So that I can easily adjust parameters on mobile.

**Acceptance Criteria:**
1. Slider controls large enough for finger input (minimum 44px touch target)
2. Number inputs use mobile-friendly input type (numeric keyboard)
3. Preset buttons large and spaced for easy tapping
4. Results table readable in card format on mobile
5. Chart responsive and readable (switch to vertical orientation if needed)

**Prerequisites:** Story 8.2, Story 4.6

---

**Story 8.6: Optimize Comparison View for Mobile**

As a user,
I want comparison view usable on mobile,
So that I can compare models on small screens.

**Acceptance Criteria:**
1. Model comparison cards stack vertically on mobile
2. Comparison table becomes accordion-style (tap to expand attribute sections)
3. Charts responsive: switch to vertical bars or reduce models shown
4. Swipe horizontally to see additional models if >2 selected
5. "Add Model" button accessible and functional on mobile

**Prerequisites:** Story 8.4, Story 5.2

---

**Story 8.7: Improve Touch Interactions and Gestures**

As a user,
I want natural touch interactions,
So that mobile experience feels native.

**Acceptance Criteria:**
1. Swipe gestures implemented where appropriate (swipe to dismiss modals, swipe between comparison models)
2. Pull-to-refresh on main table (optional)
3. Long-press context menus disabled where irrelevant
4. Tap feedback (ripple effects or state changes)
5. Scroll momentum feels natural (no janky scrolling)

**Prerequisites:** Story 8.6

---

**Story 8.8: Test and Fix Tablet Experience**

As a user,
I want optimized tablet experience,
So that I can use platform on iPad or Android tablet.

**Acceptance Criteria:**
1. Test on tablet breakpoint (768px - 1024px)
2. Layout uses hybrid approach: compact but not mobile cards
3. Sidebar filters remain visible (narrower width)
4. Table shows more columns than mobile but fewer than desktop
5. Two-column comparison layout on tablet (vs single column mobile)
6. Touch targets sized for fingers but layout more information-dense

**Prerequisites:** Story 8.7

---

**Story 8.9: Optimize Performance for Mobile Networks**

As a developer,
I want mobile performance optimized,
So that platform loads quickly on slower connections.

**Acceptance Criteria:**
1. Images optimized and compressed (lazy loading)
2. Code splitting: load only what's needed for current view
3. API responses minimized (only essential fields for mobile)
4. Service worker caching for offline capability (optional)
5. Page load <3 seconds on 3G connection
6. Lighthouse mobile score >80

**Prerequisites:** Story 8.8

---

**Story 8.10: Add Progressive Web App (PWA) Features**

As a user,
I want to add platform to home screen,
So that I can access it like a native app.

**Acceptance Criteria:**
1. Web app manifest created (icons, theme colors, display mode)
2. Service worker registered for basic caching
3. "Add to Home Screen" prompt shown on eligible devices
4. App opens in standalone mode when launched from home screen
5. Splash screen configured
6. Works offline with cached data (view previously loaded models)

**Prerequisites:** Story 8.9

---

## Story Guidelines Reference

**Story Format:**

```
**Story [EPIC.N]: [Story Title]**

As a [user type],
I want [goal/desire],
So that [benefit/value].

**Acceptance Criteria:**
1. [Specific testable criterion]
2. [Another specific criterion]
3. [etc.]

**Prerequisites:** [Dependencies on previous stories, if any]
```

**Story Requirements:**

- **Vertical slices** - Complete, testable functionality delivery
- **Sequential ordering** - Logical progression within epic
- **No forward dependencies** - Only depend on previous work
- **AI-agent sized** - Completable in 2-4 hour focused session
- **Value-focused** - Integrate technical enablers into value-delivering stories

---

**Total Stories: 83 stories across 8 epics**

**For implementation:** Use the `create-story` workflow (Phase 4) to generate individual story implementation plans from this epic breakdown. Each story will be expanded into detailed development specifications with technical approach, file changes, and testing criteria.

**Next Steps:**
1. Review and refine epic breakdown with stakeholders
2. Run UX specification workflow for detailed UI/UX design
3. Run solution-architecture workflow for technical architecture
4. Begin Phase 4 implementation with Epic 1

---

_This epic breakdown serves as the complete tactical implementation roadmap for LLM Cost Comparison Platform MVP._
