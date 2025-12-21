using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TPV.MODELOAK
{    public class Erreserba
    {
        public int id { get; set; }
        public int mahaiaId { get; set; }
        public string izena { get; set; }
        public long telefonoa { get; set; }
        public DateTime erreserbaData { get; set; }
        public int pertsonaKop { get; set; }
        public string egoera { get; set; }
        public string oharrak { get; set; }
    }
}
