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
