using GenericParsing;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests
{
    public static class DataTableExtensions
    {
        public static void AddColIfNotExist(this DataRow row, string name, Type type)
        {
            if (!row.Table.Columns.Contains(name))
            {
                row.Table.Columns.Add(name, type);
                if (type == typeof(decimal))
                    row[name] = new decimal(0);
            }
        }

        public static void ReorderColumns(this DataTable dt, List<string> columnsOrderList, int offset)
        {
            Dictionary<string, int> order = new Dictionary<string, int>();
            int currentindex = offset;

            foreach (string colNamePrefix in columnsOrderList)
            {
                List<string> sortedColumnsNames = dt.Columns.Cast<DataColumn>().Where(c => c.ColumnName.StartsWith(colNamePrefix)).OrderBy(c => c.ColumnName).Select(c => c.ColumnName).ToList();

                foreach (string colName in sortedColumnsNames)
                {
                    dt.Columns[colName].SetOrdinal(currentindex);
                    currentindex++;
                }
            }
        }

        public static void RenameColumns(this DataTable dt, Dictionary<string, string> namesMap)
        {
            foreach (DataColumn c in dt.Columns)
            {
                string newName = string.Empty;
                if (namesMap.TryGetValue(c.ColumnName, out newName))
                    c.ColumnName = newName;
            }
        }

        public static DataTable CsvToDataTable(string path, Dictionary<string, Type> expectedColumnsAndTypes, char seperator, string dateTimeFormat, CultureInfo cultureInfo = null)
        {
            if (cultureInfo == null)
                cultureInfo = CultureInfo.InvariantCulture;

            DataTable dt = new DataTable();

            foreach (var pair in expectedColumnsAndTypes)
                dt.Columns.Add(pair.Key, pair.Value);

            using (GenericParser parser = new GenericParser())
            {
                parser.SetDataSource(path);

                parser.ColumnDelimiter = seperator;
                parser.FirstRowHasHeader = true;
                //parser.SkipStartingDataRows = 10;
                parser.MaxBufferSize = 4096;
                parser.MaxRows = int.MaxValue;
                parser.TextQualifier = '\"';
                parser.SkipEmptyRows = true;

                while (parser.Read())
                {
                    DataRow row = dt.NewRow();

                    foreach (var pair in expectedColumnsAndTypes)
                    {
                        //OrderUuid,Exchange,Type,Quantity,Limit,CommissionPaid,Price,Opened,Closed

                        string value = parser[pair.Key];
                        if (string.IsNullOrWhiteSpace(value))
                            continue;

                        if (pair.Value == typeof(decimal))
                        {
                            row[pair.Key] = string.IsNullOrEmpty(value) ? 0 : decimal.Parse(value, cultureInfo);
                        }
                        else if (pair.Value == typeof(DateTime))
                        {
                            row[pair.Key] = DateTime.ParseExact(value, dateTimeFormat, cultureInfo);
                        }
                        else
                        {
                            row[pair.Key] = value;
                        }
                    }
                    dt.Rows.Add(row);
                }
            }
            return dt;
        }
    }
}
