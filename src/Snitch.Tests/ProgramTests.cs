using Shouldly;
using System;
using System.IO;
using System.Threading.Tasks;
using Spectre.Console.Cli;
using Spectre.Console.Testing;
using VerifyTests;
using Xunit;
using VerifyXunit;

namespace Snitch.Tests
{
    [UsesVerify]
    public class ProgramTests
    {
        [Fact]
        [Expectation("Baz", "Default")]
        public async Task Should_Return_Expected_Result_For_Baz_Not_Specifying_Framework()
        {
            // Given
            var project = Fixture.GetPath("Baz/Baz.csproj");

            // When
            var (exitCode, output) = await Fixture.Run(project);

            // Then
            exitCode.ShouldBe(0);
            await Verifier.Verify(output);
        }

        [Fact]
        [Expectation("Solution", "Default")]
        public async Task Should_Return_Expected_Result_For_Solution_Not_Specifying_Framework()
        {
            // Given
            var solution = Fixture.GetPath("Snitch.Tests.Fixtures.sln");

            // When
            var (exitCode, output) = await Fixture.Run(solution);

            // Then
            exitCode.ShouldBe(0);
            await Verifier.Verify(output);
        }

        [Fact]
        [Expectation("Baz", "netstandard2.0")]
        public async Task Should_Return_Expected_Result_For_Baz_Specifying_Framework()
        {
            // Given
            var project = Fixture.GetPath("Baz/Baz.csproj");

            // When
            var (exitCode, output) = await Fixture.Run(project, "--tfm", "netstandard2.0");

            // Then
            exitCode.ShouldBe(0);
            await Verifier.Verify(output);
        }

        [Fact]
        [Expectation("Baz", "netstandard2.0_Strict")]
        public async Task Should_Return_Non_Zero_Exit_Code_For_Baz_When_Running_With_Strict()
        {
            // Given
            var project = Fixture.GetPath("Baz/Baz.csproj");

            // When
            var (exitCode, output) = await Fixture.Run(project, "--tfm", "netstandard2.0", "--strict");

            // Then
            exitCode.ShouldBe(-1);
            await Verifier.Verify(output);
        }

        [Fact]
        [Expectation("Baz", "Exclude_Autofac")]
        public async Task Should_Return_Expected_Result_For_Baz_When_Excluding_Library()
        {
            // Given
            var project = Fixture.GetPath("Baz/Baz.csproj");

            // When
            var (exitCode, output) = await Fixture.Run(project, "--exclude", "Autofac");

            // Then
            exitCode.ShouldBe(0);
            await Verifier.Verify(output);
        }

        [Fact]
        [Expectation("Baz", "Skip_Bar")]
        public async Task Should_Return_Expected_Result_For_Baz_When_Skipping_Project()
        {
            // Given
            var project = Fixture.GetPath("Baz/Baz.csproj");

            // When
            var (exitCode, output) = await Fixture.Run(project, "--skip", "Bar");

            // Then
            exitCode.ShouldBe(0);
            await Verifier.Verify(output);
        }

        [Fact]
        [Expectation("Baz", "Skip_Bar_NoPreRelease")]
        public async Task Should_Return_Expected_Result_For_Baz_When_Skipping_Project_And_NoReleases()
        {
            // Given
            var project = Fixture.GetPath("Baz/Baz.csproj");

            // When
            var (exitCode, output) = await Fixture.Run(project, "--skip", "Bar", "--no-prerelease");

            // Then
            exitCode.ShouldBe(0);
            await Verifier.Verify(output);
        }

        [Fact]
        [Expectation("Baz", "netstandard2.Strict.NoPreRelease")]
        public async Task Should_Return_Non_Zero_Exit_Code_For_Baz_When_Running_With_Strict_And_NoPreRelease()
        {
            // Given
            var project = Fixture.GetPath("Baz/Baz.csproj");

            // When
            var (exitCode, output) = await Fixture.Run(project, "--no-prerelease", "--strict");

            // Then
            exitCode.ShouldBe(-1);
            await Verifier.Verify(output);
        }

        [Fact]
        [Expectation("Thud", "netstandard2.Strict.NoPreRelease")]
        public async Task Should_Return_Non_Zero_Exit_Code_For_Thud_When_Running_With_Strict_And_NoPreRelease()
        {
            // Given
            var project = Fixture.GetPath("Thud/Thud.csproj");

            // When
            var (exitCode, output) = await Fixture.Run(project, "--no-prerelease", "--strict");

            // Then
            exitCode.ShouldBe(-1);
            await Verifier.Verify(output);
        }

        [Fact]
        [Expectation("Thuuud", "netstandard2.Strict.NoPreRelease")]
        public async Task Should_Return_Non_Zero_Exit_Code_For_Thuuud_When_Running_With_Strict_And_NoPreRelease()
        {
            // Given
            var project = Fixture.GetPath("Thuuud/Thuuud.csproj");

            // When
            var (exitCode, output) = await Fixture.Run(project, "--no-prerelease", "--strict");

            // Then
            exitCode.ShouldBe(-1);
            await Verifier.Verify(output);
        }

        [Fact]
        [Expectation("Thuuud", "netstandard2.NoPreRelease")]
        public async Task Should_Return_Zero_Exit_Code_For_Thuuud_When_Running_With_NoPreRelease()
        {
            // Given
            var project = Fixture.GetPath("Thuuud/Thuuud.csproj");

            // When
            var (exitCode, output) = await Fixture.Run(project, "--no-prerelease");

            // Then
            exitCode.ShouldBe(0);
            await Verifier.Verify(output);
        }

        [Fact]
        [Expectation("FooBar", "Default")]
        public async Task Should_Print_Error_For_FooBar()
        {
            // Given
            var project = Fixture.GetPath("FooBar/FooBar.csproj");

            // When
            var (exitCode, output) = await Fixture.Run(project);

            // Then
            exitCode.ShouldBe(0);
            await Verifier.Verify(output);
        }

        [Fact]
        [Expectation("FooBar", "Strict")]
        public async Task Should_Return_NonZero_Exit_Code_For_FooBar_When_Running_With_Strict()
        {
            // Given
            var project = Fixture.GetPath("FooBar/FooBar.csproj");

            // When
            var (exitCode, output) = await Fixture.Run(project, "--strict");

            // Then
            exitCode.ShouldBe(-1);
            await Verifier.Verify(output);
        }

        [Fact]
        [Expectation("FSharp", "Default")]
        public async Task Should_Return_Expected_Result_For_FSharp_Not_Specifying_Framework()
        {
            // Given
            var project = Fixture.GetPath("FSharp/FSharp.fsproj");

            // When
            var (exitCode, output) = await Fixture.Run(project);

            // Then
            exitCode.ShouldBe(0);
            await Verifier.Verify(output);
        }

        private static class Fixture
        {
            public static string GetPath(string path)
            {
                var workingDirectory = Environment.CurrentDirectory;
                var solutionDirectory = Path.GetFullPath(Path.Combine(workingDirectory, "../../../../Snitch.Tests.Fixtures"));
                return Path.GetFullPath(Path.Combine(solutionDirectory, path));
            }

            public static async Task<(int exitCode, string output)> Run(params string[] args)
            {
                var console = new TestConsole { EmitAnsiSequences = false };
                var exitCode = await Program.Run(args, c => c.ConfigureConsole(console));
                return (exitCode, console.Output.Trim());
            }
        }
    }
}
