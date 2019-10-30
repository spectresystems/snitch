using System;
using System.Diagnostics.CodeAnalysis;

namespace Snitch.Analysis
{
    internal class ProjectReporter
    {
        private readonly IConsole _console;

        public ProjectReporter(IConsole console)
        {
            _console = console ?? throw new ArgumentNullException(nameof(console));
        }

        public void WriteToConsole([NotNull] ProjectAnalyzerResult result)
        {
            _console.WriteLine();

            if (result.NoPackagesToRemove)
            {
                // Output the result.
                _console.ForegroundColor = ConsoleColor.Green;
                _console.WriteLine("No packages to remove.");
                _console.WriteLine();
                _console.ResetColor();
                return;
            }

            // Packages that can be removed.
            if (result.CanBeRemoved.Count > 0)
            {
                // Output the result.
                _console.ForegroundColor = ConsoleColor.Yellow;
                _console.WriteLine("The following packages can be removed:");
                _console.WriteLine();
                _console.ResetColor();

                foreach (var item in result.CanBeRemoved)
                {
                    _console.ForegroundColor = ConsoleColor.Cyan;
                    _console.Write("   {0}", item.Package.Name);
                    _console.ForegroundColor = ConsoleColor.DarkGray;
                    _console.WriteLine(" (ref by {0})", item.Original.Project.Name);
                    _console.ResetColor();
                }

                _console.WriteLine();
            }

            // Packages that might be removed.
            if (result.MightBeRemoved.Count > 0)
            {
                // Output the result.
                _console.ForegroundColor = ConsoleColor.Yellow;
                _console.WriteLine("The following packages might be removed:");
                _console.WriteLine();
                _console.ResetColor();

                foreach (var item in result.MightBeRemoved)
                {
                    _console.ForegroundColor = ConsoleColor.Cyan;
                    _console.Write("   {0}", item.Package.Name);
                    _console.ForegroundColor = ConsoleColor.DarkGray;
                    _console.WriteLine(" (ref by {0})", item.Original.Project.Name);
                    _console.Write("      ");
                    _console.ForegroundColor = ConsoleColor.Yellow;
                    _console.Write(item.Package.Version.ToString());
                    _console.ForegroundColor = ConsoleColor.DarkGray;

                    if (item.Package.Version > item.Original.Package.Version)
                    {
                        _console.Write(" <- ");
                    }
                    else
                    {
                        _console.Write(" -> ");
                    }

                    _console.ForegroundColor = ConsoleColor.Yellow;
                    _console.Write(item.Original.Package.Version.ToString());
                    _console.ForegroundColor = ConsoleColor.DarkGray;
                    _console.WriteLine($" ({item.Original.Project.Name})");
                    _console.ResetColor();
                }

                _console.WriteLine();
            }
        }
    }
}
