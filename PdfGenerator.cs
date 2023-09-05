using System;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Kernel.Pdf.Canvas;
using iText.IO.Image;
using System.IO;
using GenerateTDSService.DTO;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using iText.Layout.Properties;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Collections;
using System.Runtime.Remoting.Messaging;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using iText.IO.Font.Constants;
using iText.Kernel.Font;
using iText.Kernel.Colors;
using iText.Layout.Borders;
using GenerateTDSService.Model;

namespace GenerateTDSService.Utils
{
    public class PdfGenerator
    {
        public static bool GenerateBuyNotFoundExcel(IEnumerable<BuyNotFound> groupedlist,string fileName, string folderPath)
        {
            try
            {
                string path = folderPath + fileName + ".pdf";
                if (!groupedlist.Any())
                    return false;
                
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

               
                using (PdfDocument pdfDoc = new PdfDocument(new PdfWriter(new FileStream(path, FileMode.Create, FileAccess.Write))))
                {
                    using (Document doc = new Document(pdfDoc))
                    {
                        pdfDoc.AddNewPage();
                        float width = pdfDoc.GetDefaultPageSize().GetWidth();
                        float height = pdfDoc.GetDefaultPageSize().GetHeight();
                        PdfCanvas canvas = new PdfCanvas(pdfDoc.GetFirstPage());
                        canvas.Rectangle(20, 20, width - 40, height - 40);
                        canvas.Stroke();


                        float[] pointColumnWidths = { 100F, 100F, 100F, 100F, 100F, 100F,100F };
                        Table table = new Table(pointColumnWidths);
                        Style headerCellStyle = new Style().SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD)).SetFontSize(12);
                        Style cellStyle = new Style().SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA)).SetFontSize(10);

                        table.AddHeaderCell(new Cell().Add(new Paragraph("Client Code").AddStyle(headerCellStyle)));
                        table.AddHeaderCell(new Cell().Add(new Paragraph("Security").AddStyle(headerCellStyle)));
                        table.AddHeaderCell(new Cell().Add(new Paragraph("ISIN").AddStyle(headerCellStyle)));
                        table.AddHeaderCell(new Cell().Add(new Paragraph("Trade Date").AddStyle(headerCellStyle)));
                        table.AddHeaderCell(new Cell().Add(new Paragraph("Trade Code").AddStyle(headerCellStyle)));
                        table.AddHeaderCell(new Cell().Add(new Paragraph("Qty").AddStyle(headerCellStyle)));
                        table.AddHeaderCell(new Cell().Add(new Paragraph("SellValue").AddStyle(headerCellStyle)));

                        foreach (var clients in groupedlist)
                        {
                                table.AddCell(new Cell().Add(new Paragraph(clients.ClientCode.ToString()).AddStyle(cellStyle)));
                                table.AddCell(new Cell().Add(new Paragraph(clients.Security.ToString()).AddStyle(cellStyle)));
                                table.AddCell(new Cell().Add(new Paragraph(clients.ISIN.ToString()).AddStyle(cellStyle)));
                                table.AddCell(new Cell().Add(new Paragraph(clients.Trandate.ToShortDateString()).AddStyle(cellStyle)));
                                table.AddCell(new Cell().Add(new Paragraph(clients.TradeCode.ToString()).AddStyle(cellStyle)));
                                table.AddCell(new Cell().Add(new Paragraph(clients.Qty.ToString()).AddStyle(cellStyle)));
                                table.AddCell(new Cell().Add(new Paragraph(clients.SellValue.ToString()).AddStyle(cellStyle)));
                                //table.AddCell(clients.ClientCode.ToString());
                                //table.AddCell(clients.Security);
                                //table.AddCell(clients.ISIN);
                                //table.AddCell(clients.Trandate.ToString("yyyy-MM-dd"));
                                //table.AddCell(clients.TradeCode);
                                //table.AddCell(clients.Qty.ToString());
                                //table.AddCell(clients.SellValue.ToString());
                        }                        
                        doc.Add(table);
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                //throw ex;
                //Console.WriteLine(ex.Message);
                return false;
            }
            return true;
        }
        public static bool GenerateTDSPdf(List<TDSReturn> TaxComputedlist, List<DailyTransactions> DailyTransactionlst, List<QuarterSummary> QuarterSummarylst,string path)
        {
            try
            {
                decimal sumOfLGGain = 0;
                decimal sumOfSGGain = 0;
                if (!DailyTransactionlst.Any())
                    return false;
                using (PdfDocument pdfDoc = new PdfDocument(new PdfWriter(new FileStream(path, FileMode.Create, FileAccess.Write))))
                {
                    using (Document doc = new Document(pdfDoc))
                    {
                        pdfDoc.AddNewPage();
                        float width = pdfDoc.GetDefaultPageSize().GetWidth();
                        float height = pdfDoc.GetDefaultPageSize().GetHeight();
                        PdfCanvas canvas = new PdfCanvas(pdfDoc.GetFirstPage());
                        doc.SetMargins(5, 5, 5, 5);
                       

                        Paragraph heading = new Paragraph("Geojit Financial Service Ltd").SetTextAlignment(TextAlignment.CENTER).SetFontSize(12).SetBold();
                        doc.Add(heading);
                        Paragraph Subheading = new Paragraph("CAPITAL GAIN STATEMENT").SetTextAlignment(TextAlignment.CENTER).SetFontSize(10);
                        doc.Add(Subheading);

                        Paragraph paragraph = new Paragraph().Add(new Text("CustomerName: " + DailyTransactionlst[0].ClientName).SetTextAlignment(TextAlignment.LEFT)
                                           .SetFontSize(8)).Add("     ")
                                           .Add(new Text("TradeCode: " + DailyTransactionlst[0].TradeCode)
                                               .SetTextAlignment(TextAlignment.CENTER)
                                               .SetFontSize(8)).Add("    ").Add(new Text("Trade Date: " + DailyTransactionlst[0].TranDateSale.ToShortDateString())
                                               .SetTextAlignment(TextAlignment.CENTER)
                                               .SetFontSize(8));
                        doc.Add(paragraph);
                        float[] pointColumnWidths = { 50F, 100F, 100F, 40F, 100F, 100F, 50F,100F,40F};
                        Table table = new Table(pointColumnWidths);
                        //table.SetHeight(15);

                        Style headerCellStyle = new Style().SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA)).SetFontSize(8);
                        Style cellStyle = new Style().SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA)).SetFontSize(6);


                        table.AddHeaderCell(new Cell(1, 1).Add(new Paragraph("EXCH").AddStyle(headerCellStyle)));
                        table.AddHeaderCell(new Cell(1, 1).Add(new Paragraph("Scrip name").AddStyle(headerCellStyle)));
                        table.AddHeaderCell(new Cell(1, 1).Add(new Paragraph("Pur Date").AddStyle(headerCellStyle)));
                        table.AddHeaderCell(new Cell(1, 1).Add(new Paragraph("Qty").AddStyle(headerCellStyle)));
                        table.AddHeaderCell(new Cell(1, 1).Add(new Paragraph("Pur Value[A]").AddStyle(headerCellStyle)));
                        table.AddHeaderCell(new Cell(1, 1).Add(new Paragraph("Sale Date").AddStyle(headerCellStyle)));
                        table.AddHeaderCell(new Cell(1, 1).Add(new Paragraph("Sale Value[B]").AddStyle(headerCellStyle)));
                        table.AddHeaderCell(new Cell(1, 1).Add(new Paragraph("Gain/Loss[C=B-A]").AddStyle(headerCellStyle)));
                        table.AddHeaderCell(new Cell(1, 1).Add(new Paragraph("Gain Type").AddStyle(headerCellStyle)));
                        foreach (var clients in DailyTransactionlst)
                        {
                            table.AddCell(new Cell().Add(new Paragraph(clients.PRODUCT.ToString()).AddStyle(cellStyle)));
                            table.AddCell(new Cell().Add(new Paragraph(clients.Security.ToString()).AddStyle(cellStyle)));
                            table.AddCell(new Cell().Add(new Paragraph(clients.TranDateBuy.ToShortDateString()).AddStyle(cellStyle)));
                            table.AddCell(new Cell().Add(new Paragraph(clients.BuyQty.ToString()).AddStyle(cellStyle)));
                            table.AddCell(new Cell().Add(new Paragraph(clients.BuyValue.ToString()).AddStyle(cellStyle)));
                            table.AddCell(new Cell().Add(new Paragraph(clients.TranDateSale.ToShortDateString()).AddStyle(cellStyle)));
                            table.AddCell(new Cell().Add(new Paragraph(clients.SaleValue.ToString()).AddStyle(cellStyle)));
                            table.AddCell(new Cell().Add(new Paragraph((clients.SaleValue - clients.BuyValue).ToString()).AddStyle(cellStyle)));
                            table.AddCell(new Cell().Add(new Paragraph(clients.Type.ToString()).AddStyle(cellStyle)));
                        }
                        
                        Cell spanningCell = new Cell(1, 4).Add(new Paragraph("Total Long term\nTotal Short term")).SetFontColor(DeviceRgb.RED);
                        spanningCell.AddStyle(cellStyle);
                        table.AddCell(spanningCell);
                        for (int i = 0; i < 5; i++)
                        {
                            if (i == 0)
                            {
                                decimal sumOfLGBuyValue = DailyTransactionlst
                                            .Where(transaction => transaction.Type == "LG") 
                                            .Sum(transaction => transaction.BuyValue);
                                decimal sumOfSGBuyValue = DailyTransactionlst
                                            .Where(transaction => transaction.Type == "SG") 
                                            .Sum(transaction => transaction.BuyValue);
                                Cell totalPurValueCell = new Cell(1, 1)
                                    .Add(new Paragraph($"{sumOfLGBuyValue}\n{sumOfSGBuyValue}")) // Replace with your actual calculations
                                    .AddStyle(cellStyle)
                                    .SetFontColor(DeviceRgb.RED);
                                table.AddCell(totalPurValueCell);                                
                            }
                            else if(i==2)                                
                            {
                                decimal sumOfLGSaleValue = DailyTransactionlst
                                            .Where(transaction => transaction.Type == "LG")
                                            .Sum(transaction => transaction.SaleValue);
                                decimal sumOfSGSaleValue = DailyTransactionlst
                                            .Where(transaction => transaction.Type == "SG")
                                            .Sum(transaction => transaction.SaleValue);
                                Cell totalPurValueCell = new Cell(1, 1)
                                    .Add(new Paragraph($"{sumOfLGSaleValue}\n{sumOfSGSaleValue}")) // Replace with your actual calculations
                                    .AddStyle(cellStyle)
                                    .SetFontColor(DeviceRgb.RED);
                                table.AddCell(totalPurValueCell);
                            }
                            else if (i == 3)
                            {
                                 sumOfLGGain = DailyTransactionlst
                                            .Where(transaction => transaction.Type == "LG")
                                            .Sum(transaction => transaction.SaleValue-transaction.BuyValue);
                                 sumOfSGGain = DailyTransactionlst
                                            .Where(transaction => transaction.Type == "SG")
                                            .Sum(transaction => transaction.SaleValue-transaction.BuyValue);
                                Cell totalPurValueCell = new Cell(1, 1)
                                    .Add(new Paragraph($"{sumOfLGGain}\n{sumOfSGGain}")) // Replace with your actual calculations
                                    .AddStyle(cellStyle)
                                    .SetFontColor(DeviceRgb.RED);
                                table.AddCell(totalPurValueCell);
                            }
                            else
                            {
                                
                                    table.AddCell(new Cell().Add(new Paragraph("")));

                            }
                        }

                        doc.Add(table);

                        Paragraph subheading1 = new Paragraph("Summary of CapitalGains and TDS for the date").SetTextAlignment(TextAlignment.CENTER).SetFontSize(14).SetFontColor(DeviceRgb.RED);
                        doc.Add(subheading1);
                        Table summaryTable = new Table(UnitValue.CreatePercentArray(15)).UseAllAvailableWidth();
                        Cell longTermHeaderCell = new Cell(1, 8)
                            .Add(new Paragraph("Long Term Capital Gain"))
                            .SetTextAlignment(TextAlignment.CENTER)
                            .SetFontColor(DeviceRgb.BLACK);
                        summaryTable.AddHeaderCell(longTermHeaderCell);

                        Cell shortTermHeaderCell = new Cell(1, 7)
                            .Add(new Paragraph("Short Term Capital Gain"))
                            .SetTextAlignment(TextAlignment.CENTER)
                            .SetFontColor(DeviceRgb.BLACK);
                        summaryTable.AddHeaderCell(shortTermHeaderCell);


                        string[] columnHeadings = new string[]
                        {
                            "Date",
                            "Opening Net gain(loss)(A)",
                            "Current Date gain/[loss][B]",
                            "Loss Adjusted from current date gain[C]",
                            "Net taxable current date gain",
                            "TDS Amount on taxable gain",
                            "TDS %",
                            "Closing Net gain(loss)",
                            "Opening Net gain(loss)",
                            "Current Date gain/[loss][B]",
                            "Loss Adjusted from current date gain[C]",
                            "Net taxable current date gain",
                            "TDS Amount on taxable gain",
                            "TDS %",
                            "Closing Net gain(loss)",
                        };
                        foreach (string colhead in columnHeadings)
                        {
                            Cell headingCell = new Cell(1, 1)
                                .Add(new Paragraph(colhead).AddStyle(headerCellStyle));
                            summaryTable.AddHeaderCell(headingCell);
                        }
                        foreach(var dailysummary in TaxComputedlist)
                        {
                                                                                
                            summaryTable.AddCell(new Cell().Add(new Paragraph(dailysummary.TransSaleDate.ToShortDateString()).AddStyle(cellStyle)));//Date
                            summaryTable.AddCell(new Cell().Add(new Paragraph(dailysummary.OpeningBalLT>=0? dailysummary.OpeningBalLT.ToString() : $"({Math.Abs(dailysummary.OpeningBalLT)})").AddStyle(cellStyle)));//Opening net gain (loss)
                            summaryTable.AddCell(new Cell().Add(new Paragraph(dailysummary.DailySetOffLT>=0? dailysummary.DailySetOffLT.ToString(): $"({Math.Abs(dailysummary.DailySetOffLT)})").AddStyle(cellStyle)));//Current date gain/loss
                            summaryTable.AddCell(new Cell().Add(new Paragraph(dailysummary.OpeningBalLT>=0?"0": Math.Abs(dailysummary.OpeningBalLT).ToString()).AddStyle(cellStyle)));//loss adjusted from current date gain
                            if (dailysummary.OpeningBalLT > 0)
                            {
                                dailysummary.OpeningBalLT = 0;
                            }
                            summaryTable.AddCell(new Cell().Add(new Paragraph((dailysummary.DailySetOffLT-dailysummary.OpeningBalLT)>0? (dailysummary.DailySetOffLT - dailysummary.OpeningBalLT).ToString():$"({Math.Abs(dailysummary.DailySetOffLT - dailysummary.OpeningBalLT)})".ToString()).AddStyle(cellStyle)));// net taxable current date gain
                            summaryTable.AddCell(new Cell().Add(new Paragraph((dailysummary.LT_Tax).ToString()).AddStyle(cellStyle)));//TDS Amount on taxable gain
                            summaryTable.AddCell(new Cell().Add(new Paragraph((dailysummary.LT_TaxPercentage).ToString()).AddStyle(cellStyle)));//TDS%
                            summaryTable.AddCell(new Cell().Add(new Paragraph((dailysummary.ClosingBalLT) > 0 ? (dailysummary.ClosingBalLT).ToString() : $"({Math.Abs(dailysummary.ClosingBalLT)})").AddStyle(cellStyle)));

                            summaryTable.AddCell(new Cell().Add(new Paragraph(dailysummary.OpeningBalST >= 0 ? dailysummary.OpeningBalST.ToString() : $"({Math.Abs(dailysummary.OpeningBalST)})").AddStyle(cellStyle)));//Opening net gain (loss)
                            summaryTable.AddCell(new Cell().Add(new Paragraph(dailysummary.DailySetOffST >= 0 ? dailysummary.DailySetOffST.ToString() : $"({Math.Abs(dailysummary.DailySetOffST)})").AddStyle(cellStyle)));//Current date gain/loss
                            summaryTable.AddCell(new Cell().Add(new Paragraph(dailysummary.OpeningBalST >= 0 ? "0" : Math.Abs(dailysummary.OpeningBalST).ToString()).AddStyle(cellStyle)));//loss adjusted from current date gain
                            if (dailysummary.OpeningBalST > 0)
                            {
                                dailysummary.OpeningBalST = 0;
                            }
                            summaryTable.AddCell(new Cell().Add(new Paragraph((dailysummary.DailySetOffST - dailysummary.OpeningBalST) > 0 ? (dailysummary.DailySetOffST - dailysummary.OpeningBalST).ToString() : $"({Math.Abs(dailysummary.DailySetOffST - dailysummary.OpeningBalST)})".ToString()).AddStyle(cellStyle)));// net taxable current date gain
                            summaryTable.AddCell(new Cell().Add(new Paragraph((dailysummary.ST_Tax).ToString()).AddStyle(cellStyle)));//TDS Amount on taxable gain
                            summaryTable.AddCell(new Cell().Add(new Paragraph((dailysummary.ST_TaxPercentage).ToString()).AddStyle(cellStyle)));//TDS%
                            summaryTable.AddCell(new Cell().Add(new Paragraph((dailysummary.ClosingBalST)>0? (dailysummary.ClosingBalST).ToString(): $"({Math.Abs(dailysummary.ClosingBalST)})").AddStyle(cellStyle)));
                        }

                        doc.Add(summaryTable);
                        Table secondsummaryTable = new Table(UnitValue.CreatePercentArray(15)).UseAllAvailableWidth();
                        Cell QuarterHeaderCell = new Cell(1, 16)
                            .Add(new Paragraph("Summary of CapitalGains and TDS for the quarter"))
                            .SetTextAlignment(TextAlignment.CENTER)
                            .SetFontColor(DeviceRgb.RED);
                        secondsummaryTable.AddHeaderCell(QuarterHeaderCell);
                        Cell QuarterlongTermHeaderCell = new Cell(1, 8)
                            .Add(new Paragraph("Long Term Capital Gain"))
                            .SetTextAlignment(TextAlignment.CENTER)
                            .SetFontColor(DeviceRgb.BLACK);
                        secondsummaryTable.AddHeaderCell(QuarterlongTermHeaderCell);

                        Cell QuartershortTermHeaderCell = new Cell(1, 7)
                            .Add(new Paragraph("Short Term Capital Gain"))
                            .SetTextAlignment(TextAlignment.CENTER)
                            .SetFontColor(DeviceRgb.BLACK);
                        secondsummaryTable.AddHeaderCell(QuartershortTermHeaderCell);

                        foreach (string colhead in columnHeadings)
                        {
                            Cell headingCell = new Cell(1, 1)
                                .Add(new Paragraph(colhead).AddStyle(headerCellStyle));
                            secondsummaryTable.AddHeaderCell(headingCell);
                        }

                        foreach (var quartersummary in QuarterSummarylst)
                        {
                            decimal lossadjusted = 0;decimal lossadjustedlt = 0;
                            secondsummaryTable.AddCell(new Cell().Add(new Paragraph(quartersummary.MonthName).AddStyle(cellStyle)));//Date

                            secondsummaryTable.AddCell(new Cell().Add(new Paragraph(quartersummary.OpeningBalanceLT >= 0 ? quartersummary.OpeningBalanceLT.ToString() : $"({Math.Abs(quartersummary.OpeningBalanceLT)})").AddStyle(cellStyle)));//Opening net gain (loss)
                            secondsummaryTable.AddCell(new Cell().Add(new Paragraph(quartersummary.LTGain >= 0 ? quartersummary.LTGain.ToString() : $"({Math.Abs(quartersummary.LTGain)})").AddStyle(cellStyle)));//Current date gain/loss
                            secondsummaryTable.AddCell(new Cell().Add(new Paragraph(quartersummary.OpeningBalanceLT >= 0 ? "0" : Math.Abs(quartersummary.OpeningBalanceLT).ToString()).AddStyle(cellStyle)));//loss adjusted from current date gain
                            if (quartersummary.OpeningBalanceLT >= 0)
                            {
                                lossadjustedlt = 0;
                            }
                            else
                            {
                                lossadjustedlt = Math.Abs(quartersummary.OpeningBalanceLT);
                            }                           
                            secondsummaryTable.AddCell(new Cell().Add(new Paragraph((quartersummary.LTGain - lossadjustedlt) > 0 ? (quartersummary.LTGain - lossadjustedlt).ToString() : $"({Math.Abs(quartersummary.LTGain - lossadjustedlt)})".ToString()).AddStyle(cellStyle)));// net taxable current date gain
                            secondsummaryTable.AddCell(new Cell().Add(new Paragraph((quartersummary.LTTaxSum).ToString()).AddStyle(cellStyle)));//TDS Amount on taxable gain
                            secondsummaryTable.AddCell(new Cell().Add(new Paragraph((quartersummary.LTTaxPercentage).ToString()).AddStyle(cellStyle)));//TDS%
                            secondsummaryTable.AddCell(new Cell().Add(new Paragraph((quartersummary.ClosingBalanceLT) > 0 ? quartersummary.ClosingBalanceLT.ToString() : $"({Math.Abs(quartersummary.ClosingBalanceLT)})".ToString()).AddStyle(cellStyle)));// net taxable current date gain


                            //Short Term
                            secondsummaryTable.AddCell(new Cell().Add(new Paragraph(quartersummary.OpeningBalanceST >= 0 ? quartersummary.OpeningBalanceST.ToString() : $"({Math.Abs(quartersummary.OpeningBalanceST)})").AddStyle(cellStyle)));//Opening net gain (loss)
                            secondsummaryTable.AddCell(new Cell().Add(new Paragraph(quartersummary.STGain >= 0 ? quartersummary.STGain.ToString() : $"({Math.Abs(quartersummary.STGain)})").AddStyle(cellStyle)));//Current date gain/loss
                            secondsummaryTable.AddCell(new Cell().Add(new Paragraph(quartersummary.OpeningBalanceST >= 0 ? "0" : Math.Abs(quartersummary.OpeningBalanceST).ToString()).AddStyle(cellStyle)));//loss adjusted from current date gain
                            if(quartersummary.OpeningBalanceST >= 0)
                            {
                                lossadjusted = 0;
                            }
                            else
                            {
                                lossadjusted = Math.Abs(quartersummary.OpeningBalanceST);
                            }
                            secondsummaryTable.AddCell(new Cell().Add(new Paragraph((quartersummary.STGain - lossadjusted) > 0 ? (quartersummary.STGain - lossadjusted).ToString() : $"({Math.Abs(quartersummary.STGain - lossadjusted)})".ToString()).AddStyle(cellStyle)));// net taxable current date gain
                            secondsummaryTable.AddCell(new Cell().Add(new Paragraph((quartersummary.STTaxSum).ToString()).AddStyle(cellStyle)));//TDS Amount on taxable gain
                            secondsummaryTable.AddCell(new Cell().Add(new Paragraph((quartersummary.STTaxPercentage).ToString()).AddStyle(cellStyle)));//TDS%
                            secondsummaryTable.AddCell(new Cell().Add(new Paragraph((quartersummary.ClosingBalanceST) > 0 ? quartersummary.ClosingBalanceST.ToString() : $"({Math.Abs(quartersummary.ClosingBalanceST)})".ToString()).AddStyle(cellStyle)));// net taxable current date gain

                        }
                        
                        doc.Add(secondsummaryTable);
                        

                       


                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                //throw ex;
                //Console.WriteLine(ex.Message);
                return false;
            }
            return true;
        }
    }

   
}
