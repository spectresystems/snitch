using System;
using Autofac;
using Spectre.Console.Cli;

namespace Snitch.Utilities
{
    internal sealed class TypeResolver : ITypeResolver
    {
        private readonly IContainer _scope;

        public TypeResolver(IContainer scope)
        {
            _scope = scope ?? throw new ArgumentNullException(nameof(scope));
        }

        public object? Resolve(Type? type)
        {
            if (type is null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            return _scope.Resolve(type);
        }
    }
}
