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
    
    public partial class NotaCredito
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public NotaCredito()
        {
            this.NotaCreditoDetalle = new HashSet<NotaCreditoDetalle>();
        }
    
        public int NotaCreditoId { get; set; }
        public Nullable<int> VentaId { get; set; }
        public Nullable<int> ClienteId { get; set; }
        public string Destinatario { get; set; }
        public Nullable<System.DateTime> Fecha { get; set; }
        public Nullable<decimal> Total { get; set; }
        public Nullable<int> RemitoId { get; set; }
        public Nullable<int> UsuarioId { get; set; }
        public bool Activo { get; set; }
        public string Observaciones { get; set; }
    
        public virtual Cliente Cliente { get; set; }
        public virtual Remito Remito { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<NotaCreditoDetalle> NotaCreditoDetalle { get; set; }
        public virtual UsuarioReferencia UsuarioReferencia { get; set; }
        public virtual Venta Venta { get; set; }
    }
}