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
using Android.Graphics;
using System.Net;

namespace WeatherApp
{

    class ViewHolderForecastView : Java.Lang.Object
    {
        public ImageView Icon { get; set; }
        public TextView Time { get; set; }
        public TextView Text { get; set; }
    }

    public class ForecastView
    {

        public string Icon { get; set; }
        public string Time { get; set; }
        public string Text { get; set; }
    }

    class ForecastViewListAdapter : BaseAdapter<ForecastView>
    {
        List<ForecastView> forecastsView;

        public ForecastViewListAdapter(List<ForecastView> forecastsView)
        {
            this.forecastsView = forecastsView;
        }

        public override ForecastView this[int position]
        {
            get
            {
                return forecastsView[position];
            }
        }

        public override int Count
        {
            get
            {
                return forecastsView.Count;
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
                view = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.RowForecastView, parent, false);

                var icon = view.FindViewById<ImageView>(Resource.Id.FWIcon);
                var time = view.FindViewById<TextView>(Resource.Id.FWTimeText);
                var text = view.FindViewById<TextView>(Resource.Id.FWText);


                view.Tag = new ViewHolderForecastView() { Icon = icon, Time = time, Text = text};
            }

            var holder = (ViewHolderForecastView)view.Tag;

            var imageBitmap = GetImageBitmapFromUrl("http://openweathermap.org/img/w/" + forecastsView[position].Icon + ".png");

            holder.Icon.SetImageBitmap(imageBitmap);
            holder.Time.Text = forecastsView[position].Time;
            holder.Text.Text = forecastsView[position].Text;

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