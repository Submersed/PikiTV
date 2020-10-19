using BlackApp.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BlackApp.Service
{
    public static class DBcommands
    {
        public static TvShow GetCorrectTvShow(string title, TvGuideDBContext dataContext)
        {
            TvShow result = dataContext.TvShows.ToList().FirstOrDefault(s => s.Title == title);
            if (result == null)
            {

                dataContext.Entry(new TvShow() { Title = title }).State = EntityState.Added;
                dataContext.SaveChanges();
                return dataContext.TvShows.ToList().FirstOrDefault(s => s.Title == title);
            }
            else
            {
                return result;
            }
        }

        public static TvProgram GetCorrectTvProgram(TvProgram tvProgram, TvGuideDBContext dataContext)
        {
            TvProgram result = dataContext.tvPrograms.ToList().FirstOrDefault(s => s.Title == tvProgram.Title);
            if (result == null)
            {
                return tvProgram;
            }
            else
            {
                return result;
            }
        }

        public static List<TvEvent> GetEventByTvShow(TvShow tvShow)
        {
            using (var dataContext = new TvGuideDBContext())
            {
                TvShow result = GetCorrectTvShow(tvShow.Title, dataContext);

                List<TvEvent> tvEvents = dataContext.tvEvents.ToList().FindAll(s => s.TvShow == tvShow);
                if (tvEvents == null)
                {
                    return null;
                }
                else
                {
                    return tvEvents;
                }
            }
        }

        public static List<TvEvent> GetEventByTvProgram(TvProgram tvProgram)
        {
            using (var dataContext = new TvGuideDBContext())
            {
                TvProgram result = GetCorrectTvProgram(tvProgram, dataContext);

                List<TvEvent> tvEvents = dataContext.tvEvents.ToList().FindAll(s => s.TvProgram == tvProgram);
                if (tvEvents == null)
                {
                    return null;
                }
                else
                {
                    return tvEvents;
                }
            }
        }
    }
}