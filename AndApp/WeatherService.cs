using Android.App;
using Android.Content;
using Android.OS;
using Android.Preferences;
using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;

namespace WeatherApp
{
    [Service]
    class WeatherService : Service
    {
        public const int SERVICE_RUNNING_NOTIFICATION_ID = 10000;

        public override IBinder OnBind(Intent intent)
        {
            return null;
        }

        public override void OnCreate()
        {

            base.OnCreate();
        }

        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {
            var nMgr = (NotificationManager)GetSystemService(NotificationService);

            ISharedPreferences d = PreferenceManager.GetDefaultSharedPreferences(this);
            string defaultCity = d.GetString("pref_default_country", intent.GetStringExtra("DefaultCity"));

            LogDB logDB = GetWeatherData(defaultCity);

            var notification = new Notification(Resource.Drawable.Icon, "New Message");
            var pendingIntent = PendingIntent.GetActivity(this, 0, new Intent(this, typeof(MainActivity)), 0);
            notification.SetLatestEventInfo(this, GetString(Resource.String.app_name), 
                string.Format(GetString(Resource.String.notification_content_text),logDB.City,logDB.temp,logDB.description), pendingIntent);
            nMgr.Notify(10000, notification);
            notification.Flags |= NotificationFlags.AutoCancel;
            
            return StartCommandResult.NotSticky;
        }



        public LogDB GetWeatherData(string City)
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

                return currentdata;
            }
            catch
            {
                return new LogDB();
            }
        }
    }
}