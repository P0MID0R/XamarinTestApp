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

namespace WeatherApp
{
    class ViewHolder : Java.Lang.Object
    {
        public ImageView Icon { get; set; }
        public TextView Temp { get; set; }
        public TextView City { get; set; }
    }
}