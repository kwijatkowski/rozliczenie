using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rozliczenie
{
    public class OperationData
    {
        public string OrderUuid;
        public string Exchange;
        public string Type;
        public decimal Quantity;
        public decimal Limit;
        public decimal CommissionPaid;
        public decimal Price;
        public DateTime Opened;
        public DateTime Closed;
    }
}
