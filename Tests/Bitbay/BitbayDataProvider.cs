using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using Test;
using System.Globalization;

namespace Tests.Bitbay
{
    public class BitbayDataProvider : ExchangeDataProvider
    {
        //configuration
        private static string dateFormatBitbay = "dd-MM-yyyy HH:mm:ss";
        private static char separator = ';';
        private static Dictionary<string, Type> columnsAndTypes = new Dictionary<string, Type>() {
            {  BitbayColumns.Names.orderUid, typeof(string) },
            {  BitbayColumns.Names.exchange, typeof(string) },
            {  BitbayColumns.Names.type, typeof(string)                 },
            {  BitbayColumns.Names.quantity, typeof(decimal)            },
            {  BitbayColumns.Names.limit, typeof(decimal)               },
            //{  ConfStrings.commissionPaid, typeof(decimal)      },
            {  BitbayColumns.Names.total, typeof(decimal)               },
            //{  BitbayColumns.Names.opened, typeof(DateTime)             },
            {  BitbayColumns.Names.closed, typeof(DateTime) }
};

        public override int ColumnsReorderIndexOffset()
        {
            return BitbayColumns.colReorderIndex;
        }

        public override Transaction DataRowToTransaction(DataRow row)
        {            
                Transaction t = new Transaction();

                t.OrderUuid = row.Field<string>(BitbayColumns.Names.orderUid);
                t.Exchange = row.Field<string>(BitbayColumns.Names.exchange);
                t.TransactionType = row.Field<string>(BitbayColumns.Names.type) == BitbayColumns.Content.sellType ? Transaction.Type.LIMIT_SELL : Transaction.Type.LIMIT_BUY;
            //t.Quantity = decimal.Parse(row[BitbayColumns.Names.quantity].ToString(), CultureInfo.InvariantCulture);
            t.Quantity = (decimal) row[BitbayColumns.Names.quantity];
            t.CommissionPaid = new decimal(0);
            //                t.Total = decimal.Parse(row[BitbayColumns.Names.total].ToString(), CultureInfo.InvariantCulture);
            //                t.Limit = decimal.Parse(row[BitbayColumns.Names.limit].ToString(), CultureInfo.InvariantCulture);
            t.Total = (decimal)row[BitbayColumns.Names.total];
            t.Limit = (decimal)row[BitbayColumns.Names.limit];

            t.Opened = DateTime.Parse(row[BitbayColumns.Names.closed].ToString());
                t.Closed = DateTime.Parse(row[BitbayColumns.Names.closed].ToString());
                t.processed = false;
                t.availableVolume = t.Quantity;// decimal.Parse(t.availableVolume.ToString(), CultureInfo.InvariantCulture); // not possible to initialize earlier
                t.baseAsset = GetBaseAsset(t.Exchange);
                t.commisionAsset = GetBaseAsset(t.Exchange);


                return t;
            }


        public override string GetBaseAsset(string pair)
        {
            return pair.Split(' ').Last();
        }

        public override List<string> GetBaseCurrencies()
        {
            return new List<string> { "PLN" }; //extend if needed
        }

        public override Dictionary<string, Dictionary<DateTime, decimal>> GetClosingDailyPricesForBaseAsset(List<String> assets)
        {
            return new Dictionary<string, Dictionary<DateTime, decimal>>();
        }

        public override List<string> GetColumnsOrder()
        {
            return ColumnsOrder.columnsOrder;
        }

        public override Dictionary<string, string> GetColumnsTranslations()
        {
            return BitbayColumns.columnsTranslations;
        }

        public override int GetMarketColumnIndex()
        {
            return 1;
        }

        public override string GetMarketForRow(DataRow row)
        {
            return row.Field<string>(BitbayColumns.Names.exchange);
        }

        public override int GetTransactionCloseDateIndex()
        {
            return 6;
        }

        public override string GetTransactionUidForRow(DataRow row)
        {
            return row.Field<string>(BitbayColumns.Names.orderUid);
        }

        public override DataTable inputFileToDataTable(string operationsDataPathCsv)
        {
            return DataTableExtensions.CsvToDataTable(operationsDataPathCsv, columnsAndTypes, separator, dateFormatBitbay);
            //, CultureInfo.GetCultureInfo("pl-PL"));
        }

        public override void UpdateExchangeSpecificRows(DataRow row, Transaction t)
        {

        }
    }
}
