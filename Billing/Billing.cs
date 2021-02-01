using System;
using ThinkVoipTool.Skyswitch;

namespace ThinkVoipTool.Billing
{
    internal class Billing
    {
        private DateTime _dateTime;
        private string _day;
        private string _dayUrl;
        private string _lastMonthUrl;
        private string _month;

        private string _monthUrl =
            $"https://pbx.skyswitch.com/ns-api/?type=Off-net&object=cdr2&action=count&format=json&domain=AdmiralsCove.22335.service&range_interval=5%20HOUR&end_date=2021-01-31%2023:59:59&start_date=2021-01-01%000:00:00";

        private SkySwitchToken _token;
        private string _year;


        public Billing(string clientHostUrl)
        {
            _token = new SkySwitchToken();
            _dateTime = DateTime.Now;
            var today = DateTime.Today;
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

        //public int MinutesUsedCurrentMonth()
        //{
            //Test Change.


        //}
    }
}