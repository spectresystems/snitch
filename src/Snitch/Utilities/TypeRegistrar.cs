using System;
using Autofac;
using Spectre.Console.Cli;

namespace Snitch.Utilities
{
    internal sealed class TypeRegistrar : ITypeRegistrar
    {
        private readonly ContainerBuilder _builder;

        public TypeRegistrar()
        {
            _builder = new ContainerBuilder();
        }

        public ITypeResolver Build()
        {
            return new TypeResolver(_builder.Build());
        }

        public void Register(Type service, Type implementation)
        {
            _builder.RegisterType(implementation).As(service).SingleInstance();
        }

        public void RegisterInstance(Type service, object implementation)
        {
            _builder.RegisterInstance(implementation).As(service);
        }

        public void RegisterLazy(Type service, Func<object> factory)
        {
            _builder.Register(_ => factory()).As(service);
        }
    }
}
