using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Globalization;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Widget;
using Android.Preferences;
using Android.Content.PM;

using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using OxyPlot.Annotations;
using OxyPlot.Xamarin.Android;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace WeatherApp
{
    [Activity(Label = "ForecastDetail",
        ParentActivity = typeof(MainActivity),
        ScreenOrientation = ScreenOrientation.Portrait)]
    public class ForecastDetailActivity : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.Forecast);
            ServiceControl.StopAlarmService();
            try
            {
                showInfo();
            }
            catch
            {
                Toast.MakeText(ApplicationContext, GetString(Resource.String.error_message), ToastLength.Long).Show();
            }
        }

        private async Task showInfo()
        {
            Forecast forecastData = JsonConvert.DeserializeObject<Forecast>(Intent.GetStringExtra("forecastData"));
            var forecastListData = JObject.Parse(Intent.GetStringExtra("forecastListData"));

            this.Title = forecastData.Date.ToString("dd.MM.yy");

            FindViewById<ListView>(Resource.Id.listView1).Adapter =
            new ForecastViewListAdapter(GetForecastList((forecastListData), forecastData.Date));

            printPlot(forecastListData, forecastData);
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
                        Time = DateTime.ParseExact((string)inputDate[i]["dt_txt"], "yyyy-MM-dd HH:mm:ss",
                                System.Globalization.CultureInfo.InvariantCulture).ToString("HH:mm"),
                        Text = 
                        string.Format(
                            GetString(Resource.String.FMtext),
                            (Math.Round(Convert.ToDouble(JSONtemp.Replace('.', ',')) - 273.15)).ToString(),
                            (string)inputDate[i]["weather"][0]["description"]
                           )
                    });
                }
            }
            return list;
        }

        private async Task printPlot(JObject inputJSON, Forecast forecast)
        {
            try
            {
                PlotView plotView = FindViewById<PlotView>(Resource.Id.plot_view);
                plotView.Model = CreatePlotModel(inputJSON, forecast);
            }
            catch (Exception ex)
            {
                Toast.MakeText(ApplicationContext, ex.Message, ToastLength.Long).Show();
            }
        }

        private PlotModel CreatePlotModel(JObject inputJSON, Forecast forecast)
        {
            try
            {
                string Title = string.Format("{0} {1} ({2})",
                    PreferenceManager.GetDefaultSharedPreferences(Application.Context).GetString("pref_default_country", ""),
                    forecast.Date.ToString("dddd", new CultureInfo(GetString(Resource.String.CultureInfo))),
                    forecast.Date.ToString("dd.MM.yy"));
                var plotModel = new PlotModel
                {
                    Title = Title,
                    TitleColor = OxyColors.White,
                    PlotAreaBorderColor = OxyColors.Transparent,
                    TitlePadding = 20
                };
                JArray inputDate = (JArray)inputJSON["list"];
                double xAxe = 0, minV = 0, maxV = 0;
                for (int i = 0; i < inputDate.Count; i++)
                {
                    if (forecast.Date.ToShortDateString() == DateTime.ParseExact((string)inputDate[i]["dt_txt"], "yyyy-MM-dd HH:mm:ss",
                                          System.Globalization.CultureInfo.InvariantCulture).ToShortDateString())
                    {
                        string JSONtemp = (string)inputDate[i]["main"]["temp"];
                        double temp = Math.Round(Convert.ToDouble(JSONtemp.Replace('.', ',')) - 273.15);
                        if (temp > maxV) maxV = temp;
                        if (temp < minV) minV = temp;
                    }
                }
                plotModel.Axes.Add(new LinearAxis
                {
                    Position = AxisPosition.Bottom,
                    Title = GetString(Resource.String.PlotHorizontal),
                    MinimumPadding = 0.05,
                    MaximumPadding = 0.05,
                    TitleColor = OxyColors.Gray,
                    TextColor = OxyColors.Gray,
                    AxislineColor = OxyColors.Gray,
                    TicklineColor = OxyColors.Gray,                   
                    MajorStep = 3,
                    MinorGridlineStyle = LineStyle.Dash,
                    MajorGridlineStyle = LineStyle.Dash,
                    ExtraGridlineStyle = LineStyle.Dash,
                    IsZoomEnabled = false,
                    IsPanEnabled = false                  
                });
                plotModel.Axes.Add(new LinearAxis
                {
                    Position = AxisPosition.Left,
                    Maximum = maxV + 1,
                    Minimum = minV - 5,
                    MajorStep = 5,
                    IsZoomEnabled = false,
                    IsPanEnabled = false,                   
                    IsAxisVisible = false
                });
                var series1 = new LineSeries
                {
                    MarkerType = MarkerType.Circle,
                    MarkerSize = 3,
                    MarkerStroke = OxyColors.White,
                    MarkerFill = OxyColors.SteelBlue,
                    Color = OxyColors.White,
                    Smooth = true
                };
                for (int i = 0; i < inputDate.Count; i++)
                {
                    DateTime dateTime = DateTime.ParseExact((string)inputDate[i]["dt_txt"], "yyyy-MM-dd HH:mm:ss",
                                          System.Globalization.CultureInfo.InvariantCulture);
                    if (forecast.Date.ToShortDateString() == dateTime.ToShortDateString())
                    {
                        string JSONtemp = (string)inputDate[i]["main"]["temp"];
                        series1.Points.Add(new DataPoint(dateTime.Hour, Convert.ToDouble(JSONtemp.Replace('.', ',')) - 273.15));
                        var pointAnnotation1 = new PointAnnotation()
                        {
                            X = dateTime.Hour,
                            Y = Convert.ToDouble(JSONtemp.Replace('.', ',')) - 273.15,
                            Shape = MarkerType.None,
                            TextColor = OxyColors.White,
                            Text = Math.Round(Convert.ToDouble(JSONtemp.Replace('.', ',')) - 273.15).ToString(),
                        };
                        plotModel.Annotations.Add(pointAnnotation1);
                        xAxe += 1;
                    }
                }
                plotModel.Series.Add(series1);
                return plotModel;
            }
            catch (Exception ex)
            {
                Toast.MakeText(ApplicationContext, ex.Message, ToastLength.Long).Show();
                return new PlotModel();
            }
        }
    }
}