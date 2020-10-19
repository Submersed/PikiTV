using BlackApp.Models.data;
using BlackApp.ModelViews.Configurator;
using BlackApp.Service;
using BlackApp.Views.Configurator;
using BlackApp.Views.Settings;
using Rg.Plugins.Popup.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    public partial class MainView : TabbedPage
    {
        public CancellationTokenSource cancellation = new CancellationTokenSource();
        public static bool test = false;
        public MainView()
        {
            BarBackgroundColor = Color.FromHex("#f9a602");
            InitializeComponent();
            App.Updater = true;
            StartTimer(5);

            // Don't repeat the timer (we will start a new timer when the request is finished)

            //spoofdata.FillEvents();
        }

        protected override bool OnBackButtonPressed()
        {
            if (Application.Current.MainPage is MainView mainPage)
            {

                if (PopupNavigation.Instance.PopupStack.Count > 0)
                {
                    if (!App.Updater)
                    {
                        Device.BeginInvokeOnMainThread(async () =>
                        {
                            if (PopupNavigation.Instance.PopupStack[0] is SetShows)
                            {
                                await (PopupNavigation.Instance.PopupStack[0] as SetShows).ConfirmChanges();
                            }
                            else if (PopupNavigation.Instance.PopupStack[0] is SetPrograms)
                            {
                                await (PopupNavigation.Instance.PopupStack[0] as SetPrograms).ConfirmChanges();
                            }
                            
                        });
                    }
                    return true;
                }
                else if (Navigation.NavigationStack.Count > 0)
                {
                    if (!App.Updater)
                    {
                        Device.BeginInvokeOnMainThread(async () =>
                        {
                            await Navigation.PopAsync();
                        });
                        return true;
                    }
                }
                else if (Navigation.ModalStack.Count > 0)
                {
                    if (!App.Updater)
                    {
                        Device.BeginInvokeOnMainThread(async () =>
                        {
                            await Navigation.PopModalAsync();
                        });
                        return true;
                    }
                }

                else
                {
                    Device.BeginInvokeOnMainThread(async () =>
                    {

                        var result = await this.DisplayAlert("alert!", "do you want to exit?", "yes", "no");
                        if (result)
                        {
                            Process.GetCurrentProcess().CloseMainWindow();
                            Process.GetCurrentProcess().Close();
                        }
                    });
                    return true;
                    //return base.OnBackButtonPressed();
                }
                return base.OnBackButtonPressed();
            }
            else
            {
                return true;
            }
        }

        public void StartTimer(int seconds)
        {

            CancellationTokenSource cts = this.cancellation;
            Device.StartTimer(TimeSpan.FromSeconds(seconds), () =>
            {
                App.Updater = true;
                if (cts.IsCancellationRequested)
                {
                    cancellation.Dispose(); // Clean up old token source....
                    cancellation = new CancellationTokenSource(); // "Reset" the cancellation token source...
                    return false;                                  
                    // Don't repeat the timer (we will start a new timer when the request is finished)
                }

                Task.Run(async () =>
                {

                    // Do the actual request and wait for it to finish.

                    // Switch back to the UI thread to update the UI
                    if (await CheckUpdate())
                    {
                        Device.BeginInvokeOnMainThread(() =>
                        {
                            // Update the UI
                            // ...
                            // Now repeat by scheduling a new request
                            PromptUpdate();
                        });
                    }
                });
                App.Updater = false;
                return false;
            });
            
        }

        public void PromptUpdate()
        {
            Device.BeginInvokeOnMainThread(async () =>
            {
                var answer = await DisplayAlert("New Update?", "Would you like to Update", "Yes", "No");
                if (answer)
                {
                    //LoadingWindowMW.LW = new LoadingWindow();
                    await Navigation.PushModalAsync(new LoadingWindow(), true);

                    if (LoadingWindowMW.Update())
                    {
                        await Navigation.PopModalAsync(true);
                    }
                    else
                    {
                        await Navigation.PopModalAsync(true);
                        await DisplayAlert("Prekinitev!", "Prekinitev pridobivanja podatkov", "Ok");
                    }
                }
                else
                {
                    StartTimer(300);
                }
            });
        }

        private async Task<bool> CheckUpdate()
        {

            var current = Connectivity.NetworkAccess;
            if (current == NetworkAccess.Internet)
            {
                int days = await LoadingWindowMW.CheckForLastUpdate();
                if (days != 7)
                {
                    return true;
                    //var answer = await DisplayAlert("New Update?", "Would you like to Update", "Yes", "No");
                    //if (answer)
                    //{
                    //    LoadingWindowMW.LW = new LoadingWindow();
                    //    App.Current.MainPage = LoadingWindowMW.LW;

                    //    if (await LoadingWindowMW.Update())
                    //    {
                    //        Application.Current.MainPage = new MainView();
                    //    }
                    //}

                }
            }
            return false;
        }

        public async Task PushNavigationsetPrograms()
        {
             await PopupNavigation.PushAsync(new SetPrograms());
        }
    }
}