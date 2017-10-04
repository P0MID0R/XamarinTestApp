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
using Android.Preferences;
using Android.Content.Res;

namespace WeatherApp
{
    public class ServiceControl
    {
        public static void StopAlarmService()
        {
            var alarmIntent = new Intent(Application.Context, typeof(WeatherService));
            var pending = PendingIntent.GetBroadcast(Application.Context, 0, alarmIntent, PendingIntentFlags.UpdateCurrent);
            var alarmManager = Application.Context.GetSystemService("alarm").JavaCast<AlarmManager>();
            alarmManager.Cancel(pending);
        }
        public static void StartAlarmService(string lang)
        {
            ISharedPreferences d = PreferenceManager.GetDefaultSharedPreferences(Application.Context);
            var alarmIntent = new Intent(Application.Context, typeof(WeatherService));
            string intervalPref = d.GetString("pref_default_frequency", "");
            alarmIntent.PutExtra("DefaultCity", d.GetString("pref_default_country", ""));
            alarmIntent.PutExtra("lang", lang);

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

            var pending = PendingIntent.GetBroadcast(Application.Context, 0, alarmIntent, PendingIntentFlags.UpdateCurrent);

            var alarmManager = Application.Context.GetSystemService("alarm").JavaCast<AlarmManager>();
            alarmManager.SetRepeating(AlarmType.ElapsedRealtime, SystemClock.ElapsedRealtime(), interval, pending);
        }
    }
}