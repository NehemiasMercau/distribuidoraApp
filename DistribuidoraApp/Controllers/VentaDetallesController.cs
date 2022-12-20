using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using DistribuidoraAPI.Helpers;
using DistribuidoraAPI.Models;
using Negocio.entidades;
using Negocio.enumeradores;

namespace DistribuidoraAPI.Controllers
{
    [Authorize]
    [SessionExpire]
    public class VentaDetallesController : Controller
    {
        private DistribuidoraDBEntities db = new DistribuidoraDBEntities();

        
        // GET: VentaDetalles
        public ActionResult Index(int? VentaId)
        {
            if (VentaId == null) { return new HttpStatusCodeResult(HttpStatusCode.BadRequest); }
            List<VentaDetalle> listVentaDetalle = db.VentaDetalle.Include(v => v.Combo).Include(v => v.Producto).Include(v => v.Venta).Where(x => x.Activo == true && x.VentaId == VentaId).ToList();
            Venta oVenta = db.Venta.Find((int)VentaId);

            ViewBag.DatosVenta = "Id Venta: " + oVenta.VentaId + " - Fecha: " + oVenta.Fecha + " - Estado: " + oVenta.Estado.Nombre;
            if (oVenta.Cliente != null) { ViewBag.DatosCliente = oVenta.Cliente.RazonSocial + ". " + oVenta.Cliente.CUIT; }


            List<ProductoListViewModel> list = listVentaDetalle.Select(x => new ProductoListViewModel
            {
                ProductoId = (x.ProductoId!=null) ? (int)x.ProductoId : (int)x.ComboId,
                Nombre = (x.ProductoId != null) ? x.Producto.Nombre : x.Combo.Nombre,
                Precio = Convert.ToDecimal(x.Precio.Replace(".", ",")),                
                Cambio = (int)x.MonedaId,
                CambioStr = x.Moneda.Nombre,
                Marca = (x.ProductoId != null) ? x.Producto.Marca.Nombre : "Combo",
                Codigo = (x.ProductoId != null) ? x.Producto.Codigo : x.Combo.Codigo,
                Cantidad = (decimal)x.Cantidad

            }).ToList();

            //List<Root> listDolar = PrecioHelper.GetPrecioDolar();
            //decimal precioUSD = Convert.ToDecimal(listDolar.FirstOrDefault().casa.venta.ToString());
            decimal precioUSD;
            if (Session["Dolar"] != null)
            {
                precioUSD = Convert.ToDecimal(Session["Dolar"].ToString());
            }
            else
            {
                precioUSD = PrecioHelper.GetPrecioDolarStatico();
                Session.Add("Dolar", precioUSD);
            }
            ViewBag.PrecioUSD = precioUSD;
            ViewBag.VentaObjeto = oVenta;
            ViewBag.EstadoId = oVenta.EstadoId;
            return View(list);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
