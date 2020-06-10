using Android.Content;
using PGBus.Droid.CustomRenderer;
using PGBus.Renderers;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(CustomEntryRenderer), typeof(CustomEntry))]
namespace PGBus.Droid.CustomRenderer
{

    public class CustomEntry : EntryRenderer
    {
        public CustomEntry(Context context) : base(context)
        {
        }

        protected override void OnElementChanged(ElementChangedEventArgs<Entry> e)
        {
            base.OnElementChanged(e);
            Control?.SetBackground(null);

        }
    }
}