using Hodlr.Droid;
using Hodlr.Interfaces;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Xamarin.Forms;

[assembly: Dependency(typeof(CurrencySymbolManager_Android))]
namespace Hodlr.Droid
{
    public class CurrencySymbolManager_Android : ICurrencySymbolManager
    {
        private readonly Dictionary<string, RegionInfo> SymbolsByCode;

        public CurrencySymbolManager_Android()
        {
            SymbolsByCode = new Dictionary<string, RegionInfo>();

            var regions = CultureInfo.GetCultures(CultureTypes.SpecificCultures)
                          .Select(x => new RegionInfo(x.LCID));

            foreach (var region in regions)
            {
                if (!SymbolsByCode.ContainsKey(region.ISOCurrencySymbol))
                {
                    SymbolsByCode.Add(region.ISOCurrencySymbol, region);
                }
            }
        }

        public CultureInfo GetCulture(string name)
        {
            return CultureInfo.GetCultures(CultureTypes.AllCultures)
                  .Where(c => c.Name.EndsWith(string.Format("-{0}", name))).FirstOrDefault();
        }

        public RegionInfo GetRegion(string code)
        {
            return SymbolsByCode[code];
        }
    }
}