using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using Test;

namespace Tests
{
    interface IExchangeDataProvider
    {
        Dictionary<string, Dictionary<DateTime, decimal>> GetClosingDailyPricesForBaseAsset(List<String> assets);
        DataTable inputFileToDataTable(string filePath);
        Transaction DataRowToTransaction(DataRow row);
        List<Transaction> DataTableToTransactionList(DataTable table);
        List<String> GetBaseCurrencies();
        String GetBaseAsset(string pair);
        int GetMarketColumnIndex();
        string GetMarketForRow(DataRow row);
        string GetTransactionUidForRow(DataRow row);
        int GetTransactionCloseDateIndex();
        void UpdateExchangeSpecificRows(DataRow row, Transaction t);
        int ColumnsReorderIndexOffset();
        Dictionary<string, string> GetColumnsTranslations();
        List<string> GetColumnsOrder();
    }
}
