using Negocio.entidades;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace DistribuidoraAPI.Models
{
    public class ProductosCombosViewModel
    {
        public List<Producto> listProducto { get; set; }
        public List<Combo> listCombo { get; set; }
    }

    public class ProductoViewModel
    {
        public int ProductoId { get; set; }
        [Required]
        public int TipoProductoId { get; set; }
        [Required]
        public int TipoDuracionId { get; set; }
        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Por favor, ingrese un valor númerico")]
        public int Cantidad { get; set; }
        public List<Deposito> Depositos { get; set; }
        [Required]
        public int MarcaId { get; set; }
        [Required]
        [StringLength(1000, ErrorMessage = "Cantidad máxima de caracteres: 1000")]
        public string Nombre { get; set; }
        [Required]
        [StringLength(2000, ErrorMessage = "Cantidad máxima de caracteres: 2000")]
        public string Codigo { get; set; }
        [Required(ErrorMessage = "El campo Precio de Costo es obligatorio")]
        [RegularExpression(@"^\d+(,\d{1,2})?$", ErrorMessage = "Ingresar un monto válido")]
        [DataType(DataType.Text)]
        public Nullable<decimal> PrecioCosto { get; set; }
        public Nullable<System.DateTime> FechaModificacion { get; set; }
        [Required]
        public Nullable<int> Minimo { get; set; }
        public int[] DepositosList { get; set; }
        public decimal Precio { get; set; }
        public int Cambio { get; set; }

        public Nullable<int> ProveedorId { get; set; }
    }

    public class ProductoListViewModel
    {
        public int ProductoId { get; set; }
        public string Nombre { get; set; }
        public string Codigo { get; set; }
        public int Minimo { get; set; }
        public decimal Precio { get; set; }
        public int Cambio { get; set; }
        public string CambioStr { get; set; }
        public string Marca { get; set; }
        public string TipoProducto { get; set; }
        public decimal Cantidad { get; set; }
        public int TipoStock { get; set; }
        public bool Alerta { get; set; }
        public string AlertStr { get; set; }
        public int ListaId { get; set; }
        public decimal SubTotal { get; set; }
        public bool Cargado { get; set; }
    }

    public class ComboViewModel
    {
        public int ComboId { get; set; }
        [Required]
        [StringLength(1000, ErrorMessage = "Cantidad máxima de caracteres: 1000")]
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public int Cantidad { get; set; }
        public List<Producto> MultiProductos { get; set; }
        public Nullable<System.DateTime> FechaAlta { get; set; }
        [Required(ErrorMessage = "El campo Precio de Costo es obligatorio")]
        [RegularExpression(@"^\d+(,\d{1,2})?$", ErrorMessage = "Ingresar un monto válido")]
        public Nullable<decimal> PrecioCosto { get; set; }
       
        public string Codigo { get; set; }
        public Nullable<System.DateTime> FechaModificacion { get; set; }
        public Nullable<int> Minimo { get; set; }
        public int[] DepositosList { get; set; }
        
    }

    public class StockViewModel
    {
        public List<DepositoStockModel> listDepositoStockModel { get; set; }

    }

    public class DepositoStockModel
    {
        public Deposito oDeposito { get; set; }
        public int Cantidad { get; set; }
    }

    public class PrecioCustom
    {
        public string Costo { get; set; }
        public string VentaMinorista { get; set; }
        public string VentaMayorista { get; set; }
    }
}