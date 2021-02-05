using System;

namespace ThinkVoipTool.Billing
{
    internal enum Months
    {
        January = 1,
        February = 2,
        March = 3,
        April = 4,
        May = 5,
        June = 6,
        July = 7,
        August = 8,
        September = 9,
        October = 10,
        November = 11,
        December = 12
    }

    public class Month
    {
        private readonly string _monthNumber;
        private readonly string _name;
        private readonly string _startDay = "1";
        private readonly string _year;
        private string _callsMade;
        private string _lastDay;
        private string _minutesUsed;


        public Month(int monthNumber)
        {
            _monthNumber = monthNumber.ToString();
            _name = ((Months) monthNumber).ToString();
            _lastDay = DateTime.DaysInMonth(DateTime.Today.Year, monthNumber).ToString();
            _year = DateTime.Now.AddMonths(-monthNumber + 1).Year.ToString();
        }


        public string LastDay
        {
            get => _lastDay;
            set => _lastDay = value;
        }

        public string Name => _name;
        public string Year => _year;
        public string StartDay => _startDay;
        public string MonthNumber => _monthNumber;

        public string MinutesUsed
        {
            get => _minutesUsed;
            set => _minutesUsed = value;
        }

        public string CallsMade
        {
            get => _callsMade;
            set => _callsMade = value;
        }
    }
}