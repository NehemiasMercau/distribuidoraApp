//------------------------------------------------------------------------------
// <auto-generated>
//     Este código se generó a partir de una plantilla.
//
//     Los cambios manuales en este archivo pueden causar un comportamiento inesperado de la aplicación.
//     Los cambios manuales en este archivo se sobrescribirán si se regenera el código.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Negocio.entidades
{
    using System;
    using System.Collections.Generic;
    
    public partial class Factura
    {
        public int FacturaId { get; set; }
        public int VentaId { get; set; }
        public string iFacturaId { get; set; }
        public string RespuestaAPI { get; set; }
        public Nullable<System.DateTime> FechaCreacion { get; set; }
        public bool Activo { get; set; }
        public Nullable<bool> Realizada { get; set; }
    
        public virtual Venta Venta { get; set; }
    }
}
