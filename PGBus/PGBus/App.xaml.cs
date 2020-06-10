using PGBus.Views;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]
namespace PGBus
{
    public partial class App : Application
    {

        public App()
        {
            InitializeComponent();

#if DEBUG
            HotReloader.Current.Run(this, new HotReloader.Configuration
            {
                DeviceUrlPort = 8001
            });
#endif

            MainPage = new MainPage();


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
