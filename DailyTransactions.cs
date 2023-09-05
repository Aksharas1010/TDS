using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenerateTDSService.DTO
{
    public class DailyTransactions
    {
        public int TransID { get; set; }
        public int Clientid { get; set; }
        public string Type { get; set; }
        public string ClientCode { get; set; }
        public string ClientName { get; set; }
        public string PRODUCT { get; set; }
        public string TradeCode { get; set; }
        public DateTime TranDateBuy { get; set; }
        public string Security { get; set; }
        public string ISIN { get; set; }
        public int BuyQty { get; set; }
        public Decimal BuyValue { get; set; }
        public DateTime TranDateSale { get; set; }
        public int SaleQty { get; set; }
        public Decimal SaleValue { get; set; }
    }
}
