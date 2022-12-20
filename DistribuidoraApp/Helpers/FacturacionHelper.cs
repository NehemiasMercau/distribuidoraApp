using DistribuidoraAPI.Util;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using Negocio.gestores;
using System.Web.Script.Serialization;
using System.Net.Http;
using System.Net;
using Newtonsoft.Json;

namespace DistribuidoraAPI.Helpers
{
    public class FacturacionHelper
    {
        private static readonly HttpClient client = new HttpClient();

        public static FacturaResponse EmitirFactura(Negocio.entidades.Venta oVenta)
        {
            string Server = ConfigurationManager.AppSettings["SERVERFACTURAPRODUCCION"];
            string Email = ConfigurationManager.AppSettings["EMAILFACTURAPRODUCCION"];
            string Password = ConfigurationManager.AppSettings["PASSWORDFACTURAPRODUCCION"];

            Root oFacturaRequest = new Root();
            oFacturaRequest.Email = Email;
            oFacturaRequest.Password = Password;

            try
            {
                Cliente oCliente = new Cliente
                {
                    Actualizar = "false",
                    RazonSocial = oVenta.Cliente.RazonSocial,
                    Identificador = oVenta.Cliente.CUIT,
                    Email = oVenta.Cliente.Email,
                    Direccion = oVenta.Cliente.Direccion,
                    Localidad = "",
                    CodigoPostal = "1234",
                    Provincia = 14,
                    CondicionImpositiva = oVenta.Cliente.CondicionIVAId,
                    TipoDocumento = 1,//CUIT
                    TipoPersona = oVenta.Cliente.PersonaTipoId
                };

                List<DetalleFactura> listDetalleFactura = new List<DetalleFactura>();
                foreach (Negocio.entidades.VentaDetalle item in oVenta.VentaDetalle.Where(x => x.Activo == true))
                {
                    DetalleFactura oDetalleFactura = new DetalleFactura()
                    {
                        Cantidad = (decimal)item.Cantidad,
                        ValorUnitario = Convert.ToDecimal(item.Precio),
                        Total = Convert.ToDecimal(item.Precio),
                        Descripcion = (item.ProductoId != null) ? item.Producto.Nombre : item.Combo.Nombre,
                        Codigo = (item.ProductoId != null) ? item.Producto.Codigo : item.Combo.Codigo,
                        AlicuotaIVA = 3,
                        IVA = 0,
                        UnidadMedida = 7, //Unidades
                        Bonificacion = 0,
                        ConceptoFactura = 1 //producto,

                    };
                    listDetalleFactura.Add(oDetalleFactura);
                }

                Factura oFactura = new Factura
                {
                    Numero = oVenta.VentaId,
                    Fecha = DateTime.Now.ToString("yyyy-MM-dd"),
                    FechaVencimiento = DateTime.Now.ToString("yyyy-MM-dd"),
                    FechaDesde = DateTime.Now.ToString("yyyy-MM-dd"),
                    FechaHasta = DateTime.Now.ToString("yyyy-MM-dd"),
                    AutoEnvioCorreo = true,
                    PuntoVenta = 9,//dejarlo por defecto. PARA TODAS LAS FACTURAS (poner el 9)
                    FormaPago = oVenta.TipoCobroId,
                    TipoComprobante = 4,
                    Cliente = oCliente,
                    DetalleFactura = listDetalleFactura

                };
                oFacturaRequest.Factura = oFactura;

                string result = "";
                WebRequest oRequest = WebRequest.Create("https://" + Server + "/API/EmitirFactura");
                oRequest.Method = "POST";
                oRequest.ContentType = "application/json";

                using (var oSW = new StreamWriter(oRequest.GetRequestStream()))
                {
                    string json1 = new JavaScriptSerializer().Serialize(oFacturaRequest);
                    oSW.Write(json1);
                    oSW.Flush();
                    oSW.Close();
                }

                WebResponse oResponse = oRequest.GetResponse();
                using (var oSR = new StreamReader(oResponse.GetResponseStream()))
                {
                    result = oSR.ReadToEnd().Trim();
                }

                FacturaResponse facturaResponse = JsonConvert.DeserializeObject<FacturaResponse>(result);
                if (facturaResponse.Exito)
                {
                    Negocio.entidades.Factura factura = new Negocio.entidades.Factura()
                    {
                        Activo = true,
                        VentaId = oVenta.VentaId,
                        RespuestaAPI = facturaResponse.Mensaje,
                        FechaCreacion = DateTime.Now,
                        iFacturaId = facturaResponse.IdFactura,
                        Realizada = true
                    };
                    GestorFactura.Insertar(factura);
                } else
                {
                    Negocio.entidades.Factura factura = new Negocio.entidades.Factura()
                    {
                        Activo = true,
                        VentaId = oVenta.VentaId,
                        RespuestaAPI = facturaResponse.Mensaje,
                        FechaCreacion = DateTime.Now,
                        iFacturaId = facturaResponse.IdFactura,
                        Realizada = false
                    };
                    GestorFactura.Insertar(factura);
                }
              
                return facturaResponse;
            }
            catch (Exception ex)
            {
                Negocio.entidades.Factura factura = new Negocio.entidades.Factura()
                {
                    Activo = true,
                    VentaId = oVenta.VentaId,
                    RespuestaAPI = ex.Message,
                    FechaCreacion = DateTime.Now,
                    iFacturaId = "ERROR",
                    Realizada = false
                };
                GestorFactura.Insertar(factura);
                FacturaResponse facturaResponse = new FacturaResponse();
                facturaResponse.Mensaje = ex.Message;
                facturaResponse.Exito = false;
                facturaResponse.IdFactura = "";
                
                return facturaResponse;
            }

        }

        public static void LoadJson()
        {
            Root facturaResponse = JsonReaderFactura.getFromJson();
        }

    }
}