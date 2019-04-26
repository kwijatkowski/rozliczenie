using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Tests
{
    public static class NBPDataProvider
    {
        public static string  baseAddress { get{ return "http://api.nbp.pl/api/exchangerates/rates/a"; }  }

        private static string buildRequestUrl(string currency, DateTime startDate, DateTime endDate)
        {
            return $"{baseAddress}/{currency}/{startDate.ToString("yyyy-MM-dd")}/{endDate.ToString("yyyy-MM-dd")}?format=json";
        }

        public static Dictionary<DateTime, decimal> GetUsdAskForDates(string currency, DateTime startDate, DateTime endDate)
        {
            var def = new
            {
                table = "",
                currency = "",
                code = "",
                rates = new[] { new { no = "", effectiveDate = "", mid = "" } }
            };

            Dictionary<DateTime, decimal> usdPrices = new Dictionary<DateTime, decimal>();

            DateTime tmpStartDate = startDate;
            DateTime tmpEndDate = tmpStartDate.AddDays(365);

            if (tmpEndDate > endDate)
                tmpEndDate = endDate;

            while (tmpEndDate <= endDate)
            { 
                string requestURL = buildRequestUrl(currency, tmpStartDate, tmpEndDate);
                var json = HttpHelper.GetDataFromAddress<string>(requestURL).GetAwaiter().GetResult();
                var data = JsonConvert.DeserializeAnonymousType(json, def);
                foreach (var rate in data.rates)
                    usdPrices.Add(DateTime.ParseExact(rate.effectiveDate, "yyyy-MM-dd", CultureInfo.InvariantCulture), decimal.Parse(rate.mid, CultureInfo.InvariantCulture));

                if (tmpEndDate == endDate)
                    break;

                tmpStartDate = tmpEndDate;
                tmpEndDate = tmpEndDate.AddDays(365);

                if (tmpEndDate > endDate)
                    tmpEndDate = endDate;
            }

            return usdPrices;
        }
    }
}
