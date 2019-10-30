using System;

namespace Snitch
{
    public interface IConsole
    {
        ConsoleColor ForegroundColor { get; set; }
        ConsoleColor BackgroundColor { get; set; }

        void ResetColor();

        void Write(string format, params object[] args);
        void WriteLine(string format, params object[] args);
    }
}
