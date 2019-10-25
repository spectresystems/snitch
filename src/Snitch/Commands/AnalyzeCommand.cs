using System;
using System.ComponentModel;
using System.IO;
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

            [CommandOption("--strict")]
            [Description("Returns exit code 0 only if no conflicts were found.")]
            public bool Strict { get; set; }
        }

        public override int Execute(CommandContext context, Settings settings)
        {
            settings.ProjectPath = PathUtility.GetProjectPath(settings.ProjectPath);

            // Analyze the project.
            var project = ProjectBuilder.Build(settings.ProjectPath, settings.TargetFramework);
            var result = ProjectAnalyzer.Analyze(project);

            // Write the report.
            ProjectReporter.Write(result);

            // Return exit code.
            return settings.Strict
                ? (result.NoPackagesToRemove ? 0 : -1)
                : 0;
        }
    }
}
