using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

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
                    WritePackageCanBeRemoved(item);
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
                    WritePackageMayBeRemoved(item);
                }

                _console.WriteLine();
            }
        }

        public void WriteToConsole([NotNull] List<ProjectAnalyzerResult> results)
        {
            var resultsWithPackageToRemove = results.Where(r => r.CanBeRemoved.Count > 0).ToList();
            var resultsWithPackageMayBeRemove = results.Where(r => r.MightBeRemoved.Count > 0).ToList();

            if (resultsWithPackageToRemove.Count > 0)
            {
                foreach (var result in resultsWithPackageToRemove)
                {
                    _console.ForegroundColor = ConsoleColor.Yellow;
                    _console.Write("The following packages can be removed from : ");
                    _console.ForegroundColor = ConsoleColor.White;
                    _console.WriteLine(result.Project);
                    _console.ResetColor();
                    foreach (var item in result.CanBeRemoved)
                    {
                        WritePackageCanBeRemoved(item);
                    }
                    _console.WriteLine();
                }
            }

            if (resultsWithPackageMayBeRemove.Count > 0)
            {
                foreach (var result in resultsWithPackageToRemove)
                {
                    _console.ForegroundColor = ConsoleColor.Yellow;
                    _console.WriteLine("The following packages might be removed:");
                    _console.ForegroundColor = ConsoleColor.Cyan;
                    _console.WriteLine(result.Project);
                    _console.WriteLine();
                    _console.ResetColor();
                    foreach (var item in result.CanBeRemoved)
                    {
                        WritePackageMayBeRemoved(item);
                    }
                }
            }
        }

        private void WritePackageCanBeRemoved(PackageToRemove item)
        {
            _console.ForegroundColor = ConsoleColor.Cyan;
            _console.Write("   {0}", item.Package.Name);
            _console.ForegroundColor = ConsoleColor.DarkGray;
            _console.WriteLine(" (ref by {0})", item.Original.Project.Name);
            _console.ResetColor();
        }

        private void WritePackageMayBeRemoved(PackageToRemove item)
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
    }
}
