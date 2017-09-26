using System.Collections.Generic;
using System.Net;

using Android.Views;
using Android.Widget;
using Android.Graphics;

namespace WeatherApp
{
    class ViewHolder : Java.Lang.Object
    {
        public ImageView Icon { get; set; }
        public TextView Temp { get; set; }
        public TextView City { get; set; }
    }

    class LogListAdapter : BaseAdapter<LogDB>
    {
        List<LogDB> logs;

        public LogListAdapter(List<LogDB> logs)
        {
            this.logs = logs;
        }

        public override LogDB this[int position]
        {
            get
            {
                return logs[position];
            }
        }

        public override int Count
        {
            get
            {
                return logs.Count;
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
                view = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.RowLog, parent, false);

                var icon = view.FindViewById<ImageView>(Resource.Id.weatherIcon);
                var temp = view.FindViewById<TextView>(Resource.Id.logTemp);
                var city_date = view.FindViewById<TextView>(Resource.Id.logCity);

                view.Tag = new ViewHolder() {
                    Icon = icon,
                    Temp = temp,
                    City = city_date };
            }

            var holder = (ViewHolder)view.Tag;

            var imageBitmap = GetImageBitmapFromUrl("http://openweathermap.org/img/w/" + logs[position].icon + ".png");

            holder.Icon.SetImageBitmap(imageBitmap);
            holder.Temp.Text = logs[position].temp.ToString();
            holder.City.Text = 
                logs[position].City + ". " 
                + logs[position].date.ToString("dd/MM/yyyy") 
                + " at " + logs[position].date.ToString("HH:mm");

            return view;

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