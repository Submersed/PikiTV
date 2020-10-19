using System;
using System.Collections.Generic;
using System.Text;

namespace BlackApp.Models
{
    public class DailyEvents
    {
        public DateTime AirDateTime { get; set; }
        public int Height { get; set; }
        public List<TvEvent> TvEvents { get; set; }

        public DailyEvents()
        {
            TvEvents = new List<TvEvent>();
        }
    }
}
