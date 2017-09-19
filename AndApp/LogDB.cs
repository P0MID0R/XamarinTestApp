using System;

using SQLite;

namespace WeatherApp
{
    class LogDB
    {
        [PrimaryKey, AutoIncrement]
        public int ID { get; set; }
        public DateTime date { get; set; }
        public string City { get; set; }
        public double temp { get; set; }
        public string icon { get; set; }

    }
}