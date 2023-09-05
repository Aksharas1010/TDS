using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenerateTDSService.DTO
{
    internal class ClientModel
    {
       
        public int? Clientid { get; set; }
       
        public string Name { get; set; }  
        public string ClientEmail { get; set; }
        public string Location { get; set; }
        public string LocEmail { get; set; }
        public string Type { get; set; }
        public string PDFPassword { get; set; }

    }
}
