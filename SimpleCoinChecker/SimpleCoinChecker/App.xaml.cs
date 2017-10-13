using Newtonsoft.Json;
using Hodlr.Models;
using System.Collections.Generic;
using Xamarin.Forms;
using System;

namespace Hodlr
{
    public partial class App : Application
    {
        public static DateTime lastConvertRefresh;
        public static DatabaseManager db;
        public static Dictionary<string, FiatValue> FiatValues;
        public static FiatConvert FiatConvert;
        public static string FiatPref = "USD";

        public App()
        {
            InitializeComponent();
            db = new DatabaseManager();

            AppCache cache = db.GetCache();
            if (cache != null)
            {
                if(!string.IsNullOrWhiteSpace(cache.FiatPref)) FiatPref = cache.FiatPref;
                lastConvertRefresh = cache.LastConvertRefresh;
                FiatValues = JsonConvert.DeserializeObject<Dictionary<string, FiatValue>>(cache.ValueDataJson);
                FiatConvert = JsonConvert.DeserializeObject<FiatConvert>(cache.ConvertDataJson);
            }

            MainPage = new NavigationPage(new Pages.MainPage());
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }
}
