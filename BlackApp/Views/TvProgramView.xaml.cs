using BlackApp.Models;
using BlackApp.Models.data;
using BlackApp.ModelViews.Configurator;
using BlackApp.Service;
using BlackApp.Views.Configurator;
using BlackApp.Views.Models;
using BlackApp.Views.Settings;
using Microsoft.EntityFrameworkCore;
using Rg.Plugins.Popup.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace BlackApp.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class TvProgramView : ContentPage
    {
        DateTime today = DateTime.Now;
        private int currentevent = 0;
        public static ObservableCollection<DailyEvents> viewablecollection = new ObservableCollection<DailyEvents>();
        public static ObservableCollection<TvProgram> pickerdata = new ObservableCollection<TvProgram>();
        private static StackLayout stacky = new StackLayout();
        private TvEvent temptpview = new TvEvent();
        private string tempdate;
        private List<TvEvent> selectedprograms;
        public TvProgramView()
        {
            InitializeComponent();
            Load();
        }

        private void Load()
        {
            ListOfPrograms.ItemsSource = pickerdata;

            using (var context = new TvGuideDBContext())
            {

                var selectedprograms = context.tvPrograms.Where(x => x.Selected == true).ToList();

                if (selectedprograms.Count == 0)
                {
                    ListOfPrograms.IsEnabled = false;
                }
                else
                {
                    UpdateList();
                    ListOfPrograms.IsEnabled = true;
                }
            }
        }


        private async void ToolbarItem_Clicked(object sender, EventArgs e)
        {
            if (!App.Updater)
            {
                (sender as ToolbarItem).IsEnabled = false;
                await App.MV.PushNavigationsetPrograms();
                (sender as ToolbarItem).IsEnabled = true;
            }
        }

        public static async Task UpdateList()
        {
            using (var context = new TvGuideDBContext())
            {
                var selectedprograms = context.tvPrograms.Where(x => x.Selected == true).OrderBy(x => x.Title).ToList();

                pickerdata.Clear();

                if (selectedprograms.Count == 0)
                {
                    PopupNavigation.PushAsync(new SetPrograms());
                }
                else
                {
                    selectedprograms.ToList().ForEach(pickerdata.Add);
                }
            }
        }

        private void ListOfPrograms_SelectedIndexChanged(object sender, EventArgs e)
        {
            Skrolko.ScrollToAsync(0, 0, true);
            stacky = new StackLayout();
            Skrolko.Content = stacky;
            Picker picker = new Picker();
            picker = sender as Picker;


            viewablecollection.Clear();
            stacky.Children.Clear();
            using (var context = new TvGuideDBContext())
            {

                today = DateTime.Now;
                selectedprograms = context.tvEvents.Include(x => x.TvProgram)
                    .Include(x => x.TvShow).Where(x => x.TvProgram == (picker.SelectedItem as TvProgram))
                    .OrderBy(x => x.AirDateTime).Where(x => x.AirDateTime.Day >= today.Day && x.AirDateTime.Month >= today.Month && x.AirDateTime.Year >= today.Year)
                    .ToList();
                currentevent = 0;

                if (selectedprograms.Count > 0)
                {
                    tempdate = selectedprograms[0].Date;

                    temptpview = selectedprograms.Where(x => x.AirDateTime <= today && (today - x.AirDateTime).Hours < 4).OrderBy(x => x.AirDateTime).LastOrDefault();
                    //temptpview = selectedprograms.Where(x => x.AirDateTime <= today).OrderBy(x => x.AirDateTime).Last();

                    stacky.Children.Add(new DateView(selectedprograms[0]));

                    foreach (TvEvent tvEvent in selectedprograms)
                    {
                        TPView tv = new TPView(tvEvent);
                        if (tvEvent.Date == tempdate)
                        {
                            if (temptpview != null && tvEvent == temptpview)
                            {
                                tv.BackgroundColor = new Color(0, 179, 60);
                                temptpview = null;
                            }
                            stacky.Children.Add(tv);

                            currentevent++;
                        }
                        else
                        {
                            break;
                        }
                    }

                    ListOfPrograms.IsEnabled = true;
                }
            }
        }

        private async void AddDay()
        {
            if (currentevent < selectedprograms.Count())
            {
                tempdate = selectedprograms[currentevent].Date;
                stacky.Children.Add(new DateView(selectedprograms[currentevent]));

                for (; currentevent < selectedprograms.Count(); currentevent++)
                {
                    if (selectedprograms[currentevent].Date == tempdate)
                    {
                        TPView tempview = new TPView(selectedprograms[currentevent]);
                        if (temptpview != null)
                        {
                            if (selectedprograms[currentevent].AirDateTime <= today && (today - selectedprograms[currentevent].AirDateTime).Hours < 4)
                            {
                                tempview.BackgroundColor = new Color(0, 179, 60);
                                temptpview = null;
                            }
                        }

                        stacky.Children.Add(tempview);
                        if (currentevent + 1 == selectedprograms.Count())
                        {
                            await Skrolko.ScrollToAsync(0, Skrolko.ContentSize.Height, true);
                        }
                    }
                    else
                    {
                        await Skrolko.ScrollToAsync(0, Skrolko.ContentSize.Height, true);
                        break;
                    }
                }
            }
        }

       
        private void Skrolko_Scrolled(object sender, ScrolledEventArgs e)
        {
            ScrollView scrollView = sender as ScrollView;
            double scrollingSpace = scrollView.ContentSize.Height - scrollView.Height;
            
            if (scrollingSpace <= e.ScrollY)
            {
                AddDay();
            }
        }
    }
}