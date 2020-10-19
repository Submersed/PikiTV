using BlackApp.Models;
using BlackApp.Models.data;
using BlackApp.ModelViews.Configurator;
using BlackApp.Service;
using BlackApp.Service.DBinitializers;
using BlackApp.Views.Configurator;
using Microsoft.EntityFrameworkCore;
using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace BlackApp.Views.Settings
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SetPrograms : PopupPage
    {
        public static ObservableCollection<TvProgram> tvprograms = new ObservableCollection<TvProgram>();
        public static TvGuideDBContext context = new TvGuideDBContext();
        public static List<TvProgram> OldTvPrograms = new List<TvProgram>();
        public static List<TvProgram> NewTvPrograms = new List<TvProgram>();
        public SetPrograms()
        {
            InitializeComponent();
            App.SetMode = "SetPrograms";
            context = new TvGuideDBContext();
            BindingContext = tvprograms;
            FillProgramsAsync();

        }

        private void FillProgramsAsync()
        {
            tvprograms.Clear();
            foreach (TvProgram tvprogram in context.tvPrograms.OrderBy(o => o.Title).ToList())
            {
                tvprograms.Add(tvprogram);
                if (tvprogram.Selected == true)
                {
                    OldTvPrograms.Add(tvprogram);
                }
            }
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
        }

        // ### Methods for supporting animations in your popup page ###

        // Invoked before an animation appearing
        protected override void OnAppearingAnimationBegin()
        {
            base.OnAppearingAnimationBegin();
        }

        // Invoked after an animation appearing
        protected override void OnAppearingAnimationEnd()
        {
            base.OnAppearingAnimationEnd();
        }

        // Invoked before an animation disappearing
        protected override void OnDisappearingAnimationBegin()
        {
            base.OnDisappearingAnimationBegin();
        }

        // Invoked after an animation disappearing
        protected override void OnDisappearingAnimationEnd()
        {
            base.OnDisappearingAnimationEnd();
            tvprograms.Clear();
        }

        protected override Task OnAppearingAnimationBeginAsync()
        {
            return base.OnAppearingAnimationBeginAsync();
        }

        protected override Task OnAppearingAnimationEndAsync()
        {
            return base.OnAppearingAnimationEndAsync();
        }

        protected override Task OnDisappearingAnimationBeginAsync()
        {
            return base.OnDisappearingAnimationBeginAsync();
        }

        protected override Task OnDisappearingAnimationEndAsync()
        {
            return base.OnDisappearingAnimationEndAsync();
        }

        // ### Overrided methods which can prevent closing a popup page ###

        // Invoked when a hardware back button is pressed
        protected override bool OnBackButtonPressed()
        {
            // Return true if you don't want to close this popup page when a back button is pressed
            Device.BeginInvokeOnMainThread(async () =>
            {
                await ConfirmChanges();
            });
            return true;
        }

        // Invoked when background is clicked
        protected override bool OnBackgroundClicked()
        {
            // Return false if you don't want to close this popup page when a background of the popup page is clicked
            return base.OnBackgroundClicked();
        }

        //private async void Button_Clicked(object sender, EventArgs e)
        //{
        //    await PopupNavigation.PopAsync(true);
        //}

        private async void Exit_Clicked(object sender, EventArgs e)
        {
            await ConfirmChanges();
        }


        public async Task ConfirmChanges()
        {
            if (NewTvPrograms.Count > 0)
            {
                string listofprograms = "";

                foreach (TvProgram tvProgram in NewTvPrograms)
                {
                    listofprograms = listofprograms + tvProgram.Title + "\n";
                }
                listofprograms = listofprograms + "\n" + "Potrebna je posodobitev!";

                var result = await DisplayAlert("Novi TvProgrami!", listofprograms, "Potrdi", "Preklici");

                if (result == true && App.CheckInternetConnection().Result)
                {
                    context.SaveChanges();
                    PopupNavigation.Instance.PopAsync();

                    await App.Current.MainPage.Navigation.PushModalAsync(LoadingWindowMW.LW = new LoadingWindow(), true);

                    if (LoadingWindowMW.Update())
                    {
                        NewTvPrograms.Clear();
                    }
                    else
                    {
                        NewTvPrograms.ForEach(x => x.Selected = false);
                        NewTvPrograms.Clear();
                        context.SaveChanges();
                        Device.BeginInvokeOnMainThread(async () => await DisplayAlert("Napaka!", listofprograms, "Potrdi", "Preklici"));
                    }
                    await TvProgramView.UpdateList();
                    Device.BeginInvokeOnMainThread(async () => await App.Current.MainPage.Navigation.PopModalAsync());

                }
                else
                {
                    List<TvProgram> selectedlist = new List<TvProgram>();

                    if (context == null)
                    {
                        context = new TvGuideDBContext();
                    }

                    foreach (TvProgram tv in NewTvPrograms)
                    {
                        tv.Selected = false;
                    }
                    NewTvPrograms.Clear();

                    //context.SaveChanges();
                    selectedlist = context.tvPrograms.Where(x => x.Selected == true).ToListAsync().Result;

                    if (selectedlist.Count > 0)
                    {
                        TvProgramView.viewablecollection.Clear();
                        TvShowView.viewablecollection.Clear();
                        TvProgramView.UpdateList();

                        await PopupNavigation.Instance.PopAsync();

                    }
                    else
                    {
                        FillProgramsAsync();
                        DisplayAlert("Prazen seznam", "Prosim izberi Tv program-e in potrdi se enkrat", "Cancel");
                    }
                }
            }
            else if (context.tvPrograms.Where(x => x.Selected == true).ToListAsync().Result.Count > 0)
            {
                TvProgramView.UpdateList();
                Device.BeginInvokeOnMainThread(async () => await PopupNavigation.Instance.PopAsync());
            }
            else
            {
                DisplayAlert("Prazen seznam", "Prosim izberi Tv program-e in potrdi se enkrat", "Cancel");
            }
        }

        private void OnClose(object sender, EventArgs e)
        {
            PopupNavigation.Instance.PopAsync();
        }

        private async void CheckBox_CheckChanged(object sender, EventArgs e)
        {
            TvProgram tvProgram = context.tvPrograms.Where(x => x.Title == (sender as Plugin.InputKit.Shared.Controls.CheckBox).Text).FirstOrDefault();
            if (tvProgram != null && tvProgram.Selected != false)
            {
                if (!OldTvPrograms.Contains(tvProgram))
                {
                    NewTvPrograms.Add(tvProgram);
                }
            }
            else if (tvProgram.Selected == false)
            {
                NewTvPrograms.Remove(tvProgram);
            }
            //context.SaveChanges();
        }
    }
}