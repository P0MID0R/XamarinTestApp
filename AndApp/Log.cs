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

namespace AndApp
{
    [Activity(Label = "Log")]
    public class Log : ListActivity
    {
        string path = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
           
            //List<LogList> loglist = JsonConvert.DeserializeObject<List<LogList>>(Intent.Extras.GetString("Log"));
            this.ListAdapter = new ArrayAdapter(this, Android.Resource.Layout.SimpleListItem1, GetAllData(System.IO.Path.Combine(path, "localAppDB.db")));
        }

        private List<LogDB> GetAllData(string path)
        {
            try
            {
                var db = new SQLiteConnection(path);
                var data = db.Query<LogDB>("SELECT * from LogDB");
                return data;
            }
            catch (SQLiteException ex)
            {
                return new List<LogDB>();
            }
        }
    }
}