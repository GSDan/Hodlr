using Android.Appwidget;
using Android.Content;
using Hodlr.Droid;
using Hodlr.Interfaces;
using System;
using System.Globalization;
using Xamarin.Forms;

[assembly: Dependency(typeof(WidgetManager_Android))]
namespace Hodlr.Droid
{
    public class WidgetManager_Android : IWidgetManager
    {
        public void UpdateWidget(double totalMoney, double profit, string fiatPref)
        {
            HodlrWidgetProvider widgetProvider = new HodlrWidgetProvider();

            Context context = MainActivity.Context;

            int[] ids = AppWidgetManager.GetInstance(context.ApplicationContext)
                .GetAppWidgetIds(new ComponentName(
                    context.ApplicationContext, 
                    Java.Lang.Class.FromType(typeof(HodlrWidgetProvider))));

            CurrencySymbolManager_Android symbolManager = new CurrencySymbolManager_Android();

            RegionInfo region = symbolManager.GetRegion(fiatPref);
            CultureInfo culture = symbolManager.GetCulture(region.Name);

            string totalVal = string.Format(culture, "{0:C}", totalMoney);
            string profitVal = string.Format(culture, "{0:C}", profit);
            string timeVal = string.Format("Updated: {0:t}", DateTime.Now);
            Android.Graphics.Color profCol = (profit >= 0) ? 
                Android.Graphics.Color.ForestGreen : Android.Graphics.Color.IndianRed;

            widgetProvider.UpdateWidgets(
                context, 
                AppWidgetManager.GetInstance(context),
                ids,
                timeVal,
                totalVal,
                profitVal,
                profCol,
                false,
                true
                );
        }
    }
}