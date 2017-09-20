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
using Android.Content.PM;

namespace WeatherApp
{
    [Activity(Label = "@string/app_name",
        Icon = "@drawable/Icon",
        MainLauncher = true,
        Theme = "@android:style/Theme.Material",
        ConfigurationChanges = ConfigChanges.Locale,
        ScreenOrientation = ScreenOrientation.Portrait)]
    public class MainActivity : Activity
    {
        string path = System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "localAppDB.db");



        protected override void OnCreate(Bundle savedInstanceState)
        {

            base.OnCreate(savedInstanceState);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);


            Button StartButton = FindViewById<Button>(Resource.Id.Start);
            Button Log_button = FindViewById<Button>(Resource.Id.Log_button);
            EditText inputText = FindViewById<EditText>(Resource.Id.inputText);
            ListView forecastView = FindViewById<ListView>(Resource.Id.forecastView);

            AppStart();

            StartButton.Click += (object sender, EventArgs e) =>
           {
               try
               {
                   if (showResult(inputText.Text, GetWeatherData(inputText.Text)))
                   {
                       forecastView.Adapter = new ForecastListAdapter(GetForecastData(inputText.Text));
                   }
                   else
                   {
                       forecastView.Adapter = new ForecastListAdapter(new List<Forecast>());
                       Toast.MakeText(ApplicationContext, GetString(Resource.String.error_message), ToastLength.Long).Show();
                   }

                   inputText.Text = string.Empty;
               }
               catch
               {
                   Toast.MakeText(ApplicationContext, GetString(Resource.String.error_message), ToastLength.Long).Show();
               }
           };
        }

        private void AppStart()
        {
            try
            {
                ListView forecastView = FindViewById<ListView>(Resource.Id.forecastView);

                connectDatabase(path);

                ISharedPreferences d = PreferenceManager.GetDefaultSharedPreferences(this);
                string defaultCity = d.GetString("pref_default_country", (string)GetCurrentLocationNetwork()["city"]);

                if (defaultCity != "")
                {

                    if (showResult(defaultCity, GetWeatherData(defaultCity)))
                    {
                        forecastView.Adapter = new ForecastListAdapter(GetForecastData(defaultCity));
                    }
                    else
                        Toast.MakeText(ApplicationContext, GetString(Resource.String.netError), ToastLength.Long).Show();
                }
                else
                    Toast.MakeText(ApplicationContext, GetString(Resource.String.selectDC), ToastLength.Long).Show();
            }
            catch
            {
                Toast.MakeText(ApplicationContext, GetString(Resource.String.netError), ToastLength.Long).Show();
            }
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
                    return null;
                }
            }
            catch
            {
                return "@string/errorDB";
            }
        }

        private string insertData(LogDB data, string path)
        {
            try
            {
                var db = new SQLiteAsyncConnection(path);
                db.InsertAsync(data);
                return null;
            }
            catch
            {
                return "@string/errorDB";
            }
        }

        public JObject GetWeatherData(string City)
        {
            try
            {
                string OWAPIkey = "a4dcc6d4ef65f67ade104ecb98972b41";
                string Url = "http://api.openweathermap.org/data/2.5/weather?q=" + City + "&appid=" + OWAPIkey + "&lang=" + Resources.Configuration.Locale.Language.ToString();
                HttpClient client = new HttpClient();
                var response = client.GetStringAsync(Url).Result;
                return JObject.Parse(response);
            }
            catch
            {
                return null;
            }
        }

        public List<Forecast> GetForecastData(string City)
        {
            try
            {
                List<Forecast> forecastList = new List<Forecast>();
                string OWAPIkey = "a4dcc6d4ef65f67ade104ecb98972b41";
                string Url = "http://api.openweathermap.org/data/2.5/forecast?q=" + City + "&appid=" + OWAPIkey + "&lang=" + Resources.Configuration.Locale.Language.ToString();
                HttpClient client = new HttpClient();
                var response = client.GetStringAsync(Url).Result;

                var forecastData = JObject.Parse(response);
                Forecast tempForecast = new Forecast
                {
                    temp6 = "-",
                    temp12 = "-",
                    temp18 = "-"
                };

                for (var i = 0; i <= 35; i++)
                {
                    DateTime tempdate = DateTime.ParseExact((string)forecastData["list"][i]["dt_txt"], "yyyy-MM-dd HH:mm:ss",
                                       System.Globalization.CultureInfo.InvariantCulture);

                    if (tempdate.TimeOfDay.Hours == 6)
                    {
                        string JSONtemp = (string)forecastData["list"][i]["main"]["temp"];
                        tempForecast.icon = (string)forecastData["list"][i]["weather"][0]["icon"];
                        tempForecast.temp6 = (Math.Round(Convert.ToDouble(JSONtemp.Replace('.', ',')) - 273.15)).ToString();
                    }
                    if (tempdate.TimeOfDay.Hours == 12)
                    {
                        string JSONtemp = (string)forecastData["list"][i]["main"]["temp"];
                        tempForecast.icon = (string)forecastData["list"][i]["weather"][0]["icon"];
                        tempForecast.temp12 = (Math.Round(Convert.ToDouble(JSONtemp.Replace('.', ',')) - 273.15)).ToString();
                    }
                    if (tempdate.TimeOfDay.Hours == 18)
                    {
                        string JSONtemp = (string)forecastData["list"][i]["main"]["temp"];
                        tempForecast.icon = (string)forecastData["list"][i]["weather"][0]["icon"];
                        tempForecast.temp18 = (Math.Round(Convert.ToDouble(JSONtemp.Replace('.', ',')) - 273.15)).ToString();
                        tempForecast.date = tempdate.Day.ToString("D2") + "." + tempdate.Month.ToString("D2"); ;
                        forecastList.Add(tempForecast);
                        tempForecast = new Forecast();
                    }
                }
                return forecastList;
            }
            catch
            {
                return null;
            }
        }

        public JObject GetCurrentLocationNetwork() //free http://ip-api.com api
        {
            string Url = "http://ip-api.com/json";
            HttpClient client = new HttpClient();
            var response = client.GetStringAsync(Url).Result;
            return JObject.Parse(response);
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
                double temp = Math.Round(Convert.ToDouble(JSONtemp.Replace('.', ',')) - 273.15);

                string icon = (string)JsonInput["weather"][0]["icon"];
                var imageBitmap = GetImageBitmapFromUrl("http://openweathermap.org/img/w/" + icon + ".png");
                weathericon.SetImageBitmap(imageBitmap);
                CityName.Text = string.Format(GetString(Resource.String.currentToast), (string)JsonInput["name"], temp.ToString(), (string)JsonInput["weather"][0]["description"]);
                imageLayout.Visibility = ViewStates.Visible;
                CityName.Visibility = ViewStates.Visible;

                LogDB currentdata = new LogDB
                {
                    City = (string)JsonInput["name"],
                    date = DateTime.Now,
                    temp = Math.Round(Convert.ToDouble(JSONtemp.Replace('.', ',')) - 273.15),
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

