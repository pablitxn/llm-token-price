# LLM Token Price Platform - Admin Panel User Guide

**Version:** 1.0
**Last Updated:** 2025-10-21
**Target Audience:** System Administrators

---

## Table of Contents

1. [Introduction](#1-introduction)
2. [Getting Started](#2-getting-started)
   - [2.1 System Requirements](#21-system-requirements)
   - [2.2 Accessing the Admin Panel](#22-accessing-the-admin-panel)
3. [Authentication & Session Management](#3-authentication--session-management)
   - [3.1 Logging In](#31-logging-in)
   - [3.2 Session Duration](#32-session-duration)
   - [3.3 Logging Out](#33-logging-out)
   - [3.4 Security Best Practices](#34-security-best-practices)
4. [Model Management](#4-model-management)
   - [4.1 Viewing Models](#41-viewing-models)
   - [4.2 Adding a New Model](#42-adding-a-new-model)
   - [4.3 Editing an Existing Model](#43-editing-an-existing-model)
   - [4.4 Deleting a Model](#44-deleting-a-model)
   - [4.5 Model Capabilities](#45-model-capabilities)
   - [4.6 Searching and Filtering](#46-searching-and-filtering)
5. [Benchmark Management](#5-benchmark-management)
   - [5.1 Understanding Benchmarks](#51-understanding-benchmarks)
   - [5.2 Viewing Benchmarks](#52-viewing-benchmarks)
   - [5.3 Adding a New Benchmark](#53-adding-a-new-benchmark)
   - [5.4 Editing a Benchmark](#54-editing-a-benchmark)
   - [5.5 Deleting a Benchmark](#55-deleting-a-benchmark)
   - [5.6 Benchmark Categories](#56-benchmark-categories)
   - [5.7 Managing Benchmark Scores](#57-managing-benchmark-scores)
6. [CSV Import](#6-csv-import)
   - [6.1 When to Use CSV Import](#61-when-to-use-csv-import)
   - [6.2 Downloading the CSV Template](#62-downloading-the-csv-template)
   - [6.3 CSV File Format](#63-csv-file-format)
   - [6.4 Uploading CSV Files](#64-uploading-csv-files)
   - [6.5 Validation Rules](#65-validation-rules)
   - [6.6 Troubleshooting CSV Import](#66-troubleshooting-csv-import)
7. [Data Quality Dashboard](#7-data-quality-dashboard)
   - [7.1 Accessing the Dashboard](#71-accessing-the-dashboard)
   - [7.2 Dashboard Metrics Explained](#72-dashboard-metrics-explained)
   - [7.3 Data Freshness Indicators](#73-data-freshness-indicators)
   - [7.4 Taking Action on Metrics](#74-taking-action-on-metrics)
8. [Audit Log](#8-audit-log)
   - [8.1 What is Logged](#81-what-is-logged)
   - [8.2 Viewing Audit History](#82-viewing-audit-history)
   - [8.3 Filtering Logs](#83-filtering-logs)
   - [8.4 Exporting Audit Data](#84-exporting-audit-data)
9. [Troubleshooting](#9-troubleshooting)
   - [9.1 Common Errors](#91-common-errors)
   - [9.2 Authentication Issues](#92-authentication-issues)
   - [9.3 Validation Errors](#93-validation-errors)
   - [9.4 Import Failures](#94-import-failures)
   - [9.5 Performance Issues](#95-performance-issues)
10. [API Reference](#10-api-reference)
11. [Support & Feedback](#11-support--feedback)

---

## 1. Introduction

The **LLM Token Price Platform Admin Panel** is a secure web interface designed for administrators to manage the platform's data on Large Language Model (LLM) providers, pricing, and performance benchmarks.

### Key Features

- **Model Management**: Create, view, update, and delete LLM model records with pricing and capability data
- **Benchmark Management**: Define and maintain performance benchmark definitions and scores
- **Bulk CSV Import**: Import large datasets of benchmark scores efficiently
- **Data Quality Dashboard**: Monitor data freshness and completeness with real-time metrics
- **Audit Logging**: Track all administrative changes for compliance and debugging *(Coming Soon)*
- **Secure Authentication**: JWT-based authentication with HttpOnly cookies for XSS protection

### Technology Stack

- **Frontend**: React 19 with TypeScript, TailwindCSS 4, TanStack Query
- **Backend**: ASP.NET Core 9 with PostgreSQL 16 and Redis 7.2
- **Architecture**: Hexagonal Architecture (Ports & Adapters pattern)

---

## 2. Getting Started

### 2.1 System Requirements

**Supported Browsers:**
- Google Chrome (v120+)
- Mozilla Firefox (v120+)
- Microsoft Edge (v120+)
- Safari (v17+)

**Network Requirements:**
- Stable internet connection
- Access to admin panel URL (typically `https://yourdomain.com/admin`)
- Port 443 (HTTPS) must be open

### 2.2 Accessing the Admin Panel

1. Navigate to the admin panel URL: `https://yourdomain.com/admin/login`
2. You will be prompted to log in (see [Section 3.1](#31-logging-in))
3. Upon successful authentication, you'll be redirected to the Admin Dashboard

> **Note**: Admin credentials are provided by your system administrator. Contact support if you need access.

---

## 3. Authentication & Session Management

### 3.1 Logging In

**Step-by-Step Login Process:**

1. **Navigate** to the login page: `/admin/login`

2. **Enter Credentials**:
   - **Username**: Minimum 3 characters, maximum 50 characters
   - **Password**: Minimum 6 characters, maximum 100 characters

3. **Submit** the form by clicking "Sign in"

4. **Validation**: The form validates inputs client-side before submission:
   - Empty fields are not allowed
   - Username/password length requirements are enforced
   - Error messages appear inline below each field

5. **Authentication**:
   - Server validates credentials against the admin database
   - On success: JWT token is set in an HttpOnly cookie (24-hour expiration)
   - On failure: Error message displayed: *"Login failed. Please try again."*

**Technical Details:**
- **Endpoint**: `POST /api/admin/auth/login`
- **Security**:
  - HttpOnly cookies prevent XSS attacks (JavaScript cannot access token)
  - Secure flag enforced in production (HTTPS only)
  - SameSite=Strict prevents CSRF attacks

<!-- [Screenshot: Admin Login Page] -->

### 3.2 Session Duration

- **Session Length**: 24 hours from login
- **Cookie Expiration**: Token expires at `DateTimeOffset.UtcNow.AddHours(24)`
- **Idle Timeout**: None (session remains active for full 24 hours)

**Session Expiration Behavior:**
- When your session expires, you'll receive a `401 Unauthorized` error on the next API request
- You'll be automatically redirected to the login page
- You must log in again to continue working

**Best Practices:**
- Log out when stepping away from your workstation
- Do not share your credentials
- Use a password manager for secure credential storage

### 3.3 Logging Out

**Manual Logout:**

1. Click the "Logout" button in the admin panel header (top right)
2. Confirmation: "Logout successful"
3. JWT cookie is cleared immediately
4. You are redirected to the login page

**Technical Details:**
- **Endpoint**: `POST /api/admin/auth/logout`
- **Action**: Deletes the `admin_token` cookie from your browser
- **Response**: 200 OK with message: `{ "success": true, "message": "Logout successful" }`

### 3.4 Security Best Practices

**Do:**
- ✅ Log out after completing administrative tasks
- ✅ Use strong, unique passwords (12+ characters with mixed case, numbers, symbols)
- ✅ Keep your browser updated to the latest version
- ✅ Use HTTPS always (verify the padlock icon in the address bar)

**Don't:**
- ❌ Share your admin credentials with anyone
- ❌ Use public/shared computers for admin tasks
- ❌ Ignore browser security warnings
- ❌ Use the same password across multiple systems

---

## 4. Model Management

### 4.1 Viewing Models

The **Admin Models** page displays all LLM models in the system, including inactive models.

**Accessing the Models Page:**
- Navigate to **"Models"** from the admin panel sidebar
- Default view shows all models sorted by most recently updated

**Table Columns:**
- **Name**: Model identifier (e.g., "GPT-4 Turbo", "Claude 3.5 Sonnet")
- **Provider**: Company offering the model (e.g., "OpenAI", "Anthropic")
- **Version**: Model version string (e.g., "gpt-4-turbo-2024-04-09")
- **Input Price**: Cost per 1 million input tokens (USD)
- **Output Price**: Cost per 1 million output tokens (USD)
- **Status**: `active` (visible to public) or `inactive` (hidden from public API)
- **Updated**: Last modification timestamp with freshness indicator
- **Actions**: Edit, Delete buttons

**Pagination:**
- Default: 20 models per page
- Options: 10, 20, 50, 100 models per page
- Navigation: Previous/Next page controls
- Total count displayed (e.g., "Showing 1-20 of 150 models")

<!-- [Screenshot: Admin Models Page with table view] -->

### 4.2 Adding a New Model

**Step-by-Step:**

1. Click **"Add Model"** button (top right of Models page)

2. **Fill in Required Fields**:

   **Basic Information:**
   - **Name*** (required): Model display name (e.g., "GPT-4o")
   - **Provider*** (required): Company name (e.g., "OpenAI")
   - **Version** (optional): Version identifier (e.g., "gpt-4o-2024-05-13")
   - **Status*** (required): Select `active` or `inactive`

   **Pricing Information:**
   - **Input Price Per 1M Tokens*** (required): Decimal value (e.g., 2.50)
   - **Output Price Per 1M Tokens*** (required): Decimal value (e.g., 10.00)
   - **Currency*** (required): Three-letter code (default: "USD")

   **Metadata:**
   - **Release Date** (optional): Date picker (ISO 8601 format)

3. **Capabilities Section** (expandable):
   - **Vision**: Checkbox (supports image inputs)
   - **Tools**: Checkbox (supports function calling)
   - **JSON Mode**: Checkbox (structured output support)
   - **Max Context Window**: Number (tokens, e.g., 128000)
   - **Max Output Tokens**: Number (tokens, e.g., 4096)

4. **Validation**:
   - Client-side validation using Zod schema
   - Required fields highlighted if missing
   - Pricing must be positive numbers
   - Duplicate detection: Name + Provider combination must be unique

5. **Submit**:
   - Click **"Create Model"** button
   - Loading state: Button shows "Creating..." with spinner
   - On success: Redirected to model details page
   - On error: Inline error message displayed

**API Endpoint**: `POST /api/admin/models`

**Duplicate Handling**:
If a model with the same Name + Provider already exists, you'll receive:
```
Error: Model with this name and provider already exists
Code: DUPLICATE_MODEL
```

<!-- [Screenshot: Add Model Form] -->

### 4.3 Editing an Existing Model

**Step-by-Step:**

1. From the Models page, click **"Edit"** button for the target model
2. Pre-populated form appears with current model data
3. Modify any fields (same validation rules as creation)
4. **Timestamps**:
   - `UpdatedAt` is automatically refreshed to current UTC time
   - `PricingUpdatedAt` is only updated if pricing fields change (input/output prices)
5. Click **"Save Changes"**
6. Confirmation message: "Model updated successfully"

**Technical Details:**
- **Endpoint**: `PUT /api/admin/models/{id}`
- **Timestamp Logic**:
  - All updates refresh `UpdatedAt`
  - Only pricing changes refresh `PricingUpdatedAt` (for freshness tracking)
- **Cache Invalidation**: Public model cache is cleared on update

**Conflict Resolution**:
If another admin updates the same model simultaneously, the last write wins. No optimistic locking is currently implemented.

<!-- [Screenshot: Edit Model Form] -->

### 4.4 Deleting a Model

**Deletion Type**: Soft Delete (preserves data for audit)

**Step-by-Step:**

1. Click **"Delete"** button for the target model

2. **Confirmation Dialog** (Two-Step Verification):
   - **Step 1**: "Are you sure you want to delete [Model Name]?" → Click "Yes"
   - **Step 2**: Type "DELETE" to confirm → Click "Confirm Delete"

3. **Processing**:
   - Model `is_active` flag set to `false` in database
   - Model removed from public API responses
   - Still visible in admin panel with `inactive` status

4. **Confirmation**: "Model deleted successfully"

**Technical Details:**
- **Endpoint**: `DELETE /api/admin/models/{id}`
- **Database**: Sets `is_active = false` (does NOT remove row)
- **Public API**: Inactive models excluded from `GET /api/models`
- **Admin API**: Inactive models still visible (filter by status)

**Recovery:**
Soft-deleted models can be recovered by:
1. Editing the model
2. Changing status from `inactive` to `active`
3. Saving changes

**⚠️ Warning**: Deleting a model does NOT cascade delete benchmark scores. Scores are preserved for historical data integrity.

<!-- [Screenshot: Delete Confirmation Dialog] -->

### 4.5 Model Capabilities

Capabilities define the features supported by each model.

**Capability Fields:**

| Capability | Type | Description | Example |
|------------|------|-------------|---------|
| **Vision** | Boolean | Supports image inputs (multimodal) | GPT-4 Vision, Claude 3.5 Sonnet |
| **Tools** | Boolean | Supports function calling / tool use | GPT-4, Claude 3.5 |
| **JSON Mode** | Boolean | Structured JSON output guarantee | GPT-4 Turbo with json_object mode |
| **Max Context Window** | Number | Maximum input tokens accepted | 128,000 tokens |
| **Max Output Tokens** | Number | Maximum tokens in response | 4,096 tokens |

**How to Set Capabilities:**

1. In the Add/Edit Model form, expand the **"Capabilities"** section
2. Check boxes for supported features
3. Enter numeric values for token limits
4. Save the model

**Public API Impact:**
- Capabilities are included in `GET /api/models` responses
- Users can filter/sort by capability in the comparison table

### 4.6 Searching and Filtering

**Search Bar:**
- **Location**: Top of Models page
- **Searches**: Model name and provider (case-insensitive)
- **Real-time**: Results update as you type
- **Example**: "gpt" returns "GPT-4", "GPT-4 Turbo", "GPT-3.5"

**Filter Options:**

1. **Provider Filter**:
   - Dropdown with all unique providers
   - Example: "OpenAI", "Anthropic", "Google", "Meta"
   - Select to show only models from that provider

2. **Status Filter**:
   - Options: "All", "Active", "Inactive"
   - Default: "All"
   - Use "Inactive" to find soft-deleted models

3. **Pagination**:
   - Page size selector: 10, 20, 50, 100
   - Default: 20 models per page

**Combining Filters:**
All filters work together (AND logic):
- Search: "claude" + Provider: "Anthropic" + Status: "Active"
- Returns: All active Claude models from Anthropic

<!-- [Screenshot: Search and filter controls] -->

---

## 5. Benchmark Management

### 5.1 Understanding Benchmarks

**What are Benchmarks?**
Benchmarks are standardized tests that measure LLM performance across different capabilities. Examples:
- **MMLU** (Massive Multitask Language Understanding): Tests general knowledge
- **HumanEval**: Measures code generation ability
- **GSM8K**: Evaluates mathematical reasoning

**Why are Benchmarks Important?**
- Enable objective comparison between models
- Calculate the **QAPS** (Quality-Adjusted Price per Score) metric
- Help users choose the best-value model for their use case

**Benchmark Anatomy:**
- **Name**: Test identifier (e.g., "MMLU", "HumanEval")
- **Category**: Capability area (Reasoning, Code, Math, Language, Multimodal)
- **Typical Range**: Min/max expected scores (for outlier detection)
- **Weight in QAPS**: Importance percentage in quality calculation
- **Interpretation**: Whether "Higher is better" or "Lower is better"

### 5.2 Viewing Benchmarks

**Accessing Benchmarks:**
- Navigate to **"Benchmarks"** from the admin panel sidebar

**Table Columns:**
- **Name**: Benchmark identifier
- **Category**: One of 5 categories (Reasoning, Code, Math, Language, Multimodal)
- **Typical Range**: Min - Max expected scores
- **Weight**: Percentage contribution to QAPS (total across all benchmarks = 100%)
- **Interpretation**: "Higher is better" or "Lower is better"
- **Status**: `active` or `inactive`
- **Actions**: Edit, Delete

**Filtering:**
- **Category Filter**: Show only benchmarks in a specific category
- **Include Inactive**: Checkbox to show/hide inactive benchmarks

<!-- [Screenshot: Admin Benchmarks Page] -->

### 5.3 Adding a New Benchmark

**Step-by-Step:**

1. Click **"Add Benchmark"** button

2. **Fill in Required Fields**:

   **Basic Information:**
   - **Name*** (required): Benchmark identifier (e.g., "MMLU Pro", "MATH-500")
   - **Category*** (required): Select from dropdown:
     - Reasoning (30% default QAPS weight)
     - Code (25% default)
     - Math (20% default)
     - Language (15% default)
     - Multimodal (10% default)
   - **Description** (optional): Brief explanation of what the benchmark measures

   **Scoring Parameters:**
   - **Typical Range Min** (optional): Expected minimum score (e.g., 0.0)
   - **Typical Range Max** (optional): Expected maximum score (e.g., 100.0)
   - **Interpretation*** (required): Select "Higher is better" or "Lower is better"
   - **Weight in QAPS** (optional): Decimal percentage (e.g., 0.30 for 30%)

   **Metadata:**
   - **Status*** (required): `active` or `inactive`

3. **Validation**:
   - Name must be unique (case-insensitive)
   - Range: Min must be ≤ Max
   - Weight: Must be between 0.0 and 1.0 (0% to 100%)

4. **Submit**:
   - Click **"Create Benchmark"**
   - On success: Redirected to benchmarks list
   - On duplicate: Error "Benchmark with this name already exists"

**API Endpoint**: `POST /api/admin/benchmarks`

<!-- [Screenshot: Add Benchmark Form] -->

### 5.4 Editing a Benchmark

**Step-by-Step:**

1. Click **"Edit"** button for the target benchmark
2. Modify fields (same validation as creation)
3. Click **"Save Changes"**

**⚠️ Warning**: Changing `Typical Range` or `Weight in QAPS` will affect:
- Normalized scores for all models on this benchmark
- QAPS calculations for "Best Value" rankings
- Cache is automatically invalidated on save

### 5.5 Deleting a Benchmark

**Deletion Type**: Soft Delete

**Step-by-Step:**

1. Click **"Delete"** button
2. **Dependency Check**: If benchmark has associated scores, you'll see:
   ```
   Warning: This benchmark has 15 associated scores across 8 models.
   Deleting will set status to 'inactive' but preserve score history.
   Type "DELETE" to confirm.
   ```
3. Type "DELETE" to confirm
4. Benchmark status set to `inactive`

**Technical Details:**
- **Endpoint**: `DELETE /api/admin/benchmarks/{id}`
- **Soft Delete**: Sets `is_active = false`
- **Scores Preserved**: Benchmark scores remain in database (referential integrity)
- **QAPS Impact**: Inactive benchmarks excluded from QAPS calculation

### 5.6 Benchmark Categories

The platform uses 5 standardized categories aligned with the QAPS algorithm:

| Category | Weight (Default) | Description | Example Benchmarks |
|----------|------------------|-------------|-------------------|
| **Reasoning** | 30% | General knowledge, logic, problem-solving | MMLU, Big-Bench Hard, HellaSwag |
| **Code** | 25% | Code generation, debugging, understanding | HumanEval, MBPP, CodeContests |
| **Math** | 20% | Mathematical reasoning, calculations | GSM8K, MATH, MathQA |
| **Language** | 15% | Natural language understanding, generation | TruthfulQA, LAMBADA, SQuAD |
| **Multimodal** | 10% | Vision, audio, cross-modal reasoning | MMMU, VQAv2, ChartQA |

**Why These Weights?**
- Reflects industry prioritization of general reasoning and coding
- Can be customized per benchmark for specific use cases
- Total should sum to 100% for accurate QAPS calculation

### 5.7 Managing Benchmark Scores

**What are Benchmark Scores?**
Individual test results for a model on a specific benchmark. Example:
- Model: "GPT-4 Turbo"
- Benchmark: "MMLU"
- Score: 86.4
- Max Score: 100.0
- Test Date: 2024-04-15

**Adding a Benchmark Score:**

1. Navigate to a model's detail page
2. Scroll to **"Benchmark Scores"** section
3. Click **"Add Score"**

4. **Fill in Score Form**:
   - **Benchmark*** (required): Select from dropdown (grouped by category)
   - **Score*** (required): Decimal value (e.g., 86.4)
   - **Max Score** (optional): Maximum possible score (e.g., 100.0)
   - **Test Date** (optional): Date picker (defaults to today)
   - **Source URL** (optional): Link to official benchmark report
   - **Verified** (optional): Checkbox for verified/official scores
   - **Notes** (optional): Additional context (max 500 characters)

5. **Validation**:
   - **Duplicate Prevention**: Model + Benchmark combination must be unique
   - **Out-of-Range Warning**: If score falls outside `Typical Range`:
     - Yellow warning icon: "Score is outside typical range - verify accuracy"
     - Submission still allowed (allows for exceptional performance)
   - **Normalized Score**: Automatically calculated using formula:
     ```
     Normalized Score = (Score - Min) / (Max - Min)
     Clamped to [0.0, 1.0]
     If Min == Max, returns 1.0
     ```

6. **Submit**:
   - Click **"Add Score"**
   - Normalized score calculated on backend
   - QAPS cache invalidated for this model
   - Success message: "Benchmark score added successfully"

**API Endpoint**: `POST /api/admin/models/{modelId}/benchmarks`

**Viewing Scores:**
- Scores displayed in a table on model detail page
- Columns: Benchmark Name, Category, Score, Normalized Score, Verified, Test Date
- Sort by: Category, Score, Date
- Filter by: Category, Verified status

**Editing/Deleting Scores:**
- **Edit**: Click score row → Modify → Save (recalculates normalized score)
- **Delete**: Click "Delete" → Confirm → Hard delete (removes from database)

<!-- [Screenshot: Add Benchmark Score Form with out-of-range warning] -->

---

## 6. CSV Import

### 6.1 When to Use CSV Import

**Use CSV Import For:**
- ✅ Bulk importing 10+ benchmark scores at once
- ✅ Migrating data from external sources
- ✅ Initial platform seeding with historical data
- ✅ Periodic updates from automated benchmark systems

**Use Manual Entry For:**
- ❌ Single score additions (faster than CSV preparation)
- ❌ Ad-hoc corrections or updates
- ❌ When you need immediate visual feedback per score

### 6.2 Downloading the CSV Template

**Step-by-Step:**

1. Navigate to **"Benchmarks"** → **"Import CSV"** tab
2. Click **"Download Template"** button
3. CSV file downloads: `benchmark-scores-template.csv`

**Template Structure:**

```csv
model_id,benchmark_id,score,max_score,test_date,source_url,verified,notes
123e4567-e89b-12d3-a456-426614174000,987fcdeb-51a2-43f8-9d3c-789012345678,86.4,100.0,2024-04-15,https://example.com/results,true,Official OpenAI report
```

### 6.3 CSV File Format

**Column Specifications:**

| Column | Required | Type | Description | Example | Validation |
|--------|----------|------|-------------|---------|-----------|
| **model_id** | ✅ Yes | UUID | Model's unique identifier | `abc12345-...` | Must exist in database |
| **benchmark_id** | ✅ Yes | UUID | Benchmark's unique identifier | `def67890-...` | Must exist in database |
| **score** | ✅ Yes | Decimal | Test result | `86.4` | Must be numeric |
| **max_score** | ❌ No | Decimal | Maximum possible score | `100.0` | If provided, must be ≥ score |
| **test_date** | ❌ No | ISO 8601 | Date of benchmark run | `2024-04-15` | Format: YYYY-MM-DD |
| **source_url** | ❌ No | URL | Link to official results | `https://...` | Must be valid URL if provided |
| **verified** | ❌ No | Boolean | Is score official? | `true` / `false` | Defaults to `false` |
| **notes** | ❌ No | String | Additional context | `Q1 2024 release` | Max 500 characters |

**Formatting Rules:**

- **Header Row**: First row must contain exact column names (case-sensitive)
- **Encoding**: UTF-8
- **Delimiter**: Comma (`,`)
- **Quotes**: Use double quotes (`"`) for values containing commas
- **Empty Values**: Leave cell empty (not "NULL" or "N/A")
- **Line Endings**: LF (`\n`) or CRLF (`\r\n`) both supported

**Example Valid CSV:**

```csv
model_id,benchmark_id,score,max_score,test_date,source_url,verified,notes
a1b2c3d4-e5f6-7890-abcd-ef1234567890,11111111-2222-3333-4444-555555555555,92.5,100.0,2024-03-15,https://openai.com/research/gpt4,true,GPT-4 official benchmark
a1b2c3d4-e5f6-7890-abcd-ef1234567890,22222222-3333-4444-5555-666666666666,78.3,100.0,2024-03-15,,false,Internal testing
```

### 6.4 Uploading CSV Files

**Step-by-Step:**

1. **Prepare CSV File**:
   - Follow format specification above
   - Validate data in spreadsheet software (Excel, Google Sheets)
   - Save as `.csv` format

2. **Upload**:
   - Navigate to **"Benchmarks"** → **"Import CSV"** tab
   - **Option A**: Click **"Choose File"** button → Select CSV file
   - **Option B**: Drag and drop CSV file into the upload zone

3. **Validation**:
   - **File Type**: Must be `.csv` extension
   - **File Size**: Maximum 10MB (approximately 100,000 rows)
   - Client-side validation runs immediately

4. **Upload Process**:
   - Click **"Upload and Import"** button
   - Progress indicator shows: "Processing row X of Y (Z%)"
   - Server processes rows sequentially

5. **Results**:
   - **Success Summary**: "Imported 45 rows successfully"
   - **Partial Success**: "Imported 42 rows, 3 rows failed (see details below)"
   - **Failure Details**: Table showing failed rows with error messages

**API Endpoint**: `POST /api/admin/benchmarks/import-csv`

**Technical Details:**
- **Content-Type**: `multipart/form-data`
- **Streaming**: File processed in chunks (no full load into memory)
- **Transaction**: Currently row-by-row (Task 6: All-or-nothing transaction is planned)
- **Duplicate Handling**: Rows with existing Model+Benchmark combination are skipped

<!-- [Screenshot: CSV Upload interface with drag-and-drop zone] -->

### 6.5 Validation Rules

**Row-Level Validation:**

Each row is validated before import. Errors prevent that row from being imported:

| Validation Rule | Error Message | Resolution |
|----------------|---------------|-----------|
| **model_id not found** | "Model with ID {id} does not exist" | Verify UUID, create model first |
| **benchmark_id not found** | "Benchmark with ID {id} does not exist" | Verify UUID, create benchmark first |
| **score is empty** | "Score is required" | Provide numeric value |
| **score not numeric** | "Score must be a number" | Remove non-numeric characters |
| **max_score < score** | "Max score must be greater than or equal to score" | Fix max_score value |
| **test_date invalid** | "Test date must be in format YYYY-MM-DD" | Use ISO 8601 date format |
| **source_url invalid** | "Source URL is not a valid URL" | Provide full URL with http:// or https:// |
| **Duplicate entry** | "Score already exists for this model and benchmark" | Row skipped, no error (idempotent) |

**File-Level Validation:**

| Validation | Limit | Error Behavior |
|-----------|-------|----------------|
| **File size** | 10MB max | Upload rejected before processing |
| **File type** | `.csv` only | Upload rejected |
| **Header row** | Exact column names | Processing fails, entire import rejected |
| **Malformed CSV** | Unclosed quotes, wrong delimiters | Row-by-row errors |

### 6.6 Troubleshooting CSV Import

#### Problem: "Invalid CSV format"

**Symptoms**: Upload rejected immediately

**Causes**:
- Missing or incorrect header row
- Wrong file encoding (not UTF-8)
- Using semicolon (`;`) instead of comma (`,`)

**Solutions**:
1. Open CSV in text editor (Notepad++, VS Code)
2. Verify first line matches: `model_id,benchmark_id,score,max_score,test_date,source_url,verified,notes`
3. Check for byte-order mark (BOM) - remove if present
4. Re-save as UTF-8 without BOM

---

#### Problem: "File size exceeds 10MB limit"

**Symptoms**: Upload button disabled, file rejected

**Causes**:
- CSV file contains >100,000 rows
- Many long text fields (source_url, notes)

**Solutions**:
1. Split CSV into multiple files (<10MB each)
2. Remove unnecessary columns (notes, source_url for bulk imports)
3. Compress repeated data (deduplicate model_id, benchmark_id)

---

#### Problem: Multiple rows fail with "Model not found"

**Symptoms**: Import summary shows 50+ failed rows with model_id errors

**Causes**:
- Using model names instead of UUIDs
- Copying UUIDs from wrong environment (dev vs prod)
- Models deleted after CSV preparation

**Solutions**:
1. Export current model list: **Models** → **Export** → Copy `id` column
2. Use VLOOKUP in Excel to map model names to correct UUIDs
3. Re-validate all model_id values exist in current database

---

#### Problem: "Duplicate entry" for all rows

**Symptoms**: All rows skipped with "Score already exists" message

**Causes**:
- Re-uploading the same CSV file
- Scores already manually entered

**Solutions**:
1. Check model detail pages to verify existing scores
2. If updating scores: Delete old scores first, then re-import
3. For incremental import: Filter out existing combinations in CSV

---

#### Problem: Import stuck at "Processing row X of Y"

**Symptoms**: Progress bar stops updating, browser unresponsive

**Causes**:
- Network timeout (large file, slow connection)
- Server-side processing error
- Database deadlock

**Solutions**:
1. Wait 2-3 minutes (server may still be processing)
2. Check browser console for errors (F12 → Console tab)
3. Refresh page and verify partial import success in model detail pages
4. Contact administrator with row number where import stalled

---

## 7. Data Quality Dashboard

### 7.1 Accessing the Dashboard

**Navigation**:
- Click **"Dashboard"** in the admin panel sidebar
- Default landing page after login

**Purpose**:
Monitor data freshness, completeness, and platform health at a glance.

<!-- [Screenshot: Admin Dashboard overview] -->

### 7.2 Dashboard Metrics Explained

The dashboard displays 9 key metrics in card format:

#### Freshness Metrics (Story 2.12)

**1. Total Active Models**
- **Description**: Count of all models with `status = 'active'`
- **Purpose**: Indicates platform coverage
- **Good**: Matches expected LLM market size (100-150 models as of 2024)
- **Action Needed**: If count decreases unexpectedly, check for accidental deletions

**2. Models Needing Updates (>7 days)**
- **Description**: Count of active models where `updated_at < 7 days ago`
- **Visual**: Yellow indicator (stale data warning)
- **Purpose**: Identifies models with outdated information
- **Action Needed**: Review and update pricing/capabilities for these models

**3. Critical Updates Needed (>30 days)**
- **Description**: Count of active models where `updated_at < 30 days ago`
- **Visual**: Red indicator (critical staleness)
- **Purpose**: Flags severely outdated data
- **Action Needed**: **High priority** - Update immediately to maintain platform credibility

**4. Recently Updated (last 7 days)**
- **Description**: Count of active models where `updated_at >= 7 days ago`
- **Visual**: Green indicator (fresh data)
- **Purpose**: Shows recent maintenance activity
- **Good**: >20% of total models updated weekly indicates active data management

**5. Pricing Needing Updates (>30 days)**
- **Description**: Count of active models where `pricing_updated_at < 30 days ago`
- **Purpose**: Tracks pricing freshness specifically (critical for cost comparisons)
- **Action Needed**: Review recent provider announcements for price changes

#### Data Quality Metrics (Story 2.13 Task 15)

**6. Incomplete Models (<3 benchmarks)**
- **Description**: Count of active models with fewer than 3 benchmark scores
- **Purpose**: Identifies models lacking sufficient performance data for QAPS calculation
- **Threshold**: Minimum 3 benchmarks required for credible quality scoring
- **Action Needed**: Add benchmark scores for these models (prioritize major benchmarks: MMLU, HumanEval, GSM8K)

**7. Recent Additions (last 7 days)**
- **Description**: Count of models where `created_at >= 7 days ago`
- **Purpose**: Tracks platform growth and new model coverage
- **Good**: Regular additions indicate active platform maintenance

**8. Average Benchmarks per Model**
- **Description**: `AVG(benchmark_scores.count)` across all active models
- **Purpose**: Indicates overall data completeness
- **Good**: ≥5 benchmarks per model (covers all 5 categories)
- **Excellent**: ≥10 benchmarks per model (comprehensive performance profile)

**9. Models by Provider (Chart)**
- **Description**: Bar chart showing model count per provider
- **Purpose**: Visualizes platform coverage across LLM vendors
- **Example**:
  ```
  OpenAI:      25 models
  Anthropic:   12 models
  Google:      18 models
  Meta:         8 models
  Mistral:      7 models
  ```
- **Action Needed**: If major provider is underrepresented, prioritize adding their models

**Cache Duration**: Metrics are cached for **5 minutes** (300 seconds) to reduce database load.

### 7.3 Data Freshness Indicators

Dashboard uses color-coded indicators for visual scanning:

| Indicator | Threshold | Color | Meaning | Action |
|-----------|-----------|-------|---------|--------|
| **Fresh** | Updated <7 days ago | 🟢 Green | Data is current | No action needed |
| **Stale** | Updated 7-30 days ago | 🟡 Yellow | Data aging, review recommended | Schedule update |
| **Critical** | Updated >30 days ago | 🔴 Red | Data outdated, credibility risk | **Update immediately** |

**Timestamp Format**:
- **Absolute**: "2024-10-15 14:30 UTC"
- **Relative**: "5 days ago" (uses `date-fns` library)
- **Tooltip**: Hover for exact timestamp

### 7.4 Taking Action on Metrics

**Workflow for Addressing Stale Data:**

1. **Identify**: Review dashboard metrics
2. **Prioritize**:
   - Critical updates (>30 days) first
   - Incomplete models (<3 benchmarks) second
   - Stale pricing (>30 days) third
3. **Update**:
   - Click metric card to see affected models
   - Batch edit models: **Models** page → Filter by "Last Updated" → Sort oldest first
4. **Verify**: Refresh dashboard after 5 minutes to see updated metrics

**Quick Actions:**

- **Stale Models**: Models page → Sort by "Updated" ascending → Edit top 10
- **Incomplete Models**: Models page → Custom filter: "Benchmarks < 3" → Add scores
- **Provider Coverage**: Dashboard chart → Identify underrepresented provider → Research and add models

---

## 8. Audit Log

> **Note**: Audit logging is planned for implementation in **Task 14** of Story 2.13. This section describes the planned functionality.

### 8.1 What is Logged

**Logged Events** (Planned):
- ✅ Model CRUD operations (Create, Update, Delete)
- ✅ Benchmark CRUD operations
- ✅ Benchmark score additions/updates/deletions
- ✅ CSV imports (file metadata, row counts, errors)
- ✅ Admin login/logout events
- ❌ Read operations (GET requests) - not logged to reduce noise

**Audit Log Entry Structure:**

| Field | Type | Description | Example |
|-------|------|-------------|---------|
| **ID** | UUID | Unique audit event identifier | `abc123...` |
| **Timestamp** | DateTime | When event occurred (UTC) | `2024-10-15T14:30:00Z` |
| **User ID** | String | Admin username who performed action | `admin@example.com` |
| **Action** | Enum | Type of operation | `CREATE`, `UPDATE`, `DELETE` |
| **Entity Type** | String | What was changed | `Model`, `Benchmark`, `BenchmarkScore` |
| **Entity ID** | UUID | Identifier of changed entity | `def456...` |
| **Old Values** | JSON | State before change (for UPDATE) | `{"price": 5.00}` |
| **New Values** | JSON | State after change | `{"price": 6.50}` |

**Example Audit Entry:**

```json
{
  "id": "7f8e9d0c-1b2a-3c4d-5e6f-7a8b9c0d1e2f",
  "timestamp": "2024-10-15T14:30:00Z",
  "userId": "admin@example.com",
  "action": "UPDATE",
  "entityType": "Model",
  "entityId": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
  "oldValues": {
    "inputPricePer1M": 5.00,
    "outputPricePer1M": 15.00
  },
  "newValues": {
    "inputPricePer1M": 6.50,
    "outputPricePer1M": 19.50
  }
}
```

### 8.2 Viewing Audit History

**Planned UI:**

- **Navigation**: Admin panel → **"Audit Log"** sidebar item
- **Table View**: Paginated list of audit events (20 per page)
- **Columns**:
  - Timestamp (relative + tooltip with absolute)
  - User
  - Action (with color-coded badges: CREATE=green, UPDATE=blue, DELETE=red)
  - Entity Type
  - Entity Name (denormalized for quick reference)
  - Details (expandable JSON diff)

<!-- [Placeholder: Screenshot of Audit Log table] -->

### 8.3 Filtering Logs

**Planned Filter Options:**

1. **Date Range**:
   - Presets: Last 24 hours, Last 7 days, Last 30 days, Custom range
   - Date pickers for start/end dates

2. **User**:
   - Dropdown with all admin users
   - Autocomplete search

3. **Action Type**:
   - Checkboxes: CREATE, UPDATE, DELETE
   - Multi-select (can combine)

4. **Entity Type**:
   - Checkboxes: Model, Benchmark, BenchmarkScore
   - Filter to specific entity types

5. **Entity Search**:
   - Search by entity ID or name
   - Example: "GPT-4" → shows all audit events for GPT-4 model

**Example Filter:**
- Date Range: Last 7 days
- User: `admin@example.com`
- Action: UPDATE
- Entity Type: Model

Result: All model updates by `admin@example.com` in the last week.

### 8.4 Exporting Audit Data

**Planned Export Functionality:**

**Formats:**
- CSV (for spreadsheet analysis)
- JSON (for programmatic processing)

**Export Process:**

1. Apply desired filters (date range, user, action, etc.)
2. Click **"Export"** button
3. Select format (CSV or JSON)
4. File downloads: `audit-log-YYYY-MM-DD.csv` or `.json`

**CSV Export Columns:**
- Timestamp, User, Action, Entity Type, Entity ID, Entity Name, Old Values (JSON), New Values (JSON)

**Use Cases:**
- Compliance reporting (quarterly audit trail exports)
- Debugging (find who changed pricing for a specific model)
- Analytics (how many models updated per week?)

---

## 9. Troubleshooting

### 9.1 Common Errors

#### Error: "401 Unauthorized"

**Symptoms**:
- Sudden redirect to login page
- Error message: "Unauthorized"

**Causes**:
- Session expired (24-hour timeout)
- JWT cookie deleted (browser settings, incognito mode)
- Invalid/malformed token

**Solutions**:
1. **Log in again**: Most common resolution
2. **Clear browser cookies**: Settings → Privacy → Clear cookies for this site
3. **Try incognito/private mode**: Tests for browser extension conflicts
4. **Check system time**: JWT validation requires accurate clocks (±5 min tolerance)

---

#### Error: "400 Bad Request - Validation Error"

**Symptoms**:
- Form submission fails
- Red error message below form fields

**Causes**:
- Required field left empty
- Invalid data format (e.g., text in numeric field)
- FluentValidation rule violation

**Solutions**:
1. **Check required fields**: Look for red asterisks (*)
2. **Validate data types**:
   - Prices: Positive decimal numbers only
   - Dates: Use date picker or ISO 8601 format (YYYY-MM-DD)
   - UUIDs: Valid GUID format (8-4-4-4-12 hex digits)
3. **Read inline error messages**: Specific guidance appears below each field
4. **Check browser console**: F12 → Console tab for detailed validation errors

---

#### Error: "409 Conflict - Duplicate Model"

**Symptoms**:
- Model creation fails
- Error: "Model with this name and provider already exists"

**Causes**:
- Name + Provider combination must be unique (e.g., "GPT-4" + "OpenAI")

**Solutions**:
1. **Search for existing model**: Models page → Search bar → Enter model name
2. **Differentiate with version**: Add version to name (e.g., "GPT-4 (June 2024)")
3. **Edit existing model**: If duplicate, update existing record instead

---

#### Error: "404 Not Found"

**Symptoms**:
- "Model not found" or "Benchmark not found"

**Causes**:
- Entity deleted by another admin
- Incorrect UUID in URL
- Soft-deleted entity (inactive)

**Solutions**:
1. **Verify ID**: Copy UUID from list page, not from old bookmarks
2. **Check "Include Inactive" filter**: May be soft-deleted
3. **Breadcrumb navigation**: Use admin panel navigation instead of direct URLs

---

#### Error: "500 Internal Server Error"

**Symptoms**:
- Generic error message: "An unexpected error occurred"
- Technical error code in browser console

**Causes**:
- Backend service failure
- Database connection issue
- Unhandled exception

**Solutions**:
1. **Refresh page**: Transient errors often resolve
2. **Try again in 5 minutes**: May be temporary backend issue
3. **Check admin dashboard status**: Look for system health indicators
4. **Contact administrator**: Provide:
   - Exact error message
   - What you were doing when error occurred
   - Browser console log (F12 → Console → Screenshot)

---

### 9.2 Authentication Issues

#### Problem: "Can't log in despite correct credentials"

**Diagnostic Steps**:

1. **Verify username/password**:
   - Case-sensitive
   - No extra spaces
   - Use password manager to rule out typos

2. **Check browser console**:
   - F12 → Console tab
   - Look for CORS errors: `Access to fetch at 'https://...' from origin '...' has been blocked`
   - If CORS error: Administrator must configure `CORS_ALLOWED_ORIGINS` environment variable

3. **Test in different browser**:
   - Rules out browser-specific issues (extensions, cache)

4. **Verify account status**:
   - Contact administrator to confirm account is active
   - Admin accounts may have expiration dates

---

#### Problem: "Session expires too quickly"

**Symptoms**:
- Logged out after <1 hour of activity

**Causes**:
- Browser blocking third-party cookies
- Aggressive privacy settings (Safari, Firefox with Enhanced Tracking Protection)

**Solutions**:
1. **Whitelist admin domain**: Browser settings → Cookies → Allow for `[admin-domain]`
2. **Disable tracking protection for this site**: Browser extension settings
3. **Use Chrome/Edge**: Recommended browsers for admin panel

---

### 9.3 Validation Errors

#### Problem: "Price validation fails with negative number error"

**Symptoms**:
- Error: "Input price must be positive"
- You entered a positive number

**Causes**:
- Locale-specific decimal separator (comma vs period)
- Currency symbol included ($, €)

**Solutions**:
1. **Use period (.) as decimal separator**: `2.50` not `2,50`
2. **Remove currency symbols**: `10.00` not `$10.00`
3. **Use numeric keypad**: Ensures proper number format

---

#### Problem: "Duplicate benchmark score" but score doesn't exist

**Symptoms**:
- Error: "Score already exists for this model and benchmark"
- You checked model detail page - score not visible

**Causes**:
- Soft-deleted score still in database (unique constraint violation)
- Concurrent admin created score simultaneously

**Solutions**:
1. **Refresh model page**: May be cache issue
2. **Expand "Include Inactive"**: Score may be soft-deleted
3. **Wait 5 minutes and retry**: Cache invalidation delay

---

### 9.4 Import Failures

#### Problem: "CSV import completes but zero rows imported"

**Symptoms**:
- Upload success message
- Import summary: "0 rows imported, 50 rows failed"

**Causes**:
- All model_id or benchmark_id values invalid
- Header row missing or incorrect

**Solutions**:
1. **Download error report**: Click "Download Error Log" button
2. **Verify header row**: Must match template exactly (case-sensitive)
3. **Validate UUIDs**: Export model/benchmark lists, cross-reference

---

#### Problem: "File upload button disabled"

**Symptoms**:
- "Upload and Import" button grayed out
- No file selected

**Causes**:
- File too large (>10MB)
- File not `.csv` extension

**Solutions**:
1. **Check file size**: Right-click file → Properties → Size
2. **Rename extension**: Change `.txt` or `.xlsx` to `.csv`
3. **Split file**: If >10MB, divide into multiple CSVs

---

### 9.5 Performance Issues

#### Problem: "Admin panel loads slowly"

**Symptoms**:
- Page takes >5 seconds to load
- Table pagination slow

**Causes**:
- Large dataset (1000+ models)
- Slow network connection
- Cache disabled

**Solutions**:
1. **Enable pagination**: Use page size of 20 (default) not 100
2. **Clear browser cache**: F12 → Application tab → Clear storage
3. **Check network speed**: Run speed test, contact IT if <1 Mbps
4. **Reduce filters**: Too many simultaneous filters can slow queries

---

#### Problem: "Dashboard metrics take long to load"

**Symptoms**:
- Dashboard shows "Loading..." for >10 seconds
- Metrics eventually appear

**Causes**:
- Cache expired (5-minute TTL)
- Database query performance (100,000+ benchmark scores)

**Solutions**:
1. **Wait for initial load**: Subsequent visits will be cached (5 min)
2. **Avoid refreshing frequently**: Let cache TTL expire naturally
3. **Contact administrator**: May need database indexing optimization

---

## 10. API Reference

For developers integrating with the admin panel programmatically, the following endpoints are available:

**Base URL**: `https://yourdomain.com/api/admin`

**Authentication**: All endpoints require JWT token in `admin_token` HttpOnly cookie

### Authentication Endpoints

| Method | Endpoint | Description | Request Body | Response |
|--------|----------|-------------|--------------|----------|
| `POST` | `/auth/login` | Admin login | `{"username": string, "password": string}` | `{"success": bool, "message": string}` + Cookie |
| `POST` | `/auth/logout` | Admin logout | None | `{"success": true, "message": "Logout successful"}` |

### Model Management Endpoints

| Method | Endpoint | Description | Query Params | Response |
|--------|----------|-------------|--------------|----------|
| `GET` | `/models` | List all models (incl. inactive) | `page`, `pageSize`, `searchTerm`, `provider`, `status` | `AdminApiResponse<List<AdminModelDto>>` or `AdminApiResponse<PagedResult<AdminModelDto>>` |
| `GET` | `/models/{id}` | Get model by ID | None | `AdminApiResponse<AdminModelDto>` |
| `POST` | `/models` | Create new model | `CreateModelRequest` | `AdminApiResponse<AdminModelDto>` (201 Created) |
| `PUT` | `/models/{id}` | Update model | `CreateModelRequest` | `AdminApiResponse<AdminModelDto>` |
| `DELETE` | `/models/{id}` | Soft delete model | None | 204 No Content |

### Benchmark Management Endpoints

| Method | Endpoint | Description | Query Params | Response |
|--------|----------|-------------|--------------|----------|
| `GET` | `/benchmarks` | List all benchmarks | `includeInactive`, `category` | `List<BenchmarkResponseDto>` |
| `GET` | `/benchmarks/{id}` | Get benchmark by ID | None | `BenchmarkResponseDto` |
| `POST` | `/benchmarks` | Create new benchmark | `CreateBenchmarkRequest` | `BenchmarkResponseDto` (201 Created) |
| `PUT` | `/benchmarks/{id}` | Update benchmark | `UpdateBenchmarkRequest` | `BenchmarkResponseDto` |
| `DELETE` | `/benchmarks/{id}` | Soft delete benchmark | None | 204 No Content |
| `POST` | `/benchmarks/import-csv` | Bulk import scores | CSV file (multipart/form-data) | `CSVImportResultDto` |

### Benchmark Score Endpoints

| Method | Endpoint | Description | Request Body | Response |
|--------|----------|-------------|--------------|----------|
| `POST` | `/models/{modelId}/benchmarks` | Add benchmark score | `CreateBenchmarkScoreDto` | `AdminApiResponse<BenchmarkScoreResponseDto>` (201) |
| `GET` | `/models/{modelId}/benchmarks` | Get all scores for model | None | `AdminApiResponse<List<BenchmarkScoreResponseDto>>` |
| `PUT` | `/models/{modelId}/benchmarks/{scoreId}` | Update score | `CreateBenchmarkScoreDto` | `AdminApiResponse<BenchmarkScoreResponseDto>` |
| `DELETE` | `/models/{modelId}/benchmarks/{scoreId}` | Delete score | None | 204 No Content |

### Dashboard Endpoints

| Method | Endpoint | Description | Cache Duration | Response |
|--------|----------|-------------|----------------|----------|
| `GET` | `/dashboard/metrics` | Get dashboard metrics | 5 minutes | `{data: DashboardMetricsDto}` |

**Common Response Codes**:
- `200 OK`: Success
- `201 Created`: Resource created successfully
- `204 No Content`: Successful deletion
- `400 Bad Request`: Validation error
- `401 Unauthorized`: Authentication required or expired
- `404 Not Found`: Resource not found
- `409 Conflict`: Duplicate resource (unique constraint violation)
- `500 Internal Server Error`: Unexpected server error

**Example API Call (cURL)**:

```bash
# Login
curl -X POST https://yourdomain.com/api/admin/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"admin","password":"securepass"}' \
  -c cookies.txt

# Get models (uses cookie from login)
curl -X GET https://yourdomain.com/api/admin/models?page=1&pageSize=20 \
  -b cookies.txt
```

---

## 11. Support & Feedback

### Getting Help

**Technical Support**:
- **Email**: support@llmpricing.com
- **Response Time**: 24-48 hours (business days)
- **Include in Request**:
  - Screenshot of error
  - Browser console log (F12 → Console)
  - Steps to reproduce issue

**Bug Reports**:
- **GitHub Issues**: [https://github.com/your-org/llm-token-price/issues](https://github.com/your-org/llm-token-price/issues)
- **Include**:
  - Expected behavior
  - Actual behavior
  - Environment (browser version, OS)

**Feature Requests**:
- **GitHub Discussions**: [https://github.com/your-org/llm-token-price/discussions](https://github.com/your-org/llm-token-price/discussions)
- **Upvote existing requests**: Check if someone else already suggested it

### Documentation Feedback

Help us improve this guide:
- **Typos/Errors**: Submit PR to fix documentation directly
- **Missing Information**: Open GitHub issue with "documentation" label
- **Clarity Improvements**: Email support@llmpricing.com with suggestions

### Changelog

**Version 1.0 (2025-10-21)**:
- Initial admin panel documentation
- Covers: Authentication, Model Management, Benchmark Management, CSV Import, Dashboard, Audit Log (planned), Troubleshooting
- Screenshots placeholders (to be added in Task 16.8)

---

**End of Admin Panel User Guide**

*This documentation is auto-generated and maintained by the development team. Last updated: 2025-10-21*
