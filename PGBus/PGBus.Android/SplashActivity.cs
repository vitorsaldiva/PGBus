using Android.App;
using Android.Content;
using Android.OS;
using System.Threading.Tasks;

namespace PGBus.Droid
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme.Splash", MainLauncher = true, NoHistory = true)]
    public class SplashActivity : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            //Iniciando mapa


        }

        protected override void OnResume()
        {
            base.OnResume();
            var startUp = new Task(() =>
            {
                var intent = new Intent(this, typeof(MainActivity));
                StartActivity(intent);
            });
            startUp.ContinueWith(t => Finish());
            startUp.Start();
        }

    }
}