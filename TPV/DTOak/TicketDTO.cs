using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TPV.DTOak
{
    public class TicketDTO
    {
        public int PlateraId { get; set; }
        public string Izena { get; set; } = "";
        public int Kantitatea { get; set; }
        public decimal PrezioaUnitatea { get; set; }
    }
}
