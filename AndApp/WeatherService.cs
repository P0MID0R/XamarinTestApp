using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.OS;
using Android.Preferences;
using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;

namespace WeatherApp
{
    [BroadcastReceiver]
    class WeatherService : BroadcastReceiver
    {
        string lang;

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

                return currentdata;
            }
            catch
            {
                return new LogDB();
            }
        }
    }
}