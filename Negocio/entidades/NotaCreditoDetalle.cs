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
    
    public partial class NotaCreditoDetalle
    {
        public int NotaCreditoDetalleId { get; set; }
        public Nullable<int> NotaCreditoId { get; set; }
        public Nullable<int> ProductoId { get; set; }
        public Nullable<int> ComboId { get; set; }
        public Nullable<int> Cantidad { get; set; }
        public Nullable<decimal> Monto { get; set; }
        public Nullable<bool> Activo { get; set; }
    
        public virtual Combo Combo { get; set; }
        public virtual NotaCredito NotaCredito { get; set; }
        public virtual Producto Producto { get; set; }
    }
}
