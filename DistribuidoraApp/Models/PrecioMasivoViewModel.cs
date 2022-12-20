using Negocio.enumeradores;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DistribuidoraAPI.Models
{
    public class PrecioMasivoViewModel
    {
        public int PrecioListaId { get; set; }
        public decimal Precio { get; set; }
        public bool Cargado { get; set; }
        public string Moneda { get; set; }
        public string Codigo { get; set; }
        public string Nombre { get; set; }
        public string Lista { get; set; }
    }
}