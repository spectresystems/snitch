using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Spectre.Console;

namespace Snitch.Analysis
{
    internal class ProjectFileReporter
    {
        private readonly IAnsiConsole _console;

        public ProjectFileReporter(IAnsiConsole console)
        {
            _console = console ?? throw new ArgumentNullException(nameof(console));
        }

        internal void WriteToFile(List<ProjectAnalyzerResult> analyzerResults, string outputFileName, bool noPreRelease)
        {
            var results =
                analyzerResults
                    .Where(x => x.CanBeRemoved.Count > 0 || x.MightBeRemoved.Count > 0 || x.HasPreReleases)
                    .Select(x => new
                    {
                        x.Project,
                        CanBeRemoved = x.CanBeRemoved.Select(y => new
                        {
                             PackageName = y.Package.Name,
                             PackageVersion = y.Package.Version?.OriginalVersion,
                             ReferencedBy = y.Original.Project.Name,
                        }),
                        MightBeRemoved = x.MightBeRemoved.Select(y => new
                        {
                             PackageName = y.Package.Name,
                             PackageVersion = y.Package.Version?.OriginalVersion,
                             ReferencedBy = y.Original.Project.Name,
                             ReferencePackageVersion = y.Original.Package.Version?.OriginalVersion,
                        }),
                        PreRelease = noPreRelease ? null : x.PreReleasePackages.Select(y => new
                        {
                            PackageName = y.Name,
                            PackageVersion = y.Version?.OriginalVersion,
                        }),
                    });

            using FileStream createStream = File.Create(outputFileName);
            JsonSerializer.Serialize(createStream, results, new JsonSerializerOptions()
            {
                WriteIndented = true,
            });

            _console.WriteLine();
            _console.MarkupLine($"[green]Results written to {outputFileName}![/]");
            _console.WriteLine();
        }
    }
}
