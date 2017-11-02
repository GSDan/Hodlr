using System.Globalization;

namespace Hodlr.Interfaces
{
    public interface ICurrencySymbolManager
    {
        RegionInfo GetRegion(string code);
        CultureInfo GetCulture(string name);
    }
}
