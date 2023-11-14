using System;
using System.ComponentModel;
using Spectre.Console.Cli;

namespace Snitch.Commands
{
    [Description("Prints the Snitch version number")]
    public sealed class VersionCommand : Command
    {
        public override int Execute(CommandContext context)
        {
            Console.WriteLine(typeof(VersionCommand).Assembly.GetName().Version);
            return 0;
        }
    }
}