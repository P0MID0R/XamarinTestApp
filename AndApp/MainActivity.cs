using Android.App;
using Android.Widget;
using Android.Content;
using Android.OS;
using System;
using System.Net.Http;
using System.Net;
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
using Android.Locations;
using System.Linq;
using Android.Graphics;

namespace WeatherApp
{
    [Activity(Label = "WeatherApp", Icon = "@drawable/Icon", MainLauncher = true)]
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
            ImageView weathericon = FindViewById<ImageView>(Resource.Id.imageView1);
            LinearLayout imageLayout = FindViewById<LinearLayout>(Resource.Id.linearLayout1);
            imageLayout.Visibility = ViewStates.Invisible;
            TextView CityName = FindViewById<TextView>(Resource.Id.textView1);
            CityName.Visibility = ViewStates.Invisible;

            string JSONtemp = (string)GetWeatherData("Minsk")["main"]["temp"];
            double temp = Convert.ToDouble(JSONtemp.Replace('.', ',')) - 273.15;
            Toast.MakeText(ApplicationContext, "Погода в " + "Минск" + " сейчас: " + temp.ToString() + "C", ToastLength.Long).Show();


            string path = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);


            createDatabase(System.IO.Path.Combine(path, "localAppDB.db"));



            StartButton.Click += (object sender, EventArgs e) =>
           {
               try
               {
                   var data = GetWeatherData(inputText.Text);
                   if (data != null)
                   {
                       JSONtemp = (string)data["main"]["temp"];
                       temp = Convert.ToDouble(JSONtemp.Replace('.', ',')) - 273.15;

                       Toast.MakeText(ApplicationContext, "Погода в " + inputText.Text + " сейчас: " + temp.ToString() + "C", ToastLength.Long).Show();

                       string icon = (string)data["weather"][0]["icon"];
                       var imageBitmap = GetImageBitmapFromUrl("http://openweathermap.org/img/w/"+ icon + ".png");
                       weathericon.SetImageBitmap(imageBitmap);
                       CityName.Text = (string)data["name"];
                       imageLayout.Visibility = ViewStates.Visible;
                       CityName.Visibility = ViewStates.Visible;

                       LogDB currentdata = new LogDB();
                       currentdata.City = (string)data["name"];
                       currentdata.date = DateTime.Now;
                       currentdata.temp = temp;
                       insertUpdateData(currentdata, System.IO.Path.Combine(path, "localAppDB.db"));
                       inputText.Text = string.Empty;
                   }
               }
               catch
               {
                   Toast.MakeText(ApplicationContext, "Ошибка", ToastLength.Long).Show();
               }
           };

            Log_button.Click += (object sender, EventArgs e) =>
            {
                var callLog = new Intent(this, typeof(LogActivity));
                StartActivity(callLog);
            };

        }

        private string createDatabase(string path)
        {
            try
            {
                var connection = new SQLiteConnection(path);
                {
                    connection.CreateTable<LogDB>();
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

        public JObject GetWeatherData(string City)
        {
            try
            {
                string key = "a4dcc6d4ef65f67ade104ecb98972b41";
                string Url = "http://api.openweathermap.org/data/2.5/weather?q=" + City + "&appid=" + key;
                HttpClient client = new HttpClient();
                var response = client.GetStringAsync(Url).Result;
                return JObject.Parse(response);
            }
            catch
            {
                return null;
            }
        }

        private Bitmap GetImageBitmapFromUrl(string url)
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

