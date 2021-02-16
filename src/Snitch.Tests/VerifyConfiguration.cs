using Spectre.Verify.Extensions;
using System.Runtime.CompilerServices;
using VerifyTests;

namespace Sntich.Tests
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
