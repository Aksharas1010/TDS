using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenerateTDSService.DTO
{
    public class BuyNotFound
    {
		public int Clientid { get; set; }
		public string ClientCode { get; set; }
		public string TradeCode { get; set; }
		public DateTime Trandate { get; set; }
		public string Security { get; set; }
		public string ISIN { get; set; }
		public int Qty { get; set; }
		public int SellValue { get; set; }
		public string Euser { get; set; }
		public DateTime Lastupdatedon { get; set; }
		public string Location { get; set; }
		public string LocationEmail { get; set; }

    }
}
