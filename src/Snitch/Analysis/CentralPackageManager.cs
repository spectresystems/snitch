using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace Snitch.Analysis
{
    internal sealed class CentralPackageManager
    {
        private readonly Dictionary<string, string> _packageVersions;
        private readonly Dictionary<string, string> _globalPackageReferences;
        private readonly bool _isCentralManagementEnabled;

        public bool IsCentralManagementEnabled => _isCentralManagementEnabled;

        public CentralPackageManager(string projectPath)
        {
            if (projectPath == null)
            {
                throw new ArgumentNullException(nameof(projectPath));
            }

            _packageVersions = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            _globalPackageReferences = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            var directoryPackagesPath = FindDirectoryPackagesProps(projectPath);
            if (!string.IsNullOrEmpty(directoryPackagesPath))
            {
                _isCentralManagementEnabled = LoadDirectoryPackages(directoryPackagesPath);
            }
        }

        public string? GetPackageVersion(string packageName)
        {
            if (_packageVersions.TryGetValue(packageName, out var version))
            {
                return version;
            }

            return null;
        }

        public string? GetGlobalPackageVersion(string packageName)
        {
            if (_globalPackageReferences.TryGetValue(packageName, out var version))
            {
                return version;
            }

            return null;
        }

        public IEnumerable<Package> GetGlobalPackages()
        {
            return _globalPackageReferences
                .Where(kvp => !string.IsNullOrEmpty(kvp.Value))
                .Select(kvp => new Package(kvp.Key, kvp.Value, null));
        }

        public bool HasPackageVersion(string packageName)
        {
            return _packageVersions.ContainsKey(packageName);
        }

        public bool HasGlobalPackageReference(string packageName)
        {
            return _globalPackageReferences.ContainsKey(packageName);
        }

        private string? FindDirectoryPackagesProps(string projectPath)
        {
            var currentDirectory = Path.GetDirectoryName(projectPath);

            while (currentDirectory != null)
            {
                var directoryPackagesPath = Path.Combine(currentDirectory, "Directory.Packages.props");
                if (File.Exists(directoryPackagesPath))
                {
                    return directoryPackagesPath;
                }

                var parentDirectory = Directory.GetParent(currentDirectory);
                if (parentDirectory == null)
                {
                    break;
                }

                currentDirectory = parentDirectory.FullName;
            }

            return null;
        }

        private bool LoadDirectoryPackages(string directoryPackagesPath)
        {
            try
            {
                var document = XDocument.Load(directoryPackagesPath);
                var root = document.Root;

                if (root == null)
                {
                    return false;
                }

                var isCentralManagementEnabled = IsCentralManagementEnabledInFile(root);

                if (!isCentralManagementEnabled)
                {
                    var directoryBuildPropsPath = Path.Combine(Path.GetDirectoryName(directoryPackagesPath)!, "Directory.Build.props");
                    if (File.Exists(directoryBuildPropsPath))
                    {
                        isCentralManagementEnabled = IsCentralManagementEnabledInPropsFile(directoryBuildPropsPath);
                    }
                }

                if (!isCentralManagementEnabled)
                {
                    return false;
                }

                var itemGroups = root.Elements("ItemGroup");
                foreach (var itemGroup in itemGroups)
                {
                    var packageVersions = itemGroup.Elements("PackageVersion");
                    foreach (var packageVersion in packageVersions)
                    {
                        var include = packageVersion.Attribute("Include")?.Value;
                        var version = packageVersion.Attribute("Version")?.Value;

                        if (!string.IsNullOrEmpty(include) && !string.IsNullOrEmpty(version))
                        {
                            _packageVersions[include] = version;
                        }
                    }

                    var globalPackageReferences = itemGroup.Elements("GlobalPackageReference");
                    foreach (var globalPackageReference in globalPackageReferences)
                    {
                        var include = globalPackageReference.Attribute("Include")?.Value;
                        var version = globalPackageReference.Attribute("Version")?.Value;

                        if (!string.IsNullOrEmpty(include) && !string.IsNullOrEmpty(version))
                        {
                            _globalPackageReferences[include] = version;
                        }
                    }
                }

                return isCentralManagementEnabled;
            }
            catch (Exception)
            {
                // If we can't parse the file, just continue without central management
                return false;
            }
        }

        private static bool IsCentralManagementEnabledInFile(XElement root)
        {
            var propertyGroups = root.Elements("PropertyGroup");
            foreach (var propertyGroup in propertyGroups)
            {
                var centralManagementElement = propertyGroup.Element("ManagePackageVersionsCentrally");
                if (centralManagementElement != null &&
                    bool.TryParse(centralManagementElement.Value, out var isCentralManagement) &&
                    isCentralManagement)
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsCentralManagementEnabledInPropsFile(string propsFilePath)
        {
            try
            {
                var document = XDocument.Load(propsFilePath);
                var root = document.Root;

                if (root == null)
                {
                    return false;
                }

                return IsCentralManagementEnabledInFile(root);
            }
            catch (Exception)
            {
                // If we can't parse the file, just continue without central management
                return false;
            }
        }
    }
}