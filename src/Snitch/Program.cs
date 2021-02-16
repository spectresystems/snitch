using System;
using System.Threading.Tasks;
using Snitch.Commands;
using Snitch.Utilities;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Snitch
{
    public static class Program
    {
        public static async Task<int> Main(string[] args)
        {
            return await Run(args);
        }

        public static async Task<int> Run(string[] args, Action<IConfigurator>? configator = null)
        {
            var app = new CommandApp(new TypeRegistrar());

            app.SetDefaultCommand<AnalyzeCommand>();
            app.Configure(config =>
            {
                config.SetApplicationName("snitch");
                configator?.Invoke(config);

                config.UseStrictParsing();
                config.ValidateExamples();

                config.AddExample(new[] { "Project.csproj" });
                config.AddExample(new[] { "Project.csproj", "-e", "Foo", "-e", "Bar" });
                config.AddExample(new[] { "Project.csproj", "--tfm", "net462" });
                config.AddExample(new[] { "Project.csproj", "--tfm", "net462", "--strict" });

                config.AddExample(new[] { "Solution.sln" });
                config.AddExample(new[] { "Solution.sln", "-e", "Foo", "-e", "Bar" });
                config.AddExample(new[] { "Solution.sln", "--tfm", "net462" });
                config.AddExample(new[] { "Solution.sln", "--tfm", "net462", "--strict" });

                config.AddCommand<VersionCommand>("version");
            });

            return await app.RunAsync(args);
        }
    }
}
