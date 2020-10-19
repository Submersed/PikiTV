using BlackApp.Models;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace BlackApp.Service
{
    public class TvGuideDBContext : DbContext
    {
        private static bool _BaseNotExists = true;
        private const string databaseName = "TvGuide.db";
        private static string databasePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), databaseName);
        public TvGuideDBContext()
        {
            if (_BaseNotExists)
            {
                //bool i = Database.EnsureDeleted();

                bool _BaseNotExists = Database.EnsureCreated();

                if (!_BaseNotExists)
                {
                    //Initializer();
                }
                else
                {
                    //Updater();
                }
            }
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionbuilder)
        {
            optionbuilder.UseSqlite(@"Data Source=" + @databasePath);
            optionbuilder.EnableSensitiveDataLogging();
            //optionbuilder.Options 
            //using (SqliteCommand cmd = c.CreateCommand())
            //{
            //    cmd.CommandText = "PRAGMA encoding = \"UTF-16\"";
            //    cmd.ExecuteNonQuery();
            //}
        }


        public DbSet<TvShow> TvShows { get; set; }
        public DbSet<TvProgram> tvPrograms { get; set; }
        public DbSet<TvEvent> tvEvents { get; set; }
        public DbSet<TvUpdateLog> TvUpdateLogs { get; set; }


    }
}
