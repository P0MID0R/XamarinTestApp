using System;

using SQLite;
using System.Collections.Generic;

namespace WeatherApp
{
    public class LogDB
    {
        [PrimaryKey, AutoIncrement]
        public int ID { get; set; }
        public DateTime date { get; set; }
        public string City { get; set; }
        public double temp { get; set; }
        public string icon { get; set; }
        public string description { get; set; }
    }

    public class LastData
    {
        [PrimaryKey, AutoIncrement]
        public int ID { get; set; }
        public DateTime date { get; set; }
        public string City { get; set; }
        public string icon { get; set; }
        public string temp6 { get; set; }
        public string temp12 { get; set; }
        public string temp18 { get; set; }
        public string description { get; set; }
    }
}