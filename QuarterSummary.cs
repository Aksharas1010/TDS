using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DocumentFormat.OpenXml.Spreadsheet;

namespace GenerateTDSService.DTO
{
    public class QuarterSummary
    {
        public int Quarter { get; set; }
        public int MonthNumber { get; set; }
        public string MonthName { get; set; }
        public Decimal OpeningBalanceST { get; set; }
        public Decimal ClosingBalanceST { get; set; }
        public Decimal STGain { get; set; }
        public Decimal LTGain { get; set; }
        public Decimal STTaxSum { get; set; }
        public Decimal LTTaxSum { get; set; }
        public Decimal OpeningBalanceLT { get; set; }
        public Decimal ClosingBalanceLT { get; set; }
        public Decimal STTaxPercentage { get; set; }
        public Decimal LTTaxPercentage { get; set; }


    }
}
