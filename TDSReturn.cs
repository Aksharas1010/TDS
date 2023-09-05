using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenerateTDSService.DTO
{
    public class TDSReturn
    {
        public string Client { get; set; }
        public DateTime TransSaleDate { get; set; }
        public Decimal Sum_Short_Term_Profit { get; set; }
        public Decimal Sum_Long_Term_Profit { get; set; }
        public Decimal Profit { get; set; }
        public Decimal OpeningBalST { get; set; }
        public Decimal OpeningBalLT { get; set; }
        public Decimal ClosingBalLT { get; set; }
        public Decimal ClosingBalST { get; set; }
        public Decimal Adjusted_Short_Term { get; set; }
        public Decimal Adjusted_Long_Term { get; set; }
        public Decimal TaxableGain { get; set; }
        public Decimal ST_Tax { get; set; }
        public Decimal LT_Tax { get; set; }
        public Decimal ST_TaxPercentage { get; set; }
        public Decimal LT_TaxPercentage { get; set; }
        public Decimal DailySetOffLT { get; set; }
        public Decimal DailySetOffST { get; set; }

    }
}
