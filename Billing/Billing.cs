using System;
using System.Collections.Generic;
using ThinkVoipTool.Skyswitch;

namespace ThinkVoipTool.Billing
{
    internal class Billing
    {
        private string _day;
        private string _dayUrl;
        private string _lastMonthUrl;
        private List<Month> _lastSixMonths;
        private string _month;


        private string _monthUrl =
            "https://pbx.skyswitch.com/ns-api/?type=Off-net&object=cdr2&action=count&format=json&domain=AdmiralsCove.22335.service&range_interval=5%20HOUR&end_date=2021-01-31%2023:59:59&start_date=2021-01-01%000:00:00";

        private SkySwitchToken _token;
        private string _year;


        public Billing(string clientHostUrl)
        {
            _token = new SkySwitchToken();
            var today = DateTime.Today;
            var currentMonth = DateTime.Today.Month;
            var year = today.Year;

            for (var i = 0; i < 6; i++)
            {
                var month = new Month(currentMonth - i);

                _lastSixMonths.Add(month);
            }


            var endOfMonth = new DateTime(today.Year,
                today.Month,
                DateTime.DaysInMonth(today.Year,
                    today.Month));
        }


        public static int MonthEnd(int month)
        {
            var today = DateTime.Today;

            var endOfMonth = new DateTime(today.Year,
                today.Month,
                DateTime.DaysInMonth(today.Year,
                    today.Month));
            return 1;
        }
    }
}