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
            List<Transaction> allTransactions = (from t in connection.Table<Transaction>()
                    select t).ToList();

            List<Transaction> toUpdate = new List<Transaction>();

            for(int i = 0; i < allTransactions.Count; i++)
            {
                if(allTransactions[i].DataVersion < Transaction.CurrentDataVersion)
                {
                    // Old format of data! Update it
                    if (allTransactions[i].DataVersion < 2)
                    {
                        allTransactions[i].AcquireCrypto = allTransactions[i].AcquireBtc;
                        allTransactions[i].CryptoAmount = allTransactions[i].BtcAmount;
                        if (string.IsNullOrWhiteSpace(allTransactions[i].CryptoCurrency)) allTransactions[i].CryptoCurrency = "BTC";
                    }

                    allTransactions[i].DataVersion = Transaction.CurrentDataVersion;
                    toUpdate.Add(allTransactions[i]);
                }
            }

            foreach(Transaction t in toUpdate)
            {
                UpdateTransaction(t);
            }

            return allTransactions;
        }

        public void DeleteTransaction(int id)
        {
            connection.Delete<Transaction>(id);
        }

        public void AddTransaction(Transaction data)
        {
            connection.Insert(data);
        }

        public void UpdateTransaction(Transaction data)
        {
            connection.InsertOrReplace(data);
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
