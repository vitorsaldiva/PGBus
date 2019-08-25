using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using PGBus.Views;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]
namespace PGBus
{
    public partial class App : Application
    {

        public App()
        {
            InitializeComponent();


            MainPage = new MainPage();

#if DEBUG
            HotReloader.Current.Run(this, new HotReloader.Configuration
            {
                DeviceUrlPort = 8001
            });
#endif
        }

        protected override void OnStart()
        {
            // Handle when your app starts
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }
}
