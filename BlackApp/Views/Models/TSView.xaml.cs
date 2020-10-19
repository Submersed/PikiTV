using BlackApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace BlackApp.Views.Models
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class TSView : ContentView
    {
        public TSView(TvEvent tvEvent)
        {
            InitializeComponent();
            BindingContext = tvEvent;
        }
    }
}