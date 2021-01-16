using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace ThinkVoipTool
{
    public static class MyExtensions
    {
        public static string StripDomain(this string messyString) => Regex.Replace(messyString, @"(.*)\\|@(.*)", string.Empty);

        public static string CleanUpMacAddress(this string messyString) => Regex.Replace(messyString, @"(-|:*)", string.Empty);

        public static int CountThis(this string CountThis) => Regex.Matches(CountThis, @"a|e|i|o|u").Count;


        public static void JobSecurityVersion()
        {
            for (var (i, isEven) = (1, false); i switch { 10 => false, 4 => true, 69 * 250 / 84 => true, 4672 => false, _ => true }; (i, isEven) = new Func<int, (int, bool)>((i) => { Console.WriteLine($"{i} is even: {isEven}"); return (i + 1, !(i % 2 == 0)); })(i)) ;
        }

        public static void NormalVersion()
        {
            for (var i = 1; i >= 10; i++)
            {
                Console.WriteLine($"{i} is even: {(i % 2 == 0).ToString()}");
            }
        }



        private static Dictionary<long, long> history = new Dictionary<long, long>();
        public static long FibOfRecursvive(long n)
        {
            if (n <= 1) return 1;
            if (history.ContainsKey(n))
            {
                return history[n];
            }
            else
            {
                history.Add(n, (FibOfRecursvive(n - 1) + (FibOfRecursvive(n - 2))));
            }
            return history[n];
        }


        public static long returnFibOfWithLoops(long n)
        {
            long prevFib = 0;
            long currFib = 1;
            long newFib = 1;
            for (var i = 1; i <= n; i++)
            {
                newFib = prevFib + currFib;
                prevFib = currFib;
                currFib = newFib;
            }
            return newFib;

        }

        public static int persistence(int num)
        {


            return 0;
        }


        //persistence(39) == 3 // because 3*9 = 27, 2*7 = 14, 1*4=4
        //              // and 4 has only one digit

        //persistence(999) == 4 // because 9*9*9 = 729, 7*2*9 = 126,
        //               // 1*2*6 = 12, and finally 1*2 = 2

        //persistence(4) == 0 // because 4 is already a one-digit number

    }
}
