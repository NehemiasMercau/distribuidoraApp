using System;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using Negocio.entidades;

namespace DistribuidoraAPI.Controllers
{
    [Authorize(Roles = "Caja, Administracion, Desarrollo")]
    [SessionExpire]
    public class FacturasController : Controller
    {
        private DistribuidoraDBEntities db = new DistribuidoraDBEntities();

        // GET: Facturas
        public ActionResult Index()
        {
            var factura = db.Factura.Include(f => f.Venta).Where(x => x.Activo == true);
            return View(factura.OrderByDescending(x => x.FechaCreacion).ToList());
        }

        public ActionResult GetVenta(string id)
        {
            int ventaId = Convert.ToInt32(id);
            Factura oFactura = db.Factura.Where(x => x.Activo == true && x.VentaId == ventaId).FirstOrDefault();
            if (oFactura != null)
            {
                return Redirect("https://app.ifactura.com.ar/Factura/ImprimirExterno/" + oFactura.iFacturaId);
            } else
            {
                return View("ErrorGeneral");
            }
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
