using BlackApp.Models;
using BlackApp.Service;
using Microsoft.EntityFrameworkCore;
using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace BlackApp.Views.Settings
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SetShows : PopupPage
    {
        public static ObservableCollection<TvShow> tvShows = new ObservableCollection<TvShow>();
        public static TvGuideDBContext context = new TvGuideDBContext();

        public SetShows()
        {
            InitializeComponent();
            App.SetMode = "SetShows";
            tvShows.Clear();
            context = new TvGuideDBContext();

            BindingContext = tvShows;

            DateTime today = DateTime.Now;

            context.tvEvents
                .Include(x => x.TvShow).Include(x => x.TvProgram).Where(x => x.TvProgram.Selected == true)
                .OrderBy(x => x.AirDateTime).Where(x => x.AirDateTime.Day >= today.Day && x.AirDateTime.Month >= today.Month && x.AirDateTime.Year >= today.Year)
                .Select(x => x.TvShow).Distinct().OrderBy(x => x.Title).ForEachAsync(x => tvShows.Add(x));
        }

        private void OnItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            var item = e.SelectedItem as TvShow;

            if (item != null)
            {
                item.Selected = !item.Selected;

                using (var context = new TvGuideDBContext())
                {
                    context.Entry(item).State = EntityState.Modified;
                    context.SaveChanges();
                }
            }
        }
        protected override bool OnBackButtonPressed()
        {
            // Return true if you don't want to close this popup page when a back button is pressed
            Device.BeginInvokeOnMainThread(async () =>
            {
                await ConfirmChanges();
            });
            return true;
        }

        protected override void OnDisappearingAnimationEnd()
        {
            base.OnDisappearingAnimationEnd();
        }

        private async void Exit_Clicked(object sender, EventArgs e)
        {
            ConfirmChanges();
        }

        public async Task ConfirmChanges()
        {
            List<TvShow> selectedlist = new List<TvShow>();


            if (context == null)
            {
                context = new TvGuideDBContext();
            }

            context.SaveChanges();
            TvShowView.pickerdata.Clear();

            selectedlist = context.TvShows.Where(x => x.Selected == true).ToListAsync().Result;

            if (selectedlist.Count > 0)
            {
                TvShowView.UpdateList(context);
                PopupNavigation.Instance.PopAsync();
            }
            else
            {
                DisplayAlert("Prazen seznam", "Prosim izberi Tv program-e in potrdi se enkrat", "Cancel");
            }
        }
    }
}