using System;
using System.Collections.Generic;
using System.Text;

namespace Snitch.Tests.Utilities
{
    public static class StringExtensions
    {
        public static string NormalizeLineEndings(this string value)
        {
            if (value != null)
            {
                value = value.Replace("\r\n", "\n");
                value = value.Replace("\r", string.Empty);
                return value.Replace("\n", "\r\n");
            }
            return string.Empty;
        }
    }
}
