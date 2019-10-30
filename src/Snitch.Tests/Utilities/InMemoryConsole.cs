using System;
using System.IO;

namespace Snitch.Tests.Utilities
{
    public sealed class InMemoryConsole : IConsole
    {
        private readonly StringWriter _writer;

        public ConsoleColor ForegroundColor { get; set; }
        public ConsoleColor BackgroundColor { get; set; }

        public InMemoryConsole()
        {
            _writer = new StringWriter();
        }

        public void ResetColor()
        {
        }

        public void Write(string format, params object[] args)
        {
            _writer.Write(format, args);
        }

        public void WriteLine(string format, params object[] args)
        {
            _writer.WriteLine(format, args);
        }

        public string GetOutput()
        {
            _writer.Flush();
            return _writer.ToString().NormalizeLineEndings().Trim();
        }
    }
}
