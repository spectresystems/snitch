using System.Threading.Tasks;
using Snitch.Commands;
using Spectre.Cli;

namespace Snitch
{
    public static class Program
    {
        public static async Task<int> Main(string[] args)
        {
            var app = new CommandApp<AnalyzeCommand>();
            app.Configure(config =>
            {
                config.SetApplicationName("snitch");

                config.UseStrictParsing();
                config.ValidateExamples();

                config.AddExample(new[] { "Project.csproj" });
                config.AddExample(new[] { "Project.csproj", "-e", "Foo", "-e", "Bar" });
                config.AddExample(new[] { "Project.csproj", "--tfm", "net462" });
                config.AddExample(new[] { "Project.csproj", "--tfm", "net462", "--strict" });

                config.AddCommand<VersionCommand>("version");
            });

            return await app.RunAsync(args);
        }
    }
}
