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
    public class DepositosController : Controller
    {
        private DistribuidoraDBEntities db = new DistribuidoraDBEntities();

        // GET: Depositos
        public ActionResult Index()
        {
            var deposito = db.Deposito.Include(d => d.Sucursal);
            return View(deposito.ToList());
        }

        // GET: Depositos/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Deposito deposito = db.Deposito.Find(id);
            if (deposito == null)
            {
                return HttpNotFound();
            }
            return View(deposito);
        }

        // GET: Depositos/Create
        public ActionResult Create()
        {
            ViewBag.SucursalId = new SelectList(db.Sucursal, "SucursalId", "Nombre");
            return View();
        }

        // POST: Depositos/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "DepositoId,Nombre,SucursalId,Direccion,Activo")] Deposito deposito)
        {
            if (ModelState.IsValid)
            {
                db.Deposito.Add(deposito);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.SucursalId = new SelectList(db.Sucursal, "SucursalId", "Nombre", deposito.SucursalId);
            return View(deposito);
        }

        // GET: Depositos/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Deposito deposito = db.Deposito.Find(id);
            if (deposito == null)
            {
                return HttpNotFound();
            }
            ViewBag.SucursalId = new SelectList(db.Sucursal, "SucursalId", "Nombre", deposito.SucursalId);
            return View(deposito);
        }

        // POST: Depositos/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "DepositoId,Nombre,SucursalId,Direccion,Activo")] Deposito deposito)
        {
            if (ModelState.IsValid)
            {
                db.Entry(deposito).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.SucursalId = new SelectList(db.Sucursal, "SucursalId", "Nombre", deposito.SucursalId);
            return View(deposito);
        }

        // GET: Depositos/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Deposito deposito = db.Deposito.Find(id);
            if (deposito == null)
            {
                return HttpNotFound();
            }
            return View(deposito);
        }

        // POST: Depositos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Deposito deposito = db.Deposito.Find(id);
            db.Deposito.Remove(deposito);
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
