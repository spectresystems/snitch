using System.ComponentModel;
using Snitch.Analysis;
using Snitch.Analysis.Utilities;
using Spectre.Cli;

namespace Snitch.Commands
{
    public sealed class AnalyzeCommand : Command<AnalyzeCommand.Settings>
    {
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
            public string[]? Ignore { get; set; }

            [CommandOption("-s|--strict")]
            [Description("Returns exit code 0 only if no conflicts were found.")]
            public bool Strict { get; set; }
        }

        public override int Execute(CommandContext context, Settings settings)
        {
            settings.ProjectPath = PathUtility.GetProjectPath(settings.ProjectPath);

            // Analyze the project.
            var project = ProjectBuilder.Build(settings.ProjectPath, settings.TargetFramework);
            var result = ProjectAnalyzer.Analyze(project);

            if (settings.Ignore?.Length > 0)
            {
                // Filter packages that should be excluded.
                result = result.Filter(settings.Ignore);
            }

            ProjectReporter.WriteToConsole(result);

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
