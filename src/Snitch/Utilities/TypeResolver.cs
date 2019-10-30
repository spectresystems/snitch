using System;
using Autofac;
using Spectre.Cli;

namespace Snitch.Utilities
{
    internal sealed class TypeResolver : ITypeResolver
    {
        private readonly IContainer _scope;

        public TypeResolver(IContainer scope)
        {
            _scope = scope;
        }

        public object? Resolve(Type? type)
        {
            return _scope.Resolve(type);
        }
    }
}
