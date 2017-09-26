using Android.App;
using Android.Content;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Preferences;
using Android.Widget;
using System.Threading.Tasks;


namespace WeatherApp
{
    [Activity(Label = "@string/app_name", 
        Theme = "@android:style/Theme.NoTitleBar", 
        MainLauncher = true, NoHistory = true)]
    public class SplashActivity : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            ISharedPreferences d = PreferenceManager.GetDefaultSharedPreferences(this);
            if (d.GetBoolean("pref_loading_show", true))
            {
                SetContentView(Resource.Layout.Splash);
                AnimationDrawable animation = (AnimationDrawable)FindViewById<ImageView>(Resource.Id.logo).Drawable;
                animation.Start();
            }
            else
            {
                SetContentView(Resource.Layout.Splash);
                StartActivity(new Intent(Application.Context, typeof(MainActivity)));
            }
        }

        protected override void OnResume()
        {
            base.OnResume();
            ISharedPreferences d = PreferenceManager.GetDefaultSharedPreferences(this);
            if (d.GetBoolean("pref_loading_show", true))
            {
                Task startupWork = new Task(() => { SimulateStartup(); });
                startupWork.Start();
            }
        }

        protected override void OnRestart()
        {
            base.OnRestart();
            //StartActivity(new Intent(Application.Context, typeof(MainActivity)));
        }

        protected override void OnPause()
        {
            base.OnPause();
        }

        private async void SimulateStartup()
        {
            await Task.Delay(1500);
            StartActivity(new Intent(Application.Context, typeof(MainActivity)));
        }
    }
}