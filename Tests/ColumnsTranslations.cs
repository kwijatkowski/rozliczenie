using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests
{
    public static class ColumnsTranslations
    {
        public static class Bittrex
        {
            public static Dictionary<string, string> colNamesMap = new Dictionary<string, string>()
        {
            { ConfStrings.orderUId, "identyfikatorTransakcji" },
            { ConfStrings.profit, "zysk" },
            { ConfStrings.closed, "data zamknięcia" },
            { ConfStrings.quantity, "ilosc" },
            { ConfStrings.limit, "kurs" },
            { ConfStrings.commissionPaid, "prowizja" },
            { ConfStrings.price, "cena" },
            { ConfStrings.opened, "data otwarcia" },
            { ConfStrings.type, "typ" },
            { ConfStrings.exchange, "para" }
        };
        }


    }
}
