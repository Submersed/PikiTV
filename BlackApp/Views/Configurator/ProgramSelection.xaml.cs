using BlackApp.Models;
using BlackApp.Models.data;
using BlackApp.ModelViews.Configurator;
using BlackApp.Service;
using BlackApp.Service.DBinitializers;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace BlackApp.Views.Configurator
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ProgramSelection : ContentPage
    {
        public static ObservableCollection<TvProgram> tvprograms = new ObservableCollection<TvProgram>();
        public static TvGuideDBContext context = new TvGuideDBContext();
        public static List<TvProgram> AddTvPrograms = new List<TvProgram>();



        public ProgramSelection()
        {
            InitializeComponent();
            context = new TvGuideDBContext();
            if (App.CheckInternetConnection().Result)
            {
                BindingContext = tvprograms;
                foreach (TvProgram tvprogram in context.tvPrograms.OrderBy(o => o.Title).ToList())
                {
                    tvprograms.Add(tvprogram);
                }
            }
            else
            {
                if (DisplayAlert("Ni povezave", "Prosim povezi se na internet in poskusi ponovno", "Ok").IsCompleted)
                {
                    System.Diagnostics.Process.GetCurrentProcess().CloseMainWindow();
                }   
            }
            
        }

        private async void Button_Clicked(object sender, EventArgs e)
        {
            if (AddTvPrograms.Count > 0)
            {

                await Navigation.PushModalAsync(LoadingWindowMW.LW = new LoadingWindow());


                 if (LoadingWindowMW.Update())
                {
                    Device.BeginInvokeOnMainThread(async () => await Navigation.PopModalAsync(true));

                    if (App.MV == null)
                    {
                        App.MV = new MainView();
                    }
                    Application.Current.MainPage = App.MV;
                }
                else 
                {
                    Device.BeginInvokeOnMainThread(async () => await Navigation.PopModalAsync(true));
                    Device.BeginInvokeOnMainThread(async () => await DisplayAlert("Prekinitev!", "Prekinjeno pridobivanje podatkov", "Cancel"));
                }
                
            }
            else
            {
                Device.BeginInvokeOnMainThread(async () => await DisplayAlert("Prazen seznam", "Prosim izberi Tv program-e in potrdi se enkrat", "Cancel"));
            }
        }

        public async void SetLoadingWindow()
        {
            LoadingWindowMW.LW = new LoadingWindow();
            await Navigation.PushModalAsync(LoadingWindowMW.LW);
        }

        private async void CheckBox_CheckChanged(object sender, EventArgs e)
        {
            TvProgram tvProgram = context.tvPrograms.Where(x => x.Title == (sender as Plugin.InputKit.Shared.Controls.CheckBox).Text).FirstOrDefault();
            if (tvProgram != null && tvProgram.Selected != false)
            {
                AddTvPrograms.Add(tvProgram);
            }
            else if (tvProgram.Selected == false)
            {
                AddTvPrograms.Remove(tvProgram);
            }
            context.SaveChanges();
        }
    }

}