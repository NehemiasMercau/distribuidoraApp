using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace DistribuidoraAPI.Models
{
    public class VentaViewModel
    {
        public int VentaId { get; set; }

        [Required(ErrorMessage = "El campo Tipo de Venta es obligatorio")]
        public int TipoVentaId { get; set; }
        [Required(ErrorMessage = "El campo Estado es obligatorio")]
        public int EstadoId { get; set; }
        [Required(ErrorMessage = "El campo Sucursal es obligatorio")]
        public int SucursalId { get; set; }
        public Nullable<int> ClienteId { get; set; }
        [Required(ErrorMessage = "El campo Tipo de Cobro es obligatorio")]
        public int TipoCobroId { get; set; }
        public decimal Total { get; set; }
        public Nullable<decimal> Descuento { get; set; }
        public Nullable<decimal> Final { get; set; }
        public System.DateTime Fecha { get; set; }
        public Nullable<System.DateTime> FechaModificacion { get; set; }
        [DataType(DataType.MultilineText)]
        public string Observaciones { get; set; }
        public string DireccionEnvio { get; set; }
        public Nullable<decimal> CostoEnvio { get; set; }
        public bool Pendiente { get; set; }
        public Nullable<decimal> Recargo { get; set; }
        public List<int> ProductoId { get; set; }
        public List<int> CombosList { get; set; }
        public string TipoVenta { get; set; }
        public string Sucursal { get; set; }
        public string Cliente { get; set; }
        public string TipoCobro { get; set; }
        public bool ConFactura { get; set; }
        public int UsuarioId { get; set; }
        public int ArqueoId { get; set; }
        public decimal Promos { get; set; }
        public Nullable<int> PreventistaId { get; set; }
    }
}