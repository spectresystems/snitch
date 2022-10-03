using System.Collections.Generic;
using System.Linq;
using Buildalyzer;
using NuGet.Frameworks;

namespace Snitch.Analysis
{
    internal static class AnalyzerResultExtensions
    {
        public static string? GetProjectAssetsFilePath(this IAnalyzerResult result)
        {
            return result?.GetProperty("ProjectAssetsFile");
        }

        public static string GetNearestFrameworkMoniker(this IEnumerable<IAnalyzerResult> source, string framework)
        {
            var current = NuGetFramework.Parse(framework, DefaultFrameworkNameProvider.Instance);
            return current.GetNearestFrameworkMoniker(source.Select(x => x.TargetFramework));
        }

        private static string GetNearestFrameworkMoniker(this NuGetFramework framework, IEnumerable<string> candidates)
        {
            var provider = DefaultFrameworkNameProvider.Instance;
            var reducer = new FrameworkReducer();

            var mappings = new Dictionary<NuGetFramework, string>(
                candidates.ToDictionary(
                    x => NuGetFramework.Parse(x, provider), y => y, new NuGetFrameworkFullComparer()));

            return mappings[reducer.GetNearest(framework, mappings.Keys)];
        }
    }
}