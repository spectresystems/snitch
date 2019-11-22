using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Snitch.Analysis;
using Snitch.Analysis.Utilities;
using Spectre.Cli;

namespace Snitch.Commands
{
    [Description("Shows transitive package dependencies that can be removed")]
    public sealed class AnalyzeCommand : Command<AnalyzeCommand.Settings>
    {
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
            _builder = new ProjectBuilder(console);
            _analyzer = new ProjectAnalyzer();
            _reporter = new ProjectReporter(console);
        }

        public override int Execute(CommandContext context, Settings settings)
        {
            var projectsToAnalyze = PathUtility.GetProjectPaths(settings.ProjectOrSolutionPath);

            var analyzerResults = new List<ProjectAnalyzerResult>();

            foreach (var projectToAnalyze in projectsToAnalyze)
            {
                // Analyze the project.
                var project = _builder.Build(projectToAnalyze, settings.TargetFramework, settings.Skip);
                var result = _analyzer.Analyze(project);

                if (settings.Exclude?.Length > 0)
                {
                    // Filter packages that should be excluded.
                    result = result.Filter(settings.Exclude);
                }

                analyzerResults.Add(result);
                _reporter.WriteToConsole(result);
            }

            if (analyzerResults.Count > 1)
            {
                _reporter.WriteToConsole(analyzerResults);
            }

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
