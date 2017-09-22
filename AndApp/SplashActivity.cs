using Android.App;
using Android.Content;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Widget;
using System.Threading.Tasks;


namespace WeatherApp
{
    [Activity(Label = "@string/app_name", Theme = "@android:style/Theme.NoTitleBar", MainLauncher = true, NoHistory = true)]
    public class SplashActivity : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.Splash);
            AnimationDrawable animation = (AnimationDrawable)FindViewById<ImageView>(Resource.Id.logo).Drawable;
            animation.Start();
        }

        protected override void OnResume()
        {
            base.OnResume();
            Task startupWork = new Task(() => { SimulateStartup(); });
            startupWork.Start();
        }

        protected override void OnRestart()
        {
            base.OnRestart();
            StartActivity(new Intent(Application.Context, typeof(MainActivity)));
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