/**
 * Unit tests for BenchmarkNormalizer domain service
 * Tests score normalization logic, edge cases, and range validation
 * Story 2.10 - Benchmark Score Entry Form
 */

using FluentAssertions;
using LlmTokenPrice.Domain.Services;
using Xunit;

namespace LlmTokenPrice.Domain.Tests.Services
{
    public class BenchmarkNormalizerTests
    {
        private readonly BenchmarkNormalizer _sut;

        public BenchmarkNormalizerTests()
        {
            _sut = new BenchmarkNormalizer();
        }

        public class Normalize : BenchmarkNormalizerTests
        {
            [Theory]
            [InlineData(50, 0, 100, 0.5)] // Mid-range
            [InlineData(0, 0, 100, 0.0)]  // Minimum
            [InlineData(100, 0, 100, 1.0)] // Maximum
            [InlineData(75, 0, 100, 0.75)] // Upper quarter
            [InlineData(25, 0, 100, 0.25)] // Lower quarter
            public void ShouldNormalizeScoreCorrectly_WhenWithinRange(
                decimal score,
                decimal min,
                decimal max,
                decimal expected)
            {
                // Act
                var result = _sut.Normalize(score, min, max);

                // Assert
                result.Should().Be(expected);
            }

            [Theory]
            [InlineData(87.5, 0, 100, 0.875)] // MMLU typical score
            [InlineData(0.85, 0, 1, 0.85)]    // Normalized benchmark (0-1 scale)
            [InlineData(650, 200, 800, 0.75)] // SAT-style (200-800 range)
            public void ShouldNormalizeRealWorldBenchmarkScores(
                decimal score,
                decimal min,
                decimal max,
                decimal expected)
            {
                // Act
                var result = _sut.Normalize(score, min, max);

                // Assert
                result.Should().Be(expected);
            }

            [Fact]
            public void ShouldReturnOne_WhenMinEqualsMax()
            {
                // Arrange
                var score = 100m;
                var min = 100m;
                var max = 100m;

                // Act
                var result = _sut.Normalize(score, min, max);

                // Assert
                result.Should().Be(1.0m);
            }

            [Theory]
            [InlineData(150, 0, 100, 1.0)] // Score above max
            [InlineData(-50, 0, 100, 0.0)] // Score below min
            [InlineData(200, 0, 100, 1.0)] // Far above max
            [InlineData(-100, 0, 100, 0.0)] // Far below min
            public void ShouldClampToZeroOne_WhenScoreOutOfRange(
                decimal score,
                decimal min,
                decimal max,
                decimal expected)
            {
                // Act
                var result = _sut.Normalize(score, min, max);

                // Assert
                result.Should().Be(expected);
            }

            [Theory]
            [InlineData(0.123456789, 0, 1)]
            [InlineData(87.654321, 0, 100)]
            public void ShouldPreserveDecimalPrecision(decimal score, decimal min, decimal max)
            {
                // Act
                var result = _sut.Normalize(score, min, max);

                // Assert
                result.Should().BeOfType(typeof(decimal));
                // Verify precision is maintained (not rounded prematurely)
                var denormalized = (result * (max - min)) + min;
                denormalized.Should().BeApproximately(score, 0.000001m);
            }

            [Fact]
            public void ShouldHandleVerySmallRanges()
            {
                // Arrange
                var score = 0.0005m;
                var min = 0.0001m;
                var max = 0.001m;

                // Act
                var result = _sut.Normalize(score, min, max);

                // Assert
                result.Should().BeApproximately(0.4444m, 0.0001m);
            }

            [Fact]
            public void ShouldHandleVeryLargeRanges()
            {
                // Arrange
                var score = 500000m;
                var min = 0m;
                var max = 1000000m;

                // Act
                var result = _sut.Normalize(score, min, max);

                // Assert
                result.Should().Be(0.5m);
            }

            [Theory]
            [InlineData(50, 100, 0)] // Inverted range (max < min)
            public void ShouldHandleInvertedRanges(decimal score, decimal min, decimal max)
            {
                // Act
                var result = _sut.Normalize(score, min, max);

                // Assert
                // Should still clamp to [0, 1]
                result.Should().BeInRange(0m, 1m);
            }
        }

        public class IsWithinTypicalRange : BenchmarkNormalizerTests
        {
            [Theory]
            [InlineData(50, 0, 100, true)]  // Mid-range
            [InlineData(0, 0, 100, true)]   // At minimum
            [InlineData(100, 0, 100, true)] // At maximum
            [InlineData(75, 0, 100, true)]  // Within range
            public void ShouldReturnTrue_WhenScoreWithinRange(
                decimal score,
                decimal min,
                decimal max,
                bool expected)
            {
                // Act
                var result = _sut.IsWithinTypicalRange(score, min, max);

                // Assert
                result.Should().Be(expected);
            }

            [Theory]
            [InlineData(150, 0, 100, false)] // Above maximum
            [InlineData(-50, 0, 100, false)] // Below minimum
            [InlineData(100.1, 0, 100, false)] // Slightly above
            [InlineData(-0.1, 0, 100, false)] // Slightly below
            public void ShouldReturnFalse_WhenScoreOutOfRange(
                decimal score,
                decimal min,
                decimal max,
                bool expected)
            {
                // Act
                var result = _sut.IsWithinTypicalRange(score, min, max);

                // Assert
                result.Should().Be(expected);
            }

            [Fact]
            public void ShouldReturnTrue_WhenMinEqualsMax()
            {
                // Arrange
                var score = 100m;
                var min = 100m;
                var max = 100m;

                // Act
                var result = _sut.IsWithinTypicalRange(score, min, max);

                // Assert
                result.Should().BeTrue();
            }

            [Theory]
            [InlineData(87.5, 60, 95, true)]  // Typical MMLU score
            [InlineData(45.2, 60, 95, false)] // Below typical MMLU
            [InlineData(98.7, 60, 95, false)] // Above typical MMLU
            public void ShouldValidateRealWorldBenchmarkRanges(
                decimal score,
                decimal min,
                decimal max,
                bool expected)
            {
                // Act
                var result = _sut.IsWithinTypicalRange(score, min, max);

                // Assert
                result.Should().Be(expected);
            }
        }
    }
}
