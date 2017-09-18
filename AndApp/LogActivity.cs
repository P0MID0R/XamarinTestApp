using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using SQLite;

namespace WeatherApp
{
    [Activity(Label = "Log")]
    public class LogActivity : ListActivity
    {
        string path = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            if (GetAllData(System.IO.Path.Combine(path, "localAppDB.db")).Count != 0)
            {
                this.ListAdapter = new LogListAdapter(GetAllData(System.IO.Path.Combine(path, "localAppDB.db")));
                //this.ListAdapter = new ArrayAdapter(this, Android.Resource.Layout.SimpleListItem1, GetAllData(System.IO.Path.Combine(path, "localAppDB.db")));
            }
            else 
                this.ListAdapter = new ArrayAdapter(this, Android.Resource.Layout.SimpleListItem1, new string[] { "Лог пуст" });

        }

        private List<LogDB> GetAllData(string path)
        {
            try
            {
                var db = new SQLiteConnection(path);
                var data = db.Query<LogDB>("SELECT * from LogDB");
                return data;
            }
            catch 
            {
                List<LogDB> outputList = new List<LogDB>();
                return outputList;
            }
        }
    }
}