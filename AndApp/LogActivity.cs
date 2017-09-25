using System.Collections.Generic;
using System.Linq;

using Android.App;
using Android.OS;
using Android.Widget;
using Android.Runtime;
using Android.Content;

using SQLite;

namespace WeatherApp
{
    [Activity(Label = "@string/Log", 
        Theme = "@android:style/Theme.Material", 
        ParentActivity = typeof(MainActivity))]
    public class LogActivity : ListActivity
    {
        string path = System.IO.Path.Combine(
            System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), 
            "localAppDB.db");

        protected override void OnCreate(Bundle savedInstanceState)
        {
            ServiceControl.StopAlarmService();
            base.OnCreate(savedInstanceState);

            if (GetAllData(path).Count != 0)
            {
                this.ListAdapter = new LogListAdapter(GetAllData(path));
            }
            else 
                this.ListAdapter = new ArrayAdapter(
                    this, Android.Resource.Layout.SimpleListItem1, 
                    new string[] { "@string/emptyLog" });
        }

        private List<LogDB> GetAllData(string path)
        {
            try
            {
                var db = new SQLiteConnection(path);
                var data = db.Query<LogDB>("SELECT * from LogDB");
                return data.AsEnumerable().Reverse().ToList();
            }
            catch 
            {
                List<LogDB> outputList = new List<LogDB>();
                return outputList;
            }
        }
    }
}