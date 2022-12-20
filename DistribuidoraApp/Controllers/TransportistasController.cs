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
    public class TransportistasController : Controller
    {
        private DistribuidoraDBEntities db = new DistribuidoraDBEntities();

        // GET: Transportistas
        public ActionResult Index()
        {
            return View(db.Transportista.Where(x => x.Activo == true).ToList());
        }

        // GET: Transportistas/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Transportista transportista = db.Transportista.Find(id);
            if (transportista == null)
            {
                return HttpNotFound();
            }
            return View(transportista);
        }

        // GET: Transportistas/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Transportistas/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que quiere enlazarse. Para obtener 
        // más detalles, vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Nombre,Apellido")] Transportista transportista)
        {
            if (ModelState.IsValid)
            {
                transportista.Activo = true;
                transportista.Nombre = transportista.Nombre.ToUpper();
                db.Transportista.Add(transportista);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(transportista);
        }

        // GET: Transportistas/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Transportista transportista = db.Transportista.Find(id);
            if (transportista == null)
            {
                return HttpNotFound();
            }
            return View(transportista);
        }

        // POST: Transportistas/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que quiere enlazarse. Para obtener 
        // más detalles, vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "TransportistaId,Nombre,Apellido,FechaUltimoTransporte")] Transportista transportista)
        {
            if (ModelState.IsValid)
            {
                transportista.Activo = true;
                transportista.Nombre = transportista.Nombre.ToUpper();
                db.Entry(transportista).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(transportista);
        }


        [HttpPost]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Transportista transportista = db.Transportista.Find(id);
            if (transportista == null)
            {
                return HttpNotFound();
            }
            try
            {
                transportista.Activo = false;
                db.Entry(transportista).State = EntityState.Modified;

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
