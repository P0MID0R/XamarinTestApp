using System;
using System.Net.Http;
using System.Net;
using System.Collections.Generic;

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

using SQLite;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using Android.Runtime;

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

            CircularPulsingButton StartButton = FindViewById<CircularPulsingButton>(Resource.Id.Start);
            Button Log_button = FindViewById<Button>(Resource.Id.Log_button);
            EditText inputText = FindViewById<EditText>(Resource.Id.inputText);
            ListView forecastView = FindViewById<ListView>(Resource.Id.forecastView);

            AppStartAsync();


            StartButton.Click += async (object sender, EventArgs e) =>
               {
                   try
                   {
                       InputMethodManager imm = (InputMethodManager)GetSystemService(Context.InputMethodService);

                       DisplayResult(await GetWeatherData(inputText.Text));
                       forecastView.Adapter = new ForecastListAdapter(await GetForecastData(inputText.Text));

                       imm.HideSoftInputFromWindow(inputText.WindowToken, 0);
                       inputText.Text = string.Empty;
                   }
                   catch
                   {
                       Toast.MakeText(ApplicationContext, GetString(Resource.String.error_message), ToastLength.Long).Show();
                   }
               };
        }

        protected override void OnRestart()
        {
            NotificationManager notificationManager = (NotificationManager)this.GetSystemService(Context.NotificationService);
            notificationManager.CancelAll();
            var alarmIntent = new Intent(this, typeof(WeatherService));
            var pending = PendingIntent.GetBroadcast(this, 0, alarmIntent, PendingIntentFlags.UpdateCurrent);
            var alarmManager = GetSystemService(AlarmService).JavaCast<AlarmManager>();
            alarmManager.Cancel(pending);
            base.OnRestart();
        }

        protected override void OnStop()
        {
            ISharedPreferences d = PreferenceManager.GetDefaultSharedPreferences(this);
            var alarmIntent = new Intent(this, typeof(WeatherService));
            string intervalPref = d.GetString("pref_default_frequency", "");
            alarmIntent.PutExtra("DefaultCity", d.GetString("pref_default_country", ""));

            long interval = 0;

            switch (intervalPref)
            {
                case "IntervalOneMinute":
                    interval = AlarmManager.IntervalFifteenMinutes / 15;
                    break;
                case "IntervalFifteenMinutes":
                    interval = AlarmManager.IntervalFifteenMinutes;
                    break;
                case "IntervalHalfHour":
                    interval = AlarmManager.IntervalHalfHour;
                    break;
                case "IntervalHour":
                    interval = AlarmManager.IntervalHour;
                    break;
                case "IntervalHalfDay":
                    interval = AlarmManager.IntervalHalfDay;
                    break;
                case "IntervalDay":
                    interval = AlarmManager.IntervalDay;
                    break;
            }

            var pending = PendingIntent.GetBroadcast(this, 0, alarmIntent, PendingIntentFlags.UpdateCurrent);

            var alarmManager = GetSystemService(AlarmService).JavaCast<AlarmManager>();
            alarmManager.SetRepeating(AlarmType.ElapsedRealtime, SystemClock.ElapsedRealtime() + interval, interval, pending);
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

        public async Task<LogDB> GetWeatherData(string City)
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

        public async Task<List<Forecast>> GetForecastData(string City)
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

        public JObject GetCurrentLocationNetwork() //free http://ip-api.com api
        {
            string Url = "http://ip-api.com/json";
            HttpClient client = new HttpClient();
            var response = client.GetStringAsync(Url).Result;
            return JObject.Parse(response);
        }

        public bool DisplayResult(LogDB inputData)
        {
            ImageView weathericon = FindViewById<ImageView>(Resource.Id.imageView1);
            ListView LogList = FindViewById<ListView>(Resource.Id.listView1);
            LinearLayout imageLayout = FindViewById<LinearLayout>(Resource.Id.linearLayout1);
            TextView CityName = FindViewById<TextView>(Resource.Id.textView1);

            var imageBitmap = GetImageBitmapFromUrl("http://openweathermap.org/img/w/" + inputData.icon + ".png");
            weathericon.SetImageBitmap(imageBitmap);
            CityName.Text = string.Format(GetString(Resource.String.currentToast), inputData.City, inputData.temp.ToString(), inputData.description);
            imageLayout.Visibility = ViewStates.Visible;
            CityName.Visibility = ViewStates.Visible;

            return true;
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

