using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DistribuidoraAPI.Util
{
    
    public class DetalleFactura
    {
        public decimal Cantidad { get; set; }
        public decimal ValorUnitario { get; set; }
        public decimal Total { get; set; }
        public string Descripcion { get; set; }
        public string Codigo { get; set; }
        public int AlicuotaIVA { get; set; }
        public decimal IVA { get; set; }
        public int UnidadMedida { get; set; }
        public int Bonificacion { get; set; }
        public int ConceptoFactura { get; set; }
    }

    public class Cliente
    {
        public string Actualizar { get; set; }
        public string RazonSocial { get; set; }
        public string Identificador { get; set; }
        public string Email { get; set; }
        public string Direccion { get; set; }
        public string Localidad { get; set; }
        public string CodigoPostal { get; set; }
        public int Provincia { get; set; }
        public int CondicionImpositiva { get; set; }
        public int TipoPersona { get; set; }
        public int TipoDocumento { get; set; }
    }

    public class Factura
    {
        public int Numero { get; set; }
        public string Fecha { get; set; }
        public string FechaVencimiento { get; set; }
        public string FechaDesde { get; set; }
        public string FechaHasta { get; set; }
        public bool AutoEnvioCorreo { get; set; }
        public int PuntoVenta { get; set; }
        public int FormaPago { get; set; }
        public int TipoComprobante { get; set; }
        public List<DetalleFactura> DetalleFactura { get; set; }
        public Cliente Cliente { get; set; }
    }

    public class Root
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public Factura Factura { get; set; }
    }
}