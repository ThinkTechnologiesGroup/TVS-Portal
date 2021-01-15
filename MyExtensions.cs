using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace ThinkVoipTool
{
    public static class MyExtensions
    {
        public static string StripDomain(this string messyString) => Regex.Replace(messyString, @"(.*)\\|@(.*)", string.Empty);



    }
}
