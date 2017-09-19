using Android.App;
using Android.Widget;
using Android.Content;
using Android.OS;
using System;
using System.Net.Http;
using System.Net;
using System.Threading;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Android.Runtime;
using Android.Views;
using Android.Media;
using System.Collections.Generic;
using SQLite;
using SQLitePCL;
using System.Threading.Tasks;
using Android.Locations;
using System.Linq;
using Android.Graphics;
using Android.Preferences;

namespace WeatherApp
{
    [Activity(Label = "WeatherApp", Icon = "@drawable/Icon", MainLauncher = true)]
    public class MainActivity : Activity
    {
        string path =  System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "localAppDB.db");

        protected override void OnCreate(Bundle savedInstanceState)
        {

            base.OnCreate(savedInstanceState);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            Button StartButton = FindViewById<Button>(Resource.Id.Start);
            Button Log_button = FindViewById<Button>(Resource.Id.Log_button);
            EditText inputText = FindViewById<EditText>(Resource.Id.inputText);

            connectDatabase(path);

            ISharedPreferences d = PreferenceManager.GetDefaultSharedPreferences(this);
            string defaultCity = d.GetString("pref_default_country", "");

            if (defaultCity != "")
            {
                if (!showResult(defaultCity, GetWeatherData(defaultCity)))
                    Toast.MakeText(ApplicationContext, "Error", ToastLength.Long).Show();
            }
            else
                Toast.MakeText(ApplicationContext, "Please select default city in options", ToastLength.Long).Show();

            StartButton.Click += (object sender, EventArgs e) =>
           {
               try
               {
                   if (!showResult(inputText.Text, GetWeatherData(inputText.Text)))
                   {
                       Toast.MakeText(ApplicationContext, "Error", ToastLength.Long).Show();
                   }
                   inputText.Text = string.Empty;
               }
               catch
               {
                   Toast.MakeText(ApplicationContext, "Error", ToastLength.Long).Show();
               }
           };
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Drawable.op_menu, menu);
            return true;
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Resource.Id.Log_button:
                    {
                        var callLog = new Intent(this, typeof(LogActivity));
                        StartActivity(callLog);
                        return true;
                    }
                case Resource.Id.settings_button:
                    {
                        var callLog = new Intent(this, typeof(SettingsActivity));
                        StartActivity(callLog);
                        return true;
                    }
            }
            return base.OnOptionsItemSelected(item);
        }

        private string connectDatabase(string path)
        {
            try
            {
                var connection = new SQLiteConnection(path);
                {
                    connection.CreateTable<LogDB>();
                    return "Database created";
                }
            }
            catch (SQLiteException ex)
            {
                return ex.Message;
            }
        }

        private string insertData(LogDB data, string path)
        {
            try
            {
                var db = new SQLiteAsyncConnection(path);
                db.InsertAsync(data);
                return "Single data file inserted or updated";
            }
            catch (SQLiteException ex)
            {
                return ex.Message;
            }
        }

        public JObject GetWeatherData(string City)
        {
            try
            {
                string key = "a4dcc6d4ef65f67ade104ecb98972b41";
                string Url = "http://api.openweathermap.org/data/2.5/weather?q=" + City + "&appid=" + key;
                HttpClient client = new HttpClient();
                var response = client.GetStringAsync(Url).Result;
                return JObject.Parse(response);
            }
            catch
            {
                return null;
            }
        }

        public bool showResult(string InputCity, JObject JsonInput)
        {

            try
            {
                ImageView weathericon = FindViewById<ImageView>(Resource.Id.imageView1);
                ListView LogList = FindViewById<ListView>(Resource.Id.listView1);
                LinearLayout imageLayout = FindViewById<LinearLayout>(Resource.Id.linearLayout1);
                imageLayout.Visibility = ViewStates.Invisible;
                TextView CityName = FindViewById<TextView>(Resource.Id.textView1);
                CityName.Visibility = ViewStates.Invisible;

                string JSONtemp = (string)JsonInput["main"]["temp"];
                double temp = Convert.ToDouble(JSONtemp.Replace('.', ',')) - 273.15;

                Toast.MakeText(ApplicationContext, "Wheather in " + InputCity + " now: " + temp.ToString() + "C", ToastLength.Long).Show();

                string icon = (string)JsonInput["weather"][0]["icon"];
                var imageBitmap = GetImageBitmapFromUrl("http://openweathermap.org/img/w/" + icon + ".png");
                weathericon.SetImageBitmap(imageBitmap);
                CityName.Text = (string)JsonInput["name"] + " " + temp.ToString() + "C ( " + (string)JsonInput["weather"][0]["main"] + " )";
                imageLayout.Visibility = ViewStates.Visible;
                CityName.Visibility = ViewStates.Visible;

                LogDB currentdata = new LogDB
                {
                    City = (string)JsonInput["name"],
                    date = DateTime.Now,
                    temp = Convert.ToDouble(JSONtemp.Replace('.', ',')) - 273.15,
                    icon = (string)JsonInput["weather"][0]["icon"]
                };
                insertData(currentdata, path);
                return true;
            }
            catch
            {
                return false;
            }

        }

        public Bitmap GetImageBitmapFromUrl(string url)
        {
            Bitmap imageBitmap = null;

            using (var webClient = new WebClient())
            {
                var imageBytes = webClient.DownloadData(url);
                if (imageBytes != null && imageBytes.Length > 0)
                {
                    imageBitmap = BitmapFactory.DecodeByteArray(imageBytes, 0, imageBytes.Length);
                }
            }

            return imageBitmap;
        }
    }

}

