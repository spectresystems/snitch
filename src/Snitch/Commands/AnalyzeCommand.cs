using System;
using System.ComponentModel;
using System.IO;
using Snitch.Analysis;
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

        public override ValidationResult Validate(CommandContext context, Settings settings)
        {
            if (!string.IsNullOrWhiteSpace(settings.ProjectPath))
            {
                settings.ProjectPath = Path.GetFullPath(settings.ProjectPath);
                if (!File.Exists(settings.ProjectPath))
                {
                    return ValidationResult.Error("Project does not exist.");
                }
            }
            else
            {
                var working = Environment.CurrentDirectory;
                var projects = Directory.GetFiles(working, "*.csproj");
                if (projects.Length == 0)
                {
                    return ValidationResult.Error("No project file found.");
                }

                if (projects.Length > 1)
                {
                    return ValidationResult.Error("More than one project file found.");
                }

                settings.ProjectPath = Path.GetFullPath(projects[0]);
            }

            return base.Validate(context, settings);
        }

        public override int Execute(CommandContext context, Settings settings)
        {
            // Header
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("Analysing project ");
            Console.Write(Path.GetFileNameWithoutExtension(settings.ProjectPath));
            Console.WriteLine("...");
            Console.WriteLine();
            Console.ResetColor();

            // Analyze the project.
            var project = ProjectBuilder.Build(settings.ProjectPath, settings.TargetFramework);
            var result = ProjectAnalyzer.Analyze(project);

            // Write the report.
            ProjectReporter.Write(result);

            // Return success.
            if (settings.Strict)
            {
                return result.NoPackagesToRemove ? 0 : -1;
            }
            else
            {
                return 0;
            }
        }
    }
}
