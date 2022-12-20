using Negocio.entidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DistribuidoraAPI.Models
{
    public class NombreProductoCustom
    {
        public Producto oProducto  {get;set;}
        public int count { get; set; }
    }
}