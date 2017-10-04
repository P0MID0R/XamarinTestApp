using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.OS;
using Android.Preferences;
using Newtonsoft.Json.Linq;
using SQLite;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace WeatherApp
{
    [BroadcastReceiver]
    class WeatherService : BroadcastReceiver
    {
        string lang;
        string path = System.IO.Path.Combine(
           System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal),
           "localAppDB.db");

        public override void OnReceive(Context context, Intent intent)
        {
            var defaultCity = intent.GetStringExtra("DefaultCity");
            lang = intent.GetStringExtra("lang");

            LogDB logDB = GetWeatherData(defaultCity);

            var resultIntent = new Intent(context, typeof(MainActivity));
            resultIntent.SetFlags(ActivityFlags.NewTask | ActivityFlags.ClearTask);

            var pending = PendingIntent.GetActivity(context, 0,
                resultIntent,
                PendingIntentFlags.CancelCurrent);

            var builder =
                new Notification.Builder(context)
                    .SetContentTitle(logDB.temp.ToString() + " (" + logDB.description + ")")
                    .SetContentText(logDB.date.ToString())
                    .SetSmallIcon(Resource.Drawable.Icon);

            builder.SetContentIntent(pending);

            var notification = builder.Build();

            var manager = NotificationManager.FromContext(context);
            manager.Notify(1337, notification);
        }

        public LogDB GetWeatherData(string City)
        {
            try
            {
                string OWAPIkey = "a4dcc6d4ef65f67ade104ecb98972b41";
                string Url = "http://api.openweathermap.org/data/2.5/weather?q=" + City + "&appid=" + OWAPIkey + "&lang=" + lang;
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
                SaveDatainDB(City, lang);
                return currentdata;
            }
            catch
            {
                return new LogDB();
            }
        }

        public async Task SaveDatainDB(string City, string locale)
        {
            try
            {
                List<LastData> lastDatalist = new List<LastData>();
                string OWAPIkey = "a4dcc6d4ef65f67ade104ecb98972b41";
                string Url = "http://api.openweathermap.org/data/2.5/forecast?q=" + City
                    + "&appid=" + OWAPIkey
                    + "&lang=" + locale;
                HttpClient client = new HttpClient();
                var response = client.GetStringAsync(Url).Result;
                var webClient = new WebClient();
                var forecastData = JObject.Parse(response);

                Url = "http://api.openweathermap.org/data/2.5/weather?q="
                    + City + "&appid=" + OWAPIkey
                    + "&lang=" + locale;
                var JsonInput = JObject.Parse(client.GetStringAsync(Url).Result);

                string JSONtempC = (string)JsonInput["main"]["temp"];
                LogDB currentdata = new LogDB
                {
                    City = (string)JsonInput["name"],
                    date = DateTime.Now,
                    temp = Math.Round(Convert.ToDouble(JSONtempC.Replace('.', ',')) - 273.15),
                    icon = (string)JsonInput["weather"][0]["icon"],
                    description = (string)JsonInput["weather"][0]["description"]
                };

                lastDatalist.Add(new LastData
                {
                    City = City,
                    date = currentdata.date,
                    icon = Convert.ToBase64String(
                                webClient.DownloadData("http://openweathermap.org/img/w/" + currentdata.icon + ".png")),
                    ID = 0,
                    temp12 = currentdata.temp.ToString(),
                    description = currentdata.description
                });

                LastData tempLB = new LastData
                {
                    City = City,
                    date = DateTime.Now,
                    description = "",
                    icon = "",
                    temp6 = "-",
                    temp12 = "-",
                    temp18 = "-",
                    ID = 1
                };

                int counter = 1;

                for (var i = 0; i < ((JArray)forecastData["list"]).Count; i++)
                {
                    DateTime tempdate = DateTime.ParseExact((string)forecastData["list"][i]["dt_txt"], "yyyy-MM-dd HH:mm:ss",
                                       System.Globalization.CultureInfo.InvariantCulture);

                    if (tempdate.TimeOfDay.Hours == 6)
                    {
                        string JSONtemp = (string)forecastData["list"][i]["main"]["temp"];

                        tempLB.temp6 = (Math.Round(Convert.ToDouble(JSONtemp.Replace('.', ',')) - 273.15)).ToString();
                    }
                    if (tempdate.TimeOfDay.Hours == 12)
                    {
                        string JSONtemp = (string)forecastData["list"][i]["main"]["temp"];
                        tempLB.icon = Convert.ToBase64String(
                                webClient.DownloadData("http://openweathermap.org/img/w/" + (string)forecastData["list"][i]["weather"][0]["icon"] + ".png"));
                        tempLB.temp12 = (Math.Round(Convert.ToDouble(JSONtemp.Replace('.', ',')) - 273.15)).ToString();
                    }
                    if (tempdate.TimeOfDay.Hours == 18)
                    {
                        string JSONtemp = (string)forecastData["list"][i]["main"]["temp"];
                        if (tempLB.icon == "")
                            tempLB.icon = Convert.ToBase64String(
                                webClient.DownloadData("http://openweathermap.org/img/w/" + (string)forecastData["list"][i]["weather"][0]["icon"] + ".png"));
                        tempLB.temp18 = (Math.Round(Convert.ToDouble(JSONtemp.Replace('.', ',')) - 273.15)).ToString();
                        tempLB.date = tempdate;
                        lastDatalist.Add(tempLB);
                        counter += 1;
                        tempLB = new LastData
                        {
                            City = City,
                            date = DateTime.Now,
                            description = "",
                            icon = "",
                            temp6 = "-",
                            temp12 = "-",
                            temp18 = "-",
                            ID = counter
                        };
                    }
                }
                var db = new SQLiteConnection(path);
                var data = db.Query<LastData>("SELECT * from LastData");
                foreach (var item in lastDatalist)
                {
                    db.InsertOrReplace(item);
                }
                data = db.Query<LastData>("SELECT * from LastData");
            }
            catch
            {             
            }
        }
    }
}