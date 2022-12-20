using Negocio.entidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DistribuidoraAPI.Models
{
    public class ProductoStockViewModel
    {
        public decimal Cantidad { get; set; }
        public decimal CantidadReservada { get; set; }
        public Producto oProducto { get; set; }
        
    }
}