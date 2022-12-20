using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using DistribuidoraAPI.Extensions;
using DistribuidoraAPI.Helpers;
using DistribuidoraAPI.Models;
using Microsoft.AspNet.Identity;
using Negocio.entidades;
using Negocio.enumeradores;
using Negocio.gestores;

namespace DistribuidoraAPI.Controllers
{
    [Authorize]
    [SessionExpire]
    public class ProductosController : Controller
    {
        private DistribuidoraDBEntities db = new DistribuidoraDBEntities();

        // GET: Productos
        public ActionResult Index()
        {
            List<Producto> listProductos = db.Producto.Include(p => p.Marca).Include(p => p.TipoDuracion).Include(p => p.TipoProducto).Where(p => p.Activo == true).ToList();

            List<ProductoStockViewModel> listProductoStockViewModel = new List<ProductoStockViewModel>();

            foreach (Producto item in listProductos)
            {
                listProductoStockViewModel.Add(new ProductoStockViewModel
                {
                    oProducto = item,
                    Cantidad = item.StockDeposito.Where(x => x.Activo == true).FirstOrDefault().Cantidad,
                    CantidadReservada = (decimal)item.StockDeposito.Where(x => x.Activo == true).FirstOrDefault().CantidadUsada,
                });
            }

            //listProductoStockViewModel = listProductos.Select(x => new ProductoStockViewModel
            //{
            //    oProducto = (Producto)x,
            //    Cantidad = x.StockDeposito.Where(x => x.Activo == true).FirstOrDefault().Cantidad,
            //    CantidadReservada = (decimal)x.StockDeposito.Where(x => x.Activo == true).FirstOrDefault().CantidadUsada,
            //}).ToList();
            return View(listProductoStockViewModel.ToList());
        }

        // GET: Productos/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Producto producto = db.Producto.Find(id);
            if (producto == null)
            {
                return HttpNotFound();
            }
            return PartialView("_Details", producto);
        }

        // GET: Productos/Create
        public ActionResult Create()
        {
            ViewBag.MarcaId = new SelectList(db.Marca.Where(x => x.Activo == true), "MarcaId", "Nombre");
            ViewBag.TipoDuracionId = new SelectList(db.TipoDuracion.Where(x => x.Activo == true), "TipoDuracionId", "Nombre");
            ViewBag.TipoProductoId = new SelectList(db.TipoProducto.Where(x => x.Activo == true), "TipoProductoId", "Nombre");
            ViewBag.Depositos = new MultiSelectList(db.Deposito.Where(x => x.Activo == true), "DepositoId", "Nombre");
            ViewBag.ProveedorId = new SelectList(db.Proveedor.Where(x => x.Activo == true), "ProveedorId", "Nombre");

            return View();
        }

        // POST: Productos/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Codigo,DepositosList,TipoProductoId,TipoDuracionId,MarcaId,Nombre,PrecioCosto,Minimo,ProveedorId")] ProductoViewModel producto)
        {
            if (ModelState.IsValid)
            {
                Producto oProductoExistente = GestorProducto.getByCodigo(producto.Codigo);
                if (oProductoExistente != null)
                {
                    ModelState.AddModelError("Codigo", "El código ingresado ya existe");
                }
                else
                {
                    Producto oProducto = new Producto();
                    oProducto.TipoProductoId = producto.TipoProductoId;
                    oProducto.TipoDuracionId = producto.TipoDuracionId;
                    oProducto.MarcaId = producto.MarcaId;
                    oProducto.Nombre = producto.Nombre.ToUpper();
                    oProducto.PrecioCosto = producto.PrecioCosto;
                    oProducto.Minimo = producto.Minimo;
                    oProducto.Codigo = producto.Codigo;
                    oProducto.ProveedorId = producto.ProveedorId;
                    int[] depositosDefault = new int[1] { 1 };
                    producto.DepositosList = depositosDefault;

                    if (producto.DepositosList != null)
                    {
                        try
                        {
                            GestorEntidadesConexion.inicializarContexto();
                            GestorEntidadesConexion.beginTransaccion();
                            GestorProducto.Insertar(oProducto);
                            PrecioHelper.verificarPrecioLista(oProducto.ProductoId, 0);
                            if (ProductoHelper.guardarStockPorTipo(oProducto.ProductoId, 0, TipoStockE.Producto, producto.Cantidad, producto.DepositosList, TipoAccionE.Guardar))
                            {
                                GestorEntidadesConexion.commitTransaccion();
                                return RedirectToAction("Index");
                            }
                            else
                            {
                                GestorEntidadesConexion.rollbackTransaccion();
                                ModelState.AddModelError("", "No se pudo guardar el stock o existe algún problema.");
                            }
                        }
                        catch (Exception ex)
                        {
                            ModelState.AddModelError("", "Ocurrió un Error al intentar guardar los datos, y no se guardaron.");
                            GestorEntidadesConexion.rollbackTransaccion();
                        }
                        finally
                        {
                            GestorEntidadesConexion.disposeTransaccion();
                        }
                    }
                    else
                    {
                        ModelState.AddModelError("", "No se pudieron guardar los datos, seleccionar depósitos");
                    }
                }


            }
            else
            {
                ModelState.AddModelError("", "No se pudieron guardar los datos, revise los campos faltantes");
            }


            ViewBag.MarcaId = new SelectList(db.Marca.Where(x => x.Activo == true), "MarcaId", "Nombre", producto.MarcaId);
            ViewBag.TipoDuracionId = new SelectList(db.TipoDuracion.Where(x => x.Activo == true), "TipoDuracionId", "Nombre", producto.TipoDuracionId);
            ViewBag.TipoProductoId = new SelectList(db.TipoProducto.Where(x => x.Activo == true), "TipoProductoId", "Nombre", producto.TipoProductoId);
            ViewBag.ProveedorId = new SelectList(db.Proveedor.Where(x => x.Activo == true), "ProveedorId", "Nombre", producto.ProveedorId);

            if (producto.DepositosList != null)
            {
                ViewBag.Depositos = new MultiSelectList(db.Deposito.Where(x => x.Activo == true), "DepositoId", "Nombre", db.Deposito
                    .Where(x => producto.DepositosList.Contains(x.DepositoId)).Select(x => x.DepositoId).ToArray());
            }
            else
            {
                ViewBag.Depositos = new MultiSelectList(db.Deposito.Where(x => x.Activo == true), "DepositoId", "Nombre", producto.DepositosList);
            }

            return View(producto);
        }

        [HttpGet]
        public ActionResult AltaStock(int? id, int? tipoStock)
        {
            if (id == null || tipoStock == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            List<StockDeposito> listDepositoStockModel;
            if ((TipoStockE)tipoStock == TipoStockE.Producto)
            {
                listDepositoStockModel = GestorStock.getDepositosConStock((int)id, 0);
                Producto oProducto = GestorProducto.getById((int)id);
                ViewBag.Producto = oProducto.Nombre + " (" + oProducto.Marca.Nombre + ")";
                ViewBag.ProductoId = oProducto.ProductoId;
                ViewBag.ComboId = 0;
            }
            else
            {
                Combo oCombo = GestorCombo.getById((int)id);
                ViewBag.Combo = oCombo.Nombre + ", " + oCombo.Descripcion;
                listDepositoStockModel = GestorStock.getDepositosConStock(0, (int)id);
                ViewBag.ComboId = oCombo.ComboId;
                ViewBag.ProductoId = 0;
            }
            ViewBag.Depositos = new SelectList(db.Deposito.Where(x => x.Activo == true), "DepositoId", "Nombre");

            return View("AltaStock", listDepositoStockModel);
        }

        [HttpPost]
        public ActionResult AgregarStock(int? cboDepositos, double? txtCantidad, double? txtCantidadDisponible, int? ProductoId, int? ComboId)
        {
            if (cboDepositos == null || ProductoId == null || ComboId == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            try
            {
                if (txtCantidad == null)
                {
                    txtCantidad = 0;
                }
                string descripcion = ""; string tipo = "";
                StockDeposito oStockDeposito = GestorStock.GetStockDeposito(cboDepositos.Value, (int)ProductoId, (int)ComboId);
                tipo = (ProductoId != 0) ? "Producto" : "Combo";
                descripcion = "Se actualizó el stock del " + tipo + ": " + ProductoId.ToString() + ". Cantidad original: " + oStockDeposito.Cantidad +
                       ". Agregado: " + txtCantidad.ToString() + ". Total final: " + (oStockDeposito.Cantidad + (decimal)txtCantidad).ToString();
                oStockDeposito.Cantidad += (decimal)txtCantidad;
                if (txtCantidadDisponible != null)
                {
                    oStockDeposito.CantidadUsada = (decimal)txtCantidadDisponible;
                }
                else
                {
                    oStockDeposito.CantidadUsada += (decimal)txtCantidad;
                }

                GestorStock.actualizarStockDeposito(oStockDeposito);
                GestorStock.agregarActualizacionStock(descripcion, 1);

            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Ocurrió un Error al intentar guardar los datos.");
                List<StockDeposito> listDepositoStockModel;
                if (ProductoId != 0)
                {
                    listDepositoStockModel = GestorStock.getDepositosConStock((int)ProductoId, 0);
                    Producto oProducto = GestorProducto.getById((int)ProductoId);
                    ViewBag.Producto = oProducto.Nombre + " (" + oProducto.Marca.Nombre + ")";
                    ViewBag.ProductoId = oProducto.ProductoId;
                    ViewBag.ComboId = 0;
                }
                else
                {
                    Combo oCombo = GestorCombo.getById((int)ComboId);
                    ViewBag.Combo = oCombo.Nombre + ", " + oCombo.Descripcion;
                    listDepositoStockModel = GestorStock.getDepositosConStock(0, (int)ComboId);
                    ViewBag.ComboId = oCombo.ComboId;
                    ViewBag.ProductoId = 0;
                }
                ViewBag.Depositos = new SelectList(db.Deposito.Where(x => x.Activo == true), "DepositoId", "Nombre");
                return View("AltaStock", listDepositoStockModel);
            }

            int? idParam = 0; int? tipoStockParam = 0;
            if (ProductoId != 0)
            {
                idParam = (int)ProductoId; tipoStockParam = 1;
            }
            else { idParam = (int)ComboId; tipoStockParam = 2; }

            return RedirectToAction("AltaStock", new { id = idParam, tipoStock = tipoStockParam });



        }

        public ActionResult AltaStockMasiva()
        {

            List<StockDeposito> listStockDeposito;

            listStockDeposito = GestorStock.GetStockDepositoProductos(1);//hay solo 1 deposito

            ViewBag.TipoProductoId = new SelectList(db.TipoProducto.Where(x => x.Activo == true), "TipoProductoId", "Nombre");
            ViewBag.MarcaId = new SelectList(db.Marca.Where(x => x.Activo == true), "MarcaId", "Nombre");
            ViewBag.ProveedorId = new SelectList(db.Proveedor.Where(x => x.Activo == true), "ProveedorId", "Nombre");
            return View(listStockDeposito);
        }

        [HttpPost]
        public ActionResult AltaStockMasivaBusqueda(int? ProveedorId, int? MarcaId, int? TipoProductoId)
        {
            List<StockDeposito> listStockDeposito = db.StockDeposito.Include("Producto").Where(x => x.Activo == true && x.ProductoId != null && x.DepositoId == 1).ToList();

            if (ProveedorId != null)
            {
                listStockDeposito = listStockDeposito.Where(x => x.Producto.ProveedorId == (int)ProveedorId).ToList();
            }
            if (MarcaId != null)
            {
                listStockDeposito = listStockDeposito.Where(x => x.Producto.MarcaId == (int)MarcaId).ToList();
            }
            if (TipoProductoId != null)
            {
                listStockDeposito = listStockDeposito.Where(x => x.Producto.TipoProductoId == (int)TipoProductoId).ToList();
            }


            //List<PrecioMasivoViewModel> list = listPrecioLista.Select(x => new PrecioMasivoViewModel
            //{
            //    Codigo = (x.ProductoId != null) ? x.Producto.Codigo : x.Combo.Codigo,
            //    Cargado = x.Cargado,
            //    Nombre = (x.ProductoId != null) ? x.Producto.Nombre : x.Combo.Nombre,
            //    Precio = (decimal)x.Precio,
            //    PrecioListaId = x.PrecioListaId,
            //    Lista = x.Lista.Nombre,
            //    Moneda = x.Moneda.Nombre
            //}).ToList();
            //list = list.OrderBy(x => x.PrecioListaId).ThenBy(x => x.Nombre).ThenBy(x => x.Precio).ThenBy(x => x.Lista).ToList();

            return PartialView("_AltaStockMasiva", listStockDeposito);
        }

        [HttpPost]
        public JsonResult ActualizarStockUnico(string stock, int id)
        {
            try
            {
                actualizarDatosStock(Convert.ToDecimal(stock), id);
                //StockDeposito oStockDeposito = db.StockDeposito.Find(id);
                //string descripcion = "Se actualizó el stock del Producto: " + oStockDeposito.ProductoId.ToString() + ". Cantidad original: " + oStockDeposito.Cantidad +
                //       ". Agregado: " + stock + ". Total final: " + (oStockDeposito.Cantidad + Convert.ToDecimal(stock)).ToString();
                //oStockDeposito.Cantidad += Convert.ToDecimal(stock);
                ////if (txtCantidadDisponible != null)
                ////{
                ////    oStockDeposito.CantidadUsada = (decimal)txtCantidadDisponible;
                ////}
                ////else
                ////{
                //oStockDeposito.CantidadUsada += Convert.ToDecimal(stock);
                ////}

                //GestorStock.actualizarStockDeposito(oStockDeposito);
                
                //GestorStock.agregarActualizacionStock(descripcion, Convert.ToInt32(User.Identity.GetUsuarioId()));

                var result = new { Success = "True", Message = "Stock Actualizado", icon = "success" };
                return Json(result);
            }
            catch (Exception ex)
            {
                var result = new { Success = "False", Message = "Ocurrió un error", error = ex.Message, icon = "error" };
                return Json(result);
            }


        }

        [HttpPost]
        public JsonResult ActualizarStockGeneral(string cantidad, int[] stockIds)
        {
            try
            {
                porcentaje = porcentaje.Replace(".", ",");
                decimal Porcentaje = Convert.ToDecimal(porcentaje);
                Porcentaje = (Porcentaje / 100) + 1;

                foreach (int id in stockIds)
                {
                    actualizarDatosStock(Convert.ToDecimal(cantidad), id);

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

        public void actualizarDatosStock(decimal cantidad, int StockDepositoId)
        {
            StockDeposito oStockDeposito = db.StockDeposito.Find(StockDepositoId);
            string descripcion = "Se actualizó el stock del Producto: " + oStockDeposito.ProductoId.ToString() + ". Cantidad original: " + oStockDeposito.Cantidad +
                   ". Agregado: " + cantidad + ". Total final: " + (oStockDeposito.Cantidad + cantidad).ToString();
            oStockDeposito.Cantidad += cantidad;
            //if (txtCantidadDisponible != null)
            //{
            //    oStockDeposito.CantidadUsada = (decimal)txtCantidadDisponible;
            //}
            //else
            //{
            oStockDeposito.CantidadUsada += cantidad;
            //}

            GestorStock.actualizarStockDeposito(oStockDeposito);
            ;
            GestorStock.agregarActualizacionStock(descripcion, Convert.ToInt32(User.Identity.GetUsuarioId()));

        }

        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Producto oProducto = db.Producto.Find(id);
            if (oProducto == null)
            {
                return HttpNotFound();
            }

            ProductoViewModel oProductoViewModel = new ProductoViewModel();

            oProductoViewModel.ProductoId = oProducto.ProductoId;
            oProductoViewModel.TipoProductoId = oProducto.TipoProductoId;
            oProductoViewModel.TipoDuracionId = oProducto.TipoDuracionId;
            oProductoViewModel.MarcaId = oProducto.MarcaId;
            oProductoViewModel.Nombre = oProducto.Nombre;
            oProductoViewModel.PrecioCosto = oProducto.PrecioCosto;
            oProductoViewModel.Minimo = oProducto.Minimo;
            oProductoViewModel.Codigo = oProducto.Codigo;

            List<Deposito> listDepositos = oProducto.StockDeposito
               .Where(x => x.ProductoId == oProducto.ProductoId && x.Activo == true)
               .Select(x => x.Deposito)
               .ToList();
            // oProductoViewModel.DepositosList = listDepositos;

            ViewBag.MarcaId = new SelectList(db.Marca.Where(x => x.Activo == true), "MarcaId", "Nombre", oProductoViewModel.MarcaId);
            ViewBag.TipoDuracionId = new SelectList(db.TipoDuracion.Where(x => x.Activo == true), "TipoDuracionId", "Nombre", oProductoViewModel.TipoDuracionId);
            ViewBag.TipoProductoId = new SelectList(db.TipoProducto.Where(x => x.Activo == true), "TipoProductoId", "Nombre", oProductoViewModel.TipoProductoId);
            ViewBag.Depositos = new MultiSelectList(db.Deposito.Where(x => x.Activo == true), "DepositoId", "Nombre", oProducto.StockDeposito
                .Where(x => x.ProductoId == oProducto.ProductoId && x.Activo == true)
                .Select(x => x.DepositoId).ToArray());
            ViewBag.ProveedorId = new SelectList(db.Proveedor.Where(x => x.Activo == true), "ProveedorId", "Nombre", oProducto.ProveedorId);

            return View(oProductoViewModel);
        }

        // POST: Productos/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ProductoId,Codigo,DepositosList,TipoProductoId,TipoDuracionId,MarcaId,Nombre,PrecioCosto,Minimo,ProveedorId")] ProductoViewModel oProductoViewModel)
        {
            if (ModelState.IsValid)
            {
                Producto oProducto = GestorProducto.getById(oProductoViewModel.ProductoId);
                oProducto.TipoProductoId = oProductoViewModel.TipoProductoId;
                oProducto.TipoDuracionId = oProductoViewModel.TipoDuracionId;
                oProducto.MarcaId = oProductoViewModel.MarcaId;
                oProducto.Nombre = oProductoViewModel.Nombre.ToUpper();
                oProducto.PrecioCosto = oProductoViewModel.PrecioCosto;
                oProducto.Minimo = oProductoViewModel.Minimo;
                oProducto.Codigo = oProductoViewModel.Codigo;
                oProducto.ProveedorId = oProductoViewModel.ProveedorId;
                int[] depositosDefault = new int[1] { 1 };
                oProductoViewModel.DepositosList = depositosDefault;
                if (oProductoViewModel.DepositosList != null)
                {
                    try
                    {
                        GestorEntidadesConexion.inicializarContexto();
                        GestorEntidadesConexion.beginTransaccion();
                        GestorProducto.Actualizar(oProducto);
                        PrecioHelper.verificarPrecioLista(oProducto.ProductoId, 0);
                        if (ProductoHelper.guardarStockPorTipo(oProducto.ProductoId, 0, TipoStockE.Producto, 0, oProductoViewModel.DepositosList, TipoAccionE.Editar))
                        {
                            GestorEntidadesConexion.commitTransaccion();
                            return RedirectToAction("Index");
                        }
                        else
                        {
                            GestorEntidadesConexion.rollbackTransaccion();
                            ModelState.AddModelError("", "No se pudo guardar el stock o existe algún problema.");
                        }
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError("", "Ocurrió un Error al intentar guardar los datos, y no se guardaron.");
                        GestorEntidadesConexion.rollbackTransaccion();
                    }
                    finally
                    {
                        GestorEntidadesConexion.disposeTransaccion();
                    }
                }
                else
                {
                    ModelState.AddModelError("", "No se pudieron guardar los datos, seleccionar depósitos");
                }
            }
            else
            {
                ModelState.AddModelError("", "No se pudieron guardar los datos, revise los campos faltantes");
            }

            ViewBag.MarcaId = new SelectList(db.Marca.Where(x => x.Activo == true), "MarcaId", "Nombre", oProductoViewModel.MarcaId);
            ViewBag.TipoDuracionId = new SelectList(db.TipoDuracion.Where(x => x.Activo == true), "TipoDuracionId", "Nombre", oProductoViewModel.TipoDuracionId);
            ViewBag.TipoProductoId = new SelectList(db.TipoProducto.Where(x => x.Activo == true), "TipoProductoId", "Nombre", oProductoViewModel.TipoProductoId);
            ViewBag.Depositos = new MultiSelectList(db.Deposito.Where(x => x.Activo == true), "DepositoId", "Nombre", oProductoViewModel.DepositosList);
            ViewBag.ProveedorId = new SelectList(db.Proveedor.Where(x => x.Activo == true), "ProveedorId", "Nombre", oProductoViewModel.ProveedorId);

            return View(oProductoViewModel);
        }

        [HttpPost]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Producto producto = db.Producto.Find(id);
            if (producto == null)
            {
                return HttpNotFound();
            }
            try
            {
                producto.Activo = false;
                db.Entry(producto).State = EntityState.Modified;
                var lists = producto.ComboProducto.Where(x => x.Activo == true).ToList();
                foreach (ComboProducto item in lists)
                {
                    item.Activo = false;
                }
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
