using System;
using Autofac;
using Spectre.Cli;

namespace Snitch.Utilities
{
    internal sealed class TypeRegistrar : ITypeRegistrar
    {
        private readonly ContainerBuilder _builder;

        public TypeRegistrar(IConsole console)
        {
            _builder = new ContainerBuilder();
            _builder.RegisterInstance(console).As<IConsole>();
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
    }
}
