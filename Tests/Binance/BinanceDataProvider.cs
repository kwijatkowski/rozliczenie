using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tests.Bittrex;
using System.Globalization;
using System.Collections.Specialized;
using System.Web;
using System.Data;
using Trustee.EPPlusHelper;
using Test;

namespace Tests.Binance
{
    public class BinanceDataProvider : ExchangeDataProvider
    {
        //https://github.com/binance-exchange/binance-official-api-docs/blob/master/rest-api.md

        //unix start of the year 1514764800
        //end 1546300799

        //https://api.binance.com/api/v1/klines?symbol=LTCBTC&interval=1d&startTime=1514764800000&endTime=1546300800000
        //https://api.binance.com/api/v1/klines?symbol=LTCBTC&interval=1d&startTime=0&endTime=1

        private string baseUrl = "https://api.binance.com/api/v1/";
        private string method = "klines";
        private Dictionary<string, string> parameters = new Dictionary<string, string>() {
            {"interval", "1d"  },
            {"startTime", UnixTimeConverter.DateTimeToUnixTimeStamp(new DateTime(2017,12,28)).ToString() }, //"1514764800000" },
            {"endTime", UnixTimeConverter.DateTimeToUnixTimeStamp(new DateTime(2019 ,01,02)).ToString() },//"1546300800000" }
        };

        //#region exportFileColumnsNames
        //string date = "Date(UTC)";
        //string market = "Market";
        //string type = "Type";
        //string price = "Price";
        //string amount = "Amount";
        //string total = "Total";
        //string fee = "Fee";
        //string feeCoin = "Fee Coin";
        //string uid = "Uid";
        //#endregion

        //string sellTransaction = "SELL";
        //string buyTransaction = "BUY";

        public List<string> baseCurrencies = new List<string> { "BTC", "USDT" };

        public override Transaction DataRowToTransaction(DataRow row)
        {
            Transaction t = new Transaction();

            t.OrderUuid = row.Field<string>(BinanceColumns.Names.orderUid);
            t.Exchange = row.Field<string>(BinanceColumns.Names.exchange);
            t.TransactionType = row.Field<string>(BinanceColumns.Names.type) == BinanceColumns.Content.sellType ? Transaction.Type.LIMIT_SELL : Transaction.Type.LIMIT_BUY;
            t.Quantity = decimal.Parse(row[BinanceColumns.Names.quantity].ToString(), CultureInfo.InvariantCulture);
            t.CommissionPaid = decimal.Parse(row[BinanceColumns.Names.commissionPaid].ToString(), CultureInfo.InvariantCulture);
            t.Total = decimal.Parse(row[BinanceColumns.Names.total].ToString(), CultureInfo.InvariantCulture);
            t.Limit = decimal.Parse(row[BinanceColumns.Names.limit].ToString(), CultureInfo.InvariantCulture);
            t.Opened = DateTime.Parse(row[BinanceColumns.Names.closed].ToString());
            t.Closed = DateTime.Parse(row[BinanceColumns.Names.closed].ToString());
            t.processed = false;
            t.availableVolume = t.Quantity;// decimal.Parse(t.availableVolume.ToString(), CultureInfo.InvariantCulture); // not possible to initialize earlier
            t.baseAsset = GetBaseAsset(t.Exchange);
            t.commisionAsset = row.Field<string>(BinanceColumns.Names.feeCoin);

            if (t.commisionAsset != t.baseAsset)
            {
                t.CommissionPaid = t.CommissionPaid * t.Limit; //need to bring it to base asset, so we can calculate profits correctly later on
                t.commisionAsset = t.baseAsset;
            }

            return t;
        }

        public override void UpdateExchangeSpecificRows(DataRow row, Transaction t)
        {
            row[BinanceColumns.Names.feeCoin] = t.commisionAsset;
            row[BinanceColumns.Names.commissionPaid] = t.CommissionPaid;
        }

        public override List<String> GetBaseCurrencies()
        {
            return new List<string> { "BTC" };
        }

        public override string GetBaseAsset(string pair)
        {
            return GetBaseCurrencies().First(e => pair.EndsWith(e));
        }

        public override Dictionary<string, Dictionary<DateTime, decimal>> GetClosingDailyPricesForBaseAsset(List<string> baseCurrencies)
        {
            Dictionary<string, Dictionary<DateTime, decimal>> currencyAndDailyClosingPrices = new Dictionary<string, Dictionary<DateTime, decimal>>();

            foreach (string baseCurrency in baseCurrencies)
            {
                parameters["symbol"] = $"{baseCurrency}USDT";
                string requestUrl = HttpHelper.BuildRequestUrl(baseUrl, method, parameters);
                var json = HttpHelper.GetDataFromAddress<string>(requestUrl).GetAwaiter().GetResult();

                var des = DeserializeCandlestickData(json);

                if (des == null) //just skip as we do not have data
                    continue;

                currencyAndDailyClosingPrices.Add(baseCurrency, des);
            }

            return currencyAndDailyClosingPrices;
        }

        public override DataTable inputFileToDataTable(string filePath)
        {
            Helper helper = new Helper();
            var ds = helper.GetSheetsContent(filePath, 1);
            return ds.Tables[0];
        }

        public override string GetMarketForRow(DataRow row)
        {
            return row.Field<string>(BinanceColumns.Names.exchange);
        }

        private Dictionary<DateTime, decimal> DeserializeCandlestickData(string json)
        {
            Dictionary<DateTime, decimal> closingPrices = new Dictionary<DateTime, decimal>();

            var def = new List<string[]>();
            List<string[]> candlesList = JsonConvert.DeserializeAnonymousType(json, def);

            foreach (var candle in candlesList)
            {
                BinanceCandleData bc = new BinanceCandleData(candle);
                closingPrices[bc.CloseTime] = bc.Close;
            }

            return closingPrices;
        }

        public override int GetMarketColumnIndex()
        {
            return 2;
        }

        public override string GetTransactionUidForRow(DataRow row)
        {
            return row.Field<string>(BinanceColumns.Names.orderUid);
        }
        public override int GetTransactionCloseDateIndex()
        {
            return 1;
        }

        public override int ColumnsReorderIndexOffset()
        {
            return 0;
        }

        public override Dictionary<string, string> GetColumnsTranslations()
        {
            return BinanceColumns.columnsTranslations;
        }

        public override List<string> GetColumnsOrder()
        {
            return ColumnsOrder.columnsOrder;
        }
    }
}
