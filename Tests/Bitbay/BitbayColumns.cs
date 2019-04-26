using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests.Bitbay
{
    public static class BitbayColumns
    {
            public static class Names
            {
                public static string orderUid = "ID";
                public static string closed = "Data operacji";
                public static string quantity = "Ilość";
                public static string limit = "Kurs";
                public static string total = "Wartość";
                public static string type = "Rodzaj";
                public static string exchange = "Rynek";
            }

            public static class Content
            {
                public static string buyType = "Kupno";
                public static string sellType = "Sprzedaż";
            }

            public static Dictionary<string, string> columnsTranslations = new Dictionary<string, string>() {
            { Names.orderUid, "identyfikatorTransakcji" },
            { Names.closed, "data zamknięcia" },
            { Names.quantity, "ilosc1" },
            { Names.limit, "kurs" },
            { Names.total, "cena" },
            { Names.type, "typ" },
            { Names.exchange, "para" }
        };

            public static int colReorderIndex = 0;
        }

    }
