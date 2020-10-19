using BlackApp.Models.data;
using BlackApp.ModelViews.Configurator;
using BlackApp.Service;
using BlackApp.Service.DBinitializers;
using BlackApp.Views;
using BlackApp.Views.Configurator;
using BlackApp.Views.Settings;
using Rg.Plugins.Popup.Services;
using System;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace BlackApp
{
    public partial class App : Application
    {
        public static bool Updater = false;
        public static bool StartConfigurator = false;
        public static string SetMode;
        public static MainView MV;
        public App(bool startConfigurator)
        {
            StartConfigurator = startConfigurator;
            InitializeComponent();
            Plugin.InputKit.Shared.Controls.CheckBox.GlobalSetting.BorderColor = Color.Red;
            Plugin.InputKit.Shared.Controls.CheckBox.GlobalSetting.Size = 36;
            Plugin.InputKit.Shared.Controls.RadioButton.GlobalSetting.Color = Color.Red;
        }

        public async static Task<bool> CheckInternetConnection()
        {
            string CheckUrl = "http://google.com";

            try
            {
                HttpWebRequest iNetRequest = (HttpWebRequest)WebRequest.Create(CheckUrl);

                iNetRequest.Timeout = 5000;

                WebResponse iNetResponse = iNetRequest.GetResponse();

                iNetResponse.Close();

                return true;

            }
            catch (WebException ex)
            {
                return false;
            }
        }

        protected async override void OnStart()
        {
            if (StartConfigurator)
            {
                MainPage = new ProgramSelection();
            }
            else
            {
                //await StartUp();
                if(MV == null)
                { MainPage = MV = new MainView(); }
            }
        }

        protected override void OnSleep()
        {
            MV.cancellation.Cancel();
            LoadingWindowMW.Cancel.Cancel();
        }

        protected async override void OnResume()
        {
            Updater = true;
            MV.cancellation.Dispose();
            MV.cancellation = new CancellationTokenSource();
            Device.BeginInvokeOnMainThread(() => MV.StartTimer(5));

            LoadingWindowMW.Cancel.Dispose();
            LoadingWindowMW.Cancel = new CancellationTokenSource();
        }
    }
}
