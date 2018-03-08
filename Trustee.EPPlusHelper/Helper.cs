using OfficeOpenXml;
using System;
using System.Data;
using System.IO;

namespace Trustee.EPPlusHelper
{
    public class Helper
    {
        public DataSet GetSheetsContent(string xlsxPath, int headerRows = 0, string password = null)
        {
            if (!File.Exists(xlsxPath))
                throw new Exception($"file not found {xlsxPath}");

            FileInfo fi = new FileInfo(xlsxPath);
            if (fi.Extension != ".xlsx")
                throw new Exception($"incorrect file format {fi.Extension}");

            Stream stream = File.OpenRead(xlsxPath);

            return GetSheetsContent(stream, headerRows,password);
        }

        public DataSet GetSheetsContent(Stream stream, int headerRows = 0, string password = null)
        {
            DataSet dataSet = new DataSet();

            using (ExcelPackage excelPkg = new ExcelPackage())
            {
                if (password == null)
                    excelPkg.Load(stream);
                else
                    excelPkg.Load(stream, password);

                foreach (ExcelWorksheet sheet in excelPkg.Workbook.Worksheets)
                {
                    DataTable t = GetDataFromSheet(sheet, headerRows);
                    if (t != null && t.Rows.Count > 0 && t.Columns.Count > 0)
                        dataSet.Tables.Add(t);
                }
            }

            return dataSet;
        }

        /// <summary>
        /// Reading all the data from the sheet
        /// </summary>
        /// <param name="sheet">sheet to be read</param>
        /// <param name="headerRows">number of header rows. Name of the column will be set to content of the cell taken from the last header row</param>
        /// <returns></returns>
        private DataTable GetDataFromSheet(ExcelWorksheet sheet, int headerRows = 0)
        {
            DataTable dt = new DataTable(sheet.Name);

            if (sheet.Dimension == null)
                return null;

            int rows = sheet.Dimension.End.Row;
            int columns = sheet.Dimension.End.Column;

            if (rows == 0 || columns == 0)
                return null;

            //add columns with names as the content of the last header row
            for (int j = 1; j <= columns; j++)
            {
                string columnName = string.Empty;

                if (headerRows != 0)
                {
                    columnName = sheet.Cells[1, j].Value == null ? j.ToString() :  sheet.Cells[1, j].Value.ToString();

                    while (dt.Columns.Contains(columnName))
                    {
                        columnName = string.Concat(columnName, "_1");
                    }
                }
                else
                {
                    columnName = j.ToString();
                }
                dt.Columns.Add(columnName, typeof(string)); //todo: consider other types of the columns (culture aware)
            }

            //indexed in the excel starting from 1
            for (int i = headerRows+ 1; i <= rows; i++)
            {
                DataRow row = dt.NewRow();
                for (int j = 1; j <= columns; j++)
                {
                    row[j - 1] = sheet.Cells[i, j].Value;
                }

                dt.Rows.Add(row);
            }

            return dt;
        }

        public void DataTableToExcel(DataTable dt, bool includeHeader, string path)
        {
            using (ExcelPackage excelPkg = new ExcelPackage())
            {
                var sheet = excelPkg.Workbook.Worksheets.Add(
                    string.IsNullOrWhiteSpace(dt.TableName)? $"sheet{excelPkg.Workbook.Worksheets.Count+ 1}" : dt.TableName);

                int headerRowIndex = 0;

                if (includeHeader)
                {
                    headerRowIndex = 1;
                    for (int j = 1; j < dt.Columns.Count + 1; j++)
                        sheet.Cells[headerRowIndex, j].Value = dt.Columns[j-1].ColumnName;
                }

                    for (int i= 1; i<dt.Rows.Count + 1; i++)
                        for(int j=1; j<dt.Columns.Count + 1; j++)
                            sheet.Cells[i + headerRowIndex, j].Value = dt.Rows[i - 1][j - 1] is DateTime? $"{dt.Rows[i - 1][j - 1].ToString()}." : dt.Rows[i - 1][j - 1];                    

                excelPkg.SaveAs(new FileInfo(path));
            }

        }
    }
}