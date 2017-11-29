using Hodlr.Interfaces;
using Hodlr.Models;
using SQLite;
using System.Collections.Generic;
using System.Linq;
using Xamarin.Forms;

namespace Hodlr
{
    public class DatabaseManager
    {
        private SQLiteConnection connection;

        public DatabaseManager()
        {
            connection = DependencyService.Get<ISQLite>().GetConnection();
            connection.CreateTable<Transaction>();
            connection.CreateTable<AppCache>();
        }

        public IEnumerable<Transaction> GetTransactions()
        {
            var allTransactions = (from t in connection.Table<Transaction>()
                    select t).ToList();

            // If data is out of date, update it to new format and save it
            // (Should only go through once per app update)
            foreach(Transaction t in allTransactions.Where(t => t.DataVersion < Transaction.CurrentDataVersion))
            {
                t.AcquireCrypto = t.AcquireBtc;
                t.CryptoAmount = t.BtcAmount;
                if (string.IsNullOrWhiteSpace(t.CryptoCurrency)) t.CryptoCurrency = "BTC";
                AddOrUpdateTransaction(t);
            }

            return allTransactions;
        }

        public void DeleteTransaction(int id)
        {
            connection.Delete<Transaction>(id);
        }

        public void AddOrUpdateTransaction(Transaction data)
        {
            connection.Insert(data);
        }

        public AppCache GetCache()
        {
            return connection.Table<AppCache>().FirstOrDefault();
        }

        public void DeleteCache(int id)
        {
            connection.Delete<AppCache>(id);
        }

        public void AddCache(AppCache data)
        {
            connection.DeleteAll<AppCache>();
            connection.Insert(data);
        }

    }
}
