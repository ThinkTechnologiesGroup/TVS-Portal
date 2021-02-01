using System;

namespace ThinkVoipTool.Billing
{
    public enum Months
    {
        January = 1,
        February = 2,
        March = 3,
        April = 4,
        Mat = 5,
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
        private const string StartDay = "1";
        private readonly string _lastDay;
        private readonly string _name;

        public Month(int monthNumber)
        {
            _name = ((Months) monthNumber).ToString();
            _lastDay = DateTime.DaysInMonth(DateTime.Today.Year, monthNumber).ToString();
        }

        public string LastDay => _lastDay;
        public string Name => _name;
    }
}