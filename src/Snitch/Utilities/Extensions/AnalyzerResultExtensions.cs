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

            // A multi targeting project also yields a result without a target framework.
            var candidates = source.Select(x => x.TargetFramework).Where(x => !string.IsNullOrWhiteSpace(x));

            return current.GetNearestFrameworkMoniker(candidates);
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