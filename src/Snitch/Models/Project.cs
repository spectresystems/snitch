using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Snitch.Analyzing
{
    [DebuggerDisplay("{GetProjectName(),nq}")]
    public sealed class Project
    {
        public string Path { get; set; }
        public string Filename { get; set; }
        public string TargetFramework { get; set; }
        public List<Project> ProjectReferences { get; set; }
        public List<Package> Packages { get; set; }

        public Project(string path)
        {
            Path = path ?? throw new ArgumentNullException(nameof(path));
            Filename = System.IO.Path.GetFileNameWithoutExtension(Path);
            ProjectReferences = new List<Project>();
            Packages = new List<Package>();
        }

        private string GetProjectName()
        {
            return System.IO.Path.GetFileName(Path);
        }
    }
}
