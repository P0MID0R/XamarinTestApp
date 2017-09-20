using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Graphics.Drawables;
using System.Threading;
using Android.Animation;
using System.Threading.Tasks;
using Android.Support.V7.App;

namespace WeatherApp
{
    [Activity(Label = "@string/app_name", Theme = "@android:style/Theme.NoTitleBar", MainLauncher = true, NoHistory = true)]
    public class SplashActivity : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.Splash);
            //StartActivity(new Intent(Application.Context, typeof(MainActivity)));
        }

        public override void OnWindowFocusChanged(bool hasFocus)
        {
                ImageView imageView = FindViewById<ImageView>(Resource.Id.loading_animation);
                AnimationDrawable animation = (AnimationDrawable)imageView.Drawable;
                animation.Start();
        }

        protected override void OnResume()
        {
            base.OnResume();
            Task startupWork = new Task(() => { SimulateStartup(); });
            startupWork.Start();
        }

        private async void SimulateStartup()
        {
            await Task.Delay(2000);
            StartActivity(new Intent(Application.Context, typeof(MainActivity)));
        }
    }
}