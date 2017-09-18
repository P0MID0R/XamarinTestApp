using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SQLite;
using SQLitePCL;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace AndApp
{
    class LogDB
    {
        [PrimaryKey, AutoIncrement]
        public int ID { get; set; }
        public string date { get; set; }
        public string City { get; set; }
        public double temp { get; set; }

        public override string ToString()
        {
            return "В " + this.City + " погода " + this.date + " была " + this.temp; ;
        }
    }
}