using BlackApp.Models;
using BlackApp.Service;
using BlackApp.Views.Models;
using BlackApp.Views.Settings;
using Microsoft.EntityFrameworkCore;
using Rg.Plugins.Popup.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace BlackApp.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class TvShowView : ContentPage
    {
        public static ObservableCollection<DailyEvents> viewablecollection = new ObservableCollection<DailyEvents>();
        public static ObservableCollection<TvShow> pickerdata = new ObservableCollection<TvShow>();
        public static Picker Picker = new Picker();
        private static StackLayout stacky = new StackLayout();
        public TvShowView()
        {
            InitializeComponent();
            //Content.BindingContext = viewablecollection;
            Load();
        }

        public void Load()
        {
            Picker = ListOfShows;
            ListOfShows.ItemsSource = pickerdata;

            using (var context = new TvGuideDBContext())
            {
                var selectedprograms = context.TvShows.Where(x => x.Selected == true).ToList();

                if (selectedprograms.Count == 0)
                {
                    ListOfShows.IsEnabled = false;
                }
                else
                {
                    UpdateList(context);
                    ListOfShows.IsEnabled = true;
                }
            }
        }

        private async void ToolbarItem_Clicked(object sender, EventArgs e)
        {
            (sender as ToolbarItem).IsEnabled = false;
            await PopupNavigation.PushAsync(new SetShows());
            (sender as ToolbarItem).IsEnabled = true;

        }

        public static void UpdateList(TvGuideDBContext context)
        {
            if (context == null)
            { context = new TvGuideDBContext(); }
            using (context)
            {

                var selectedprograms = context.TvShows.Where(x => x.Selected == true).OrderBy(x => x.Title).ToList();

                pickerdata.Clear();

                if (selectedprograms.Count == 0)
                {
                    if (context.tvEvents.ToList().Count > 0)
                    {
                        PopupNavigation.PushAsync(new SetShows());
                    }
                }
                else
                {
                    selectedprograms.ToList().ForEach(pickerdata.Add);
                    Picker.IsEnabled = true;
                }
            }
        }

        private void ListOfShows_SelectedIndexChanged(object sender, EventArgs e)
        {
            bool active = true;
            Skrolko.ScrollToAsync(0, 0, true);
            stacky = new StackLayout();
            Skrolko.Content = stacky;
            Picker picker = new Picker();
            picker = sender as Picker;

            if (picker.SelectedItem != null)
            {
                viewablecollection.Clear();
                test.Children.Clear();
                using (var context = new TvGuideDBContext())
                {
                    DateTime today = DateTime.Now;

                    var selectedprograms = context.tvEvents.Include(x => x.TvProgram).Include(x => x.TvShow).Where(x => x.TvShow.Id == (picker.SelectedItem as TvShow).Id).OrderBy(x => x.AirDateTime >= DateTime.Now).ToList();

                    if (selectedprograms.Count > 0)
                    {
                        string tempdate = selectedprograms[0].Date;

                        stacky.Children.Add(new DateView(selectedprograms[0]));

                        foreach (TvEvent tvEvent in selectedprograms)
                        {
                            TSView tv = new TSView(tvEvent);
                            if (tvEvent.Date != tempdate)
                            {
                                tempdate = tvEvent.Date;
                                stacky.Children.Add(new DateView(tvEvent));

                            }
                            else
                            {
                                if (active && tvEvent.AirDateTime > today)
                                {
                                    tv.BackgroundColor = new Color(0, 179, 60);
                                    active = false;
                                }
                                stacky.Children.Add(tv);
                            }
                        }
                        ListOfShows.IsEnabled = true;
                    }
                }
            }
        }
    }
}