using Android.App;
using Android.Content;
using Android.OS;
using Android.Preferences;

namespace WeatherApp
{
    [Activity(Label = "@string/settings", Theme = "@android:style/Theme.Material")]
    public class SettingsActivity : PreferenceActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            AddPreferencesFromResource(Resource.Drawable.Settings);
            ISharedPreferences d = PreferenceManager.GetDefaultSharedPreferences(this);
            string data = d.GetString("pref_default_country", "Minsk");
        }
    }
}
