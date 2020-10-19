using BlackApp.Models;
using BlackApp.Service;
using BlackApp.Views.Configurator;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Xamarin.Forms;

namespace BlackApp.ModelViews.Configurator
{
    public static class LoadingWindowMW
    {
        private static System.Timers.Timer _timer = new System.Timers.Timer();
        public static LoadingWindow LW;
        public static CancellationTokenSource Cancel = new CancellationTokenSource();

        public static async Task<int> CheckForLastUpdate()
        {
            int day = 7;
            using (var context = new TvGuideDBContext())
            {
                TvUpdateLog resultlog = context.TvUpdateLogs.ToListAsync().Result.LastOrDefault();
                DateTime today = DateTime.UtcNow;

                if (resultlog != null && resultlog.UpdateTime.Day < today.Day && resultlog.UpdateTime.Month <= today.Month && resultlog.UpdateTime.Year <= today.Year)
                {
                    double result = (today - resultlog.UpdateTime).TotalDays;
                    int tempdays = Convert.ToInt32(result);
                    day = 7 - tempdays;

                    if (tempdays >= 7)
                    {
                        return day = 0;
                    }
                }
                else
                {
                    return day;
                }
            }
            return day;
        }

        public static Dictionary<TvProgram,int> UpdateCheck()
        {
            int tempdays;
            double cur;

            Dictionary<TvProgram, int> listofprograms = new Dictionary<TvProgram, int>();

            using (var context = new TvGuideDBContext())
            {
                List<TvProgram> TvPrograms = context.tvPrograms.Where(x => x.Selected == true).ToList();
                DateTime today = DateTime.Now;
                if (TvPrograms.Count == 0)
                {
                    Application.Current.MainPage = new ProgramSelection();
                }
                else 
                {
                    foreach (TvProgram tvProgram in TvPrograms)
                    {
                        List<TvEvent> tvEvents = context.tvEvents.Include(x => x.TvProgram).Where(x => x.TvProgram == tvProgram).OrderBy(x => x.AirDateTime).ToList();
                        TvEvent tvEvent = tvEvents.LastOrDefault();

                        if (tvEvent != null)
                        {
                            cur = (tvEvent.AirDateTime - today).Days + 1;
                            if (cur >= 1 && cur < 7)
                            {
                                listofprograms.Add(tvProgram, Convert.ToInt32(cur));
                            }
                            else
                            {
                                if (cur <= 0)
                                {
                                    listofprograms.Add(tvProgram, 0);
                                }
                            }
                        }
                        else
                        {
                            listofprograms.Add(tvProgram, 0);
                        }
                    }
                }
            }
            return listofprograms;
        }

        public static bool Update()
        {
            try
            {
                Dictionary<TvProgram, int> resultUpdateCheck = UpdateCheck();
                using (var context = new TvGuideDBContext())
                {
                    int days = 0;
                    if (UpdatingEvents(resultUpdateCheck))
                    {
                        context.TvUpdateLogs.Add(new TvUpdateLog() { UpdateTime = DateTime.Now });
                        context.SaveChanges();
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            catch
            {
                return false;
            }
        }

        private static bool UpdatingEvents(Dictionary<TvProgram, int> listofprograms)
        {
            try
            {
                if (listofprograms.Count > 0)
                {
                    HtmlAgilityPack.HtmlWeb web = new HtmlAgilityPack.HtmlWeb();

                    web.OverrideEncoding = Encoding.UTF8;
                    List<HtmlAgilityPack.HtmlDocument> htmldocs = new List<HtmlAgilityPack.HtmlDocument>();

                    foreach (TvProgram tvProgram in listofprograms.Keys)
                    {

                        htmldocs = new List<HtmlAgilityPack.HtmlDocument>();
#if DEBUG
                        for (int day1 = listofprograms.Where(x => x.Key == tvProgram).Select(x => x.Value).First(); day1< 7; day1++)
                        {
                            if (!Cancel.IsCancellationRequested)
                            {
                                HtmlAgilityPack.HtmlDocument doc = web.Load(string.Format("{0}/d/{1}", tvProgram.Url, day1));
                                htmldocs.Add(doc);
                            }
                            else 
                            {
                                return false;
                            }
                        }
#endif
#if RELEASE
                        for (int day1 = listofprograms.Where(x => x.Key == tvProgram).Select(x => x.Value).First(); day1 < 7; day1++)
                        {
                                if (!Cancel.IsCancellationRequested)
                                {
                                    HtmlAgilityPack.HtmlDocument doc = web.Load(string.Format("{0}/d/{1}", tvProgram.Url, day1));
                                    htmldocs.Add(doc);
                                }
                                else 
                                {
                                    return false;
                                }
                        }
#endif


                        if (!DBgenerator.GenerateTvEvents(tvProgram, htmldocs, Cancel))
                        {
                            return false;
                        }
                    }
                    return true;
                }
                else
                {
                    return true;
                }

            }
            catch
            {
                return false;
            }
        }
    }
}

