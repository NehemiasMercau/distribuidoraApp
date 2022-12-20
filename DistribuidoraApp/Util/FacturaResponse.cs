using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DistribuidoraAPI.Util
{
    public class FacturaResponse
    {
        public bool Exito { get; set; }
        public string Mensaje { get; set; }
        public string IdFactura { get; set; }
    }
}