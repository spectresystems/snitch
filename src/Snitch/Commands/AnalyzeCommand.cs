using System.ComponentModel;
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
            [CommandArgument(0, "[PROJECT]")]
            [Description("The project you want to analyze.")]
            public string ProjectPath { get; set; } = string.Empty;

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
            settings.ProjectPath = PathUtility.GetProjectPath(settings.ProjectPath);

            // Analyze the project.
            var project = _builder.Build(settings.ProjectPath, settings.TargetFramework, settings.Skip);
            var result = _analyzer.Analyze(project);

            if (settings.Exclude?.Length > 0)
            {
                // Filter packages that should be excluded.
                result = result.Filter(settings.Exclude);
            }

            _reporter.WriteToConsole(result);

            return GetExitCode(settings, result);
        }

        private static int GetExitCode(Settings settings, ProjectAnalyzerResult result)
        {
            if (settings.Strict && !result.NoPackagesToRemove)
            {
                return -1;
            }

            return 0;
        }
    }
}
