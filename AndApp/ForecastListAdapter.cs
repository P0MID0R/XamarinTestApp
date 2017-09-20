using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Graphics;


namespace WeatherApp
{
    class ViewHolderForecast : Java.Lang.Object
    {
        public ImageView Icon { get; set; }
        public TextView Temp6 { get; set; }
        public TextView Temp12 { get; set; }
        public TextView Temp18 { get; set; }
        public TextView Date { get; set; }
    }

    public class Forecast
    {

        public string icon { get; set; }
        public string temp6 { get; set; }
        public string temp12 { get; set; }
        public string temp18 { get; set; }
        public string date { get; set; }
    }

    class ForecastListAdapter : BaseAdapter<Forecast>
    {
        List<Forecast> forecasts;

        public ForecastListAdapter(List<Forecast> forecasts)
        {
            this.forecasts = forecasts;
        }

        public override Forecast this[int position]
        {
            get
            {
                return forecasts[position];
            }
        }

        public override int Count
        {
            get
            {
                return forecasts.Count;
            }
        }

        public override long GetItemId(int position)
        {
            return position;
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            var view = convertView;

            if (view == null)
            {
                view = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.RowForecast, parent, false);

                var icon = view.FindViewById<ImageView>(Resource.Id.ForecastIcon);
                var temp6 = view.FindViewById<TextView>(Resource.Id.forecastTemp6);
                var temp12 = view.FindViewById<TextView>(Resource.Id.forecastTemp12);
                var temp18 = view.FindViewById<TextView>(Resource.Id.forecastTemp18);
                var date = view.FindViewById<TextView>(Resource.Id.forecastDate);

                view.Tag = new ViewHolderForecast() { Icon = icon, Temp6 = temp6, Temp12 = temp12, Temp18 = temp18, Date = date };
            }

            var holder = (ViewHolderForecast)view.Tag;

            var imageBitmap = GetImageBitmapFromUrl("http://openweathermap.org/img/w/" + forecasts[position].icon + ".png");

            holder.Icon.SetImageBitmap(imageBitmap);
            holder.Temp6.Text = forecasts[position].temp6;
            holder.Temp12.Text = forecasts[position].temp12;
            holder.Temp18.Text = forecasts[position].temp18;
            holder.Date.Text = forecasts[position].date;

            return view;

        }

        public Bitmap GetImageBitmapFromUrl(string url)
        {
            Bitmap imageBitmap = null;

            try
            {
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
            catch
            {
                return imageBitmap;
            }
        }
    }
}