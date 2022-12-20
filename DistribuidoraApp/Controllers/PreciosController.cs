using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using DistribuidoraAPI.Helpers;
using DistribuidoraAPI.Models;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.Reporting.WebForms;
using Negocio.entidades;
using Negocio.enumeradores;
using Negocio.gestores;
using Newtonsoft.Json;

namespace DistribuidoraAPI.Controllers
{
    [Authorize(Roles = "Administracion, Desarrollo")]
    [SessionExpire]
    public class PreciosController : Controller
    {
        private DistribuidoraDBEntities db = new DistribuidoraDBEntities();

        // GET: Precios
        public ActionResult Index()
        {

            var precioLista = db.PrecioLista.Include(p => p.Combo).Include(p => p.Lista).Include(p => p.Producto);
            return View(precioLista.ToList());
        }

        // GET: Precios/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            PrecioLista precioLista = db.PrecioLista.Find(id);
            if (precioLista == null)
            {
                return HttpNotFound();
            }
            PrecioListaViewModel oPrecioListaViewModel = new PrecioListaViewModel()
            {
                PrecioListaId = precioLista.PrecioListaId,
                Precio = (decimal)precioLista.Precio,
                Lista = precioLista.Lista,
                MonedaId = (MonedaE)precioLista.MonedaId,
                ProductoId = precioLista.ProductoId,
                ComboId = precioLista.ComboId
            };
            if (precioLista.MonedaId == (int)MonedaE.ARS)
            {
                oPrecioListaViewModel.MonedaId = MonedaE.ARS;
            }
            else
            {
                oPrecioListaViewModel.MonedaId = MonedaE.USD;
            }
            return PartialView(oPrecioListaViewModel);
        }

        public ActionResult Create(int? ProductoId, int? ComboId)
        {
            if (ProductoId == null || ComboId == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            List<PrecioLista> listPrecioLista;
            GestorEntidadesConexion.inicializarContexto();
            GestorEntidadesConexion.beginTransaccion();
            try

            {
                if (ProductoId != 0)
                {
                    PrecioHelper.verificarPrecioLista((int)ProductoId, 0);
                    Producto oProducto = GestorProducto.getById((int)ProductoId);
                    ViewBag.Producto = oProducto.Nombre + " (" + oProducto.Marca.Nombre + ")";
                    ViewBag.ProductoId = oProducto.ProductoId;
                    ViewBag.ComboId = 0;
                }
                else
                {
                    PrecioHelper.verificarPrecioLista(0, (int)ComboId);
                    Combo oCombo = GestorCombo.getById((int)ComboId);
                    ViewBag.Combo = oCombo.Nombre + ", " + oCombo.Descripcion;
                    ViewBag.ComboId = oCombo.ComboId;
                    ViewBag.ProductoId = 0;
                }
                GestorEntidadesConexion.commitTransaccion();
                GestorEntidadesConexion.SaveChanges();
                GestorEntidadesConexion.disposeTransaccion();

                if (ProductoId != 0)
                {
                    listPrecioLista = db.PrecioLista.Where(x => x.ProductoId == ProductoId && x.Activo == true).ToList();
                }
                else
                {
                    listPrecioLista = db.PrecioLista.Where(x => x.ComboId == ComboId && x.Activo == true).ToList();
                }

            }
            catch (Exception)
            {
                listPrecioLista = null;
                GestorEntidadesConexion.rollbackTransaccion();
            }
            finally
            {
                GestorEntidadesConexion.disposeTransaccion();
            }

            PrecioListaViewModel oPrecioListaViewModel = new PrecioListaViewModel();
            oPrecioListaViewModel.listPrecioLista = listPrecioLista;

            return View(oPrecioListaViewModel);
        }

        // POST: Precios/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        public ActionResult Create(int? id, string precio, int? moneda)
        {

            //if (id == null || precio == null || moneda == null || precio == "")
            //{
            //    List<string> errors = new List<string>();
            //    errors.Add("Error 1");
            //    return Json(errors);
            //}
            if (moneda == null)
            {
                PrecioLista preciolista = db.PrecioLista.Find((int)id);
                return PartialView("_Create", preciolista);
            }

            PrecioLista oPrecioLista = db.PrecioLista.Find((int)id);
            oPrecioLista.PrecioAnterior = oPrecioLista.Precio;
            oPrecioLista.Precio = Convert.ToDecimal(precio);
            oPrecioLista.Fecha = DateTime.Now;
            oPrecioLista.Cargado = true;
            oPrecioLista.MonedaId = (int)moneda;
            db.Entry(oPrecioLista).State = EntityState.Modified;
            db.SaveChanges();

            PrecioLista precioLista = new PrecioLista()
            {
                Precio = oPrecioLista.Precio,
                PrecioAnterior = oPrecioLista.PrecioAnterior,
                Cargado = oPrecioLista.Cargado
            };
            ModelState.Clear();
            System.Text.StringBuilder sbb = new System.Text.StringBuilder();
            sbb.Append("<h4>Cliente Registrado!</h4>");

            //return Content(sbb.ToString());
            return PartialView("_Create", oPrecioLista);
            // return Json(precioLista);
        }

        // POST: Precios/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        //public ActionResult Edit([Bind(Include = "PrecioListaId,Precio,MonedaARS,MonedaUSD")] PrecioListaViewModel oPrecioListaViewModel)
        public ActionResult Edit(PrecioListaViewModel oPrecioListaViewModel)
        {
            PrecioLista oPrecioLista = db.PrecioLista.Find(oPrecioListaViewModel.PrecioListaId);
            if (ModelState.IsValid)
            {
                if (oPrecioListaViewModel.MonedaId != 0)
                {
                    oPrecioLista.Precio = oPrecioListaViewModel.Precio;
                    oPrecioLista.Fecha = DateTime.Now;
                    oPrecioLista.MonedaId = (int)oPrecioListaViewModel.MonedaId;
                    db.Entry(oPrecioLista).State = EntityState.Modified;
                    db.SaveChanges();
                }
            }

            if (oPrecioLista.ProductoId != null)
            {
                return RedirectToAction("Create", "Precios", new { ProductoId = oPrecioLista.ProductoId, ComboId = 0 });
            }
            else
            {
                return RedirectToAction("Create", "Precios", new { ProductoId = 0, ComboId = oPrecioLista.ComboId });
            }
        }

        public ActionResult PrecioMasivo()
        {
            List<PrecioLista> listPrecioLista = db.PrecioLista.Where(x => x.Activo == true).ToList();
            List<PrecioMasivoViewModel> list = listPrecioLista.Select(x => new PrecioMasivoViewModel
            {
                Codigo = (x.ProductoId != null) ? x.Producto.Codigo : x.Combo.Codigo,
                Cargado = x.Cargado,
                Nombre = (x.ProductoId != null) ? x.Producto.Nombre : x.Combo.Nombre,
                Precio = (decimal)x.Precio,
                PrecioListaId = x.PrecioListaId,
                Lista = x.Lista.Nombre,
                Moneda = x.Moneda.Nombre
            }).ToList();

            list = list.OrderBy(x => x.PrecioListaId).ThenBy(x => x.Nombre).ThenBy(x => x.Precio).ThenBy(x => x.Lista).ToList();

            ViewBag.TipoProductoId = new SelectList(db.TipoProducto.Where(x => x.Activo == true), "TipoProductoId", "Nombre");
            ViewBag.MarcaId = new SelectList(db.Marca.Where(x => x.Activo == true), "MarcaId", "Nombre");
            ViewBag.ListaId = new SelectList(db.Lista.Where(x => x.Activo == true), "ListaId", "Nombre");
            ViewBag.ProveedorId = new SelectList(db.Proveedor.Where(x => x.Activo == true), "ProveedorId", "Nombre");
            return View(list);
        }

        [HttpPost]
        public ActionResult PrecioMasivoBusqueda(int? ListaId, int? ProveedorId, int? MarcaId, int? TipoProductoId)
        {
            List<PrecioLista> listPrecioLista = db.PrecioLista.Where(x => x.Activo == true).ToList();
            if (ListaId != null)
            {
                listPrecioLista = listPrecioLista.Where(x => x.ListaId == (int)ListaId).ToList();
            }
            if (ProveedorId != null)
            {
                listPrecioLista = listPrecioLista.Where(x => x.ProductoId != null).ToList();
                listPrecioLista = listPrecioLista.Where(x => x.Producto.ProveedorId == (int)ProveedorId).ToList();
            }
            if (MarcaId != null)
            {
                listPrecioLista = listPrecioLista.Where(x => x.ProductoId != null).ToList();
                listPrecioLista = listPrecioLista.Where(x => x.Producto.MarcaId == (int)MarcaId).ToList();
            }
            if (TipoProductoId != null)
            {
                listPrecioLista = listPrecioLista.Where(x => x.ProductoId != null).ToList();
                listPrecioLista = listPrecioLista.Where(x => x.Producto.TipoProductoId == (int)TipoProductoId).ToList();
            }


            List<PrecioMasivoViewModel> list = listPrecioLista.Select(x => new PrecioMasivoViewModel
            {
                Codigo = (x.ProductoId != null) ? x.Producto.Codigo : x.Combo.Codigo,
                Cargado = x.Cargado,
                Nombre = (x.ProductoId != null) ? x.Producto.Nombre : x.Combo.Nombre,
                Precio = (decimal)x.Precio,
                PrecioListaId = x.PrecioListaId,
                Lista = x.Lista.Nombre,
                Moneda = x.Moneda.Nombre
            }).ToList();
            list = list.OrderBy(x => x.PrecioListaId).ThenBy(x => x.Nombre).ThenBy(x => x.Precio).ThenBy(x => x.Lista).ToList();

            return PartialView("_PrecioMasivo", list);
        }

        [HttpPost]
        public JsonResult ActualizarPrecio(string precio, int moneda, int id)
        {
            try
            {
                precio = precio.Replace(".", ",");
                decimal Precio = Convert.ToDecimal(precio);
                PrecioLista precioLista = db.PrecioLista.Find(id);
                precioLista.PrecioAnterior = precioLista.Precio;
                precioLista.Precio = Precio;
                precioLista.MonedaId = moneda;
                precioLista.Fecha = DateTime.Now;
                precioLista.Cargado = true;
                db.Entry(precioLista).State = EntityState.Modified;
                db.SaveChanges();

                var result = new { Success = "True", Message = "Precio Actualizado", icon = "success" };
                return Json(result);
            }
            catch (Exception ex)
            {
                var result = new { Success = "False", Message = "Ocurrió un error", error = ex.Message, icon = "error" };
                return Json(result);
            }


        }

        [HttpPost]
        public JsonResult ActualizarPrecioPorcentaje(string porcentaje, int[] PrecioListaIds)
        {
            try
            {
                porcentaje = porcentaje.Replace(".", ",");
                decimal Porcentaje = Convert.ToDecimal(porcentaje);
                Porcentaje = (Porcentaje / 100) + 1;

                foreach (int id in PrecioListaIds)
                {
                    PrecioLista precioLista = db.PrecioLista.Find(id);
                    precioLista.PrecioAnterior = precioLista.Precio;
                    precioLista.Precio *= Porcentaje;
                    precioLista.Fecha = DateTime.Now;
                    precioLista.Cargado = true;
                    db.Entry(precioLista).State = EntityState.Modified;

                }
                db.SaveChanges();

                var result = new { Success = "True", Message = "Precios Actualizados", icon = "success" };
                return Json(result);
            }
            catch (Exception ex)
            {
                var result = new { Success = "False", Message = "Ocurrió un error", error = ex.Message, icon = "error" };
                return Json(result);
            }


        }

        public ActionResult ImprimirEtiquetas(string ids)
        {
            string[] arrayIds = ids.Split(',');
            decimal cant = Convert.ToDecimal(arrayIds.Length);

            int cant1 = Convert.ToInt32(Math.Ceiling(cant / 3));
            int cant2 = cant1 + cant1;
            int cant3 = (arrayIds.Length - cant2);
            if (cant1 == 1)
            {
                cant2 = 0; cant3 = 0;
            }


            int[] arrayIdsInt1 = new int[cant1];
            int[] arrayIdsInt2 = new int[cant1];
            int[] arrayIdsInt3 = new int[cant3];


            for (int i = 0; i < cant1; i++)
            {
                int id = Convert.ToInt32(arrayIds[i]);
                arrayIdsInt1[i] = id;
            }
            if (cant2 > 0)
            {
                for (int i = 0; (i + cant1) < cant2; i++)
                {
                    int id = Convert.ToInt32(arrayIds[i + cant1]);
                    arrayIdsInt2[i] = id;
                }
                for (int i = 0; (i + cant2) < (cant2 + cant3); i++)
                {
                    int id = Convert.ToInt32(arrayIds[i + cant2]);
                    arrayIdsInt3[i] = id;
                }
            }



            byte[] bytes = imprimirEtiquetas(arrayIdsInt1, arrayIdsInt2, arrayIdsInt3);
            return File(bytes, "application/pdf");

        }

        private byte[] imprimirEtiquetas(int[] array1, int[] array2, int[] array3)
        {
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

            List<PrecioLista> precioListas1 = db.PrecioLista.Where(x => x.Activo == true && array1.Contains(x.PrecioListaId)).ToList();
            var lista1 = precioListas1.Select(x => new
            {
                Codigo = (x.ProductoId != null) ? x.Producto.Codigo : x.Combo.Codigo,
                Nombre = (x.ProductoId != null) ? x.Producto.Nombre : x.Combo.Nombre,
                Precio = (x.MonedaId == (int)MonedaE.ARS) ? x.Precio : (x.Precio * precioUSD)
            }).ToList();

            List<PrecioLista> precioListas2 = db.PrecioLista.Where(x => x.Activo == true && array2.Contains(x.PrecioListaId)).ToList();
            var lista2 = precioListas2.Select(x => new
            {
                Codigo = (x.ProductoId != null) ? x.Producto.Codigo : x.Combo.Codigo,
                Nombre = (x.ProductoId != null) ? x.Producto.Nombre : x.Combo.Nombre,
                Precio = (x.MonedaId == (int)MonedaE.ARS) ? x.Precio : (x.Precio * precioUSD)
            }).ToList();

            List<PrecioLista> precioListas3 = db.PrecioLista.Where(x => x.Activo == true && array3.Contains(x.PrecioListaId)).ToList();
            var lista3 = precioListas3.Select(x => new
            {
                Codigo = (x.ProductoId != null) ? x.Producto.Codigo : x.Combo.Codigo,
                Nombre = (x.ProductoId != null) ? x.Producto.Nombre : x.Combo.Nombre,
                Precio = (x.MonedaId == (int)MonedaE.ARS) ? x.Precio : (x.Precio * precioUSD)
            }).ToList();

            LocalReport localReport = new LocalReport();
            localReport.ReportPath = @"Reports/ReportEtiquetas.rdlc";
            localReport.DataSources.Add(new ReportDataSource("DataSet1", lista1));
            localReport.DataSources.Add(new ReportDataSource("DataSet2", lista2));
            localReport.DataSources.Add(new ReportDataSource("DataSet3", lista3));


            //localReport.SetParameters(new ReportParameter("tipo", lbltipo));
            //localReport.SetParameters(new ReportParameter("generador", lblgenerador));
            //localReport.SetParameters(new ReportParameter("usuario", lblusuario));
            //localReport.SetParameters(new ReportParameter("numero", oVenta.VentaId.ToString()));
            //localReport.SetParameters(new ReportParameter("formaPago", oVenta.TipoCobro.Nombre));

            string reportType = "PDF";
            string mimeType;
            string encoding;
            string fileNameExtension = "pdf";

            string deviceInfo =
             @"<DeviceInfo>
                <OutputFormat>PDF</OutputFormat>
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

            using (FileStream fs = new FileStream(Server.MapPath("~/Reports/Labels.pdf"), FileMode.Create))
            {
                PdfStamper stamper = new PdfStamper(reader, fs);

                string Printer = "FK58 Printer";
                //string Printer = "";
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

            FileStream fss = new FileStream(Server.MapPath("~/Reports/Labels.pdf"), FileMode.Open);
            byte[] bytes = new byte[fss.Length];
            fss.Read(bytes, 0, Convert.ToInt32(fss.Length));
            fss.Close();


            System.IO.File.Delete(Server.MapPath("~/Reports/Labels.pdf"));
            return bytes;
        }

        public async Task<ActionResult> GetPrecioDolar()
        {
            List<Root> myDeserializedClass;
            using (var client = new HttpClient())
            {
                var url = "https://www.dolarsi.com/api/api.php?type=valoresprincipales";
                var response = await client.GetStringAsync(url);
                myDeserializedClass = JsonConvert.DeserializeObject<List<Root>>(response);
            }


            return Json(myDeserializedClass, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetPrecioDolarStatic()
        {
            decimal precioUSD;
            if (Session["Dolar"] != null)
            {
                string s = Session["Dolar"].ToString();
                precioUSD = Convert.ToDecimal(Session["Dolar"].ToString());
            }
            else
            {
                precioUSD = PrecioHelper.GetPrecioDolarStatico();
                Session.Add("Dolar", precioUSD);
            }

            var result = new { Success = "True", Dolar = precioUSD };
            return Json(result, JsonRequestBehavior.AllowGet);
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
