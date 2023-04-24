using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text.Json;
using Spectre.Console;

namespace Snitch.Analysis
{
    internal class ProjectFileReporter
    {
        public ProjectFileReporter()
        {
        }

        internal void WriteToFile(List<ProjectAnalyzerResult> analyzerResults, string outputFileName)
        {
            var removeResult =
                analyzerResults
                    .Where(x => x.CanBeRemoved.Count > 0)
                    .Select(x => new
                    {
                        x.Project,
                        CanBeRemoved = x.CanBeRemoved.Select(y => new
                        {
                             PackageName = y.Package.Name,
                             ReferencedBy = y.Original.Project.Name,
                        }),
                        MightBeRemoved = x.MightBeRemoved.Select(y => new
                        {
                             PackageName = y.Package.Name,
                             ReferencedBy = y.Original.Project.Name,
                        }),
                    });

            using FileStream createStream = File.Create(outputFileName);
            JsonSerializer.Serialize(createStream, removeResult, new JsonSerializerOptions()
            {
                WriteIndented = true,
            });
        }
    }
}
