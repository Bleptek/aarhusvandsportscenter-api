using System;

namespace Aarhusvandsportscenter.Api.Infastructure.OldDatabase.Entities
{
    public class BookingLejer
    {
        public string Navn { get; set; }
        public string Tlf { get; set; }
        public string Mail { get; set; }

        public int Kajak { get; set; }
        public int Dbkajak { get; set; }
        public int Kano { get; set; }
        public int Jolle { get; set; }
        public int Paddle { get; set; }
        public int BTrailer { get; set; }

        public string Deal { get; set; }
        public DateTime Dato { get; set; }
        public string Kommentar { get; set; }
        public bool? Done { get; set; }
        public int Id { get; set; }
        public string DealSite { get; set; }
        public string RowColor { get; set; }
    }
}
