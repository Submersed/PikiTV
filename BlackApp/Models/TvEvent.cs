using System;
using System.Collections.Generic;
using System.Text;

namespace BlackApp.Models
{
    public class TvEvent
    {
        public int Id { get; set; }
        public TvProgram TvProgram { get; set; }
        public TvShow TvShow { get; set; }
        public string Details { get; set; }
        public DateTime AirDateTime { get; set; }

        public string Time { get; set; }
        public string Date { get; set; }
        public string ShowUrl { get; set; }
        //public string Time { get; set; } --- odstranjen
        public bool Seen { get; set; }

        public TvEvent(int id, TvProgram tvProgram, TvShow tvShow, string details, DateTime airDateTime, string showUrl , string time, string date)
        {
            this.Id = id;
            TvProgram = tvProgram;
            TvShow = tvShow;
            Details = details;
            AirDateTime = airDateTime;
            ShowUrl = showUrl;
            Time = time;
            Date = date;
            Seen = false;
        }

        public TvEvent()
        {

        }
    }
}
