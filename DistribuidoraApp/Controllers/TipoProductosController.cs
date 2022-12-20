using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Negocio.entidades;

namespace DistribuidoraAPI.Controllers
{
    [Authorize(Roles = "Administracion, Desarrollo")]
    [SessionExpire]
    public class TipoProductosController : Controller
    {
        private DistribuidoraDBEntities db = new DistribuidoraDBEntities();

        // GET: TipoProductos
        public ActionResult Index()
        {
            return View(db.TipoProducto.Where(x => x.Activo == true).ToList());
        }

        // GET: TipoProductos/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: TipoProductos/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que quiere enlazarse. Para obtener 
        // más detalles, vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Nombre")] TipoProducto tipoProducto)
        {
            if (ModelState.IsValid)
            {
                tipoProducto.Activo = true;
                tipoProducto.Nombre = tipoProducto.Nombre.ToUpper();
                db.TipoProducto.Add(tipoProducto);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(tipoProducto);
        }

        // GET: TipoProductos/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TipoProducto tipoProducto = db.TipoProducto.Find(id);
            if (tipoProducto == null)
            {
                return HttpNotFound();
            }
            return View(tipoProducto);
        }

        // POST: TipoProductos/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que quiere enlazarse. Para obtener 
        // más detalles, vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "TipoProductoId,Nombre")] TipoProducto tipoProducto)
        {
            if (ModelState.IsValid)
            {
                tipoProducto.Activo = true;
                tipoProducto.Nombre = tipoProducto.Nombre.ToUpper();
                db.Entry(tipoProducto).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(tipoProducto);
        }


        [HttpPost]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TipoProducto tipoProducto = db.TipoProducto.Find(id);
            if (tipoProducto == null)
            {
                return HttpNotFound();
            }
            try
            {
                tipoProducto.Activo = false;
                db.Entry(tipoProducto).State = EntityState.Modified;
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
