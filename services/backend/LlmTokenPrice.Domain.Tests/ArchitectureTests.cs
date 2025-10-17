using ArchUnitNET.Domain;
using ArchUnitNET.Fluent;
using ArchUnitNET.Loader;
using ArchUnitNET.xUnit;
using FluentAssertions;
using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace LlmTokenPrice.Domain.Tests;

/// <summary>
/// Tests that enforce hexagonal architecture boundaries.
/// Ensures Domain layer has zero dependencies on Infrastructure, Application, or API layers.
/// </summary>
public class ArchitectureTests
{
    private static readonly Architecture Architecture = new ArchLoader()
        .LoadAssemblies(
            typeof(LlmTokenPrice.Domain.Entities.Model).Assembly,
            typeof(LlmTokenPrice.Application.Services.ModelQueryService).Assembly,
            typeof(LlmTokenPrice.Infrastructure.Data.AppDbContext).Assembly,
            System.Reflection.Assembly.Load("LlmTokenPrice.API")
        )
        .Build();

    /// <summary>
    /// AC#4, AC#11: Validates Domain layer has zero dependencies on Infrastructure layer.
    /// This is the cornerstone of hexagonal architecture - domain logic must be pure.
    /// </summary>
    [Fact]
    public void DomainLayer_Should_Not_Depend_On_Infrastructure()
    {
        // Arrange
        var domainLayer = Types()
            .That()
            .ResideInNamespace("LlmTokenPrice.Domain", useRegularExpressions: true)
            .As("Domain Layer");

        var infrastructureLayer = Types()
            .That()
            .ResideInNamespace("LlmTokenPrice.Infrastructure", useRegularExpressions: true)
            .As("Infrastructure Layer");

        // Act & Assert
        var rule = domainLayer
            .Should()
            .NotDependOnAny(infrastructureLayer)
            .Because("Domain layer must have zero infrastructure dependencies (Hexagonal Architecture principle)");

        rule.Check(Architecture);
    }

    /// <summary>
    /// AC#4, AC#11: Validates Domain layer has zero dependencies on Application layer.
    /// Domain services should not know about use case orchestration.
    /// </summary>
    [Fact]
    public void DomainLayer_Should_Not_Depend_On_Application()
    {
        // Arrange
        var domainLayer = Types()
            .That()
            .ResideInNamespace("LlmTokenPrice.Domain", useRegularExpressions: true)
            .As("Domain Layer");

        var applicationLayer = Types()
            .That()
            .ResideInNamespace("LlmTokenPrice.Application", useRegularExpressions: true)
            .As("Application Layer");

        // Act & Assert
        var rule = domainLayer
            .Should()
            .NotDependOnAny(applicationLayer)
            .Because("Domain layer must not depend on Application layer (inverted dependency)");

        rule.Check(Architecture);
    }

    /// <summary>
    /// AC#4, AC#11: Validates Domain layer has zero dependencies on API/Presentation layer.
    /// Pure business logic should never depend on HTTP concerns.
    /// </summary>
    [Fact]
    public void DomainLayer_Should_Not_Depend_On_API()
    {
        // Arrange
        var domainLayer = Types()
            .That()
            .ResideInNamespace("LlmTokenPrice.Domain", useRegularExpressions: true)
            .As("Domain Layer");

        var apiLayer = Types()
            .That()
            .ResideInNamespace("LlmTokenPrice.API", useRegularExpressions: true)
            .As("API Layer");

        // Act & Assert
        var rule = domainLayer
            .Should()
            .NotDependOnAny(apiLayer)
            .Because("Domain layer must not depend on presentation/API concerns");

        rule.Check(Architecture);
    }

    /// <summary>
    /// AC#4, AC#11: Validates repository interfaces (ports) are defined in Domain layer.
    /// This is crucial for dependency inversion - abstractions belong in domain.
    /// </summary>
    [Fact]
    public void RepositoryInterfaces_Should_Be_In_DomainLayer()
    {
        // Arrange & Act
        var repositoryInterfaces = Architecture.Types
            .Where(t => t.Name.StartsWith("I") && t.Name.EndsWith("Repository"))
            .ToList();

        // Assert
        repositoryInterfaces.Should().NotBeEmpty("Repository interfaces should exist");

        var nonDomainRepositories = repositoryInterfaces
            .Where(t => !t.FullName.StartsWith("LlmTokenPrice.Domain"))
            .ToList();

        nonDomainRepositories.Should().BeEmpty(
            $"All repository interfaces should be in Domain layer. Found in other layers: {string.Join(", ", nonDomainRepositories.Select(t => t.FullName))}");
    }

    /// <summary>
    /// AC#4, AC#11: Validates concrete repository implementations are in Infrastructure layer.
    /// Adapters implementing ports should live in the infrastructure layer.
    /// </summary>
    [Fact]
    public void ConcreteRepositories_Should_Be_In_InfrastructureLayer()
    {
        // Arrange & Act
        var concreteRepositories = Architecture.Types
            .Where(t => t.Name.EndsWith("Repository") &&
                        !t.Name.StartsWith("I"))
            .ToList();

        // Assert
        concreteRepositories.Should().NotBeEmpty("Concrete repository implementations should exist");

        var nonInfrastructureRepositories = concreteRepositories
            .Where(t => !t.FullName.StartsWith("LlmTokenPrice.Infrastructure"))
            .ToList();

        nonInfrastructureRepositories.Should().BeEmpty(
            $"All concrete repositories should be in Infrastructure layer. Found in other layers: {string.Join(", ", nonInfrastructureRepositories.Select(t => t.FullName))}");
    }

    /// <summary>
    /// AC#4, AC#11: Validates Domain layer only references System namespaces and its own types.
    /// Domain should be pure C# with no framework dependencies (except System.*).
    /// </summary>
    [Fact]
    public void DomainLayer_Should_Only_Reference_System_Namespaces()
    {
        // Arrange
        var domainTypes = Architecture.Types
            .Where(t => t.FullName.StartsWith("LlmTokenPrice.Domain"))
            .ToList();

        var allowedNamespaces = new[]
        {
            "System",
            "LlmTokenPrice.Domain"
        };

        // Act
        var violations = new List<string>();

        foreach (var type in domainTypes)
        {
            var dependencies = type.Dependencies
                .Where(dep => !allowedNamespaces.Any(ns => dep.Target.FullName.StartsWith(ns)))
                .ToList();

            if (dependencies.Any())
            {
                violations.AddRange(dependencies.Select(dep =>
                    $"{type.FullName} depends on {dep.Target.FullName}"));
            }
        }

        // Assert
        violations.Should().BeEmpty(
            $"Domain layer should only reference System.* or LlmTokenPrice.Domain.* namespaces. Violations:\n{string.Join("\n", violations)}");
    }

    /// <summary>
    /// AC#4, AC#11: Validates Application layer can depend on Domain but not Infrastructure.
    /// Application orchestrates use cases but shouldn't know about adapters.
    /// </summary>
    [Fact]
    public void ApplicationLayer_Should_Not_Depend_On_Infrastructure()
    {
        // Arrange
        var applicationLayer = Types()
            .That()
            .ResideInNamespace("LlmTokenPrice.Application", useRegularExpressions: true)
            .As("Application Layer");

        var infrastructureLayer = Types()
            .That()
            .ResideInNamespace("LlmTokenPrice.Infrastructure", useRegularExpressions: true)
            .As("Infrastructure Layer");

        // Act & Assert
        var rule = applicationLayer
            .Should()
            .NotDependOnAny(infrastructureLayer)
            .Because("Application layer should depend on Domain ports, not Infrastructure adapters");

        rule.Check(Architecture);
    }

    /// <summary>
    /// AC#4: Validates that all domain entities are pure POCOs without EF Core attributes.
    /// Entity Framework concerns belong in Infrastructure configuration, not domain.
    /// </summary>
    [Fact]
    public void DomainEntities_Should_Not_Have_EFCore_Attributes()
    {
        // Arrange & Act
        var entitiesWithEFAttributes = Architecture.Types
            .Where(t => t.FullName.StartsWith("LlmTokenPrice.Domain.Entities"))
            .Where(t => t.Attributes.Any(a =>
                a.FullName.Contains("System.ComponentModel.DataAnnotations") ||
                a.FullName.Contains("Microsoft.EntityFrameworkCore")))
            .ToList();

        // Assert
        entitiesWithEFAttributes.Should().BeEmpty(
            "Domain entities should be pure POCOs without EF Core or validation attributes. " +
            "Use Fluent API configuration in Infrastructure layer instead. " +
            $"Entities with attributes: {string.Join(", ", entitiesWithEFAttributes.Select(e => e.Name))}");
    }
}
