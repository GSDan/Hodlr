using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Android.App;
using Android.Appwidget;
using Android.Content;
using Android.Graphics;
using Android.Widget;
using Hodlr.Models;
using Newtonsoft.Json;
using SQLite;

namespace Hodlr.Droid
{
    [BroadcastReceiver(Label = "Hodlr", Exported = true)]
    [IntentFilter(new string[] { "android.appwidget.action.APPWIDGET_UPDATE" })]
    [MetaData("android.appwidget.provider", Resource = "@xml/widget_info")]
    public class HodlrWidgetProvider : AppWidgetProvider
    {
        // NOTE: lots of code repetition in here, due to being unable to access Xamarin Forms
        // dependencies without first initializing Xamarin Forms. Will make neater at some point.

        private static CurrencySymbolManager_Android symbolManager;

        public void UpdateWidgets(Context context, AppWidgetManager appWidgetManager,
                                   int[] appWidgetIds, string updateMessage, string totalVal, string profitVal, Color profCol, bool updating, bool setIntents = false)
        {
            for (int i = 0; i < appWidgetIds.Length; i++)
            {
                int widgetId = appWidgetIds[i];
                RemoteViews remoteViews = new RemoteViews(context.PackageName, Resource.Layout.Widget);
                remoteViews.SetViewVisibility(Resource.Id.widgetButton, (updating)? 
                    Android.Views.ViewStates.Gone : Android.Views.ViewStates.Visible);
                remoteViews.SetViewVisibility(Resource.Id.widgetLoading, (updating)? 
                    Android.Views.ViewStates.Visible : Android.Views.ViewStates.Gone);

                if (!string.IsNullOrWhiteSpace(profitVal))
                {
                    remoteViews.SetTextViewText(Resource.Id.widgetProfit, profitVal);
                    remoteViews.SetTextColor(Resource.Id.widgetProfit, profCol);
                }

                if(!string.IsNullOrWhiteSpace(totalVal))
                {
                    remoteViews.SetTextViewText(Resource.Id.widgetTotal, totalVal);
                }

                if (!string.IsNullOrWhiteSpace(updateMessage))
                {
                    remoteViews.SetTextViewText(Resource.Id.widgetTime, updateMessage);
                }

                if(setIntents)
                {
                    Intent intent = new Intent(context, typeof(HodlrWidgetProvider));
                    intent.SetAction(AppWidgetManager.ActionAppwidgetUpdate);
                    intent.PutExtra(AppWidgetManager.ExtraAppwidgetIds, appWidgetIds);

                    PendingIntent pendingIntent = PendingIntent.GetBroadcast(
                        context, 0, intent, PendingIntentFlags.UpdateCurrent);

                    remoteViews.SetOnClickPendingIntent(Resource.Id.widgetButton, pendingIntent);


                    Intent activityIntent = new Intent(context, typeof(MainActivity));
                    PendingIntent pendingAct = PendingIntent.GetActivity(context, 0, activityIntent, 0);
                    remoteViews.SetOnClickPendingIntent(Resource.Id.widgetImage, pendingAct);
                }

                appWidgetManager.UpdateAppWidget(widgetId, remoteViews);
            }
        }

        public override async void OnUpdate(Context context, AppWidgetManager appWidgetManager, int[] appWidgetIds)
        {
            UpdateWidgets(context, appWidgetManager, appWidgetIds, "Loading", null, null, Color.Blue, true);

            try
            {
                // Load database manually - can't use interface without initializing Xamarin Forms
                SQLite_Android dbManager = new SQLite_Android();
                SQLiteConnection db = dbManager.GetConnection();
                db.CreateTable<Transaction>();
                db.CreateTable<AppCache>();

                List<Transaction> transactions = (from t in db.Table<Transaction>()
                                                  select t).ToList();
                AppCache cache = db.Table<AppCache>().FirstOrDefault();
                FiatConvert convert = JsonConvert.DeserializeObject<FiatConvert>(cache.ConvertDataJson);;

                if (convert == null || (DateTime.Now - cache.LastConvertRefresh) > TimeSpan.FromDays(1))
                {
                    var fiatResp = await Comms.Get<FiatConvert>(Comms.ConverterApi, "latest?base=USD");
                    if (fiatResp.Success && fiatResp.Data != null)
                    {
                        convert = fiatResp.Data;
                        cache.LastConvertRefresh = DateTime.Now;
                    }

                    if (!fiatResp.Success)
                    {
                        throw new Exception("Failed to refresh");
                    }
                }

                convert.UsdToBtc = await AppUtils.GetCurrentUsdtoBtc(AppUtils.PriceSources[cache.SourcePref]);

                if (convert.UsdToBtc < 0)
                {
                    throw new Exception("Failed to refresh");
                }

                double totalBtc = 0;
                double floatingFiat = 0;
                double totalFiatInvestment = 0;

                foreach (var tr in transactions)
                {
                    double thisFiat = AppUtils.ConvertFiat(tr.FiatCurrency, AppUtils.FiatPref, tr.FiatValue);

                    if (tr.AcquireBtc)
                    {
                        totalBtc += tr.BtcAmount;
                        totalFiatInvestment += thisFiat;
                    }
                    else
                    {
                        totalBtc -= tr.BtcAmount;
                        floatingFiat += thisFiat;
                    }
                }

                double btcFiatVal = AppUtils.GetFiatValOfBtc(AppUtils.FiatPref, totalBtc);
                double profit = floatingFiat + btcFiatVal - totalFiatInvestment;

                if (symbolManager == null) symbolManager = new CurrencySymbolManager_Android();

                RegionInfo region = symbolManager.GetRegion(cache.FiatPref);
                CultureInfo culture = symbolManager.GetCulture(region.Name);

                string totalVal = string.Format(culture, "{0:C}", btcFiatVal);
                string profitVal = string.Format(culture, "{0:C}", profit);
                string timeVal = string.Format("Updated: {0:t}", DateTime.Now);
                Color profCol = (profit >= 0) ? Color.ForestGreen : Color.IndianRed;

                UpdateWidgets(context, appWidgetManager, appWidgetIds, timeVal, totalVal, profitVal, profCol, false, true);

                cache.ConvertDataJson = JsonConvert.SerializeObject(convert);
                db.DeleteAll<AppCache>();
                db.Insert(cache);
                db.Close();
            }
            catch(Exception e)
            {
                UpdateWidgets(context, appWidgetManager, appWidgetIds,
                              string.Format("Update failed at: {0:t}", DateTime.Now), null, null, Color.Blue, false, true);
                Console.WriteLine("HODLR error: " + e.Message);
                Android.Util.Log.Error("Hodlr", "HODLR error: " + e.Message);
            }
        }
    }
}
