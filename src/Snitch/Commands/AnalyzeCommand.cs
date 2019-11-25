using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using Snitch.Analysis;
using Snitch.Analysis.Utilities;
using Spectre.Cli;

namespace Snitch.Commands
{
    [Description("Shows transitive package dependencies that can be removed")]
    public sealed class AnalyzeCommand : Command<AnalyzeCommand.Settings>
    {
        private readonly IConsole _console;
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
        }

        public AnalyzeCommand(IConsole console)
        {
            _console = console;
            _builder = new ProjectBuilder(console);
            _analyzer = new ProjectAnalyzer();
            _reporter = new ProjectReporter(console);
        }

        public override int Execute(CommandContext context, Settings settings)
        {
            var projectsToAnalyze = PathUtility.GetProjectPaths(settings.ProjectOrSolutionPath, out var entry);

            // Remove all projects that we want to skip.
            projectsToAnalyze.RemoveAll(p =>
            {
                var projectName = Path.GetFileNameWithoutExtension(p);
                return settings.Skip?.Contains(projectName, StringComparer.OrdinalIgnoreCase) ?? false;
            });

            _console.WriteLine();
            _console.Write("Processing ");
            _console.ForegroundColor = ConsoleColor.Yellow;
            _console.Write(Path.GetFileName(entry));
            _console.ResetColor();
            _console.WriteLine("...");

            var targetFramework = settings.TargetFramework;

            var analyzerResults = new List<ProjectAnalyzerResult>();
            var projectCache = new HashSet<Project>(new ProjectComparer());
            foreach (var projectToAnalyze in projectsToAnalyze)
            {
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

            _reporter.WriteToConsole(analyzerResults);

            return GetExitCode(settings, analyzerResults);
        }

        private static int GetExitCode(Settings settings, List<ProjectAnalyzerResult> result)
        {
            if (settings.Strict && result.Any(r => !r.NoPackagesToRemove))
            {
                return -1;
            }

            return 0;
        }
    }
}
