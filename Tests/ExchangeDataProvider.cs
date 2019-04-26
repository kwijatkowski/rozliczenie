using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Test;
using System.Data;

namespace Tests
{
    public abstract class ExchangeDataProvider : IExchangeDataProvider
    {
        public List<Transaction> DataTableToTransactionList(DataTable dt)
        {
            //handle empty rows 
            List<Transaction> transactions = new List<Transaction>();
            foreach (DataRow row in dt.Rows)
            {
                    transactions.Add(DataRowToTransaction(row));
            }

            return transactions;
        }

        public abstract Dictionary<string, Dictionary<DateTime, decimal>> GetClosingDailyPricesForBaseAsset(List<string> assets);

        public abstract DataTable inputFileToDataTable(string filePath);

        public abstract Transaction DataRowToTransaction(DataRow row);
        public abstract List<String> GetBaseCurrencies();
        public abstract String GetBaseAsset(string pair);
        public abstract int GetMarketColumnIndex();
        public abstract string GetMarketForRow(DataRow row);
        public abstract string GetTransactionUidForRow(DataRow row);
        public abstract int GetTransactionCloseDateIndex();
        public abstract void UpdateExchangeSpecificRows(DataRow row, Transaction t);
        public abstract int ColumnsReorderIndexOffset();
        public abstract Dictionary<string, string> GetColumnsTranslations();
        public abstract List<string> GetColumnsOrder();
    }
}
