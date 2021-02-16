using Spectre.Console;
using Spectre.Console.Rendering;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Snitch.Tests.Utilities
{
    public sealed class FakeConsole : IAnsiConsole, IDisposable
    {
        public Profile Profile { get; }
        public IAnsiConsoleCursor Cursor => new FakeAnsiConsoleCursor();
        IAnsiConsoleInput IAnsiConsole.Input => Input;
        public RenderPipeline Pipeline { get; }

        public FakeConsoleInput Input { get; }
        public string Output => Profile.Out.ToString();
        public IReadOnlyList<string> Lines => Output.TrimEnd('\n').Split(new char[] { '\n' });

        public FakeConsole(
            int width = 80, int height = 9000, Encoding encoding = null,
            bool supportsAnsi = true, ColorSystem colorSystem = ColorSystem.Standard,
            bool legacyConsole = false, bool interactive = true)
        {
            Input = new FakeConsoleInput();
            Pipeline = new RenderPipeline();

            Profile = new Profile(new StringWriter(), encoding ?? Encoding.UTF8);
            Profile.Width = width;
            Profile.Height = height;
            Profile.ColorSystem = colorSystem;
            Profile.Capabilities.Ansi = supportsAnsi;
            Profile.Capabilities.Legacy = legacyConsole;
            Profile.Capabilities.Interactive = interactive;
            Profile.Capabilities.Links = true;
        }

        public void Dispose()
        {
            Profile.Out.Dispose();
        }

        public void Clear(bool home)
        {
        }

        public void Write(IEnumerable<Segment> segments)
        {
            if (segments is null)
            {
                return;
            }

            foreach (var segment in segments)
            {
                Profile.Out.Write(segment.Text);
            }
        }
    }
}
