using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Snitch.Analysis
{
    [DebuggerDisplay("{GetProjectName(),nq}")]
    public sealed class Project
    {
        public string Path { get; }
        public string Name { get; }
        public string TargetFramework { get; set; }
        public List<Project> ProjectReferences { get; set; }
        public List<Package> Packages { get; set; }

        public Project(string path)
        {
            Path = path ?? throw new ArgumentNullException(nameof(path));
            Name = System.IO.Path.GetFileNameWithoutExtension(Path);
            TargetFramework = string.Empty;
            ProjectReferences = new List<Project>();
            Packages = new List<Package>();
        }

        private string GetProjectName()
        {
            return System.IO.Path.GetFileName(Path);
        }
    }
}
