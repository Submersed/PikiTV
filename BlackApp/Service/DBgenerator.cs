using BlackApp.Models;
using HtmlAgilityPack;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BlackApp.Service
{
    public class DBgenerator
    {
        public static HtmlWeb web = new HtmlWeb();
        public static string NapovednikUrl = "https://www.napovednik.com";
        public static string NapovednikTvSporediUrl = "https://www.napovednik.com/tv_sporedi";



        public static bool GenerateTvEvents(TvProgram tvProgram, List<HtmlDocument> hdocs, CancellationTokenSource token)
        {
            string Date = "";
            HtmlNode node;
            HtmlNodeCollection head;
            List<TvShow> tvShows = new List<TvShow>();
            TvEvent tvEvent;

            try
            {
                using (var dataContext = new TvGuideDBContext())
                {
                    Date = "";


                    tvShows = new List<TvShow>();
                    foreach (HtmlDocument doc in hdocs)
                    {
                        if (!token.IsCancellationRequested)
                        {
                            //var head = doc.DocumentNode.SelectNodes("//div[@class=\"TVlist\"]/dl/dd/a");
                            head = doc.DocumentNode.SelectNodes("//div[@class=\"TVlist\"]/dl/dd/a");
                            if (head == null)
                            {
                                head = doc.DocumentNode.SelectNodes("//div[@class=\"TVlist\"]/dl/dd");
                            }
                            Date = doc.DocumentNode.SelectSingleNode("//*[@id=\"datum\"]").Attributes[3].Value;

                            double prev = 0;
                            foreach (HtmlNode h in head)
                            {
                                node = h;
                                double cur = Double.Parse(h.ChildNodes[0].InnerText);
                                if (cur > prev)
                                {
                                    prev = cur;

                                    tvEvent = GenerateTvEvent(h, dataContext, tvProgram, Date);
                                    dataContext.Add(tvEvent);
                                    //dataContext.Entry(tvEvent).State = EntityState.Added;
                                    
                                }
                            }
                        }
                    }
                    if (!token.IsCancellationRequested)
                    {
                        dataContext.SaveChanges();
                    }
                    else
                    {
                        return false;
                    }
                }
                return true;
            }
            catch (Exception e)
            {
                return false;
                //Task.FromResult(false);
            }
            
        }

        public static TvEvent GenerateTvEvent(HtmlNode h, TvGuideDBContext dataContext, TvProgram tvProgram, string Date)
        {
            //tvShow.Title = h.ChildNodes[2].InnerText; ---- Naslov showa --- odstranjen
            TvEvent tvEvent = new TvEvent();
            tvEvent.TvShow = DBcommands.GetCorrectTvShow(h.ChildNodes[2].FirstChild.InnerText, dataContext);

            tvEvent.TvProgram = DBcommands.GetCorrectTvProgram(tvProgram, dataContext);
            tvEvent.ShowUrl = NapovednikUrl + h.Attributes[0].Value;

            //pri time pazi ker gre cez polnoc 

            //dodan time in date za izpis na sporedu
            string time = h.ChildNodes[0].InnerText;
            tvEvent.Time = time;

            //string date = doc.DocumentNode.SelectSingleNode("//*[@id=\"datum\"]").Attributes[3].Value;
            tvEvent.Date = Date;

            string Details = h.ChildNodes[2].LastChild.EndNode.InnerText;

            if (Details == "")
            {
                tvEvent.Details = h.ChildNodes[2].LastChild.InnerText;
            }


            tvEvent.AirDateTime = Parser.GetEventDateTime(Date, time);

            return tvEvent;
        }

        public static void GenerateTvPrograms()
        {
            using (var datacontext = new TvGuideDBContext())
            {
                web.OverrideEncoding = Encoding.UTF8;
                HtmlAgilityPack.HtmlDocument doc = web.Load(NapovednikTvSporediUrl);
                var head = doc.DocumentNode.SelectNodes("//div[@id=\"tvmenu\"]/a");
                if (head.Count != datacontext.tvPrograms.ToListAsync().Result.Count)
                {
                    foreach (var h in head)
                    {
                        TvProgram tvProgram = new TvProgram();
                        tvProgram.Url = NapovednikUrl + h.Attributes[0].Value;
                        tvProgram.Img = NapovednikUrl + h.ChildNodes[0].Attributes[0].Value;
                        tvProgram.Title = h.LastChild.InnerText;
                        tvProgram.Selected = false;
                        datacontext.tvPrograms.Add(DBcommands.GetCorrectTvProgram(tvProgram, datacontext));
                    }
                    datacontext.SaveChanges();
                }


            }
        }


    }
}
