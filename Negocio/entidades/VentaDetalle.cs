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
    
    public partial class VentaDetalle
    {
        public int VentaDetalleId { get; set; }
        public int VentaId { get; set; }
        public Nullable<int> ListaId { get; set; }
        public Nullable<int> ProductoId { get; set; }
        public Nullable<int> ComboId { get; set; }
        public Nullable<decimal> Cantidad { get; set; }
        public string Precio { get; set; }
        public Nullable<int> MonedaId { get; set; }
        public bool Activo { get; set; }
    
        public virtual Combo Combo { get; set; }
        public virtual Lista Lista { get; set; }
        public virtual Moneda Moneda { get; set; }
        public virtual Producto Producto { get; set; }
        public virtual Venta Venta { get; set; }
    }
}