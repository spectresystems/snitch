using Snitch.Analyzing;
using Spectre.Cli;
using System;
using System.ComponentModel;
using System.IO;
using System.Linq;

namespace Snitch.Commands
{
    public sealed class AnalyzeCommand : Command<AnalyzeCommand.Settings>
    {
        public sealed class Settings : CommandSettings
        {
            [CommandArgument(0, "<PROJECT>")]
            [Description("The project you want to analyze.")]
            public string ProjectPath { get; set; }

            [CommandOption("-t|--tfm <MONIKER>")]
            [Description("The target framework moniker to analyze.")]
            public string TargetFramework { get; set; }
        }

        public override ValidationResult Validate(CommandContext context, Settings settings)
        {
            if (!File.Exists(settings.ProjectPath))
            {
                return ValidationResult.Error("Project does not exist.");
            }
            return base.Validate(context, settings);
        }

        public override int Execute(CommandContext context, Settings settings)
        {
            // Output the result.
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine();
            Console.Write("Analysing project ");
            Console.Write(Path.GetFileNameWithoutExtension(settings.ProjectPath));
            Console.WriteLine("...");
            Console.WriteLine();
            Console.ResetColor();

            // Analyze the provided project.
            var project = DependencyWalker.Collect(settings.ProjectPath, settings.TargetFramework);

            // Process the analyzed project.
            var result = Analyzer.Analyze(project);

            if (result.Count == 0)
            {
                // Output the result.
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine();
                Console.WriteLine("No transitive packages to remove.");
                Console.WriteLine();
                Console.ResetColor();
            }
            else
            {
                Console.WriteLine();

                if (result.Any(x => x.CanBeRemoved))
                {
                    // Output the result.
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("The following packages can be removed:");
                    Console.WriteLine();
                    Console.ResetColor();

                    foreach (var item in result.Where(x => x.CanBeRemoved))
                    {
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Console.Write("   {0} ", item.Package.Package.Name);
                        Console.ForegroundColor = ConsoleColor.DarkGray;
                        Console.WriteLine(" (referenced by {0})", item.OriginalLocation.Project.Filename);
                        Console.ResetColor();
                    }
                    Console.WriteLine();
                }
                if (result.Any(x => x.VersionMismatch))
                {
                    // Output the result.
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("The following packages might be removed:");
                    Console.WriteLine();
                    Console.ResetColor();

                    foreach (var item in result.Where(x => x.VersionMismatch))
                    {
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Console.Write("   {0}", item.Package.Package.Name);
                        Console.ForegroundColor = ConsoleColor.DarkGray;
                        Console.WriteLine(" (referenced by {0})", item.OriginalLocation.Project.Filename);
                        Console.Write("      ");
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.Write(item.Package.Package.Version);
                        Console.ForegroundColor = ConsoleColor.DarkGray;
                        Console.Write(" -> ");
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.Write(item.OriginalLocation.Package.Version);
                        Console.WriteLine();
                        Console.ResetColor();
                    }
                    Console.WriteLine();
                }
            }

            return 0;
        }
    }
}
