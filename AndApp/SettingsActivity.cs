using Android.App;
using Android.Content;
using Android.OS;
using Android.Preferences;
using Android.Runtime;

namespace WeatherApp
{
    [Activity(Label = "@string/settings", Theme = "@android:style/Theme.Material", ParentActivity = typeof(MainActivity))]
    public class SettingsActivity : PreferenceActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {        
            base.OnCreate(savedInstanceState);
            ServiceControl.StopAlarmService();
            AddPreferencesFromResource(Resource.Drawable.Settings);
            ISharedPreferences d = PreferenceManager.GetDefaultSharedPreferences(this);    
        }
    }
}
