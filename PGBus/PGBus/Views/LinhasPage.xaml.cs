﻿using PGBus.Models;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace PGBus.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LinhasPage : StackLayout
    {
        public LinhasPage()
        {
            InitializeComponent();
        }

        private void ListView_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            var selected = e.Item as BusStopDescription;
        }
    }
}
