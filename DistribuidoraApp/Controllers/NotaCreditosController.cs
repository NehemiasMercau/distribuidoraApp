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
using DistribuidoraAPI.Helpers;
using DistribuidoraAPI.Models;
using DistribuidoraAPI.Util;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.Reporting.WebForms;
using Negocio.entidades;
using Negocio.enumeradores;
using Negocio.gestores;

namespace DistribuidoraAPI.Controllers
{
    [Authorize(Roles = "Administracion, Desarrollo")]
    [SessionExpire]
    public class NotaCreditosController : Controller
    {
        private DistribuidoraDBEntities db = new DistribuidoraDBEntities();

        // GET: NotaCreditos
        public ActionResult Index()
        {
            var notaCredito = db.NotaCredito.Include(n => n.Cliente).Include(n => n.Venta).Include(n => n.UsuarioReferencia);
            return View(notaCredito.Where(x => x.Activo == true).ToList());
        }


        // GET: NotaCreditos/Create
        public ActionResult Create(int? RemitoId)
        {
            if (RemitoId == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            NotaCredito notaCredito = new NotaCredito();
            if (RemitoId != null)
            {
                notaCredito.RemitoId = (int)RemitoId;
                Remito oRemito = db.Remito.Find((int)RemitoId);
                ViewBag.ClienteId = oRemito.ClienteId;
                ViewBag.Cliente = oRemito.Cliente;

                Venta oVenta = db.Venta.Find(oRemito.VentaId);
                ViewBag.VentaId = oVenta.VentaId;
                notaCredito.VentaId = oVenta.VentaId;
                ViewBag.oVenta = oVenta;
                notaCredito.ClienteId = oRemito.ClienteId;
                ViewBag.DatosVenta = "Id Venta: " + oVenta.VentaId + " - Fecha: " + oVenta.Fecha + " - Estado: " + oVenta.Estado.Nombre;
                ViewBag.DatosCliente = oRemito.Cliente.RazonSocial + ". " + oRemito.Cliente.CUIT;

                List<VentaDetalle> listVentaDetalle = oVenta.VentaDetalle.Where(x => x.Activo == true).ToList();

                List<ProductoListViewModel> list = listVentaDetalle.Select(x => new ProductoListViewModel
                {
                    ProductoId = (x.ProductoId != null) ? (int)x.ProductoId : (int)x.ComboId,
                    Nombre = (x.ProductoId != null) ? x.Producto.Nombre : x.Combo.Nombre,
                    Precio = Convert.ToDecimal(x.Precio.Replace(".", ",")),
                    Cambio = (int)x.MonedaId,
                    CambioStr = x.Moneda.Nombre,
                    Marca = (x.ProductoId != null) ? x.Producto.Marca.Nombre : "Combo",
                    Codigo = (x.ProductoId != null) ? x.Producto.Codigo : x.Combo.Codigo,
                    Cantidad = (decimal)x.Cantidad
                }).ToList();
                ViewBag.listProductoListViewModel = list;
            }
            else
            {
                ViewBag.ClienteId = new SelectList(db.Cliente, "ClienteId", "RazonSocial");
                var ventas = db.Venta.Select(x => new
                {
                    Text = "ID: " + x.VentaId.ToString() + " - $" + x.Final + " - " + x.Fecha,
                    Value = x.VentaId
                }).OrderByDescending(x => x.Value).ToList();
                ViewBag.VentaId = new SelectList(ventas, "Value", "Text");
            }
            return View(notaCredito);
        }

        // POST: NotaCreditos/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que quiere enlazarse. Para obtener 
        // más detalles, vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "VentaId,RemitoId,ClienteId,Observaciones,Total")] NotaCredito notaCredito, IEnumerable<ProductoListViewModel> productoListViewModel, IEnumerable<PromosViewModel> promosViewModels, bool Reponer)
        {
            if (ModelState.IsValid)
            {
                decimal DescuentoPromo = 0;
                string observacionesPromo = "";
                foreach (PromosViewModel item in promosViewModels)
                {
                    if (item.ComboId != 0)
                    {
                        if (observacionesPromo != "") observacionesPromo += " ; ";
                        observacionesPromo += item.Descripcion;
                        DescuentoPromo += Convert.ToDecimal(item.MontoDescontado);
                    }
                }
                

                Venta oVenta = db.Venta.Find(notaCredito.VentaId);

                if (observacionesPromo != "")
                {
                    if (notaCredito.Observaciones == null || notaCredito.Observaciones == "")
                    {
                        notaCredito.Observaciones += "(Nota de Crédito) Promociones: " + observacionesPromo;

                        if (oVenta.Observaciones == null || oVenta.Observaciones == "")
                        {
                            oVenta.Observaciones += "(Nota de Crédito) Promociones: " + observacionesPromo;
                        }
                        else oVenta.Observaciones += " - (Nota de Crédito) Promociones: " + observacionesPromo;
                    }
                    else notaCredito.Observaciones += " - (Nota de Crédito) Promociones: " + observacionesPromo;
                }

                if (oVenta.Descuento == null) { oVenta.Descuento = 0; }
                else
                {
                    oVenta.Descuento -= oVenta.Promos;
                    oVenta.Final += oVenta.Promos;
                }
                oVenta.Promos = DescuentoPromo;
                oVenta.Descuento += DescuentoPromo;
                oVenta.Final -= DescuentoPromo;
                if (oVenta.NotaCreditoMonto == null)
                {
                    oVenta.NotaCreditoMonto = 0;
                }
                oVenta.NotaCreditoMonto += notaCredito.Total;

                
                oVenta.Final -= notaCredito.Total;

                Deposito oDeposito = db.Deposito.Where(x => x.Activo == true && x.SucursalId == oVenta.SucursalId).FirstOrDefault();
                
                foreach (ProductoListViewModel item in productoListViewModel)
                {
                    if (item.Codigo != "")
                    {
                        Producto oProducto = GestorProducto.getByCodigo(item.Codigo);
                        Combo oCombo = GestorCombo.getByCodigo(item.Codigo);

                        VentaDetalle oVentaDetalle;
                        if (oProducto != null)
                        {
                            oVentaDetalle = oVenta.VentaDetalle.Where(x => x.ProductoId == oProducto.ProductoId && x.Activo == true).FirstOrDefault();
                            if (Reponer)
                            {
                                StockDeposito oStockDeposito;
                                oStockDeposito = db.StockDeposito.Where(x => x.Activo == true && x.DepositoId == oDeposito.DepositoId && x.ProductoId == oProducto.ProductoId).FirstOrDefault();
                                if(oVenta.EstadoId == (int)EstadoE.Completada)
                                {
                                    oStockDeposito.Cantidad += (decimal)item.Cantidad;
                                    oStockDeposito.CantidadUsada += (decimal)item.Cantidad;
                                } else
                                {
                                    oStockDeposito.CantidadUsada += (decimal)item.Cantidad;
                                }
                                
                                oStockDeposito.FechaActualizacion = DateTime.Now;
                                db.Entry(oStockDeposito).State = EntityState.Modified;
                                db.SaveChanges();
                            }
                            NotaCreditoDetalle notaCreditoDetalle = new NotaCreditoDetalle
                            {
                                NotaCreditoId = notaCredito.NotaCreditoId,
                                ProductoId = oProducto.ProductoId,
                                Cantidad = Convert.ToInt32(item.Cantidad),
                                Monto = item.Precio,
                                Activo = true
                            };
                            db.NotaCreditoDetalle.Add(notaCreditoDetalle);
                            //db.SaveChanges();
                        }
                        else
                        {
                            oVentaDetalle = oVenta.VentaDetalle.Where(x => x.ComboId == oCombo.ComboId && x.Activo == true).FirstOrDefault();
                            if (Reponer)
                            {
                                List<ComboProducto> listComboProductos = GestorComboProducto.getComboProductoByComboId((int)oCombo.ComboId);
                                foreach (ComboProducto comboProducto in listComboProductos)
                                {
                                    StockDeposito oStockDepositoCombo = GestorStockDeposito.getByProductoAndDeposito(comboProducto.ProductoId, oDeposito.DepositoId);
                                    if (oStockDepositoCombo != null)
                                    {
                                        oStockDepositoCombo.Cantidad += (decimal)item.Cantidad;
                                        oStockDepositoCombo.CantidadUsada += (decimal)item.Cantidad;
                                        oStockDepositoCombo.FechaActualizacion = DateTime.Now;
                                        GestorStockDeposito.Actualizar(oStockDepositoCombo);
                                    }
                                }
                            }
                            NotaCreditoDetalle notaCreditoDetalle = new NotaCreditoDetalle
                            {
                                NotaCreditoId = notaCredito.NotaCreditoId,
                                ComboId = oCombo.ComboId,
                                Cantidad = Convert.ToInt32(item.Cantidad),
                                Monto = item.Precio,
                                Activo = true
                            };
                            db.NotaCreditoDetalle.Add(notaCreditoDetalle);
                            //db.SaveChanges();
                        }

                        if (oVentaDetalle.Cantidad == item.Cantidad)
                        {
                            oVentaDetalle.Activo = false;
                        }
                        else
                        {
                            oVentaDetalle.Cantidad -= item.Cantidad;
                        }

                        db.Entry(oVentaDetalle).State = EntityState.Modified;
                    }
                }
                notaCredito.Fecha = DateTime.Now;

                string UsuarioId = User.Identity.GetUsuarioId();
                notaCredito.UsuarioId = Convert.ToInt32(UsuarioId);

                db.Entry(oVenta).State = EntityState.Modified;
                notaCredito.Activo = true;
                db.NotaCredito.Add(notaCredito);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.ClienteId = new SelectList(db.Cliente, "ClienteId", "RazonSocial", notaCredito.ClienteId);
            ViewBag.VentaId = new SelectList(db.Venta, "VentaId", "Observaciones", notaCredito.VentaId);
            return View(notaCredito);
        }

        public ActionResult ReporteNotaCredito(int? id)
        {

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            
            NotaCredito notaCredito = db.NotaCredito.Find((int)id);

            Venta oVenta = db.Venta.Find(notaCredito.VentaId);
            if (oVenta != null)
            {
                List<NotaCreditoDetalle> listNotaCreditoDetalle = notaCredito.NotaCreditoDetalle.Where(x => x.Activo == true).ToList();

                AspNetUsers inicio = db.AspNetUsers.Where(x => x.UsuarioId == oVenta.UsuarioId).FirstOrDefault();

                string UsuarioVenta = inicio.Nombre + ", " + inicio.Apellido;

                List<ProductoListViewModel> list = listNotaCreditoDetalle.Select(x => new ProductoListViewModel
                {
                    ProductoId = (x.ProductoId != null) ? (int)x.ProductoId : (int)x.ComboId,
                    Nombre = (x.ProductoId != null) ? x.Producto.Nombre : x.Combo.Nombre,
                    Precio = Convert.ToDecimal(x.Monto.ToString().Replace(".", ",")),
                    Codigo = (x.ProductoId != null) ? x.Producto.Codigo : x.Combo.Codigo,
                    Cantidad = (decimal)x.Cantidad,
                    SubTotal = (decimal)Math.Round(((decimal)x.Cantidad * Convert.ToDecimal(x.Monto.ToString().Replace(".", ","))), 2)
                }).ToList();

                LocalReport localReport = new LocalReport();
                localReport.ReportPath = @"Reports/ReportNotaCredito.rdlc";
                localReport.DisplayName = "Remito " + DateTime.Now.ToShortDateString();
                localReport.DataSources.Add(new ReportDataSource("DataSet1", list));

                string RazonSocial, CUIT, factura, direccion;
                RazonSocial = notaCredito.Cliente.RazonSocial;
                CUIT = notaCredito.Cliente.CUIT;
                              
                if (oVenta.Factura.Count > 0)
                {
                    factura = oVenta.Factura.FirstOrDefault().iFacturaId;
                }
                else factura = " ";
                if (notaCredito.Cliente.Direccion != null)
                {
                    direccion = notaCredito.Cliente.Direccion;
                }
                else direccion = " ";
                localReport.SetParameters(new ReportParameter("NumeroReferencia", notaCredito.NotaCreditoId.ToString()));
                localReport.SetParameters(new ReportParameter("Fecha", notaCredito.Fecha.Value.ToString("dd/MM/yyyy")));
                localReport.SetParameters(new ReportParameter("RazonSocial", RazonSocial));
                localReport.SetParameters(new ReportParameter("CUIT", CUIT));
                localReport.SetParameters(new ReportParameter("IVA", notaCredito.Cliente.CondicionIVA.Nombre));
                localReport.SetParameters(new ReportParameter("TipoPago", oVenta.TipoCobro.Nombre));
                localReport.SetParameters(new ReportParameter("Factura", factura));
                localReport.SetParameters(new ReportParameter("Direccion", direccion));
                localReport.SetParameters(new ReportParameter("Remito", notaCredito.RemitoId.ToString()));
                localReport.SetParameters(new ReportParameter("Venta", notaCredito.VentaId.ToString()));

                string Observaciones;
                Observaciones = (notaCredito.Observaciones != null) ?  notaCredito.Observaciones : " ";

                localReport.SetParameters(new ReportParameter("Total", "$" + oVenta.Total.ToString()));
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
            NotaCredito notaCredito = db.NotaCredito.Find(id);
            if (notaCredito == null)
            {
                return HttpNotFound();
            }
            try
            {
                notaCredito.Activo = false;
                db.Entry(notaCredito).State = EntityState.Modified;

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
