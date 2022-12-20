using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DistribuidoraAPI.Models
{
    public class Afluencia
    {
    }

    public class Observaciones
    {
    }

    public class Casa
    {
        public string compra { get; set; }
        public string venta { get; set; }
        public string agencia { get; set; }
        public string nombre { get; set; }
        public object variacion { get; set; }
        public string ventaCero { get; set; }
        public string decimales { get; set; }
        public string mejor_compra { get; set; }
        public string mejor_venta { get; set; }
        public string fecha { get; set; }
        public string recorrido { get; set; }
        public Afluencia afluencia { get; set; }
        public Observaciones observaciones { get; set; }
    }

    public class Root
    {
        public Casa casa { get; set; }
    }
}