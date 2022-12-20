using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using DistribuidoraAPI.Extensions;
using DistribuidoraAPI.Models;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.Reporting.WebForms;
using Negocio.entidades;
using Negocio.enumeradores;

namespace DistribuidoraAPI.Controllers
{
    [Authorize(Roles = "Administracion, Desarrollo")]
    //[SessionExpire]
    public class RemitosController : Controller
    {
        private DistribuidoraDBEntities db = new DistribuidoraDBEntities();

        // GET: Remitos
        public ActionResult Index(string mensaje, string tipoMensaje, int? VentaId)
        {
            ViewBag.mensaje = mensaje;
            ViewBag.tipoMensaje = tipoMensaje;
            ViewBag.ClienteId = new SelectList(db.Cliente.Where(x => x.Activo == true), "ClienteId", "RazonSocial");
            ViewBag.TransportistaId = new SelectList(db.Transportista.Where(x => x.Activo == true), "TransportistaId", "Nombre");
            var ventas = db.Venta.Select(x => new
            {
                Text = "ID: " + x.VentaId.ToString() + " - $" + x.Final + " - " + x.Fecha,
                Value = x.VentaId
            }).OrderByDescending(x => x.Value).ToList();


            DateTime FechaDesde = DateTime.Now.AddDays(-1); TimeSpan ts = new TimeSpan(0, 0, 0);
            DateTime FechaHasta = DateTime.Now; TimeSpan ts1 = new TimeSpan(23, 59, 59);
            List<Remito> listRemitos = new List<Remito>();
            listRemitos = db.Remito
                             .Include(v => v.Cliente)
                             .Include(v => v.Venta)
                             .Where(x => x.FechaCreacion >= FechaDesde && x.FechaCreacion <= FechaHasta && x.Activo == true)
                             .OrderByDescending(x => x.RemitoId)
                             .ToList();

            if (VentaId != null)
            {
                ViewBag.VentaId = new SelectList(ventas, "Value", "Text", (int)VentaId);
                listRemitos = listRemitos.Where(x => x.VentaId == VentaId).ToList();
            }
            else
            {
                ViewBag.VentaId = new SelectList(ventas, "Value", "Text");
            }
            return View(listRemitos);
        }

        [HttpPost]
        public ActionResult IndexBusqueda(int? ClienteId, int? TransportistaId, string fechas, int? VentaId)
        {
            List<Remito> listRemitos = new List<Remito>();

            string[] arrayFechas = fechas.Split('-');
            string desde = arrayFechas[0].Trim();
            string hasta = arrayFechas[1].Trim();
            DateTime FechaDesde = Convert.ToDateTime(desde);
            TimeSpan ts = new TimeSpan(0, 0, 0);
            //FechaDesde = FechaDesde.Date + ts;
            DateTime FechaHasta = Convert.ToDateTime(hasta);
            TimeSpan ts1 = new TimeSpan(23, 59, 59);
            FechaHasta = FechaHasta.Date + ts1;
            int PerfilId = Convert.ToInt32(User.Identity.GetPerfilId());
            //int TipoVentaId = (PerfilId == (int)PerfilE.Preventista) ? (int)TipoVentaE.Remota : (int)TipoVentaE.Presencial;

            //if (PerfilId == (int)PerfilE.Administracion || PerfilId == (int)PerfilE.Desarrollo)
            //{
            listRemitos = db.Remito.Include(v => v.Cliente)
                         .Include(v => v.Venta)
                         .Where(x => x.Activo == true)
                         .OrderByDescending(x => x.RemitoId)
                         .Where(x => x.FechaCreacion >= FechaDesde && x.FechaCreacion <= FechaHasta).ToList();
            //}
            //else
            //{
            //    listVentas = db.Venta
            //                 .Include(v => v.Cliente)
            //                 .Include(v => v.Sucursal)
            //                 .Include(v => v.TipoCobro)
            //                 .Include(v => v.TipoVenta)
            //                 .Where(x => x.Fecha >= FechaDesde && x.Fecha <= FechaHasta && x.Activo == true && x.TipoVentaId == TipoVentaId)
            //                 .OrderByDescending(x => x.VentaId)
            //                 .ToList();
            //}


            if (TransportistaId != null)
            {
                listRemitos = listRemitos.Where(x => x.TransportistaId == TransportistaId).ToList();
            }
            if (ClienteId != null)
            {
                listRemitos = listRemitos.Where(x => x.ClienteId == ClienteId).ToList();
            }
            if (VentaId != null)
            {
                listRemitos = listRemitos.Where(x => x.VentaId == VentaId).ToList();
            }

            if (TransportistaId != null)
            {
                decimal total = 0;
                listRemitos.ForEach(x =>
                {
                    total += (decimal)x.Venta.Final;
                });
                ViewBag.TotalTransportista = total;
            }

            return PartialView("_Index", listRemitos);
        }

        // GET: Remitos/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Remito remito = db.Remito.Find(id);
            if (remito == null)
            {
                return HttpNotFound();
            }
            return PartialView("_Details", remito);
        }

        // GET: Remitos/Create
        public ActionResult Create(int? VentaId)
        {
            if (VentaId == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

                //ViewBag.VentaId = (int)VentaId;
                //Venta oVenta = db.Venta.Find((int)VentaId);
                //ViewBag.Descripcion = "Monto: $" + oVenta.Final + " - Fecha: " + oVenta.Fecha;
                //ViewBag.Venta = oVenta;
            }
            else
            {
                var ventas = db.Venta.Select(x => new
                {
                    Text = "ID: " + x.VentaId.ToString() + " - $" + x.Final + " - " + x.Fecha,
                    Value = x.VentaId
                }).OrderByDescending(x => x.Value).ToList();
                ViewBag.VentaId = new SelectList(ventas, "Value", "Text", VentaId);
            }
            var transportista = db.Transportista.Where(x => x.Activo == true).Select(x => new
            {
                Text = x.Nombre + ", " + x.Apellido,
                Value = x.TransportistaId
            }).ToList();

            ViewBag.TransportistaId = new SelectList(transportista, "Value", "Text");

            Venta oVenta = db.Venta.Find(VentaId);
            ViewBag.ClienteId = new SelectList(db.Cliente, "ClienteId", "RazonSocial", oVenta.ClienteId);

            var preventistas = db.AspNetUsers.Where(x => x.PerfilId == (int)PerfilE.Preventista).Select(x => new
            {
                Text = x.Nombre + ", " + x.Apellido,
                Value = x.UsuarioId
            }).ToList();
            ViewBag.PreventistaId = new SelectList(preventistas, "Value", "Text");

            Remito oRemitoVenta = db.Remito.Where(x => x.VentaId == VentaId && x.Activo == true).FirstOrDefault();
            if (oRemitoVenta != null)
            {
                ViewBag.RemitoCreadoTitulo = "Ya existe un remito creado con el N° " + oRemitoVenta.RemitoId.ToString();
                ViewBag.RemitoCreado = "Datos del Remito. Fecha: " + oRemitoVenta.FechaCreacion.ToString("dd/MM/yyyy") +
                    ". Cliente: " + oRemitoVenta.Cliente.RazonSocial +
                    ". Transportista: " + oRemitoVenta.Transportista.Apellido + ", " + oRemitoVenta.Transportista.Nombre +
                    ". CUIDADO! Si crea un nuevo Remito, se reemplaza al actual. Desea continuar?";
            }

            Remito oRemito = new Remito()
            {
                Fecha = DateTime.Now.ToString("MM/dd/yyyy")
            };
            return View(oRemito);
        }

        // POST: Remitos/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que quiere enlazarse. Para obtener 
        // más detalles, vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ClienteId,VentaId,Fecha,TransportistaId,Observaciones")] Remito remito)
        {
            if (ModelState.IsValid)
            {
                Remito oRemitoVenta = db.Remito.Where(x => x.VentaId == remito.VentaId && x.Activo == true).FirstOrDefault();
                if (oRemitoVenta != null)
                {
                    oRemitoVenta.Activo = false;
                    db.Entry(oRemitoVenta).State = EntityState.Modified;
                    db.SaveChanges();
                }

                Cliente oCliente = db.Cliente.Find(remito.ClienteId);
                remito.Domicilio = oCliente.Direccion;
                remito.CUIT = oCliente.CUIT;
                remito.Activo = true;
                remito.FechaCreacion = DateTime.Now;
                string UsuarioId = User.Identity.GetUsuarioId();
                remito.UsuarioId = Convert.ToInt32(UsuarioId);
                db.Remito.Add(remito);

                Venta oVenta = db.Venta.Find(remito.VentaId);
                oVenta.ClienteId = remito.ClienteId;
                oVenta.EstadoId = (int)EstadoE.Despachado;
                db.Entry(oVenta).State = EntityState.Modified;

                db.SaveChanges();
                Transportista transportista = db.Transportista.Find(remito.TransportistaId);
                transportista.FechaUltimoTransporte = DateTime.Now;
                transportista.MontoAcumulado += db.Venta.Find(remito.VentaId).Final;
                db.Entry(transportista).State = EntityState.Modified;
                db.SaveChanges();

                string mensaje = "Remito generado. El pedido se encuentra en estado DESPACHADO";
                return RedirectToAction("Index", new
                {
                    mensaje = mensaje,
                    tipoMensaje = "success"
                });

            }

            ViewBag.TransportistaId = new SelectList(db.Transportista, "TransportistaId", "Nombre", remito.TransportistaId);
            ViewBag.ClienteId = new SelectList(db.Cliente, "ClienteId", "RazonSocial", remito.ClienteId);
            ViewBag.VentaId = new SelectList(db.Venta, "VentaId", "Observaciones", remito.VentaId);
            return View(remito);
        }

        // GET: Remitos/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Remito remito = db.Remito.Find(id);

            if (remito == null)
            {
                return HttpNotFound();
            }
            ViewBag.TransportistaId = new SelectList(db.Transportista, "TransportistaId", "Nombre", remito.TransportistaId);
            ViewBag.ClienteId = new SelectList(db.Cliente, "ClienteId", "RazonSocial", remito.ClienteId);

            var ventas = db.Venta.Select(x => new
            {
                Text = "ID: " + x.VentaId.ToString() + " - $" + x.Final + " - " + x.Fecha,
                Value = x.VentaId
            }).OrderByDescending(x => x.Value).ToList();
            ViewBag.VentaId = new SelectList(ventas, "Value", "Text", remito.VentaId);
            //ViewBag.VentaId = new SelectList(db.Venta, "VentaId", "Observaciones", remito.VentaId);
            return View(remito);
        }

        // POST: Remitos/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que quiere enlazarse. Para obtener 
        // más detalles, vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "RemitoId,UsuarioId,FechaCreacion,ClienteId,VentaId,Fecha,TransportistaId,Observaciones")] Remito remito)
        {
            if (ModelState.IsValid)
            {
                Cliente oCliente = db.Cliente.Find(remito.ClienteId);
                remito.Domicilio = oCliente.Direccion;
                remito.CUIT = oCliente.CUIT;
                remito.Activo = true;
                db.Entry(remito).State = EntityState.Modified;

                Venta oVenta = db.Venta.Find(remito.VentaId);
                oVenta.ClienteId = remito.ClienteId;
                db.Entry(oVenta).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.TransportistaId = new SelectList(db.Transportista, "TransportistaId", "Nombre", remito.TransportistaId);
            ViewBag.ClienteId = new SelectList(db.Cliente, "ClienteId", "RazonSocial", remito.ClienteId);

            var ventas = db.Venta.Select(x => new
            {
                Text = "ID: " + x.VentaId.ToString() + " - $" + x.Final + " - " + x.Fecha,
                Value = x.VentaId
            }).OrderByDescending(x => x.Value).ToList();
            ViewBag.VentaId = new SelectList(ventas, "Value", "Text", remito.VentaId);
            //ViewBag.VentaId = new SelectList(db.Venta, "VentaId", "Observaciones", remito.VentaId);
            return View(remito);
        }

        public ActionResult ReportePedido(int? id)
        {

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Remito oRemito = db.Remito.Find((int)id);


            Venta oVenta = db.Venta.Find(oRemito.VentaId);
            if (oVenta != null)
            {
                List<VentaDetalle> listVentaDetalle = oVenta.VentaDetalle.Where(x => x.Activo == true).ToList();

                AspNetUsers inicio = db.AspNetUsers.Where(x => x.UsuarioId == oVenta.UsuarioId).FirstOrDefault();

                string UsuarioVenta = inicio.Nombre + ", " + inicio.Apellido;

                List<ProductoListViewModel> list = listVentaDetalle.Select(x => new ProductoListViewModel
                {
                    ProductoId = (x.ProductoId != null) ? (int)x.ProductoId : (int)x.ComboId,
                    Nombre = (x.ProductoId != null) ? x.Producto.Nombre : x.Combo.Nombre,
                    Precio = Convert.ToDecimal(x.Precio.Replace(".", ",")),
                    Cambio = (int)x.MonedaId,
                    CambioStr = x.Moneda.Nombre,
                    Marca = (x.ProductoId != null) ? x.Producto.Marca.Nombre : "Combo",
                    Codigo = (x.ProductoId != null) ? x.Producto.Codigo : x.Combo.Codigo,
                    Cantidad = (decimal)x.Cantidad,
                    SubTotal = (decimal)Math.Round(((decimal)x.Cantidad * Convert.ToDecimal(x.Precio.Replace(".", ","))), 2)
                }).ToList();

                LocalReport localReport = new LocalReport();
                localReport.ReportPath = @"Reports/ReportRemito.rdlc";
                localReport.DisplayName = "Remito " + DateTime.Now.ToShortDateString();
                localReport.DataSources.Add(new ReportDataSource("DataSet1", list));

                string RazonSocial, CUIT, preventista, factura, transportista, direccion;
                RazonSocial = oRemito.Cliente.RazonSocial;
                CUIT = oRemito.Cliente.CUIT;
                if (oVenta.TipoVentaId == (int)TipoVentaE.Preventista)
                {
                    AspNetUsers aspNetUsers = oVenta.UsuarioReferencia.AspNetUsers.FirstOrDefault();
                    preventista = aspNetUsers.Nombre + ", " + aspNetUsers.Apellido;
                }
                else preventista = " ";

                transportista = oRemito.Transportista.Nombre + ", " + oRemito.Transportista.Apellido;
                if (oVenta.Factura.Count > 0)
                {
                    factura = oVenta.Factura.FirstOrDefault().iFacturaId;
                }
                else factura = " ";
                if (oRemito.Cliente.Direccion != null)
                {
                    direccion = oRemito.Cliente.Direccion;
                }
                else direccion = " ";
                localReport.SetParameters(new ReportParameter("NumeroRemito", oRemito.RemitoId.ToString()));
                localReport.SetParameters(new ReportParameter("Fecha", oRemito.Fecha));
                localReport.SetParameters(new ReportParameter("RazonSocial", RazonSocial));
                localReport.SetParameters(new ReportParameter("CUIT", CUIT));
                localReport.SetParameters(new ReportParameter("IVA", oRemito.Cliente.CondicionIVA.Nombre));
                localReport.SetParameters(new ReportParameter("Preventista", preventista));
                localReport.SetParameters(new ReportParameter("TipoPago", oVenta.TipoCobro.Nombre));
                localReport.SetParameters(new ReportParameter("Factura", factura));
                localReport.SetParameters(new ReportParameter("Transportista", transportista));
                localReport.SetParameters(new ReportParameter("Direccion", direccion));

                string Descuento, Recargo, Observaciones;
                Descuento = (oVenta.Descuento != null) ? oVenta.Descuento.ToString() : "0";
                Recargo = (oVenta.Recargo != null) ? "%" + oVenta.Recargo.ToString() : " ";
                Observaciones = (oRemito.Observaciones != null) ? oRemito.Observaciones : " ";

                localReport.SetParameters(new ReportParameter("Total", "$" + oVenta.Total.ToString()));
                localReport.SetParameters(new ReportParameter("Descuento", "$" + Descuento));
                localReport.SetParameters(new ReportParameter("Recargo", "%" + Recargo));
                localReport.SetParameters(new ReportParameter("Final", "$" + oVenta.Final.ToString()));
                localReport.SetParameters(new ReportParameter("Observaciones", Observaciones));


                string reportType = "PDF";
                string mimeType;
                string encoding;
                string fileNameExtension = "pdf";

                string deviceInfo =
                 @"<DeviceInfo>
                <OutputFormat>PDF</OutputFormat>
               <PageWidth>9.2in</PageWidth>
                <PageHeight>12in</PageHeight>
                <MarginTop>0.25in</MarginTop>
                <MarginLeft>0.45in</MarginLeft>
                <MarginRight>0.45in</MarginRight>
                <MarginBottom>0.25in</MarginBottom>
                </DeviceInfo>";
                Warning[] warnings;

                string[] streams;

                byte[] renderedBytes;
                //Render the report

                renderedBytes = localReport.Render(
                                reportType,
                                deviceInfo,
                                out mimeType,
                                out encoding,
                                out fileNameExtension,
                                out streams,
                                out warnings);

                var doc = new Document();
                var reader = new PdfReader(renderedBytes);
                //reader.Info["Title"] = "Detalle de " + lblVenta + " " + DateTime.Now.ToShortDateString();
                //doc.AddTitle("Detalle de " + lblVenta + " " + DateTime.Now.ToShortDateString());

                reader.Info["Title"] = "Detalle " + DateTime.Now.ToShortDateString();
                doc.AddTitle("Detalle  " + DateTime.Now.ToShortDateString());

                using (FileStream fs = new FileStream(Server.MapPath("~/Reports/Summary.pdf"), FileMode.Create))
                {
                    PdfStamper stamper = new PdfStamper(reader, fs);

                    string Printer = "";
                    if (Printer == null || Printer == "")
                    {
                        stamper.JavaScript = "var pp = getPrintParams();pp.interactive = pp.constants.interactionLevel.automatic;pp.printerName = getPrintParams().printerName;print(pp);\r";
                        stamper.Close();
                    }
                    else
                    {
                        stamper.JavaScript = "var pp = getPrintParams();pp.interactive = pp.constants.interactionLevel.automatic;pp.printerName = " + Printer + ";print(pp);\r";
                        stamper.Close();
                    }
                }
                reader.Close();

                FileStream fss = new FileStream(Server.MapPath("~/Reports/Summary.pdf"), FileMode.Open);
                byte[] bytes = new byte[fss.Length];
                fss.Read(bytes, 0, Convert.ToInt32(fss.Length));
                fss.Close();
                System.IO.File.Delete(Server.MapPath("~/Reports/Summary.pdf"));
                return File(bytes, "application/pdf");
            }
            else
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

        }

        [HttpPost]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Remito remito = db.Remito.Find(id);
            if (remito == null)
            {
                return HttpNotFound();
            }
            try
            {
                remito.Activo = false;
                db.Entry(remito).State = EntityState.Modified;

                db.SaveChanges();
                return Json(new { Success = "True" });
            }
            catch (Exception)
            {
                return Json(new { Success = "False" });
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
