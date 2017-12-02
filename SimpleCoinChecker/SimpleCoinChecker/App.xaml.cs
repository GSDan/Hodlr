using Hodlr.Models;
using System;
using Xamarin.Forms;

namespace Hodlr
{
    public partial class App : Application
    {
        private static DatabaseManager db;
        private static bool cacheLoaded;

        public static DatabaseManager DB
        {
            get
            {
                if(db == null)
                {
                    db = new DatabaseManager();
                }
                if (!cacheLoaded)
                {
                    cacheLoaded = true;
                    AppUtils.SetupCache();
                }
                return db;
            }
        }

        public App()
        {
            InitializeComponent();
            AppUtils.SetupCache();
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
