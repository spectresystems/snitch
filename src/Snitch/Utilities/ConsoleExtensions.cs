using System;
using System.Collections.Generic;
using System.Text;

namespace Snitch
{
    internal static class ConsoleExtensions
    {
        public static void WriteLine(this IConsole output)
        {
            output?.WriteLine(string.Empty);
        }
    }
}
