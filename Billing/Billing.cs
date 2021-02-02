using System;
using System.Collections.Generic;

namespace ThinkVoipTool.Billing
{
    internal class Billing
    {
        private readonly List<(Month, Usage)> _lastSixMonths;

        public Billing(string clientUrl)
        {
            _lastSixMonths = new List<(Month, Usage)>();
            var currentMonth = DateTime.Today.Month;
            for (var i = 0; i < 6; i++)
            {
                var monthNum = currentMonth - i;
                if(monthNum <= 0)
                {
                    monthNum = 12 + monthNum;
                }

                var month = new Month(monthNum);
                _lastSixMonths.Add((month, new Usage(month, clientUrl)));
            }
        }

        public IEnumerable<(Month, Usage)> LastSixMonths => _lastSixMonths;
    }
}