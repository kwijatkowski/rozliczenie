using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests.Binance
{
    public static class BinanceColumns
    {
        public static class Names
        {
            public static string orderUid = "Uid";
            public static string closed = "Date(UTC)";
            public static string quantity = "Amount";
            public static string limit = "Price";
            public static string commissionPaid = "Fee";
            public static string feeCoin = "Fee Coin";
            public static string total = "Total";
            public static string type = "Type";
            public static string exchange = "Market";
        }

        public static class Content
        {
            public static string buyType = "BUY";
            public static string sellType = "SELL";
        }

        public static Dictionary<string, string> columnsTranslations = new Dictionary<string, string>() {
            { Names.orderUid, "identyfikatorTransakcji" },
            { Names.closed, "data zamknięcia" },
            { Names.quantity, "ilosc1" },
            { Names.limit, "kurs" },
            { Names.commissionPaid, "prowizja" },
            { Names.total, "cena" },
            { Names.type, "typ" },
            { Names.exchange, "para" }//,
            //{ Content.buyType, "ZAKUP" },
            //{ Content.sellType, "SPRZEDAZ" }
        };

        public static int colReorderIndex = 0;
    }

}
