using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;
using System.Data;
using Test;

namespace Tests.Bittrex
{
    public class BittrexDataProvider : ExchangeDataProvider
    {
        private static Dictionary<string, string> parameters = new Dictionary<string, string>() {
        {"marketName", "usdt-btc"},
        {"tickInterval", "day"} };

        private static string address = "https://bittrex.com/Api/v2.0/pub/market/";
        private static string method = "GetTicks";

        private char separator = ',';
        private string dateFormat = "M/d/yyyy h:mm:ss tt";

        private static Dictionary<string, Type> columnsAndTypes = new Dictionary<string, Type>() {
            {  BittrexColumns.Names.orderUId, typeof(string) },
            {  BittrexColumns.Names.exchange, typeof(string) },
            {  BittrexColumns.Names.type, typeof(string)                 },
            {  BittrexColumns.Names.quantity, typeof(decimal)            },
            {  BittrexColumns.Names.limit, typeof(decimal)               },
            {  BittrexColumns.Names.commissionPaid, typeof(decimal)      },
            {  BittrexColumns.Names.total, typeof(decimal)               },
            {  BittrexColumns.Names.opened, typeof(DateTime)             },
            {  BittrexColumns.Names.closed, typeof(DateTime) } };

        ////https://bittrex.com/Api/v2.0/pub/market/GetTicks?marketName=usdt-btc&tickInterval=day

        //GetClosingDailyPricesForBaseAsset
        public override Dictionary<string, Dictionary<DateTime, decimal>> GetClosingDailyPricesForBaseAsset(List<string> baseCurrencies)
        {
            Dictionary<string, Dictionary<DateTime, decimal>> currencyAndDailyClosingPrices = new Dictionary<string, Dictionary<DateTime, decimal>>();

            foreach (string baseCurrency in baseCurrencies)
            {
                parameters["marketName"] = $"usdt-{baseCurrency}";
                string requestUrl = HttpHelper.BuildRequestUrl(address, method, parameters);
                var json = HttpHelper.GetDataFromAddress<string>(requestUrl).GetAwaiter().GetResult();

                var des = DeserializeBittrexData(json);

                if (des == null) //just skip as we do not have data
                    continue;

                currencyAndDailyClosingPrices.Add(baseCurrency, des);
            }

            return currencyAndDailyClosingPrices;
        }

        public override List<String> GetBaseCurrencies()
        {
            return new List<string> { "BTC", "USDT", "ETH" };
        }

        public override string GetBaseAsset(string pair)
        {
            return GetBaseCurrencies().First(e => pair.StartsWith($"{e}-"));
        }

        private Dictionary<DateTime, decimal> DeserializeBittrexData(string bittrexPricesJson)
        {
            Dictionary<DateTime, decimal> dateClosingPrice = new Dictionary<DateTime, decimal>();

            var definition = new
            {
                success = true,
                message = "",
                result = new BittrexCandleData[1]
            };

            var data = JsonConvert.DeserializeAnonymousType(bittrexPricesJson, definition);

            if (data.result == null)
                return null;

            foreach (var cd in data.result)
                dateClosingPrice.Add(DateTime.ParseExact(cd.T.Split('T').First(), "yyyy-MM-dd", CultureInfo.InvariantCulture), decimal.Parse(cd.C, CultureInfo.InvariantCulture));

            return dateClosingPrice;
        }

        public override DataTable inputFileToDataTable(string operationsDataPathCsv)
        {
            return DataTableExtensions.CsvToDataTable(operationsDataPathCsv, columnsAndTypes, separator, dateFormat);
        }

        public override Transaction DataRowToTransaction(DataRow row)
        {
            Transaction t = new Transaction();

            t.OrderUuid = row.Field<string>(BittrexColumns.Names.orderUId);
            t.Exchange = row.Field<string>(BittrexColumns.Names.exchange);
            t.TransactionType = row.Field<string>(BittrexColumns.Names.type) == BittrexColumns.Content.sellType ? Transaction.Type.LIMIT_SELL : Transaction.Type.LIMIT_BUY;
            t.Quantity = row.Field<decimal>(BittrexColumns.Names.quantity);
            t.Limit = row.Field<decimal>(BittrexColumns.Names.limit);
            t.CommissionPaid = row[BittrexColumns.Names.commissionPaid] == DBNull.Value ? 0 : row.Field<decimal>(BittrexColumns.Names.commissionPaid);
            t.Total = row.Field<decimal>(BittrexColumns.Names.total);
            t.Opened = row[BittrexColumns.Names.opened] == DBNull.Value ? DateTime.MinValue : row.Field<DateTime>(BittrexColumns.Names.opened);
            t.Closed = row.Field<DateTime>(BittrexColumns.Names.closed);
            t.processed = false;            
            t.availableVolume = t.Quantity;
            t.baseAsset = GetBaseAsset(t.Exchange);
            t.commisionAsset = t.baseAsset; //always true for bittrex

            return t;
        }

        public override int GetMarketColumnIndex()
        {
            return 1;
        }

        public override void UpdateExchangeSpecificRows(DataRow row, Transaction t) { }

        public override string GetMarketForRow(DataRow row)
        {
            return row.Field<string>(BittrexColumns.Names.exchange);
        }

        public override string GetTransactionUidForRow(DataRow row)
        {
            return row.Field<string>(BittrexColumns.Names.orderUId);
        }

        public override int GetTransactionCloseDateIndex()
        {
            return 8;
        }

        public override int ColumnsReorderIndexOffset()
        {
            return BittrexColumns.colReorderIndex;
        }

        public override Dictionary<string, string> GetColumnsTranslations()
        {
            return BittrexColumns.columnsTranslations;
        }

        public override List<string> GetColumnsOrder()
        {
            return ColumnsOrder.columnsOrder;
        }
    }
}
