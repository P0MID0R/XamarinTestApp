using Android.App;
using Android.Widget;
using Android.Content;
using Android.OS;
using System;
using System.Net.Http;
using System.Threading;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Android.Runtime;
using Android.Views;
using Android.Media;

namespace AndApp
{
    [Activity(Label = "AndApp", MainLauncher = true)]
    public class MainActivity : Activity
    {

        protected override void OnCreate(Bundle savedInstanceState)
        {

            base.OnCreate(savedInstanceState);


            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            Button StartButton = FindViewById<Button>(Resource.Id.Start);
            EditText inputText = FindViewById<EditText>(Resource.Id.inputText);

           

            string Url = "http://api.openweathermap.org/data/2.5/weather?q=";
            string appid = "&APPID=a4dcc6d4ef65f67ade104ecb98972b41";
            string City = String.Empty;
            string contents;

            StartButton.Click += async (object sender, EventArgs e) =>
            {
                HttpClient hc = new HttpClient();
                City = inputText.Text;
                contents = await hc.GetStringAsync(Url + City + appid);
                var res = JObject.Parse(contents);
                double temp = double.Parse((string)res["main"]["temp"]) - 273.15;
              

                Toast.MakeText(ApplicationContext, "Погода в " + inputText.Text + " сейчас: " + temp.ToString() + "C", ToastLength.Long).Show();

                inputText.Text = string.Empty;
            };

        }

        public override bool OnKeyUp([GeneratedEnum] Keycode keyCode, KeyEvent e)
        {


            if (keyCode == Android.Views.Keycode.VolumeUp)
            {

                Toast.MakeText(ApplicationContext, "Вверх", ToastLength.Long).Show();
                return true;
            }

            if (keyCode == Android.Views.Keycode.VolumeDown)
            {

                Toast.MakeText(ApplicationContext, "Вниз", ToastLength.Long).Show();
                return true;
            }

            return base.OnKeyUp(keyCode, e);
        }
    }

}

