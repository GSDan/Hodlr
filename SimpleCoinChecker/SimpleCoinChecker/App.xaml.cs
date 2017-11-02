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
        public static FiatConvert FiatConvert;
        public static string FiatPref = "USD";
        public static int SourcePrefIndex = 0;
        public static double UsdToBtc = 5792.57;
        public const string GDAXName = "GDAX";
        public const string BlockchainName = "Blockchain.info";
        public const string CoindeskName = "Coindesk";
        public const string CoinbaseName = "Coinbase";
        public static string[] PriceSources = new string[] { GDAXName, BlockchainName, CoinbaseName, CoindeskName };

        public App()
        {
            InitializeComponent();
            db = new DatabaseManager();

            AppCache cache = db.GetCache();
            if (cache != null)
            {
                if(!string.IsNullOrWhiteSpace(cache.FiatPref)) FiatPref = cache.FiatPref;
                lastConvertRefresh = cache.LastConvertRefresh;
                UsdToBtc = cache.UsdToBtc;
                FiatConvert = JsonConvert.DeserializeObject<FiatConvert>(cache.ConvertDataJson);
                SourcePrefIndex = cache.SourcePref;
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
