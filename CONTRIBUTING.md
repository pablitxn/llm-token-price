# Contributing to LLM Token Price Comparison Platform

Thank you for your interest in contributing to the LLM Token Price Comparison Platform! This document provides guidelines and instructions for contributing to this project.

## Table of Contents

- [Code of Conduct](#code-of-conduct)
- [Getting Started](#getting-started)
- [Development Workflow](#development-workflow)
- [Branching Strategy](#branching-strategy)
- [Commit Message Conventions](#commit-message-conventions)
- [Pull Request Process](#pull-request-process)
- [Code Review Expectations](#code-review-expectations)
- [Testing Requirements](#testing-requirements)
- [Code Style Guidelines](#code-style-guidelines)

## Code of Conduct

We are committed to providing a welcoming and inclusive environment for all contributors. Please be respectful and constructive in all interactions.

## Getting Started

1. **Fork the repository** and clone your fork:
   ```bash
   git clone https://github.com/YOUR_USERNAME/llm-token-price.git
   cd llm-token-price
   ```

2. **Set up your development environment** by following the instructions in [README.md](./README.md)

3. **Create a new branch** for your feature or fix (see [Branching Strategy](#branching-strategy))

## Development Workflow

### Prerequisites

Ensure you have the required tools installed:
- Node.js 20+
- .NET 9 SDK
- PostgreSQL 16
- Redis 7.2
- pnpm package manager
- Git

### Local Development

1. Start the backend API:
   ```bash
   cd services/backend
   dotnet run --project LlmTokenPrice.API
   ```

2. Start the frontend dev server (in a separate terminal):
   ```bash
   cd apps/web
   pnpm run dev
   ```

3. Verify your changes work correctly in both environments

## Branching Strategy

We follow a **feature branch workflow** with the following conventions:

### Branch Naming

- **Feature branches**: `feature/epic-N-short-description`
  - Example: `feature/epic-3-add-model-comparison-table`

- **Bug fixes**: `fix/short-description`
  - Example: `fix/redis-connection-nullability`

- **Documentation**: `docs/short-description`
  - Example: `docs/update-contributing-guide`

- **Refactoring**: `refactor/short-description`
  - Example: `refactor/extract-cache-service`

- **Tests**: `test/short-description`
  - Example: `test/add-unit-tests-for-qaps-calculator`

### Branch Lifecycle

1. **Create branch** from `main`:
   ```bash
   git checkout main
   git pull origin main
   git checkout -b feature/epic-3-my-feature
   ```

2. **Make changes** and commit frequently with clear messages

3. **Keep branch up to date** with main:
   ```bash
   git fetch origin
   git rebase origin/main
   ```

4. **Push to your fork**:
   ```bash
   git push origin feature/epic-3-my-feature
   ```

5. **Open a Pull Request** when ready for review

## Commit Message Conventions

We follow **Conventional Commits** specification to maintain a clean and readable git history.

### Format

```
<type>(<scope>): <subject>

[optional body]

[optional footer]
```

### Types

- **feat**: A new feature
- **fix**: A bug fix
- **docs**: Documentation changes only
- **style**: Code style changes (formatting, missing semicolons, etc.)
- **refactor**: Code changes that neither fix a bug nor add a feature
- **test**: Adding or updating tests
- **chore**: Changes to build process, tooling, or dependencies

### Scopes (Optional)

- **epic-N**: Changes related to a specific epic (e.g., `epic-1`, `epic-3`)
- **backend**: Backend-specific changes
- **frontend**: Frontend-specific changes
- **infra**: Infrastructure or DevOps changes
- **docs**: Documentation changes

### Examples

```bash
# Feature addition
feat(epic-3): add TanStack Table for model comparison

# Bug fix
fix(backend): resolve Redis connection nullability warning

# Documentation
docs(contributing): add branching strategy guidelines

# Test addition
test(calculator): add unit tests for cost estimation service

# Refactoring
refactor(api): extract health check logic into separate service
```

### Commit Message Best Practices

- **Use imperative mood** in the subject line ("add" not "added" or "adds")
- **Keep subject line under 72 characters**
- **Capitalize the subject line**
- **Do not end subject line with a period**
- **Separate subject from body with a blank line**
- **Wrap body at 72 characters**
- **Explain what and why, not how** (the code shows "how")

## Pull Request Process

### Before Opening a PR

1. **Ensure all tests pass**:
   ```bash
   # Backend tests
   cd services/backend && dotnet test

   # Frontend tests
   cd apps/web && pnpm run test
   ```

2. **Run type checking** (frontend):
   ```bash
   cd apps/web && pnpm run type-check
   ```

3. **Run linting**:
   ```bash
   cd apps/web && pnpm run lint
   ```

4. **Verify builds succeed**:
   ```bash
   # Backend
   cd services/backend && dotnet build --configuration Release

   # Frontend
   cd apps/web && pnpm run build
   ```

5. **Update documentation** if you changed APIs or added features

### PR Template

When opening a PR, include the following information:

```markdown
## Description

Brief description of changes and why they were made.

## Related Issue/Epic

Closes #123
Implements Epic 3 Story 3.1

## Type of Change

- [ ] Bug fix (non-breaking change which fixes an issue)
- [ ] New feature (non-breaking change which adds functionality)
- [ ] Breaking change (fix or feature that would cause existing functionality to not work as expected)
- [ ] Documentation update

## Testing

Describe the tests you ran and how to reproduce them:

- [ ] Unit tests added/updated
- [ ] Integration tests added/updated
- [ ] E2E tests added/updated
- [ ] Manual testing performed

## Checklist

- [ ] My code follows the code style of this project
- [ ] I have performed a self-review of my code
- [ ] I have commented my code, particularly in hard-to-understand areas
- [ ] I have made corresponding changes to the documentation
- [ ] My changes generate no new warnings
- [ ] I have added tests that prove my fix is effective or that my feature works
- [ ] New and existing unit tests pass locally with my changes
- [ ] Any dependent changes have been merged and published

## Screenshots (if applicable)

Add screenshots to help explain your changes
```

### PR Review Timeline

- **Initial review**: Within 2 business days
- **Follow-up reviews**: Within 1 business day
- **Merge**: After approval from at least one maintainer

## Code Review Expectations

### As a Contributor

- **Respond to feedback** within 2 business days
- **Be open to suggestions** and willing to make changes
- **Ask questions** if review feedback is unclear
- **Keep PRs focused** on a single feature or fix
- **Update your PR** based on review comments

### As a Reviewer

- **Be constructive and respectful** in feedback
- **Explain the "why"** behind requested changes
- **Approve PRs** that meet quality standards, even if minor improvements could be made
- **Request changes** only when necessary for correctness, security, or maintainability
- **Review within 2 business days** of PR submission

### Review Criteria

Reviewers will check for:

1. **Correctness**: Does the code do what it's supposed to do?
2. **Tests**: Are there appropriate tests with good coverage?
3. **Design**: Is the code well-designed and fit the architecture?
4. **Functionality**: Does the code behave as intended from a user perspective?
5. **Complexity**: Is the code more complex than necessary?
6. **Naming**: Are names clear and follow conventions?
7. **Comments**: Are comments clear and useful?
8. **Documentation**: Have docs been updated if needed?
9. **Security**: Are there potential security issues?
10. **Performance**: Are there obvious performance issues?

## Testing Requirements

### Backend Testing

- **Unit Tests** (xUnit + FluentAssertions):
  - Domain layer: 90%+ coverage
  - Application layer: 80%+ coverage
  - Test files: `*.Tests.cs` in corresponding test project

- **Integration Tests**:
  - Repository layer: Database interactions
  - API controllers: Full request/response cycle
  - Use TestContainers for PostgreSQL and Redis

### Frontend Testing

- **Unit Tests** (Vitest + Testing Library):
  - Utility functions: 90%+ coverage
  - Custom hooks: 80%+ coverage
  - Test files: `*.test.ts` or `*.test.tsx`

- **Component Tests**:
  - Critical UI components
  - Form validation
  - State management (Zustand stores)

- **E2E Tests** (Playwright):
  - Critical user journeys
  - Cross-browser compatibility
  - Test files: `*.spec.ts` in `e2e/` directory

### Test Naming Conventions

```typescript
// Frontend (Vitest)
describe('CostCalculator', () => {
  it('should calculate monthly cost correctly', () => {
    // ...
  });
});

// Backend (xUnit)
[Fact]
public void CalculateQAPS_WithValidScores_ReturnsCorrectValue()
{
    // Arrange
    // Act
    // Assert
}
```

## Code Style Guidelines

### Backend (C# / .NET)

- **Follow Microsoft C# Coding Conventions**: https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions
- **Use PascalCase** for public members, camelCase for private fields
- **Use async/await** for all I/O operations
- **Enable nullable reference types** (`<Nullable>enable</Nullable>`)
- **Use meaningful names** over comments when possible
- **Keep methods short** (< 20 lines when possible)
- **One class per file**, matching filename to class name

### Frontend (TypeScript / React)

- **Follow Airbnb JavaScript Style Guide**: https://github.com/airbnb/javascript
- **Use TypeScript strict mode** (no `any` types)
- **Use functional components** with hooks
- **Use PascalCase** for components, camelCase for functions/variables
- **Export components as default**, utilities as named exports
- **Keep components small** (< 200 lines)
- **Use meaningful prop names** with TypeScript interfaces

### EditorConfig

The `.editorconfig` file enforces consistent formatting:
- **Indentation**: 4 spaces (C#), 2 spaces (TS/JS)
- **Line endings**: LF (Unix-style)
- **Charset**: UTF-8
- **Trim trailing whitespace**: Yes

## Questions?

If you have questions about contributing, please:

1. Check the [README.md](./README.md) for project documentation
2. Review existing [issues](https://github.com/pablitxn/llm-token-price/issues)
3. Open a new issue with the `question` label

## License

By contributing to this project, you agree that your contributions will be licensed under the same license as the project (see [LICENSE](./LICENSE) file).

---

Thank you for contributing! 🎉
