using System;
using System.Collections.Generic;
using System.Text;

namespace BlackApp.Models.data
{
    public class spoofdata
    {
        public static List<TvEvent> tvEvents = new List<TvEvent>();

        public static List<DailyEvents> tvEventTrees = new List<DailyEvents>();
        public spoofdata()
        {
            //tvEvents = FillEvents();
        }
        public static List<DailyEvents> FillEvents()
        {
            List<TvEvent> tvEvents = new List<TvEvent>();
            TvShow tvShow1 = new TvShow();
            tvShow1.Title = "Tvshow1";
            TvShow tvShow2 = new TvShow();
            tvShow2.Title = "Tvshow2";

            TvProgram tvProgram1 = new TvProgram();
            tvProgram1.Title = "TvProgram1";
            tvProgram1.Url = "url1";
            TvProgram tvProgram2 = new TvProgram();
            tvProgram2.Title = "TvProgram2";
            tvProgram2.Url = "url2";

            TvEvent tvEvent1 = new TvEvent();
            tvEvent1.TvProgram = tvProgram1;
            tvEvent1.TvShow = tvShow1;
            tvEvent1.Time = "5:30";
            tvEvent1.Details = "tota episoda 1";
            tvEvent1.AirDateTime = DateTime.Parse("11/11/2019 00:00:00");

            TvEvent tvEvent2 = new TvEvent();
            tvEvent2.TvProgram = tvProgram2;
            tvEvent2.TvShow = tvShow2;
            tvEvent2.Time = "7:30";
            tvEvent2.Details = "tota episoda 2";
            tvEvent2.AirDateTime = DateTime.Parse("12/11/2019 00:00:00");

            tvEvents.Add(tvEvent1);
            tvEvents.Add(tvEvent2);

            
            DailyEvents tvEventTree1 = new DailyEvents();
            tvEventTree1.AirDateTime = DateTime.Parse("01/01/2019 00:00:00");
            tvEventTree1.TvEvents = tvEvents;

            DailyEvents tvEventTree2 = new DailyEvents();
            tvEventTree2.AirDateTime = DateTime.Parse("02/02/2019 00:00:00");
            tvEventTree2.TvEvents = tvEvents;

            tvEventTrees.Add(tvEventTree1);
            tvEventTrees.Add(tvEventTree2);

            return tvEventTrees;
        }
    }
}
