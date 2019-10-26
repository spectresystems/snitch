using System;
using System.Diagnostics.CodeAnalysis;

namespace Snitch.Analysis
{
    internal static class ProjectReporter
    {
        public static void WriteToConsole([NotNull] ProjectAnalyzerResult result)
        {
            Console.WriteLine();

            if (result.NoPackagesToRemove)
            {
                // Output the result.
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("No packages to remove.");
                Console.WriteLine();
                Console.ResetColor();
                return;
            }

            // Packages that can be removed.
            if (result.CanBeRemoved.Count > 0)
            {
                // Output the result.
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("The following packages can be removed:");
                Console.WriteLine();
                Console.ResetColor();

                foreach (var item in result.CanBeRemoved)
                {
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.Write("   {0}", item.Package.Name);
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.WriteLine(" (ref by {0})", item.Original.Project.Name);
                    Console.ResetColor();
                }

                Console.WriteLine();
            }

            // Packages that might be removed.
            if (result.MightBeRemoved.Count > 0)
            {
                // Output the result.
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("The following packages might be removed:");
                Console.WriteLine();
                Console.ResetColor();

                foreach (var item in result.MightBeRemoved)
                {
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.Write("   {0}", item.Package.Name);
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.WriteLine(" (ref by {0})", item.Original.Project.Name);
                    Console.Write("      ");
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.Write(item.Package.Version);
                    Console.ForegroundColor = ConsoleColor.DarkGray;

                    if (item.Package.Version > item.Original.Package.Version)
                    {
                        Console.Write(" <- ");
                    }
                    else
                    {
                        Console.Write(" -> ");
                    }

                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.Write(item.Original.Package.Version);
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.WriteLine($" ({item.Original.Project.Name})");
                    Console.ResetColor();
                }

                Console.WriteLine();
            }
        }
    }
}
