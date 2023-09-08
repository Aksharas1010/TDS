using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Web.Mvc;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.Drawing;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.Extensions.Logging;
using WebApplication1.DTO;
using WebApplication1.Models;
using WebApplication1.Utils;

public class TDSReportgenerationController : Controller
{
    private static readonly object _object = new object();

    //private readonly ILogger<TDSReportgenerationController> _logger;
    ////private static readonly ILog _logger = LogManager.GetLogger(typeof(Worker));
    //public TDSReportgenerationController(ILogger<TDSReportgenerationController> logger) => (_logger) = (logger);

    static int queryTimeout = !string.IsNullOrEmpty(ConfigurationManager.AppSettings["queryTimeout"]) ?
    Convert.ToInt32(ConfigurationManager.AppSettings["queryTimeout"]) : 1800;

    private int _maxRetryCount = Convert.ToInt32(ConfigurationManager.AppSettings["ProcessRetryCount"]);
    public ActionResult Index()
    {
        return View();
    }
    public ActionResult GenerateExcel()
    {
        try
        {
            using (GCCEntities gcc = new GCCEntities())
            {
                int clientid = 1291229150;
                gcc.Database.CommandTimeout = queryTimeout;
                string fileName ="ComputationReport-"+$"{clientid}_{DateTime.Now.ToString("yyyyMMddhhmmssfff")}";
                string folderPath = ConfigurationManager.AppSettings["FileSavePath"];
                string path = folderPath + fileName + ".xlsx";
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }
                using (var wbook = new XLWorkbook())
                {
                    string todate = "2023-09-06";
                    string fromdate = "2023-09-06";
                    DateTime currentDate = DateTime.Now;
                    int financialYearStartMonth = 4;
                    var finyear = "";
                    if (currentDate.Month < financialYearStartMonth)
                    {
                        int financialYearStart = currentDate.Year - 1;
                        int financialYearEnd = currentDate.Year;
                        finyear = financialYearStart + "-" + financialYearEnd;
                    }
                    else
                    {
                        int financialYearStart = currentDate.Year;
                        int financialYearEnd = currentDate.Year + 1;
                        finyear = financialYearStart + "-" + financialYearEnd;
                    }

                    string formattedDate = "2023-09-06";
                    var todDateParam = new SqlParameter("@todate", SqlDbType.VarChar, 20) { Value = formattedDate };
                    var clientIdParam = new SqlParameter("@clientid", SqlDbType.Int) { Value = DBNull.Value };

                    var query = $"EXEC SpGetDailyTransactionDetails '{todate}','{fromdate}',{clientid},'{finyear}'";
                    var transactionlist = gcc.Database.SqlQuery<Transactions>(query).ToList();
                    var closingRateForLongTerCapGainasJanuaryList = gcc.ClosingRateForLongTerCapGainasJanuaries.ToList();
                    if (transactionlist.Count > 0)
                    {
                        var xlBuyNotFoundWorkSheet = wbook.Worksheets.Add("Final Report");
                        var summaryLastRow = 2;
                        var summaryColumnIndex = 0;

                        xlBuyNotFoundWorkSheet.Range("B2:O2").Merge();
                        xlBuyNotFoundWorkSheet.Range("B2:O2").Style.Fill.BackgroundColor = XLColor.Yellow;
                        var rangerow = xlBuyNotFoundWorkSheet.Range("B2:O2");
                        rangerow.Style.Border.TopBorder = XLBorderStyleValues.Thin;
                        rangerow.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                        rangerow.Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                        rangerow.Style.Border.RightBorder = XLBorderStyleValues.Thin;
                        string[] headings = {
                        "PAN", "Trade Code", "ISIN", "Back Office Security Code", "Type Of Security", "Purchase Details"
                        };

                        int column = 2;
                        int startRow = 3;
                        int endRow = 4;

                        for (int i = 0; i < headings.Length; i++)
                        {
                            var range = xlBuyNotFoundWorkSheet.Range(startRow, column, endRow, column);
                            range.Merge().Value = headings[i];
                            range.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                            range.Style.Border.TopBorder = XLBorderStyleValues.Thin;
                            range.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                            range.Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                            range.Style.Border.RightBorder = XLBorderStyleValues.Thin;

                            xlBuyNotFoundWorkSheet.Column(7 + i).Width = 20;
                            column++;
                        }
                        //Purchase Details
                        var purchaseDetailsRange = xlBuyNotFoundWorkSheet.Range("G3:O3");
                        purchaseDetailsRange.Merge().Value = "Purchase Details";
                        purchaseDetailsRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        xlBuyNotFoundWorkSheet.Range("B3:O4").Style.Fill.BackgroundColor = XLColor.PeachPuff;
                        string[] headings2 = {
                        "Tr Date", "Qnty", "Pur price per share", "31st jan,2018 price per share", "Final purchase price considered",
                        "Total value", "STT Paid", "Other charges", "Cost of Acquisition"
                        };
                        int col = 7;
                        int row = 4;
                        for (int i = 0; i < headings2.Length; i++)
                        {
                            var range = xlBuyNotFoundWorkSheet.Range(row, col, row, col);
                            range.Merge().Value = headings2[i];
                            range.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                            range.Style.Border.TopBorder = XLBorderStyleValues.Thin;
                            range.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                            range.Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                            range.Style.Border.RightBorder = XLBorderStyleValues.Thin;
                            xlBuyNotFoundWorkSheet.Column(col + i).Width = 20;
                            xlBuyNotFoundWorkSheet.Row(row).Height = 30;
                            col++;
                        }
                        int dataRow = endRow + 1;
                        int purchasecol = 2;
                      
                        foreach (var item in transactionlist)
                        {
                            ExcelUtils.SetValueToCell(xlBuyNotFoundWorkSheet, dataRow, 2, item.Pan);
                            ExcelUtils.SetValueToCell(xlBuyNotFoundWorkSheet, dataRow, 3, item.ClientCode);
                            ExcelUtils.SetValueToCell(xlBuyNotFoundWorkSheet, dataRow, 4, item.ISIN);
                            ExcelUtils.SetValueToCell(xlBuyNotFoundWorkSheet, dataRow, 5, item.Security);
                            ExcelUtils.SetValueToCell(xlBuyNotFoundWorkSheet, dataRow, 6, item.SecurityType);

                            ExcelUtils.SetValueToCell(xlBuyNotFoundWorkSheet, dataRow, 7, item.TranDateBuy);
                            ExcelUtils.SetValueToCell(xlBuyNotFoundWorkSheet, dataRow, 8, item.BuyQty);
                            ExcelUtils.SetValueToCell(xlBuyNotFoundWorkSheet, dataRow, 9, item.BuyValue / item.BuyQty);

                            var security = item.Security.Trim().ToLower();
                            var matchingRate = closingRateForLongTerCapGainasJanuaryList
                                .FirstOrDefault(p => p.Security.Trim().ToLower() == security)?.Rate;

                            var fairMarketBuyValue = matchingRate.HasValue ? (matchingRate.Value * item.SaleQty).ToString() : "-";
                            ExcelUtils.SetValueToCell(xlBuyNotFoundWorkSheet, dataRow,10, closingRateForLongTerCapGainasJanuaryList.Any(p => p.Security.Trim().ToLower() == item.Security.Trim().ToLower()) ?
                                           closingRateForLongTerCapGainasJanuaryList.FirstOrDefault(p => p.Security.Trim().ToLower() == item.Security.Trim().ToLower()).Rate.ToString() : null);

                           // ExcelUtils.SetValueToCell(xlBuyNotFoundWorkSheet, dataRow, 10, fairMarketBuyValue);
                            ExcelUtils.SetValueToCell(xlBuyNotFoundWorkSheet, dataRow, 11, item.BuyValue / item.BuyQty);
                            ExcelUtils.SetValueToCell(xlBuyNotFoundWorkSheet, dataRow, 12, item.BuyValue);
                            ExcelUtils.SetValueToCell(xlBuyNotFoundWorkSheet, dataRow, 13, "stt");
                            ExcelUtils.SetValueToCell(xlBuyNotFoundWorkSheet, dataRow, 14, item.BuyExpense);
                            ExcelUtils.SetValueToCell(xlBuyNotFoundWorkSheet, dataRow, 15, item.BuyValue - item.BuyExpense);
                            for (int i = 2; i <= 15; i++)
                            {
                                var cell = xlBuyNotFoundWorkSheet.Cell(dataRow, i);
                                cell.Style.Border.TopBorder = XLBorderStyleValues.Thin;
                                cell.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                                cell.Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                                cell.Style.Border.RightBorder = XLBorderStyleValues.Thin;
                                cell.Style.Fill.BackgroundColor = XLColor.LightSteelBlue;
                            }
                            dataRow++;
                        }

                        //Sale details
                        xlBuyNotFoundWorkSheet.Range("Q2:AB2").Merge();
                        xlBuyNotFoundWorkSheet.Range("Q2:AB2").Style.Fill.BackgroundColor = XLColor.Yellow;
                        var rangerowsales = xlBuyNotFoundWorkSheet.Range("Q2:AB2");
                        rangerowsales.Merge().Value = "Part II Sales Details";
                        rangerowsales.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        rangerowsales.Style.Border.TopBorder = XLBorderStyleValues.Thin;
                        rangerowsales.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                        rangerowsales.Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                        rangerowsales.Style.Border.RightBorder = XLBorderStyleValues.Thin;
                        string[] headingssales = {
                        "PAN", "Trade Code", "ISIN", "Back Office Security Code", "Type Of Security", "Purchase Details"
                        };
                        int startColumn = 17; // Column Q
                        int endColumn = 22;   // Column U
                        int mergeStartRow = 3; // Row Q3
                        int mergeEndRow = 4;   // Row U4
                        for (int i = 0; i < headingssales.Length; i++)
                        {
                            var range = xlBuyNotFoundWorkSheet.Range(mergeStartRow, startColumn + i, mergeEndRow, startColumn + i);
                            range.Merge().Value = headingssales[i];
                            range.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                            range.Style.Border.TopBorder = XLBorderStyleValues.Thin;
                            range.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                            range.Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                            range.Style.Border.RightBorder = XLBorderStyleValues.Thin;
                            xlBuyNotFoundWorkSheet.Column(startColumn + i).Width = 10;
                        }
                        xlBuyNotFoundWorkSheet.Row(mergeStartRow).Height = 20;
                        dataRow = mergeEndRow + 1;
                        foreach (var item in transactionlist)
                        {
                            ExcelUtils.SetValueToCell(xlBuyNotFoundWorkSheet, dataRow, 17, item.Pan);
                            ExcelUtils.SetValueToCell(xlBuyNotFoundWorkSheet, dataRow, 18, item.ClientCode);
                            ExcelUtils.SetValueToCell(xlBuyNotFoundWorkSheet, dataRow, 19, item.ISIN);
                            ExcelUtils.SetValueToCell(xlBuyNotFoundWorkSheet, dataRow, 20, item.Security);
                            ExcelUtils.SetValueToCell(xlBuyNotFoundWorkSheet, dataRow, 21, item.SecurityType);

                            ExcelUtils.SetValueToCell(xlBuyNotFoundWorkSheet, dataRow, 22, item.TranDateSale);
                            ExcelUtils.SetValueToCell(xlBuyNotFoundWorkSheet, dataRow, 23, item.SaleQty);
                            ExcelUtils.SetValueToCell(xlBuyNotFoundWorkSheet, dataRow, 24, item.SaleValue / item.SaleQty);
                            ExcelUtils.SetValueToCell(xlBuyNotFoundWorkSheet, dataRow, 25, item.SaleValue);
                            ExcelUtils.SetValueToCell(xlBuyNotFoundWorkSheet, dataRow, 26, "stt");
                            ExcelUtils.SetValueToCell(xlBuyNotFoundWorkSheet, dataRow, 27, item.SellExpense);
                            ExcelUtils.SetValueToCell(xlBuyNotFoundWorkSheet, dataRow, 28, item.SaleValue - item.SellExpense);
                            for (int i = 17; i <= 28; i++)
                            {
                                var cell = xlBuyNotFoundWorkSheet.Cell(dataRow, i);
                                cell.Style.Border.TopBorder = XLBorderStyleValues.Thin;
                                cell.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                                cell.Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                                cell.Style.Border.RightBorder = XLBorderStyleValues.Thin;
                                cell.Style.Fill.BackgroundColor = XLColor.LightSteelBlue;
                            }
                            dataRow++;
                        }
                        var salesDetailsRange = xlBuyNotFoundWorkSheet.Range("V3:AB3");
                        salesDetailsRange.Merge().Value = "Sales Details";
                        salesDetailsRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        salesDetailsRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        salesDetailsRange.Style.Border.TopBorder = XLBorderStyleValues.Thin;
                        salesDetailsRange.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                        salesDetailsRange.Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                        salesDetailsRange.Style.Border.RightBorder = XLBorderStyleValues.Thin;
                        xlBuyNotFoundWorkSheet.Range("Q3:AB4").Style.Fill.BackgroundColor = XLColor.PeachPuff;
                        string[] headingssales2 = {
                        "Tr Date", "Qnty", "Sale price per share",
                        "Total value", "STT Paid", "Other charges", "Net Consideration"
                        };
                        int startCol = 22; // Column V
                        for (int i = 0; i < headingssales2.Length; i++)
                        {
                            var cell = xlBuyNotFoundWorkSheet.Cell(4, startCol + i);
                            cell.Value = headingssales2[i];
                            cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                            cell.Style.Border.TopBorder = XLBorderStyleValues.Thin;
                            cell.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                            cell.Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                            cell.Style.Border.RightBorder = XLBorderStyleValues.Thin;
                            xlBuyNotFoundWorkSheet.Column(startCol + i).Width = 10;
                        }

                        xlBuyNotFoundWorkSheet.Range("AD2:AE2").Merge();
                        xlBuyNotFoundWorkSheet.Range("AG2:AK2").Merge();
                        xlBuyNotFoundWorkSheet.Range("AM2:AP2").Merge();
                        xlBuyNotFoundWorkSheet.Range("AD2:AE2").Style.Fill.BackgroundColor = XLColor.Yellow;
                        xlBuyNotFoundWorkSheet.Range("AG2:AK2").Style.Fill.BackgroundColor = XLColor.Yellow;
                        xlBuyNotFoundWorkSheet.Range("AM2:AP2").Style.Fill.BackgroundColor = XLColor.Yellow;
                        var rangerowgain = xlBuyNotFoundWorkSheet.Range("AD2:AE2");
                        rangerowgain.Merge().Value = "Type of Gain";
                        var rangerowgainlg = xlBuyNotFoundWorkSheet.Range("AG2:AK2");
                        rangerowgainlg.Merge().Value = "Long Term Capital Gain";
                        var rangerowgainsg = xlBuyNotFoundWorkSheet.Range("AM2:AP2");
                        rangerowgainsg.Merge().Value = "Short Term Capital Gain";
                        rangerowgain.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        rangerowgainlg.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        rangerowgainsg.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        rangerowgainlg.Style.Border.TopBorder = XLBorderStyleValues.Thin;
                        rangerowgainlg.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                        rangerowgainlg.Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                        rangerowgainlg.Style.Border.RightBorder = XLBorderStyleValues.Thin;
                        rangerowgainsg.Style.Border.TopBorder = XLBorderStyleValues.Thin;
                        rangerowgainsg.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                        rangerowgainsg.Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                        rangerowgainsg.Style.Border.RightBorder = XLBorderStyleValues.Thin;
                        rangerowgain.Style.Border.TopBorder = XLBorderStyleValues.Thin;
                        rangerowgain.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                        rangerowgain.Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                        rangerowgain.Style.Border.RightBorder = XLBorderStyleValues.Thin;
                        string[] headingsgain = {
                        "POH", "Long(L)/Short(S) term","","Type of Security", "Sale Value","Coa","Capital Gain","TDS to be deducted","","Sale Value","Coa","Capital Gain","TDS to be deducted"
                        };
                        startColumn = 30; // Column AD
                        endColumn = 31;   // Column AE
                        for (int i = 0; i < headingsgain.Length; i++)
                        {
                            var x = startColumn + i;
                            var range = xlBuyNotFoundWorkSheet.Range(mergeStartRow, x, mergeEndRow, x);
                            range.Merge().Value = headingsgain[i];
                            range.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                            range.Style.Border.TopBorder = XLBorderStyleValues.Thin;
                            range.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                            range.Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                            range.Style.Border.RightBorder = XLBorderStyleValues.Thin;
                            if (x != 32 && x != 38)
                            {

                                range.Style.Fill.BackgroundColor = XLColor.PeachPuff;
                            }
                            xlBuyNotFoundWorkSheet.Column(x).Width = 10;
                        }
                        dataRow = 5; int datacolstart = 30;
                        foreach (var item in transactionlist)
                        {
                            ExcelUtils.SetValueToCell(xlBuyNotFoundWorkSheet, dataRow, datacolstart, item.DayToSell);
                            ExcelUtils.SetValueToCell(xlBuyNotFoundWorkSheet, dataRow, datacolstart+1, item.Type);
                            ExcelUtils.SetValueToCell(xlBuyNotFoundWorkSheet, dataRow, datacolstart+2, "");                         
                            ExcelUtils.SetValueToCell(xlBuyNotFoundWorkSheet, dataRow, datacolstart+3, item.SecurityType);                         
                            ExcelUtils.SetValueToCell(xlBuyNotFoundWorkSheet, dataRow, datacolstart+4, item.Type=="LG" ? item.SaleValue.ToString():"-");
                            ExcelUtils.SetValueToCell(xlBuyNotFoundWorkSheet, dataRow, datacolstart+5, item.Type=="LG"?item.BuyValue.ToString():"-");
                            ExcelUtils.SetValueToCell(xlBuyNotFoundWorkSheet, dataRow, datacolstart + 6, item.Type == "LG" ? (item.Profit > 0 ? item.Profit.ToString() : $"({Math.Abs(item.Profit)})") : "-");
                            ExcelUtils.SetValueToCell(xlBuyNotFoundWorkSheet, dataRow, datacolstart+7, item.Type == "LG" ? (item.TaxAmount > 0 ? item.TaxAmount.ToString() : $"({Math.Abs(item.TaxAmount)})") : "-");

                            ExcelUtils.SetValueToCell(xlBuyNotFoundWorkSheet, dataRow, datacolstart+8, "");
                            ExcelUtils.SetValueToCell(xlBuyNotFoundWorkSheet, dataRow, datacolstart+9, item.Type=="SG"?item.SaleValue.ToString():"-");
                            ExcelUtils.SetValueToCell(xlBuyNotFoundWorkSheet, dataRow, datacolstart+10, item.Type == "SG" ? item.BuyValue.ToString():"-");
                            ExcelUtils.SetValueToCell(xlBuyNotFoundWorkSheet, dataRow, datacolstart + 11, item.Type == "SG" ? (item.Profit > 0 ? item.Profit.ToString() : $"({Math.Abs(item.Profit)})") : "-");
                            ExcelUtils.SetValueToCell(xlBuyNotFoundWorkSheet, dataRow, datacolstart + 11, item.Type == "SG" ? (item.TaxAmount > 0 ? item.TaxAmount.ToString() : $"({Math.Abs(item.TaxAmount)})") : "-");

                            for (int i = 30; i <= 42; i++)
                            {
                                if (i != 32 && i != 38)
                                {
                                    var cell = xlBuyNotFoundWorkSheet.Cell(dataRow, i);
                                    cell.Style.Border.TopBorder = XLBorderStyleValues.Thin;
                                    cell.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                                    cell.Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                                    cell.Style.Border.RightBorder = XLBorderStyleValues.Thin;
                                    cell.Style.Fill.BackgroundColor = XLColor.LightSteelBlue;
                                }
                                
                            }
                            dataRow++;
                        }
                        using(var memoryStream = new MemoryStream())
                        {
                            wbook.SaveAs(memoryStream);
                            memoryStream.Seek(0, SeekOrigin.Begin);

                            using (var fileStream = new FileStream(path, FileMode.Create))
                            {
                                memoryStream.CopyTo(fileStream);
                            }
                        }

                        // Return the file for download
                        byte[] fileBytes = System.IO.File.ReadAllBytes(path);
                        return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);


                    }
                }


            }
            return null;
        }
        catch (Exception ex)
        {
            // _logger.LogError($"Error occurred: {ex}");
            return Content("An error occurred while generating the Excel file.");
        }
    }



    public ActionResult GenerateExcelReturnformat()
    {
        try
        {
            using (GCCEntities gcc = new GCCEntities())
            {
                int clientid = 1291229150;
                gcc.Database.CommandTimeout = queryTimeout;
                string fileName = "TaxReturn-" + $"{clientid}_{DateTime.Now.ToString("yyyyMMddhhmmssfff")}";
                string folderPath = ConfigurationManager.AppSettings["FileSavePath"];
                string path = folderPath + fileName + ".xlsx";
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }
                using (var wbook = new XLWorkbook())
                {
                    string todate = "2023-09-06";
                    string fromdate = "2023-09-06";
                    DateTime currentDate = DateTime.Now;
                    int financialYearStartMonth = 4;
                    var finyear = "";
                    if (currentDate.Month < financialYearStartMonth)
                    {
                        int financialYearStart = currentDate.Year - 1;
                        int financialYearEnd = currentDate.Year;
                        finyear = financialYearStart + "-" + financialYearEnd;
                    }
                    else
                    {
                        int financialYearStart = currentDate.Year;
                        int financialYearEnd = currentDate.Year + 1;
                        finyear = financialYearStart + "-" + financialYearEnd;
                    }
                    string formattedDate = "2023-09-06";
                    var todDateParam = new SqlParameter("@todate", SqlDbType.VarChar, 20) { Value = formattedDate };
                    var clientIdParam = new SqlParameter("@clientid", SqlDbType.Int) { Value = DBNull.Value };
                    var query = $"EXEC SpGetDailyTransactionDetails '{todate}','{fromdate}',{clientid},'{finyear}'";
                    var transactionlist = gcc.Database.SqlQuery<Transactions>(query).ToList();
                    var closingRateForLongTerCapGainasJanuaryList = gcc.ClosingRateForLongTerCapGainasJanuaries.ToList();
                    if (transactionlist.Count > 0)
                    {
                        var xlBuyNotFoundWorkSheet = wbook.Worksheets.Add("TDS");
                        var summaryLastRow = 2;
                        var summaryColumnIndex = 0;

                        
                        string[] headings = {
                        "Sl No.", "Section Under Payment Made", "Deductee Code(01-Company)(02-Others)",
                            "PAN of the deductee", "Name of the deductee", "Date of Payment/Credit",
                        "Amount Paid/Credited Rs","TDS","Surcharge","Educational Cess","Total tax Deducted","Total tax Deposited","Date of Deduction","Rate at which deducted"
                        };
                        for (int i = 0; i < headings.Length; i++)
                        {
                            var range = xlBuyNotFoundWorkSheet.Range(1, i + 1, 1, i + 1); // Start from row 1 (A1 to N1)
                            range.Merge().Value = headings[i];
                            range.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                            range.Style.Border.TopBorder = XLBorderStyleValues.Thin;
                            range.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                            range.Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                            range.Style.Border.RightBorder = XLBorderStyleValues.Thin;
                            range.Style.Fill.BackgroundColor = XLColor.SkyBlue;
                            xlBuyNotFoundWorkSheet.Column(i + 1).Width = 20; // Adjust column width
                        }
                        using (var memoryStream = new MemoryStream())
                        {
                            wbook.SaveAs(memoryStream);
                            memoryStream.Seek(0, SeekOrigin.Begin);

                            using (var fileStream = new FileStream(path, FileMode.Create))
                            {
                                memoryStream.CopyTo(fileStream);
                            }
                        }
                        byte[] fileBytes = System.IO.File.ReadAllBytes(path);
                        return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
                    }
                }
            }
            return null;
        }
        catch (Exception ex)
        {         
            return Content("An error occurred while generating the Excel file.");
        }
    }
}
