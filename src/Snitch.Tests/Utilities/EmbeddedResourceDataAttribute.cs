using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using Xunit.Sdk;

namespace Snitch.Tests.Utilities
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
    public sealed class EmbeddedResourceDataAttribute : DataAttribute
    {
        private readonly string _resource;
        private readonly object[] _args;

        public EmbeddedResourceDataAttribute(string resource, params object[] args)
        {
            _resource = resource;
            _args = args;
        }

        public override IEnumerable<object[]> GetData(MethodInfo testMethod)
        {
            var result = new object[_args.Length + 1];
            result[0] = ReadManifestData(_resource);
            for (var index = 0; index < _args.Length; index++)
            {
                result[index + 1] = _args[index];
            }
            return new[] { result };
        }

        public static string ReadManifestData(string resourceName)
        {
            var assembly = typeof(EmbeddedResourceDataAttribute).Assembly;
            resourceName = resourceName.Replace("/", ".");
            using (var stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (stream == null)
                {
                    throw new InvalidOperationException("Could not load manifest resource stream.");
                }
                using (var reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd().NormalizeLineEndings();
                }
            }
        }
    }
}
