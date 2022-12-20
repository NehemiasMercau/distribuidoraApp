using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using DistribuidoraAPI.Helpers;
using DistribuidoraAPI.Models;
using Negocio.entidades;
using Negocio.enumeradores;
using Negocio.gestores;
using Newtonsoft.Json;

namespace DistribuidoraAPI.Controllers
{
    [Authorize(Roles = "Administracion, Desarrollo")]
    [SessionExpire]
    public class CombosController : Controller
    {
        private DistribuidoraDBEntities db = new DistribuidoraDBEntities();

        // GET: Combos
        public ActionResult Index()
        {
            List<Combo> listCombos = db.Combo.Where(p => p.Activo == true).OrderByDescending(x => x.ComboId).ToList();

            return View(listCombos);
        }

        // GET: Combos/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Combo combo = db.Combo.Find(id);
            if (combo == null)
            {
                return HttpNotFound();
            }
            return PartialView("_Details", combo);
        }

        // GET: Combos/Create
        public ActionResult Create()
        {
            ComboViewModel combo = new ComboViewModel();
            combo.MultiProductos = new List<Producto>();
            ViewBag.Multi = new MultiSelectList(new List<Producto>(), "ProductoId", "Nombre");
            // ViewBag.Depositos = new MultiSelectList(db.Deposito.Where(x => x.Activo == true), "DepositoId", "Nombre");
            ViewBag.Productos = new SelectList(db.Producto.Include(p => p.TipoProducto).Where(x => x.Activo == true), "ProductoId", "Nombre", "TipoProducto.Nombre");
            return View(combo);
        }

        // POST: Combos/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Nombre,Descripcion,PrecioCosto,Codigo")] ComboViewModel oComboViewModel, int[] Multi)
        {
            if (ModelState.IsValid)
            {
                if (Multi != null)
                {
                    oComboViewModel.Minimo = 99999999;
                    int[] depositos = new int[1] { 1 };
                    oComboViewModel.DepositosList = depositos;
                    oComboViewModel.Nombre = oComboViewModel.Nombre.ToUpper();
                    Combo oCombo = ComboHelper.guardarCombo(oComboViewModel, Multi);
                    PrecioHelper.verificarPrecioLista(0, oCombo.ComboId);
                    return RedirectToAction("Index");
                    //return RedirectToAction("AltaStock", "Productos", new { id = oCombo.ComboId, tipoStock = 2 });

                    //if (ProductoHelper.guardarStockPorTipo(0, oCombo.ComboId, TipoStockE.Combo, 0, oComboViewModel.DepositosList, TipoAccionE.Guardar))
                    //{

                    //}
                    //else
                    //{
                    //    ModelState.AddModelError("", "Ocurrió un Error al intentar guardar los datos, y no se guardaron.");
                    //}


                }
                else
                {
                    ModelState.AddModelError("", "Debe agregar Productos que compongan el Combo");
                }
            }

            if (Multi == null) ModelState.AddModelError("", "Debe agregar Productos que compongan el Combo");

            List<Producto> listProductos = new List<Producto>();
            if (Multi != null)
            {
                foreach (int i in Multi)
                {
                    listProductos.Add(GestorProducto.getById(i));
                }
            }
            oComboViewModel.MultiProductos = listProductos.ToList();
            ViewBag.Multi = new MultiSelectList(listProductos.ToList(), "ProductoId", "Nombre");
            ViewBag.Productos = new SelectList(db.Producto.Include(p => p.TipoProducto).Where(x => x.Activo == true), "ProductoId", "Nombre");
            //if (oComboViewModel.DepositosList != null)
            //{
            //    ViewBag.Depositos = new MultiSelectList(db.Deposito.Where(x => x.Activo == true), "DepositoId", "Nombre", db.Deposito
            //        .Where(x => oComboViewModel.DepositosList.Contains(x.DepositoId)).Select(x => x.DepositoId).ToArray());
            //}
            //else
            //{
            //    ViewBag.Depositos = new MultiSelectList(db.Deposito.Where(x => x.Activo == true), "DepositoId", "Nombre", oComboViewModel.DepositosList);
            //}
            return View(oComboViewModel);
        }

        public ActionResult GetPrecios(int?[] valores)
        {
            PrecioCustom precioCustom = new PrecioCustom();
            decimal Costo, VentaMayorista, VentaMinorista;
            Costo = 0; VentaMayorista = 0; VentaMinorista = 0;

            foreach (int ProductoId in valores)
            {
                Producto oProducto = GestorProducto.getById(ProductoId);
                Costo += (decimal)oProducto.PrecioCosto;
            }

            precioCustom.Costo = Costo.ToString();
            precioCustom.VentaMayorista = VentaMayorista.ToString();
            precioCustom.VentaMinorista = VentaMinorista.ToString();

            return Json(precioCustom);
        }

        // GET: Combos/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Combo oCombo = db.Combo.Find(id);
            if (oCombo == null)
            {
                return HttpNotFound();
            }
            ComboViewModel oComboViewModel = new ComboViewModel();
            oComboViewModel.ComboId = oCombo.ComboId;
            oComboViewModel.Nombre = oCombo.Nombre;
            oComboViewModel.Descripcion = oCombo.Descripcion;
            oComboViewModel.PrecioCosto = oCombo.PrecioCosto;
            oComboViewModel.Minimo = oCombo.Minimo;
            oComboViewModel.Codigo = oCombo.Codigo;

            List<Producto> listProductos = oCombo.ComboProducto.Where(x => x.Activo == true).Select(x => x.Producto).ToList();
            oComboViewModel.MultiProductos = listProductos;
            ViewBag.Multi = new MultiSelectList(listProductos, "ProductoId", "Nombre");
            ViewBag.Productos = new SelectList(db.Producto.Include(p => p.TipoProducto).Where(x => x.Activo == true), "ProductoId", "Nombre", "TipoProducto.Nombre");
            //ViewBag.Depositos = new MultiSelectList(db.Deposito.Where(x => x.Activo == true), "DepositoId", "Nombre", oCombo.StockDeposito
            //    .Where(x => x.ProductoId == oCombo.ComboId && x.Activo == true)
            //    .Select(x => x.DepositoId).ToArray());
            return View(oComboViewModel);
        }

        // POST: Combos/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ComboId,Nombre,Descripcion,PrecioCosto,Codigo")] ComboViewModel oComboViewModel, int[] Multi)
        {
            if (ModelState.IsValid)
            {
                if (Multi != null)
                {
                    int[] depositos = new int[1] { 1 };
                    oComboViewModel.DepositosList = depositos;
                    oComboViewModel.Nombre = oComboViewModel.Nombre.ToUpper();
                    if (ComboHelper.editarCombo(oComboViewModel, Multi))
                    {
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        ModelState.AddModelError("", "Ocurrió un Error al intentar guardar los datos, y no se guardaron.");
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Debe agregar Productos que compongan el Combo");
                }
            }
            else
            {
                ModelState.AddModelError("", "Ocurrió un Error al intentar guardar los datos, y no se guardaron.");
            }
            if (Multi == null) ModelState.AddModelError("", "Debe agregar Productos que compongan el Combo");

            List<Producto> listProductos = new List<Producto>();
            if (Multi != null)
            {
                foreach (int i in Multi)
                {
                    listProductos.Add(GestorProducto.getById(i));
                }
            }
            oComboViewModel.MultiProductos = listProductos.ToList();
            ViewBag.Multi = new MultiSelectList(listProductos.ToList(), "ProductoId", "Nombre");
            ViewBag.Productos = new SelectList(db.Producto.Include(p => p.TipoProducto).Where(x => x.Activo == true), "ProductoId", "Nombre");
            // ViewBag.Depositos = new MultiSelectList(db.Deposito.Where(x => x.Activo == true), "DepositoId", "Nombre", oComboViewModel.DepositosList);
            return View(oComboViewModel);
        }

        [HttpPost]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Combo combo = db.Combo.Find(id);
            if (combo == null)
            {
                return HttpNotFound();
            }
            try
            {
                combo.Activo = false;
                db.Entry(combo).State = EntityState.Modified;
                var lists = combo.ComboProducto.Where(x => x.Activo == true).ToList();
                foreach(ComboProducto item in lists)
                {
                    item.Activo = false;
                }
                var stocks = db.StockDeposito.Where(x => x.Activo == true && x.ComboId == combo.ComboId).ToList();
                foreach(StockDeposito stockDeposito in stocks)
                {
                    stockDeposito.Activo = false;
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
