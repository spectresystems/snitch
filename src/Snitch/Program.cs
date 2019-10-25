using Snitch.Commands;
using Spectre.Cli;
using System.Threading.Tasks;

namespace Snitch
{
    public static class Program
    {
        public static async Task<int> Main(string[] args)
        {
            var app = new CommandApp<AnalyzeCommand>();
            app.Configure(config =>
            {
                config.SetApplicationName("Snitch");
                config.EnableXmlDoc();
            });

            return await app.RunAsync(args);
        }
    }
}
