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

        public void WriteToConsole([NotNull] List<ProjectAnalyzerResult> results)
        {
            var resultsWithPackageToRemove = results.Where(r => r.CanBeRemoved.Count > 0).ToList();
            var resultsWithPackageMayBeRemove = results.Where(r => r.MightBeRemoved.Count > 0).ToList();

            if (results.All(x => x.NoPackagesToRemove))
            {
                // Output the result.
                _console.ForegroundColor = ConsoleColor.Green;
                _console.WriteLine();
                _console.WriteLine("No packages to remove.");
                _console.WriteLine();
                _console.ResetColor();
                return;
            }

            if (resultsWithPackageToRemove.Count > 0)
            {
                _console.WriteLine();

                foreach (var result in resultsWithPackageToRemove)
                {
                    _console.ForegroundColor = ConsoleColor.Yellow;
                    _console.Write("The following packages can be removed from ");
                    _console.ForegroundColor = ConsoleColor.Cyan;
                    _console.WriteLine(result.Project);
                    _console.WriteLine();
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
                foreach (var result in resultsWithPackageMayBeRemove)
                {
                    _console.ForegroundColor = ConsoleColor.Yellow;
                    _console.Write("The following packages <might> be removed from ");
                    _console.ForegroundColor = ConsoleColor.Cyan;
                    _console.WriteLine(result.Project);
                    _console.WriteLine();
                    _console.ResetColor();
                    foreach (var item in result.MightBeRemoved)
                    {
                        WritePackageMayBeRemoved(item);
                    }

                    _console.WriteLine();
                }
            }
        }

        private void WritePackageCanBeRemoved(PackageToRemove item)
        {
            _console.Write("   * ");
            _console.ForegroundColor = ConsoleColor.Cyan;
            _console.Write("{0}", item.Package.Name);
            _console.ForegroundColor = ConsoleColor.DarkGray;
            _console.WriteLine(" (ref by {0})", item.Original.Project.Name);
            _console.ResetColor();
        }

        private void WritePackageMayBeRemoved(PackageToRemove item)
        {
            _console.Write("   * ");
            _console.ForegroundColor = ConsoleColor.Cyan;
            _console.Write("{0} ", item.Package.Name);
            _console.ForegroundColor = ConsoleColor.Gray;
            _console.WriteLine(item.Package.Version.ToString());

            if (item.Package.Version > item.Original.Package.Version)
            {
                _console.ForegroundColor = ConsoleColor.DarkGray;
                _console.Write("     Updated from ");
                _console.ForegroundColor = ConsoleColor.Gray;
                _console.Write(item.Original.Package.Version.ToString());
                _console.ForegroundColor = ConsoleColor.DarkGray;
                _console.Write(" in ");
                _console.ForegroundColor = ConsoleColor.Cyan;
                _console.WriteLine(item.Original.Project.Name);
                _console.ResetColor();
            }
            else
            {
                _console.ForegroundColor = ConsoleColor.DarkGray;
                _console.Write("     Downgraded from ");
                _console.ForegroundColor = ConsoleColor.Gray;
                _console.Write(item.Original.Package.Version.ToString());
                _console.ForegroundColor = ConsoleColor.DarkGray;
                _console.Write(" in ");
                _console.ForegroundColor = ConsoleColor.Cyan;
                _console.WriteLine(item.Original.Project.Name);
                _console.ResetColor();
            }
        }
    }
}
