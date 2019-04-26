using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests.Bittrex
{
    public static class BittrexColumns
    {
        public static class Names
        {

            public static string orderUId = "Uuid";
            public static string closed = "Closed";
            public static string quantity = "Quantity";
            public static string limit = "Limit";
            public static string commissionPaid = "Commission";
            public static string total = "Price";
            public static string opened = "Opened";
            public static string balance = "Balance";
            public static string type = "OrderType";
            public static string exchange = "Exchange";
        }

        public static class Content
        {
            public static string buyType = "LIMIT_BUY";
            public static string sellType = "LIMIT_SELL";
        }

        public static Dictionary<string,string> columnsTranslations =  new Dictionary<string, string>() {
            { Names.orderUId, "identyfikatorTransakcji" },
            { Names.closed, "data zamknięcia" },
            { Names.quantity, "ilosc1" },
            { Names.limit, "kurs" },
            { Names.commissionPaid, "prowizja" },
            { Names.total, "cena" },
            { Names.opened, "data otwarcia" },
            { Names.type, "typ" },
            { Names.exchange, "para" }//,
            //{ Content.buyType, "ZAKUP" },
            //{ Content.sellType, "SPRZEDAZ" }
        };        

        public static int colReorderIndex = 0;
    }
}
