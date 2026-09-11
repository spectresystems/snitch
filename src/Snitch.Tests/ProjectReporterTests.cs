using System.Collections.Generic;
using Shouldly;
using Snitch.Analysis;
using Spectre.Console.Testing;
using Xunit;

namespace Sntich.Tests
{
    public sealed class ProjectReporterTests
    {
        // A pinned version such as [3.5.4] is also valid Spectre.Console markup,
        // as is a project or package name containing square brackets.
        private const string PinnedVersion = "[3.5.4]";

        private static Project CreateProject(string name, params Package[] packages)
        {
            var project = new Project($"/tmp/{name}/{name}.csproj");
            project.TargetFramework = "netstandard2.0";
            project.Packages.AddRange(packages);
            return project;
        }

        private static (TestConsole Console, ProjectReporter Reporter) CreateReporter()
        {
            var console = new TestConsole().Width(200);
            return (console, new ProjectReporter(console));
        }

        [Fact]
        public void Should_Render_A_Pinned_Version_Verbatim_When_A_Package_Might_Be_Removed()
        {
            // Given
            var (console, reporter) = CreateReporter();
            var package = new Package("RavenDB.Client", "[3.6.1]", null);
            var originalPackage = new Package("RavenDB.Client", PinnedVersion, null);
            var original = CreateProject("Bar", originalPackage);
            var root = CreateProject("Foo", package);
            var result = new ProjectAnalyzerResult(
                root,
                new[] { new PackageToRemove(root, package, new ProjectPackage(original, originalPackage)) });

            // When
            reporter.WriteToConsole(new List<ProjectAnalyzerResult> { result }, false);

            // Then
            console.Output.ShouldContain("[3.6.1]");
            console.Output.ShouldContain("[3.5.4]");
        }

        [Fact]
        public void Should_Not_Interpret_A_Package_Or_Project_Name_As_Markup()
        {
            // Given
            var (console, reporter) = CreateReporter();
            var package = new Package("Red[Package]", "1.0.0", null);
            var original = CreateProject("Green[Project]", package);
            var root = CreateProject("Blue[Root]", package);
            var result = new ProjectAnalyzerResult(
                root,
                new[] { new PackageToRemove(root, package, new ProjectPackage(original, package)) });

            // When
            reporter.WriteToConsole(new List<ProjectAnalyzerResult> { result }, false);

            // Then
            console.Output.ShouldContain("Red[Package]");
            console.Output.ShouldContain("Green[Project]");
            console.Output.ShouldContain("Blue[Root]");
        }

        [Fact]
        public void Should_Not_Interpret_A_Pre_Release_Version_As_Markup()
        {
            // Given
            var (console, reporter) = CreateReporter();
            var package = new Package("Thud[Alpha]", "1.0.0-alpha", null);
            var root = CreateProject("Foo", package);
            var result = new ProjectAnalyzerResult(root, new PackageToRemove[0]);

            // When
            reporter.WriteToConsole(new List<ProjectAnalyzerResult> { result }, true);

            // Then
            console.Output.ShouldContain("Thud[Alpha]");
            console.Output.ShouldContain("1.0.0-alpha");
        }
    }
}
