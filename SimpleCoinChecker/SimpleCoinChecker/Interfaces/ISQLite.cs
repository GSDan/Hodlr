using SQLite;

namespace Hodlr.Interfaces
{
    public interface ISQLite
    {
        SQLiteConnection GetConnection();
    }
}
