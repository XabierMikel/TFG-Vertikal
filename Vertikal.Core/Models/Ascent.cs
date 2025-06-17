
namespace Vertikal.Core.Models
{
    public class Ascent
    {    
        public string UserId { get; set; }  // <--- ID de documento del usuario
        public string SummitId { get; set; } // <--- ID de documento de la cumbre
        public DateTime Date { get; set; }
        public string ValidationMethod { get; set; } // GPS, QR, NFC
    }
}
