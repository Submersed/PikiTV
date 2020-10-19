using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.V7.App;
using Android.Util;
using Android.Views;
using Android.Widget;
using BlackApp.Service;
using BlackApp.Service.DBinitializers;

namespace BlackApp.Droid
{
    [Activity(Theme = "@style/MyTheme.Splash", MainLauncher = true, NoHistory = true)]
    public class SplashActivity : AppCompatActivity
    {
        public static bool StartInstaller = true; 
        static readonly string TAG = "X:" + typeof(SplashActivity).Name;

        private const string databaseName = "TvGuide.db";
        private static string dbspecial = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
        private static string databasePath = Path.Combine(dbspecial, databaseName);

        public override void OnCreate(Bundle savedInstanceState, PersistableBundle persistentState)
        {
            base.OnCreate(savedInstanceState, persistentState);
            this.Window.AddFlags(WindowManagerFlags.Fullscreen);
            this.Window.AddFlags(WindowManagerFlags.KeepScreenOn);
            this.Window.SetFlags(WindowManagerFlags.KeepScreenOn, WindowManagerFlags.KeepScreenOn);
            Log.Debug(TAG, "SplashActivity.OnCreate");
        }

        // Launches the startup task
        protected override void OnResume()
        {
            base.OnResume();
            Task startupWork = new Task(() => { SimulateStartup(); });
            startupWork.Start();
        }

        // Prevent the back button from canceling the startup process
        public override void OnBackPressed() { }

        // Simulates background work that happens behind the splash screen
        async void SimulateStartup()
        {
            Log.Debug(TAG, "Performing some startup work that takes a bit of time.");

            try
            {
                if (!ExistingDB().Result)
                {

                    if (App.CheckInternetConnection().Result)
                    {
                        await DBInitializer.Initializer();
                        StartInstaller = true;
                    }
                    else
                    {
                        System.Diagnostics.Process.GetCurrentProcess().CloseMainWindow();
                    }
                }
                else
                {
                    //DeleteDB();1
                    //await DBInitializer.Initializer();

                    StartInstaller = DBInitializer.VerifyDatabase();
                    if (StartInstaller)
                    {
                        if (App.CheckInternetConnection().Result)
                        {
                            DeleteDB();
                            await DBInitializer.Initializer();
                        }
                        else
                        {
                            System.Diagnostics.Process.GetCurrentProcess().CloseMainWindow();
                        }
                    }

                    //if (await DBInitializer.VerifyDatabase())
                    //{
                    //    await DBInitializer.Updater();
                    //}
                    //else
                    //{
                    //    await DBInitializer.Initializer();
                    //}
                }

                Log.Debug(TAG, "Startup work is finished - starting MainActivity.");
                StartActivity(new Intent(Application.Context, typeof(MainActivity)));
            }
            catch
            {

            }

        }

        //check if file exists
        private async Task<bool> ExistingDB()
        {
            if (File.Exists(databasePath))
            {
                return true;
            }
            else
            {
                if (!Directory.Exists(dbspecial))
                {
                    Directory.CreateDirectory(dbspecial);
                }
                return false;
            }

        }

        private async void DeleteDB()
        {
            if (File.Exists(databasePath))
            {
                File.Delete(databasePath);
            }
        }
    }
}