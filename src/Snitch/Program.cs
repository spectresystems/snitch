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
                config.ValidateExamples();

                config.SetApplicationName("snitch");
                config.AddExample(new[] { "Project.csproj" });
                config.AddExample(new[] { "Project.csproj", "--tfm", "net462" });
                config.AddExample(new[] { "Project.csproj", "--tfm", "net462", "--strict" });
            });

            return await app.RunAsync(args);
        }
    }
}
