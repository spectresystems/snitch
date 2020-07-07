using Snitch.Analysis;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Snitch.Tests
{
    public sealed class PackageTests
    {
        [Fact]
        public void Lol()
        {
            Package p = new Package("Foo", "4.9.3");

            Assert.Equal("4.9.3", p.GetVersionString());
        }
    }
}
