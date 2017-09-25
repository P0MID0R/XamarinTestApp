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
using Newtonsoft.Json;
using Android.Graphics;
using System.Net;
using Android.Preferences;
using Newtonsoft.Json.Linq;
using System.Globalization;

namespace WeatherApp
{
    [Activity(Label = "ForecastDetail", ParentActivity = typeof(MainActivity))]
    public class ForecastDetailActivity : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.Forecast);
            ServiceControl.StopAlarmService();
            try
            {
                Forecast forecastData = JsonConvert.DeserializeObject<Forecast>(Intent.GetStringExtra("forecastData"));
                var forecastListData = JObject.Parse(Intent.GetStringExtra("forecastListData"));
                var imageBitmap = MainActivity.GetImageBitmapFromUrl("http://openweathermap.org/img/w/" + forecastData.Icon + ".png");
                FindViewById<ImageView>(Resource.Id.ForecastIconView).SetImageBitmap(imageBitmap);
                FindViewById<TextView>(Resource.Id.ForecastMainText).Text = forecastData.Date.ToString("dddd", new CultureInfo(GetString(Resource.String.CultureInfo))) +
                    " (" + forecastData.Date.ToString("dd.MM.yy") + ")";
                FindViewById<TextView>(Resource.Id.ForecastSupText).Text =
                    PreferenceManager.GetDefaultSharedPreferences(Application.Context).GetString("pref_default_country", "");
                this.Title = forecastData.Date.ToString("dd.MM.yy");
                FindViewById<ListView>(Resource.Id.listView1).Adapter = new ForecastViewListAdapter(GetForecastList((forecastListData), forecastData.Date));
            }
            catch
            {
                Toast.MakeText(ApplicationContext, GetString(Resource.String.error_message), ToastLength.Long).Show();
            }
        }

        private List<ForecastView> GetForecastList(JObject inputJSON, DateTime cuttentDate)
        {
            List<ForecastView> list = new List<ForecastView>();
            JArray inputDate = (JArray)inputJSON["list"];
            for (var i = 0; i < inputDate.Count; i++)
            {
                if (cuttentDate.ToShortDateString() == DateTime.ParseExact((string)inputDate[i]["dt_txt"], "yyyy-MM-dd HH:mm:ss",
                                       System.Globalization.CultureInfo.InvariantCulture).ToShortDateString())
                {
                    string JSONtemp = (string)inputDate[i]["main"]["temp"];
                    list.Add(new ForecastView
                    {
                        Icon = (string)inputDate[i]["weather"][0]["icon"],
                        MainText = (Math.Round(Convert.ToDouble(JSONtemp.Replace('.', ',')) - 273.15)).ToString() + " (" +
                        (string)inputDate[i]["weather"][0]["description"] + ")",
                        SupText = DateTime.ParseExact((string)inputDate[i]["dt_txt"], "yyyy-MM-dd HH:mm:ss",
                                       System.Globalization.CultureInfo.InvariantCulture).ToShortTimeString()
                    });
                }

            }
            return list;
        }
    }
}