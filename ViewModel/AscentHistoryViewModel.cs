using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vertikal.ViewModel
{
    public class AscentHistoryViewModel
    {
        public string SummitName { get; set; }
        public double SummitAltitude { get; set; }
        public string SummitDescription { get; set; }
        public DateTime Date { get; set; }
        public string ValidationMethod { get; set; }
    }
}
