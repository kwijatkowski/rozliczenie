using GenericParsing;
using Newtonsoft.Json;
using NUnit.Framework;
using Rozliczenie;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Test;
using Tests.Bittrex;
using Trustee.EPPlusHelper;
using Tests.Binance;
using Tests.Bitbay;

namespace Tests
{
    [TestFixture]
    public class TestClass
    {
        private string outFilePath = @"C:\Users\Kamil\OneDrive\btc\rozliczenie\2018\input\out";


        //binance
        //IExchangeDataProvider exchangeDataProvider = new BinanceDataProvider();
        //string operationsDataPathCsv = @"C:\Users\Kamil\OneDrive\btc\rozliczenie\2018\input\binance\Binance2018TradeHistoryUid.xlsx";

        //bittrex
        //IExchangeDataProvider exchangeDataProvider = new BittrexDataProvider();
        //string operationsDataPathCsv = @"C:\Users\Kamil\OneDrive\btc\rozliczenie\2018\input\bittrex\BittrexOrderHistory20172018.csv";

        //bitbay
        IExchangeDataProvider exchangeDataProvider = new BitbayDataProvider();
        string operationsDataPathCsv = @"C:\Users\Kamil\OneDrive\btc\rozliczenie\2018\input\bitbay\Bitbay20172018merged.csv";

        DateTime startDate = new DateTime(2017, 1, 1);
        DateTime endDate = new DateTime(2019, 2, 1);

        [TestCase]
        public void TestMethod()
        {            
            DataTable inputs = exchangeDataProvider.inputFileToDataTable(operationsDataPathCsv);

            //read usd-pln exchange rate for given year
            Dictionary<DateTime, decimal> usdAskForDates = NBPDataProvider.GetUsdAskForDates("usd", startDate, endDate);

            List<Transaction> allTransactions = exchangeDataProvider.DataTableToTransactionList(inputs);

            //find all the markets first
            List<string> tradingPairs = allTransactions.Select(t => t.Exchange).Distinct().OrderBy(name => name).ToList();
            List<string> baseCryptoCurrencies = exchangeDataProvider.GetBaseCurrencies();

            Dictionary<string, Dictionary<DateTime, decimal>> pairsDayClosingPrices = exchangeDataProvider.GetClosingDailyPricesForBaseAsset(baseCryptoCurrencies);

            // var rowsCollection = inputs.AsEnumerable().OrderBy(r => r[ConfStrings.closed]);

            foreach (string pair in tradingPairs)
            {
                string baseAsset = exchangeDataProvider.GetBaseAsset(pair);

                //var rowsForThisTradingPair = rowsCollection
                //    .Where(r => r.Field<string>(ConfStrings.exchange) == pair)
                //    .OrderBy(r => r.Field<DateTime>(ConfStrings.closed));


                var operationsForThisTradingPair = allTransactions.Where(t => t.Exchange == pair).OrderBy(t => t.Closed);

                var allBuys = operationsForThisTradingPair.Where(t => t.TransactionType == Transaction.Type.LIMIT_BUY);
                var allSells = operationsForThisTradingPair.Where(t => t.TransactionType == Transaction.Type.LIMIT_SELL);

                //calculate profits

                //go thru all the sells and assign buys operations (percentage is taken into account here)
                //all sell operations have partial sells assigned which represen chunks bought for certain price and then sold within this sell transaction
                //at the end we have partial sells assigned to all sell operations and we can calculate profits based od sell transaction properties only
                foreach (var sell in allSells)
                {
                    //buys before
                    var buysBefore = allBuys.Where(t => t.Closed < sell.Closed && t.availableVolume > 0);//.OrderBy(dt => dt.Closed);
                    var toSell = sell.Quantity;

                    foreach (var buy in buysBefore)
                    {
                        decimal sellingFromHere = 0;
                        sellingFromHere = toSell <= buy.availableVolume ? toSell : buy.availableVolume;

                        buy.availableVolume -= sellingFromHere;
                        toSell -= sellingFromHere;

                        decimal takenFromBuyPercentage = sellingFromHere / buy.Quantity;
                        decimal takenFromSellPercentage = sellingFromHere / sell.Quantity;

                        sell.partialSells.Add(new PartialSell()
                        {
                            Quantity = sellingFromHere,
                            BuyCommission = buy.CommissionPaid * takenFromBuyPercentage,
                            BuyPrice = buy.Total * takenFromBuyPercentage,
                            SellCommission = sell.CommissionPaid * takenFromSellPercentage,
                            SellPrice = sell.Total * takenFromSellPercentage
                        });

                        if (toSell == 0) //if sold all we needed, do not process next buys
                        {
                            break;
                        }
                    }
                }

            }
                //now having partial sells attached to sell transactions we can go tru the rows and assign/calculate profits
                //fill partial sell operations
                foreach (DataRow row in inputs.AsEnumerable())
                {
                    string operationUID = exchangeDataProvider.GetTransactionUidForRow(row); //row.Field<string>(ConfStrings.orderUId);
                    var transaction = allTransactions.First(t => t.OrderUuid == operationUID);

                    exchangeDataProvider.UpdateExchangeSpecificRows(row, transaction);

                    if (transaction.TransactionType == Transaction.Type.LIMIT_SELL)
                    {
                        var sell = transaction;

                        for (int sellNo = 0; sellNo < transaction.partialSells.Count; sellNo++)
                        {
                            string anotherSellCol = $"{ConfStrings.sellColPrefix}{sellNo.ToString()}";
                            string anotherBuyCommissionCol = $"{ConfStrings.buyCommissionPrefix}{sellNo.ToString()}";
                            string anotherSellCommissionCol = $"{ConfStrings.sellCommissionPrefix}{sellNo.ToString()}";
                            string anotherBuyCostCol = $"{ConfStrings.buyCostCol}{sellNo.ToString()}";
                            string anotherSellCostCol = $"{ConfStrings.sellIncomeCol}{sellNo.ToString()}";
                            string baseAssetProfitColName = 
                            $"{ConfStrings.profit} {exchangeDataProvider.GetBaseAsset(exchangeDataProvider.GetMarketForRow(row))}";

                            row.AddColIfNotExist(anotherSellCol, typeof(decimal));
                            row.AddColIfNotExist(anotherBuyCommissionCol, typeof(decimal));
                            row.AddColIfNotExist(anotherSellCommissionCol, typeof(decimal));
                            row.AddColIfNotExist(anotherBuyCostCol, typeof(decimal));
                            row.AddColIfNotExist(anotherSellCostCol, typeof(decimal));
                            row.AddColIfNotExist(baseAssetProfitColName, typeof(decimal));

                            PartialSell ps = sell.partialSells[sellNo];

                            row[anotherSellCol] = ps.Quantity;
                            row[anotherBuyCommissionCol] = ps.BuyCommission;
                            row[anotherSellCommissionCol] = ps.SellCommission;
                            row[anotherBuyCostCol] = ps.BuyPrice;
                            row[anotherSellCostCol] = ps.SellPrice;

                            if (row[baseAssetProfitColName] == DBNull.Value)
                                row[baseAssetProfitColName] = new decimal(0);

                            decimal prevProfit = row.Field<decimal>(baseAssetProfitColName);

                            row[baseAssetProfitColName] =
                                prevProfit
                                + sell.partialSells[sellNo].SellPrice
                                - ps.BuyPrice
                                - ps.BuyCommission
                                - ps.SellCommission;
                        }
                    }

                
            }

            #region exchange rates

            foreach (DataRow row in inputs.Rows)
            {
                //fill exchange rate which we have
                //string rowBaseCurrency = row[ConfStrings.exchange].ToString().Split('-').First();
                string rowBaseCurrency = exchangeDataProvider.GetBaseAsset(exchangeDataProvider.GetMarketForRow(row));

                string rowBaseCurrencyPair = $"USDT-{rowBaseCurrency}";
                DateTime operationCloseDate = exchangeDataProvider.DataRowToTransaction(row).Closed;//row.Field<DateTime>(ConfStrings.closed);

                if (!row.Table.Columns.Contains(rowBaseCurrencyPair))
                    row.Table.Columns.Add(rowBaseCurrencyPair, typeof(decimal));

                try
                {
                    //find the date and fill the price
                    if (rowBaseCurrency == "USDT")// || rowBaseCurrency == "PLN")
                        row[rowBaseCurrencyPair] = 1;
                    else if (pairsDayClosingPrices.ContainsKey(rowBaseCurrency))// && pairsDayClosingPrices[rowBaseCurrency].ContainsKey()
                        row[rowBaseCurrencyPair] = pairsDayClosingPrices[rowBaseCurrency][pairsDayClosingPrices[rowBaseCurrency].Keys.First(k => k.DayOfYear == operationCloseDate.DayOfYear && k.Year == operationCloseDate.Year)];
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    throw;
                    //ignore
                }

                //add the usd price from previous day
                string usdPriceColum = "USD-PLN";
                if (!row.Table.Columns.Contains(usdPriceColum))
                    row.Table.Columns.Add(usdPriceColum, typeof(decimal));

                decimal prevDayPrice = GetPriceFromDate(operationCloseDate, usdAskForDates);

                row[usdPriceColum] = prevDayPrice;
            }

#endregion exchange rates

            var results = inputs.AsEnumerable()
                .OrderBy(r => r.Field<string>(exchangeDataProvider.GetMarketColumnIndex()))
                //bittrex
                //.ThenBy(r => r.Field<DateTime>(exchangeDataProvider.GetTransactionCloseDateIndex()))

                //binance 
                //.ThenBy(r => r.Field<string>(exchangeDataProvider.GetTransactionCloseDateIndex()))

                //bitbay
                .ThenBy(r => r.Field<DateTime>(exchangeDataProvider.GetTransactionCloseDateIndex()))
                .CopyToDataTable();


            //sell volume
            //buy price
            //buy commission
            //sell price
            //sell commission
            //profit

            results.RenameColumns(exchangeDataProvider.GetColumnsTranslations());
            results.ReorderColumns(exchangeDataProvider.GetColumnsOrder(), exchangeDataProvider.ColumnsReorderIndexOffset());

            Helper h = new Helper();
            h.DataTableToExcel(results, true, Path.Combine(outFilePath, Guid.NewGuid().ToString() + ".xlsx"));
        }

        [TestCase]
        public void DeserializeCandlestickData()
        {
            Dictionary<string,Dictionary<DateTime,decimal>> prices = exchangeDataProvider.GetClosingDailyPricesForBaseAsset(new List<string> { "BTC" });
        }

        public decimal GetPriceFromDate(DateTime date, Dictionary<DateTime, decimal> prices)
        {
            decimal price = 0;
            //price from this day or first from the past (weekends are the problem)
            foreach (var pair in prices)
            {
                if (date.Date > pair.Key.Date)
                    price = pair.Value;
                else
                    break;
            }
            return price;
        }
        
        //public decimal RemainingVolume(List<DataRow> allOperationsBefore, string sellColumnPrefix)
        //{
        //    decimal remaining = new decimal(0);

        //    foreach (var row in allOperationsBefore)
        //    {
        //        //add all buys
        //        if (row.Field<string>(ConfStrings.type) == ConfStrings.buyType)
        //        {
        //            remaining += !(row[ConfStrings.quantity] is decimal) ? 0 : (decimal)row[ConfStrings.quantity];
        //        }
        //        //substract all sells
        //        else
        //        {
        //            foreach (DataColumn c in row.Table.Columns)
        //            {
        //                if (c.ColumnName.StartsWith(sellColumnPrefix))
        //                    remaining -= !(row[c] is decimal) ? 0 : (decimal)row[c];
        //            }
        //        }
        //    }
        //    return remaining; //remaining
        //}

        //public int? IndexOfFirstRowWithNullBalance(DataTable table)
        //{
        //    foreach (DataRow r in table.Rows)
        //        if (r[ConfStrings.balance] == null)
        //            return (int?)table.Rows.IndexOf(r);

        //    return null;
        //}

        //public List<string> FindAllMarkets(DataTable table)
        //{
        //    HashSet<string> hashSet = new HashSet<string>();

        //    foreach (DataRow row in table.Rows)
        //        hashSet.Add((string)row[ConfStrings.exchange]);

        //    return hashSet.ToList();
        //}

        //public decimal BalanceLeft(List<OperationData> operations)
        //{
        //    decimal balanceLeft = 0;
        //    foreach (var o in operations)
        //    {
        //        if (o.Type == ConfStrings.buyType)
        //            balanceLeft += o.Quantity;
        //        else if (o.Type == ConfStrings.sellType)
        //            balanceLeft -= o.Quantity;
        //        else
        //            throw new ArgumentException("invalid operation type");
        //    }

        //    return balanceLeft;
        //}
    }
}