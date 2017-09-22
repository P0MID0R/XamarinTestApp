using System;
using System.Net.Http;
using System.Net;
using System.Collections.Generic;
using System.Threading.Tasks;

using Android.App;
using Android.Widget;
using Android.Content;
using Android.OS;
using Android.Graphics;
using Android.Preferences;
using Android.Content.PM;
using Android.Views;
using Android.Views.InputMethods;
using IR.Sohreco.Circularpulsingbutton;
using Android.Runtime;

using SQLite;
using Newtonsoft.Json.Linq;

namespace WeatherApp
{
    [Activity(Label = "@string/app_name",
        Icon = "@drawable/Icon",
        MainLauncher = false,
        Theme = "@android:style/Theme.Material",
        ConfigurationChanges = ConfigChanges.Locale,
        ScreenOrientation = ScreenOrientation.Portrait)]
    public class MainActivity : Activity
    {
        string path = System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "localAppDB.db");

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.Main);

            ListView forecastView = FindViewById<ListView>(Resource.Id.forecastView);

            AppStartAsync();
        }

        protected override void OnRestart()
        {
            NotificationManager notificationManager = (NotificationManager)this.GetSystemService(Context.NotificationService);
            notificationManager.CancelAll();
            ServiceControl.StopAlarmService();
            base.OnRestart();
        }

        protected override void OnStop()
        {
            ServiceControl.StartAlarmService();
            base.OnStop();
        }

        private async Task AppStartAsync()
        {
            try
            {
                ListView forecastView = FindViewById<ListView>(Resource.Id.forecastView);

                connectDatabase(path);

                ISharedPreferences d = PreferenceManager.GetDefaultSharedPreferences(this);
                var prefEditor = d.Edit();
                if (d.GetString("pref_default_country", "") == "")
                {
                    prefEditor.PutString("pref_default_country", (string)GetCurrentLocationNetwork()["city"]);
                    prefEditor.PutString("pref_default_frequency", "IntervalHour");
                    prefEditor.Commit();
                }
                string defaultCity = d.GetString("pref_default_country", "");

                if (defaultCity != "")
                {
                    try
                    {
                        DisplayResult(await GetWeatherData(defaultCity));
                        forecastView.Adapter = new ForecastListAdapter(await GetForecastData(defaultCity));
                    }
                    catch
                    {
                        Toast.MakeText(ApplicationContext, GetString(Resource.String.netError), ToastLength.Long).Show();
                    }
                }
                else
                    Toast.MakeText(ApplicationContext, GetString(Resource.String.selectDC), ToastLength.Long).Show();
                FindViewById<ImageView>(Resource.Id.imageView1).Visibility = ViewStates.Visible;
                FindViewById<LinearLayout>(Resource.Id.linearLayout1).Visibility = ViewStates.Visible;
                FindViewById<TextView>(Resource.Id.weatherMessage).Visibility = ViewStates.Visible;
                FindViewById<ListView>(Resource.Id.forecastView).Visibility = ViewStates.Visible;
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
                case Resource.Id.refreshData:
                    {
                        RefreshDataAsync();
                        return true;
                    }
            }
            return base.OnOptionsItemSelected(item);
        }

        private async Task RefreshDataAsync()
        {
            try
            {
                ListView forecastView = FindViewById<ListView>(Resource.Id.forecastView);

                DisplayResult(await GetWeatherData(PreferenceManager.GetDefaultSharedPreferences(this).GetString("pref_default_country", "")));
                forecastView.Adapter = new ForecastListAdapter(await GetForecastData(PreferenceManager.GetDefaultSharedPreferences(this).GetString("pref_default_country", "")));
            }
            catch
            {
                Toast.MakeText(ApplicationContext, GetString(Resource.String.error_message), ToastLength.Long).Show();
            }
        }

        private async Task<string> connectDatabase(string path)
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

        private async Task<string> insertData(LogDB data, string path)
        {
            try
            {
                var db = new SQLiteAsyncConnection(path);
                await db.InsertAsync(data);
                return null;
            }
            catch
            {
                return "@string/errorDB";
            }
        }

        private async Task<LogDB> GetWeatherData(string City)
        {
            try
            {
                string OWAPIkey = "a4dcc6d4ef65f67ade104ecb98972b41";
                string Url = "http://api.openweathermap.org/data/2.5/weather?q=" + City + "&appid=" + OWAPIkey + "&lang=" + Resources.Configuration.Locale.Language.ToString();
                HttpClient client = new HttpClient();
                var JsonInput = JObject.Parse(client.GetStringAsync(Url).Result);

                string JSONtemp = (string)JsonInput["main"]["temp"];
                LogDB currentdata = new LogDB
                {
                    City = (string)JsonInput["name"],
                    date = DateTime.Now,
                    temp = Math.Round(Convert.ToDouble(JSONtemp.Replace('.', ',')) - 273.15),
                    icon = (string)JsonInput["weather"][0]["icon"],
                    description = (string)JsonInput["weather"][0]["description"]
                };
                await insertData(currentdata, path);

                return currentdata;
            }
            catch
            {
                return new LogDB();
            }
        }

        private async Task<List<Forecast>> GetForecastData(string City)
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
                    Icon = "",
                    Temp6 = "-",
                    Temp12 = "-",
                    Temp18 = "-"
                };

                for (var i = 0; i < ((JArray)forecastData["list"]).Count; i++)
                {
                    DateTime tempdate = DateTime.ParseExact((string)forecastData["list"][i]["dt_txt"], "yyyy-MM-dd HH:mm:ss",
                                       System.Globalization.CultureInfo.InvariantCulture);

                    if (tempdate.TimeOfDay.Hours == 6)
                    {
                        string JSONtemp = (string)forecastData["list"][i]["main"]["temp"];
                        tempForecast.Icon = (string)forecastData["list"][i]["weather"][0]["icon"];
                        tempForecast.Temp6 = (Math.Round(Convert.ToDouble(JSONtemp.Replace('.', ',')) - 273.15)).ToString();
                    }
                    if (tempdate.TimeOfDay.Hours == 12)
                    {
                        string JSONtemp = (string)forecastData["list"][i]["main"]["temp"];
                        tempForecast.Icon = (string)forecastData["list"][i]["weather"][0]["icon"];
                        tempForecast.Temp12 = (Math.Round(Convert.ToDouble(JSONtemp.Replace('.', ',')) - 273.15)).ToString();
                    }
                    if (tempdate.TimeOfDay.Hours == 18)
                    {
                        string JSONtemp = (string)forecastData["list"][i]["main"]["temp"];
                        tempForecast.Icon = (string)forecastData["list"][i]["weather"][0]["icon"];
                        tempForecast.Temp18 = (Math.Round(Convert.ToDouble(JSONtemp.Replace('.', ',')) - 273.15)).ToString();
                        tempForecast.Date = tempdate.Day.ToString("D2") + "." + tempdate.Month.ToString("D2"); ;
                        forecastList.Add(tempForecast);
                        tempForecast = new Forecast();
                    }
                }
                return forecastList;
            }
            catch
            {
                return new List<Forecast>();
            }
        }

        private JObject GetCurrentLocationNetwork() //free http://ip-api.com api
        {
            string Url = "http://ip-api.com/json";
            HttpClient client = new HttpClient();
            var response = client.GetStringAsync(Url).Result;
            return JObject.Parse(response);
        }

        private bool DisplayResult(LogDB inputData)
        {
            ImageView weathericon = FindViewById<ImageView>(Resource.Id.imageView1);
            TextView CityName = FindViewById<TextView>(Resource.Id.weatherMessage);

            var imageBitmap = GetImageBitmapFromUrl("http://openweathermap.org/img/w/" + inputData.icon + ".png");
            weathericon.SetImageBitmap(imageBitmap);
            CityName.Text = string.Format(GetString(Resource.String.currentToast), inputData.City, inputData.temp.ToString(), inputData.description);

            return true;
        }

        private Bitmap GetImageBitmapFromUrl(string url)
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

