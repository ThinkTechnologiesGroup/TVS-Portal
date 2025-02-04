﻿using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace ThinkVoipTool
{
    public static class MyExtensions
    {
        private static readonly Dictionary<long, long> History = new Dictionary<long, long>();
        public static string StripDomain(this string messyString) => Regex.Replace(messyString, @"(.*)\\|@(.*)", string.Empty);

        public static string CleanUpMacAddress(this string messyString) => Regex.Replace(messyString, @"(-|:*)", string.Empty);

        public static int CountThis(this string countThis) => Regex.Matches(countThis, @"a|e|i|o|u").Count;


        public static long FibOfRecursive(long n)
        {
            if(n <= 1)
            {
                return 1;
            }

            if(History.ContainsKey(n))
            {
                return History[n];
            }

            History.Add(n, FibOfRecursive(n - 1) + FibOfRecursive(n - 2));

            return History[n];
        }


        public static long ReturnFibOfWithLoops(long n)
        {
            long previousFib = 0;
            long currentFib = 1;
            long newFib = 1;
            for (var i = 1; i <= n; i++)
            {
                newFib = previousFib + currentFib;
                previousFib = currentFib;
                currentFib = newFib;
            }

            return newFib;
        }
    }
}