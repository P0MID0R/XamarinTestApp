using Android.App;
using Android.Widget;
using Android.Content;
using Android.OS;
using System;
using System.Net.Http;
using System.Threading;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Android.Runtime;
using Android.Views;
using Android.Media;
using System.Collections.Generic;
using SQLite;
using SQLitePCL;
using System.Threading.Tasks;

namespace AndApp
{
    [Activity(Label = "AndApp", MainLauncher = true)]
    public class MainActivity : Activity
    {

        protected override void OnCreate(Bundle savedInstanceState)
        {

            base.OnCreate(savedInstanceState);


            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            Button StartButton = FindViewById<Button>(Resource.Id.Start);
            Button Log_button = FindViewById<Button>(Resource.Id.log_button);
            EditText inputText = FindViewById<EditText>(Resource.Id.inputText);



            string Url = "http://api.openweathermap.org/data/2.5/weather?q=";
            string appid = "&APPID=a4dcc6d4ef65f67ade104ecb98972b41";
            string City = String.Empty;
            string contents;

            string path = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);

            createDatabase(System.IO.Path.Combine(path, "localAppDB.db"));




            StartButton.Click += async (object sender, EventArgs e) =>
            {
                HttpClient hc = new HttpClient();
                City = inputText.Text;
                contents = await hc.GetStringAsync(Url + City + appid);
                var res = JObject.Parse(contents);
                double temp = double.Parse((string)res["main"]["temp"]) - 273.15;

                Toast.MakeText(ApplicationContext, "Погода в " + inputText.Text + " сейчас: " + temp.ToString() + "C", ToastLength.Long).Show();

                LogDB currentdata = new LogDB();
                currentdata.City = inputText.Text;
                currentdata.date = DateTime.Now.ToString();
                currentdata.temp = temp;
                insertUpdateData(currentdata, System.IO.Path.Combine(path, "localAppDB.db"));
                inputText.Text = string.Empty;
            };

            Log_button.Click += (object sender, EventArgs e) =>
            {
                var callLog = new Intent(this, typeof(LogActivity));
                StartActivity(callLog);
            };

        }

        public override bool OnKeyUp([GeneratedEnum] Keycode keyCode, KeyEvent e)
        {


            if (keyCode == Android.Views.Keycode.VolumeUp)
            {
                Toast.MakeText(ApplicationContext, "Вверх", ToastLength.Long).Show();
                return true;
            }

            if (keyCode == Android.Views.Keycode.VolumeDown)
            {

                Toast.MakeText(ApplicationContext, "Вниз", ToastLength.Long).Show();
                return true;
            }

            return base.OnKeyUp(keyCode, e);
        }

        private async Task<string> createDatabase(string path)
        {
            try
            {            
                var connection = new SQLiteAsyncConnection(path);
                {
                    await connection.CreateTableAsync<LogDB>();
                    return "Database created";
                }
            }
            catch (SQLiteException ex)
            {
                return ex.Message;
            }
        }

        private string insertUpdateData(LogDB data, string path)
        {
            try
            {
                var db = new SQLiteAsyncConnection(path);
                    db.InsertAsync(data);
                return "Single data file inserted or updated";
            }
            catch (SQLiteException ex)
            {
                return ex.Message;
            }
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

