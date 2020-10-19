using BlackApp.Models;
using BlackApp.Models.data;
using BlackApp.ModelViews.Configurator;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlackApp.Service.DBinitializers
{
    public static class DBInitializer
    {
        public static async Task Initializer()
        {
            using (var datacontext = new TvGuideDBContext())
            {
                DBgenerator.GenerateTvPrograms();
            }
        }

        public static async Task<bool> Updater()
        {
            using (var datacontext = new TvGuideDBContext())
            {
                int days = 7;

                if (datacontext.TvUpdateLogs.ToListAsync().Result.Count > 0)
                {
                    var updatelogs = datacontext.TvUpdateLogs.ToListAsync().Result;
                    updatelogs.FindLast(x => x.UpdateTime.DayOfYear < DateTime.Now.DayOfYear || x.UpdateTime.Year < DateTime.Now.Year);

                    double result = (DateTime.Now - updatelogs[0].UpdateTime).TotalDays;
                    int tempdays = Convert.ToInt32(result);

                    if (tempdays < days)
                    {
                        days = tempdays;
                    }
                }

                //check selected

                List<TvProgram> listofselected = datacontext.tvPrograms.Where(x => x.Selected == true).ToListAsync().Result;



                if (listofselected.Count > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }

            }
        }

        public static async Task<bool> ExistingSelectedPrograms()
        {
            using (var datacontext = new TvGuideDBContext())
            {
                if (datacontext.tvPrograms.Where(x => x.Selected == true).ToListAsync().Result.Count > 0)
                {
                    return true;
                }
                return false;
            }
        }

        public static bool VerifyDatabase()
        {
            if (CleanUpDatabase())
            {
                using (var datacontext = new TvGuideDBContext())
                {
                    if (datacontext.tvPrograms.ToListAsync().Result.Count() > 0 && datacontext.tvEvents.ToListAsync().Result.Count() > 0 && datacontext.tvEvents.ToListAsync().Result.Count() > 0)
                    {

                        return false;
                    }
                    else
                    {

                        return true;
                    }
                }
            }
            return false;
        }

        public static bool CleanUpDatabase()
        {
            using (var datacontext = new TvGuideDBContext())
            {
                List<TvEvent> tvEvents = datacontext.tvEvents.OrderBy(x => x.AirDateTime).ToList();
                if (tvEvents.Count() > 0)
                {
                    DateTime today = DateTime.Now;
                    double days = (tvEvents.Last().AirDateTime - tvEvents.First().AirDateTime).Days + 1;

                    if (days > 7)
                    {

                        List<TvShow> tvShows = new List<TvShow>();

                        datacontext.tvEvents
                        .Include(x => x.TvShow).Include(x => x.TvProgram).Where(x => x.TvProgram.Selected == true)
                        .OrderBy(x => x.AirDateTime).Where(x => x.AirDateTime.Day >= today.Day && x.AirDateTime.Month >= today.Month && x.AirDateTime.Year >= today.Year)
                        .Select(x => x.TvShow).Distinct().OrderBy(x => x.Title).ForEachAsync(x => tvShows.Add(x));

                        datacontext.tvEvents
                        .OrderBy(x => x.AirDateTime).Where(x => x.AirDateTime.Day < today.Day && x.AirDateTime.Month <= today.Month && x.AirDateTime.Year <= today.Year)
                        .ForEachAsync(x => { datacontext.tvEvents.Remove(x); datacontext.SaveChanges(); });

                        datacontext.TvShows.ForEachAsync(x => { if (!tvShows.Contains(x)) { datacontext.TvShows.Remove(x); datacontext.SaveChanges(); } });

                        foreach (TvProgram tvProgram in datacontext.tvEvents.Include(x => x.TvProgram).Where(x => x.TvProgram.Selected == true).Select(x => x.TvProgram).ToList())
                        {
                            List<TvEvent> tvEvents1 = datacontext.tvEvents.Include(x => x.TvProgram).Where(x => x.TvProgram == tvProgram).OrderBy(x => x.AirDateTime).ToList();
                            TvEvent tvEvent = tvEvents1.LastOrDefault();
                            List<TvEvent> tvEventsCheck = tvEvents1.Where(x => x.AirDateTime.Day == today.Day && x.AirDateTime.Month == today.Month && x.AirDateTime.Year == today.Year).ToList();
                            if (tvEventsCheck.Count() < 16 && tvEventsCheck.Count() > 0)
                            {
                                tvEventsCheck.ForEach(x => { datacontext.tvEvents.Remove(x); datacontext.SaveChanges(); });
                            }
                        }
                    }
                }
            }
            return true;
        }

        //public static bool fixEntrysInDB()
        //{
        //    DateTime today = DateTime.Now;
        //    Dictionary<TvProgram, int> needPatch = new Dictionary<TvProgram,int>();

        //    using (var datacontext = new TvGuideDBContext())
        //    {
        //        foreach (TvProgram tvProgram in datacontext.tvEvents.Include(x => x.TvProgram).Where(x => x.TvProgram.Selected == true).Select(x => x.TvProgram).ToList())
        //        {
        //            List<TvEvent> tvEvents1 = datacontext.tvEvents.Include(x => x.TvProgram).Where(x => x.TvProgram == tvProgram).OrderBy(x => x.AirDateTime).ToList();
        //            //TvEvent tvEvent = tvEvents1.LastOrDefault();
        //            List<TvEvent> tvEventsCheck = tvEvents1.Where(x => x.AirDateTime.Day == today.Day && x.AirDateTime.Month == today.Month && x.AirDateTime.Year == today.Year).ToList();
        //            if (tvEventsCheck.Count() < 16 && tvEventsCheck.Count() > 0)
        //            {
        //                tvEventsCheck.ForEach(x => { datacontext.tvEvents.Remove(x); datacontext.SaveChanges(); });
        //                //need patching dodej dictionary z programi in day = 0 updejtaj in posli true
        //            }
        //        }


        //    }
        //}


        public static async Task<Patch> CheckForUpdates(List<TvProgram> listofselected)
        {
            int days = 0;

            using (var datacontext = new TvGuideDBContext())
            {
                if (listofselected.Count == 0)
                {
                    listofselected = datacontext.tvPrograms.Where(x => x.Selected == true).ToListAsync().Result;
                }

                if (datacontext.TvUpdateLogs.ToListAsync().Result.Count > 0)
                {
                    var updatelogs = datacontext.TvUpdateLogs.ToListAsync().Result;

                    var test = updatelogs.Where(x => x.UpdateTime.DayOfYear < DateTime.Now.DayOfYear || x.UpdateTime.Year < DateTime.Now.Year).LastOrDefault();

                    if (test != null)
                    {
                        double result = (DateTime.Now - test.UpdateTime).TotalDays;
                        int tempdays = Convert.ToInt32(result);

                        if (tempdays < days)
                        {
                            days = tempdays;
                        }
                    }
                    else
                    {
                        days = 0;
                    }
                }
                else 
                {
                    days = 7;
                }
                datacontext.TvUpdateLogs.Add(new TvUpdateLog() { UpdateTime = DateTime.Now });
                datacontext.SaveChanges();
                
            }
            return new Patch() { Tvprograms = listofselected, Days = days };
        }

        public static async Task<Patch> CheckForUpdates()
        {
            List<TvProgram> listofselected = new List<TvProgram>();
            int days = 0;

            using (var datacontext = new TvGuideDBContext())
            {
                if (listofselected.Count == 0)
                {
                    listofselected = datacontext.tvPrograms.Where(x => x.Selected == true).ToListAsync().Result;
                }

                if (datacontext.TvUpdateLogs.ToListAsync().Result.Count > 0)
                {
                    var updatelogs = datacontext.TvUpdateLogs.ToListAsync().Result;

                    var test = updatelogs.Where(x => x.UpdateTime.DayOfYear < DateTime.Now.DayOfYear || x.UpdateTime.Year < DateTime.Now.Year).LastOrDefault();

                    if (test != null)
                    {
                        double result = (DateTime.Now - test.UpdateTime).TotalDays;
                        int tempdays = Convert.ToInt32(result);

                        if (tempdays < days)
                        {
                            days = tempdays;
                        }
                    }
                    else
                    {
                        days = 0;
                    }
                }
                else
                {
                    days = 7;
                }
                datacontext.TvUpdateLogs.Add(new TvUpdateLog() { UpdateTime = DateTime.Now });
                datacontext.SaveChanges();

            }
            return new Patch() { Tvprograms = listofselected, Days = days };
        }
    }
}
