using System;
using Shouldly;
using Snitch.Analysis;
using Xunit;

namespace Sntich.Tests
{
    public sealed class PackageTests
    {
        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void Should_Not_Throw_For_A_Package_Without_A_Version(string version)
        {
            // Given, When
            var package = new Package("Autofac", version, null);

            // Then
            package.Version.ShouldBeNull();
            package.Range.ShouldBeNull();
            package.GetVersionString().ShouldBe("?");
        }

        [Fact]
        public void Should_Consider_Two_Packages_Without_A_Version_To_Be_The_Same_Version()
        {
            // Given
            var first = new Package("Autofac", null, null);
            var second = new Package("Autofac", null, null);

            // When, Then
            first.IsSameVersion(second).ShouldBeTrue();
        }

        [Fact]
        public void Should_Parse_An_Exact_Version_Range()
        {
            // Given, When
            var package = new Package("Autofac", "[3.5.4]", null);

            // Then
            package.Version.ShouldBeNull();
            package.Range.ShouldNotBeNull();
            package.GetVersionString().ShouldBe("[3.5.4]");
        }

        [Fact]
        public void Should_Consider_Two_Identical_Version_Ranges_To_Be_The_Same_Version()
        {
            // Given
            var first = new Package("Autofac", "[3.5.4]", null);
            var second = new Package("Autofac", "[3.5.4]", null);

            // When, Then
            first.IsSameVersion(second).ShouldBeTrue();
        }

        [Fact]
        public void Should_Not_Consider_A_Versionless_Package_The_Same_As_A_Versioned_One()
        {
            // Given
            var versionless = new Package("Autofac", null, null);
            var versioned = new Package("Autofac", "4.9.4", null);

            // When, Then
            versionless.IsSameVersion(versioned).ShouldBeFalse();
        }

        [Theory]
        [InlineData("1.0.0", "[1.0.0, )")]
        [InlineData("[1.0.0, )", "1.0.0")]
        public void Should_Consider_An_Exact_Version_The_Same_As_The_Range_It_Implies(string first, string second)
        {
            // Given
            var left = new Package("Autofac", first, null);
            var right = new Package("Autofac", second, null);

            // When, Then
            left.IsSameVersion(right).ShouldBeTrue();
        }

        [Theory]
        [InlineData("1.0.0", "[1.0.0]")]
        [InlineData("1.0.0", "(1.0.0, )")]
        [InlineData("1.0.0", "2.0.0")]
        public void Should_Not_Consider_Different_Version_Ranges_To_Be_The_Same_Version(string first, string second)
        {
            // Given
            var left = new Package("Autofac", first, null);
            var right = new Package("Autofac", second, null);

            // When, Then
            left.IsSameVersion(right).ShouldBeFalse();
        }

        [Fact]
        public void Should_Throw_For_A_Package_With_An_Invalid_Version()
        {
            // Given, When
            var result = Record.Exception(() => new Package("Autofac", "not-a-version", null));

            // Then
            result.ShouldBeOfType<ArgumentException>()
                .Message.ShouldStartWith("Version 'not-a-version' for package 'Autofac' is not valid.");
        }
    }
}
