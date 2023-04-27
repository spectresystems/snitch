using System.Runtime.CompilerServices;
using VerifyTests;

namespace Snitch.Tests
{
    public static class VerifyConfiguration
    {
        [ModuleInitializer]
        public static void Init()
        {
            VerifierSettings.DerivePathInfo(Expectations.Initialize);
        }
    }
}
