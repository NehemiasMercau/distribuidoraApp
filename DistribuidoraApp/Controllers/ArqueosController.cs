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
using Microsoft.AspNet.Identity;
using Microsoft.Reporting.WebForms;
using Negocio.entidades;
using Negocio.enumeradores;

namespace DistribuidoraAPI.Controllers
{
    [Authorize]
    public class ArqueosController : Controller
    {
        private DistribuidoraDBEntities db = new DistribuidoraDBEntities();

        // GET: Arqueos
        public ActionResult Index()
        {
            return View(db.Arqueo.Where(x => x.Activo == true).ToList());
        }

        // GET: Arqueos/Details/5
        public ActionResult Details(int? id, int? arqueo)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Arqueo oArqueo = db.Arqueo.Find(id);
            List<Venta> listVentas = db.Venta
                .Where(
                x => x.Activo == true &&
                x.ArqueoId == oArqueo.ArqueoId)
                .ToList();
            if (oArqueo == null)
            {
                return HttpNotFound();
            }
            ViewBag.CantVentas = listVentas.Count();
            ViewBag.listVentas = listVentas;
            AspNetUsers inicio = db.AspNetUsers.Where(x => x.UsuarioId == oArqueo.UsuarioInicioId).FirstOrDefault();
            AspNetUsers finalizo = db.AspNetUsers.Where(x => x.UsuarioId == oArqueo.UsuarioFinalizoId).FirstOrDefault();
            ViewBag.UsuarioInicio = inicio.Nombre + ", " + inicio.Apellido;
            ViewBag.UsuarioFinalizo = finalizo.Nombre + ", " + finalizo.Apellido;
            decimal dinero = 0;
            List<ArqueoResumenModel> listTipoCobros = new List<ArqueoResumenModel>();
            foreach (Venta venta in listVentas)
            {
                dinero += (decimal)venta.Final;
                ArqueoResumenModel arqueoResumenModel = new ArqueoResumenModel();
                arqueoResumenModel.Tipo = venta.TipoCobro;
                arqueoResumenModel.Valor = (decimal)venta.Final;
                var list = listTipoCobros.Where(x => x.Tipo == arqueoResumenModel.Tipo).FirstOrDefault();
                if (list != null)
                {
                    int index = listTipoCobros.IndexOf(list);
                    listTipoCobros[index].Cant += 1;
                    listTipoCobros[index].Valor += arqueoResumenModel.Valor;
                }
                else
                {
                    arqueoResumenModel.Cant = 1;
                    listTipoCobros.Add(arqueoResumenModel);
                }
            }
            ViewBag.listTipoCobros = listTipoCobros;

            ViewBag.Dinero = dinero;
            if (arqueo != null)
            {
                ViewBag.deArqueo = true;
                return PartialView(oArqueo);
            }
            else return View(oArqueo);
        }

        public ActionResult Reporte(int? id)
        {

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Arqueo oArqueo = db.Arqueo.Find(id);
            if (oArqueo.FechaFin != null)
            {
                List<Venta> listVentas = db.Venta.Where(x => x.Activo == true && x.ArqueoId == oArqueo.ArqueoId).ToList();
                //List<CobroReportViewModel> listCobrosReport = listVentas.Select(x => new CobroReportViewModel
                //{
                //    CobroId = x.CobroId,
                //    Fecha = x.Fecha,
                //    DatosPersona = (x.Persona.PersonaTipoId == (int)Negocio.enumeradores.PersonaTipoE.Fisica) ? x.Persona.Nombre + ", " + x.Persona.Apellido : x.Persona.RazonSocial,
                //    Final = (decimal)x.Final
                //}).ToList();
                if (oArqueo == null)
                {
                    return HttpNotFound();
                }
                int CantVentas = listVentas.Count();
                AspNetUsers inicio = db.AspNetUsers.Where(x => x.UsuarioId == oArqueo.UsuarioInicioId).FirstOrDefault();
                AspNetUsers finalizo = db.AspNetUsers.Where(x => x.UsuarioId == oArqueo.UsuarioFinalizoId).FirstOrDefault();
                string UsuarioInicio = inicio.Nombre + ", " + inicio.Apellido;
                string UsuarioFinalizo = finalizo.Nombre + ", " + finalizo.Apellido;
                decimal dinero = 0;
                List<ArqueoResumenModel> listTipoCobros = new List<ArqueoResumenModel>();
                foreach (Venta item in listVentas)
                {
                    dinero += (decimal)item.Final;
                    ArqueoResumenModel arqueoResumenModel = new ArqueoResumenModel();
                    arqueoResumenModel.Tipo = item.TipoCobro;
                    arqueoResumenModel.Text = item.TipoCobro.Nombre;
                    arqueoResumenModel.Valor = (decimal)item.Final;
                    var list = listTipoCobros.Where(x => x.Tipo == arqueoResumenModel.Tipo).FirstOrDefault();
                    if (list != null)
                    {
                        int index = listTipoCobros.IndexOf(list);
                        listTipoCobros[index].Cant += 1;
                        listTipoCobros[index].Valor += arqueoResumenModel.Valor;
                    }
                    else
                    {
                        arqueoResumenModel.Cant = 1;
                        listTipoCobros.Add(arqueoResumenModel);
                    }
                }
                string Dinero = dinero.ToString();

                LocalReport localReport = new LocalReport();
                localReport.ReportPath = @"Reports/ReportArqueo.rdlc";
                localReport.DisplayName = "Arqueo de Caja " + DateTime.Now.ToShortDateString();
                localReport.DataSources.Add(new ReportDataSource("DataSet1", listVentas));
                localReport.DataSources.Add(new ReportDataSource("DataSet2", listTipoCobros));

                localReport.SetParameters(new ReportParameter("FechaInicio", oArqueo.FechaInicio.ToShortDateString()));
                localReport.SetParameters(new ReportParameter("HoraInicio", oArqueo.HoraInicio));
                string fechaFin = ""; string horaFin = "";
                if (oArqueo.FechaFin != null)
                {
                    fechaFin = oArqueo.FechaFin.Value.ToShortDateString();
                    horaFin = oArqueo.HoraFin.ToString();
                }
                localReport.SetParameters(new ReportParameter("FechaFin", fechaFin));
                localReport.SetParameters(new ReportParameter("HoraFin", horaFin));
                localReport.SetParameters(new ReportParameter("Total", "$" + oArqueo.Total.ToString()));
                localReport.SetParameters(new ReportParameter("CantidadVentas", CantVentas.ToString()));
                if (User.Identity.GetPerfilId() == "2")
                {
                    localReport.SetParameters(new ReportParameter("IniciadoCon", "$" + oArqueo.Iniciado.ToString()));
                    localReport.SetParameters(new ReportParameter("Sobrante", "$" + oArqueo.Sobrante.ToString()));
                    localReport.SetParameters(new ReportParameter("Faltante", "$" + oArqueo.Faltante.ToString()));
                    localReport.SetParameters(new ReportParameter("FinalizadoCon", "$" + oArqueo.Finalizado.ToString()));
                }
                else
                {
                    localReport.SetParameters(new ReportParameter("IniciadoCon", " "));
                    localReport.SetParameters(new ReportParameter("Sobrante", " "));
                    localReport.SetParameters(new ReportParameter("Faltante", " "));
                    localReport.SetParameters(new ReportParameter("FinalizadoCon", " "));
                }
                localReport.SetParameters(new ReportParameter("DineroIngresado", "$" + Dinero));
                localReport.SetParameters(new ReportParameter("UsuarioInicio", UsuarioInicio));
                localReport.SetParameters(new ReportParameter("UsuarioFinalizo", UsuarioFinalizo));


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
                reader.Info["Title"] = "Arqueo de Caja " + DateTime.Now.ToShortDateString();
                doc.AddTitle("Arqueo de Caja " + DateTime.Now.ToShortDateString());

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

        public ActionResult IniciarArqueo(string dinero)
        {
            if (dinero == "")
            {
                dinero = "0";
            }
            if (dinero == null)
            {
                dinero = "0";
            }
            string usuarioId = User.Identity.GetUserId();
            AspNetUsers aspNetUsers = db.AspNetUsers.Where(x => x.Id == usuarioId).FirstOrDefault();
            Arqueo oArqueo = new Arqueo()
            {
                Activo = true,
                FechaInicio = DateTime.Now,
                HoraInicio = DateTime.Now.Hour.ToString() + ":" + DateTime.Now.Minute.ToString(),
                Abierto = true,
                Iniciado = Convert.ToDecimal(dinero.Replace(".", ",")),
                UsuarioInicioId = aspNetUsers.UsuarioId,
                ArqueoTipoId = (aspNetUsers.PerfilId == (int)PerfilE.Preventista) ? (int)ArqueoTipoE.Preventa : (int)ArqueoTipoE.Caja
            };
            db.Arqueo.Add(oArqueo);
            db.SaveChanges();

            return RedirectToAction("Index", "Home");
        }

        public ActionResult FinalizarArqueo(string dinero)
        {
            if (dinero == null || dinero == "")
            {
                dinero = "0";
            }

            string usuarioId = User.Identity.GetUserId();
            AspNetUsers aspNetUsers = db.AspNetUsers.Where(x => x.Id == usuarioId).FirstOrDefault();
            Arqueo oArqueo = db.Arqueo.Where(x => x.Activo == true && x.Abierto == true && x.UsuarioInicioId == aspNetUsers.UsuarioId).FirstOrDefault();
            if (oArqueo != null)
            {
                oArqueo.FechaFin = DateTime.Now;
                oArqueo.HoraFin = DateTime.Now.Hour.ToString() + ":" + DateTime.Now.Minute.ToString();
                oArqueo.Abierto = false;
                oArqueo.Finalizado = Convert.ToDecimal(dinero.Replace(".", ","));
                oArqueo.UsuarioFinalizoId = aspNetUsers.UsuarioId;
                //Buscar todos los cobros con el id de arqueo
                List<Venta> listVentas = db.Venta.Where(x => x.Activo == true && x.ArqueoId == oArqueo.ArqueoId).ToList();
                decimal Monto = 0;
                decimal cobroEfectivo = 0;
                // List<ArqueoResumenModel> listTipoCobros = new List<ArqueoResumenModel>();
                foreach (Venta venta in listVentas)
                {
                    Monto += (decimal)venta.Final;
                    //ArqueoResumenModel arqueoResumenModel = new ArqueoResumenModel();
                    //arqueoResumenModel.Tipo = venta.TipoCobro;
                    //arqueoResumenModel.Valor = (decimal)venta.Final;
                    //var list = listTipoCobros.Where(x => x.Tipo == arqueoResumenModel.Tipo).FirstOrDefault();
                    //if(list != null)
                    //{
                    //    int index = listTipoCobros.IndexOf(list);
                    //    listTipoCobros[index].Valor += arqueoResumenModel.Valor;
                    //} else
                    //{
                    //    listTipoCobros.Add(arqueoResumenModel);
                    //}
                }
                //  ViewBag.listTipoCobros = listTipoCobros;
                oArqueo.Total = Monto;
                oArqueo.TotalEfectivo = cobroEfectivo;
                decimal totalIdeal = (decimal)oArqueo.Iniciado + (decimal)oArqueo.Total;
                if (oArqueo.Finalizado == totalIdeal)
                {
                    oArqueo.Sobrante = 0;
                    oArqueo.Faltante = 0;
                }
                else
                {
                    if (oArqueo.Finalizado > totalIdeal)
                    {
                        oArqueo.Sobrante = (decimal)oArqueo.Finalizado - totalIdeal;
                        oArqueo.Faltante = 0;
                    }
                    else
                    {
                        oArqueo.Faltante = totalIdeal - (decimal)oArqueo.Finalizado;
                        oArqueo.Sobrante = 0;
                    }
                }
            }
            db.Entry(oArqueo).State = EntityState.Modified;
            db.SaveChanges();

            return RedirectToAction("Details", "Arqueos", new { id = oArqueo.ArqueoId });
        }

        [HttpPost]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Arqueo arqueo = db.Arqueo.Find(id);
            if (arqueo == null)
            {
                return HttpNotFound();
            }
            try
            {
                arqueo.Activo = false;
                db.Entry(arqueo).State = EntityState.Modified;

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
