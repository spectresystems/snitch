using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using Snitch.Analysis;
using Snitch.Analysis.Utilities;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Snitch.Commands
{
    [Description("Shows transitive package dependencies that can be removed")]
    public sealed class AnalyzeCommand : Command<AnalyzeCommand.Settings>
    {
        private readonly IAnsiConsole _console;
        private readonly ProjectBuilder _builder;
        private readonly ProjectAnalyzer _analyzer;
        private readonly ProjectReporter _reporter;

        public sealed class Settings : CommandSettings
        {
            [CommandArgument(0, "[PROJECT|SOLUTION]")]
            [Description("The project or solution you want to analyze.")]
            public string ProjectOrSolutionPath { get; set; } = string.Empty;

            [CommandOption("-t|--tfm <MONIKER>")]
            [Description("The target framework moniker to analyze.")]
            public string? TargetFramework { get; set; }

            [CommandOption("-e|--exclude <PACKAGE>")]
            [Description("One or more packages to exclude.")]
            public string[]? Exclude { get; set; }

            [CommandOption("--skip <PROJECT>")]
            [Description("One or more project references to exclude.")]
            public string[]? Skip { get; set; }

            [CommandOption("-s|--strict")]
            [Description("Returns exit code 0 only if no conflicts were found.")]
            public bool Strict { get; set; }

            [CommandOption("--no-prerelease")]
            [Description("Verifies that all package references are not pre-releases.")]
            public bool NoPreRelease { get; set; }

            [CommandOption("-o|--out <filename>")]
            [Description("The ouput file to write.")]
            public string? OutputFileName { get; set; }
        }

        public AnalyzeCommand(IAnsiConsole console)
        {
            _console = console ?? throw new ArgumentNullException(nameof(console));
            _builder = new ProjectBuilder(console);
            _analyzer = new ProjectAnalyzer();
            _reporter = new ProjectReporter(console);
        }

        public override int Execute([NotNull] CommandContext context, [NotNull] Settings settings)
        {
            var projectsToAnalyze = PathUtility.GetProjectPaths(settings.ProjectOrSolutionPath, out var entry);

            // Remove all projects that we want to skip.
            projectsToAnalyze.RemoveAll(p =>
            {
                var projectName = Path.GetFileNameWithoutExtension(p);
                return settings.Skip?.Contains(projectName, StringComparer.OrdinalIgnoreCase) ?? false;
            });

            var targetFramework = settings.TargetFramework;
            var analyzerResults = new List<ProjectAnalyzerResult>();
            var projectCache = new HashSet<Project>(new ProjectComparer());

            _console.WriteLine();

            return _console.Status().Start($"Analyzing...", ctx =>
            {
                ctx.Refresh();

                _console.MarkupLine($"Analyzing [yellow]{Path.GetFileName(entry)}[/]");

                foreach (var projectToAnalyze in projectsToAnalyze)
                {
                    if (!projectToAnalyze.EndsWith("csproj", StringComparison.OrdinalIgnoreCase)
                        && !projectToAnalyze.EndsWith("fsproj", StringComparison.OrdinalIgnoreCase))
                    {
                        var projectName = Path.GetFileNameWithoutExtension(projectToAnalyze);
                        _console.MarkupLine($"Skipping Non .NET Project [aqua]{projectName}[/]");
                        _console.WriteLine();

                        continue;
                    }

                    // Perform a design time build of the project.
                    var buildResult = _builder.Build(
                        projectToAnalyze,
                        targetFramework,
                        settings.Skip,
                        projectCache);

                    // Update the cache of built projects.
                    projectCache.Add(buildResult.Project);
                    foreach (var item in buildResult.Dependencies)
                    {
                        projectCache.Add(item);
                    }

                    // Analyze the project.
                    var analyzeResult = _analyzer.Analyze(buildResult.Project);
                    if (settings.Exclude?.Length > 0)
                    {
                        // Filter packages that should be excluded.
                        analyzeResult = analyzeResult.Filter(settings.Exclude);
                    }

                    analyzerResults.Add(analyzeResult);
                }

                // Write the report to the console
                _reporter.WriteToConsole(analyzerResults, settings.NoPreRelease);

                if (settings.OutputFileName != null)
                {
                    // Write the report to a file.
                    _reporter.WriteToFile(analyzerResults, settings.OutputFileName);
                }

                // Return the correct exit code.
                return GetExitCode(settings, analyzerResults);
            });
        }

        private static int GetExitCode(Settings settings, List<ProjectAnalyzerResult> result)
        {
            if (settings.Strict && (result.Any(r => !r.NoPackagesToRemove) || (settings.NoPreRelease && result.Any(r => r.HasPreReleases))))
            {
                return -1;
            }

            return 0;
        }
    }
}
