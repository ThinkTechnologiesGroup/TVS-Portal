using System;
using System.Collections.Generic;
using ThinkVoipTool.Skyswitch;

namespace ThinkVoipTool.Billing
{
    internal class Billing
    {
        private List<Month> _lastSixMonths;
        


        private string _monthUrl =
            "https://pbx.skyswitch.com/ns-api/?type=Off-net&object=cdr2&action=count&format=json&domain=AdmiralsCove.22335.service&range_interval=5%20HOUR&end_date=2021-01-31%2023:59:59&start_date=2021-01-01%000:00:00";

        private SkySwitchToken _token;
        


        public Billing()
        {
            _lastSixMonths = new List<Month>();
            _token = new SkySwitchToken();
            var today = DateTime.Today;
            var currentMonth = DateTime.Today.Month;

            for (var i = 0; i < 6; i++)
            {
                var monthNum = currentMonth - i;
                if(monthNum <= 0)
                {
                    monthNum = 12 + monthNum;
                }
                var month = new Month(monthNum);
                
               
                _lastSixMonths.Add(month);
            }
            
            
        }

        public IEnumerable<Month> LastSixMonths => _lastSixMonths;



    }
}