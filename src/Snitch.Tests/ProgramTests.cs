using Shouldly;
using Snitch;
using Snitch.Tests.Utilities;
using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace Sntich.Tests
{
    public class ProgramTests
    {
        [Theory]
        [EmbeddedResourceData("Snitch.Tests/Expected/Baz.output")]
        public async Task Should_Return_Expected_Result_For_Baz_Not_Specifying_Framework(string expected)
        {
            // Given
            var fixture = new Fixture();
            var project = fixture.GetPath("Baz/Baz.csproj");

            // When
            var result = await fixture.Run(project);

            // Then
            result.exitCode.ShouldBe(0);
            result.output.ShouldBe(expected);
        }

        [Theory]
        [EmbeddedResourceData("Snitch.Tests/Expected/Solution.output")]
        public async Task Should_Return_Expected_Result_For_Solution_Not_Specifying_Framework(string expected)
        {
            // Given
            var fixture = new Fixture();
            var solution = fixture.GetPath("Snitch.Tests.Fixtures.sln");

            // When
            var result = await fixture.Run(solution);

            // Then
            result.exitCode.ShouldBe(0);
            result.output.ShouldBe(expected);
        }

        [Theory]
        [EmbeddedResourceData("Snitch.Tests/Expected/Baz_netstandard20.output")]
        public async Task Should_Return_Expected_Result_For_Baz_Specifying_Framework(string expected)
        {
            // Given
            var fixture = new Fixture();
            var project = fixture.GetPath("Baz/Baz.csproj");

            // When
            var result = await fixture.Run(project, "--tfm", "netstandard2.0");

            // Then
            result.exitCode.ShouldBe(0);
            result.output.ShouldBe(expected);
        }

        [Theory]
        [EmbeddedResourceData("Snitch.Tests/Expected/Baz_netstandard20.output")]
        public async Task Should_Return_Non_Zero_Exit_Code_For_Baz_When_Running_With_Strict(string expected)
        {
            // Given
            var fixture = new Fixture();
            var project = fixture.GetPath("Baz/Baz.csproj");

            // When
            var result = await fixture.Run(project, "--tfm", "netstandard2.0", "--strict");

            // Then
            result.exitCode.ShouldBe(-1);
            result.output.ShouldBe(expected);
        }

        [Theory]
        [EmbeddedResourceData("Snitch.Tests/Expected/Baz_exclude.output")]
        public async Task Should_Return_Expected_Result_For_Baz_When_Excluding_Library(string expected)
        {
            // Given
            var fixture = new Fixture();
            var project = fixture.GetPath("Baz/Baz.csproj");

            // When
            var result = await fixture.Run(project, "--exclude", "Autofac");

            // Then
            result.exitCode.ShouldBe(0);
            result.output.ShouldBe(expected);
        }

        [Theory]
        [EmbeddedResourceData("Snitch.Tests/Expected/Baz_skip.output")]
        public async Task Should_Return_Expected_Result_For_Baz_When_Skipping_Project(string expected)
        {
            // Given
            var fixture = new Fixture();
            var project = fixture.GetPath("Baz/Baz.csproj");

            // When
            var result = await fixture.Run(project, "--skip", "Bar");

            // Then
            result.exitCode.ShouldBe(0);
            result.output.ShouldBe(expected);
        }

        public sealed class Fixture
        {
            public string GetPath(string path)
            {
                var workingDirectory = Environment.CurrentDirectory;
                var solutionDirectory = Path.GetFullPath(Path.Combine(workingDirectory, "../../../../Snitch.Tests.Fixtures"));
                return Path.GetFullPath(Path.Combine(solutionDirectory, path));
            }

            public async Task<(int exitCode, string output)> Run(params string[] args)
            {
                var console = new InMemoryConsole();
                var exitCode = await Program.Run(console, args);
                return (exitCode, console.GetOutput());
            }
        }
    }
}
