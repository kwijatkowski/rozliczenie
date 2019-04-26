using System;
using System.Collections.Generic;
using System.Data;

namespace Test
{

    public class Transaction
    {
        //private static string sellColPrefix = "ilosc sp";
        //private static string profit = "zysk";

        //private static string buyCommissionPrefix = "pr zakup";
        //private static string sellCommissionPrefix = "pr sprzedaż";
        //private static string buyCostCol = "koszt zak";
        //private static string sellIncomeCol = "przych sprz";

        public enum Type { LIMIT_BUY, LIMIT_SELL };

        public string OrderUuid;
        public string Exchange;
        public Type TransactionType;
        public decimal Quantity;
        public decimal Limit;
        public decimal CommissionPaid;
        public decimal Total;
        public DateTime Opened;
        public DateTime Closed;

        public string baseAsset;
        public string commisionAsset;
        public bool processed;
        public decimal availableVolume;
        public List<PartialSell> partialSells = new List<PartialSell>();

        public Transaction() { }

        //public Transaction(string orderuuid, string exchange, Transaction.Type transactionType, decimal quantity, decimal limit,
        //    decimal commisionPaid, decimal price, DateTime opened, DateTime closed)
        //{
        //    OrderUuid = orderuuid;
        //    Exchange = exchange;
        //    TransactionType = transactionType;
        //    Quantity = quantity;
        //    Limit = limit;
        //    CommissionPaid = commisionPaid;
        //    Price = price;
        //    Opened = opened;
        //    Closed = closed;
        //}

    }
}