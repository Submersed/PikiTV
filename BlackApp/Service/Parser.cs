using System;
using System.Collections.Generic;
using System.Text;

namespace BlackApp.Service
{
    public class Parser
    {
        public static DateTime GetEventDateTime(string date, string time)
        {
            //string datum = "28.11.2019";
            string[] dateSplit = date.Split('.');
            int year = int.Parse(dateSplit[2]);
            int month = int.Parse(dateSplit[1]);
            int day = int.Parse(dateSplit[0]);
            //string time = "23.00";
            string[] timeSplit = time.Split('.');
            int hour = int.Parse(timeSplit[0]);
            int minute = int.Parse(timeSplit[1]);
            DateTime dateTime = new DateTime(year, month, day, hour, minute, 0);
            return dateTime;
            //int i = dateTime.DayOfYear;
        }
    }
}
