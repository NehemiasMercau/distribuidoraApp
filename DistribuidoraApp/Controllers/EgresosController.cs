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
    public class EgresosController : Controller
    {
        private DistribuidoraDBEntities db = new DistribuidoraDBEntities();

        // GET: Egresos
        public ActionResult Index()
        {
            var egreso = db.Egreso.Include(e => e.Arqueo);
            return View(egreso.ToList());
        }

        // GET: Egresos/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Egreso egreso = db.Egreso.Find(id);
            if (egreso == null)
            {
                return HttpNotFound();
            }
            return View(egreso);
        }

        // GET: Egresos/Create
        public ActionResult Create()
        {
            ViewBag.ArqueoId = new SelectList(db.Arqueo, "ArqueoId", "Observaciones");
            return View();
        }

        // POST: Egresos/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "EgresoId,Monto,ArqueoId,Motivo,Activo")] Egreso egreso)
        {
            if (ModelState.IsValid)
            {
                db.Egreso.Add(egreso);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.ArqueoId = new SelectList(db.Arqueo, "ArqueoId", "Observaciones", egreso.ArqueoId);
            return View(egreso);
        }

        // GET: Egresos/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Egreso egreso = db.Egreso.Find(id);
            if (egreso == null)
            {
                return HttpNotFound();
            }
            ViewBag.ArqueoId = new SelectList(db.Arqueo, "ArqueoId", "Observaciones", egreso.ArqueoId);
            return View(egreso);
        }

        // POST: Egresos/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "EgresoId,Monto,ArqueoId,Motivo,Activo")] Egreso egreso)
        {
            if (ModelState.IsValid)
            {
                db.Entry(egreso).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.ArqueoId = new SelectList(db.Arqueo, "ArqueoId", "Observaciones", egreso.ArqueoId);
            return View(egreso);
        }

        // GET: Egresos/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Egreso egreso = db.Egreso.Find(id);
            if (egreso == null)
            {
                return HttpNotFound();
            }
            return View(egreso);
        }

        // POST: Egresos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Egreso egreso = db.Egreso.Find(id);
            db.Egreso.Remove(egreso);
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
