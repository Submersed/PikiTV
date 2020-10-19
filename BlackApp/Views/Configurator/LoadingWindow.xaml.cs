using BlackApp.Models;
using BlackApp.Service;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace BlackApp.Views.Configurator
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LoadingWindow : ContentPage
    {
        private string Mode;
        private int currentprogram = 0;
        private List<TvProgram> AllPrograms;
        public LoadingWindow()
        {
            InitializeComponent();
        }

        public async Task Update()
        {
            Device.BeginInvokeOnMainThread(() => {
                this.UpdatingText.Text = Mode + $" {AllPrograms[currentprogram]}";
                currentprogram++;
            });

        }

        public async Task ConfigureUpdate(string mode,List<TvProgram> tvPrograms)
        {
            Device.BeginInvokeOnMainThread(() => {
                Mode = mode;
                AllPrograms = tvPrograms;
                currentprogram = 0;
            });

        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
        }

        protected override bool OnBackButtonPressed()
        {
            return true;
        }

    }
}