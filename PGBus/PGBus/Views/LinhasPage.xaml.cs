using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace PGBus.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LinhasPage : ListView
    {
        

        public LinhasPage()
        {
            InitializeComponent();
        }

        async void Handle_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            if (e.Item == null)
                return;


            //Deselect Item
            ((ListView)sender).SelectedItem = null;
        }
    }
}
