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
            return (from t in connection.Table<Transaction>()
                    select t).ToList();
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
