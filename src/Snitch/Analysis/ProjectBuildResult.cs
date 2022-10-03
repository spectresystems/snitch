using System.Collections.Generic;
using System.Linq;

namespace Snitch.Analysis
{
    internal sealed class ProjectBuildResult
    {
        public Project Project { get; }
        public IReadOnlyList<Project> Dependencies { get; }

        public ProjectBuildResult(Project project, IEnumerable<Project> dependencies)
        {
            Project = project;
            Dependencies = new List<Project>(dependencies ?? Enumerable.Empty<Project>());
        }
    }
}