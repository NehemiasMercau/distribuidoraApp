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
    [Authorize(Roles = "Desarrollo")]
    public class IngresosController : Controller
    {
        private DistribuidoraDBEntities db = new DistribuidoraDBEntities();

        // GET: Ingresos
        public ActionResult Index()
        {
            var ingreso = db.Ingreso.Include(i => i.Arqueo);
            return View(ingreso.ToList());
        }

        // GET: Ingresos/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Ingreso ingreso = db.Ingreso.Find(id);
            if (ingreso == null)
            {
                return HttpNotFound();
            }
            return View(ingreso);
        }

        // GET: Ingresos/Create
        public ActionResult Create()
        {
            ViewBag.ArqueoId = new SelectList(db.Arqueo, "ArqueoId", "Observaciones");
            return View();
        }

        // POST: Ingresos/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "IngresoId,Monto,ArqueoId,Motivo,Activo")] Ingreso ingreso)
        {
            if (ModelState.IsValid)
            {
                db.Ingreso.Add(ingreso);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.ArqueoId = new SelectList(db.Arqueo, "ArqueoId", "Observaciones", ingreso.ArqueoId);
            return View(ingreso);
        }

        // GET: Ingresos/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Ingreso ingreso = db.Ingreso.Find(id);
            if (ingreso == null)
            {
                return HttpNotFound();
            }
            ViewBag.ArqueoId = new SelectList(db.Arqueo, "ArqueoId", "Observaciones", ingreso.ArqueoId);
            return View(ingreso);
        }

        // POST: Ingresos/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "IngresoId,Monto,ArqueoId,Motivo,Activo")] Ingreso ingreso)
        {
            if (ModelState.IsValid)
            {
                db.Entry(ingreso).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.ArqueoId = new SelectList(db.Arqueo, "ArqueoId", "Observaciones", ingreso.ArqueoId);
            return View(ingreso);
        }

        // GET: Ingresos/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Ingreso ingreso = db.Ingreso.Find(id);
            if (ingreso == null)
            {
                return HttpNotFound();
            }
            return View(ingreso);
        }

        // POST: Ingresos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Ingreso ingreso = db.Ingreso.Find(id);
            db.Ingreso.Remove(ingreso);
            db.SaveChanges();
            return RedirectToAction("Index");
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
