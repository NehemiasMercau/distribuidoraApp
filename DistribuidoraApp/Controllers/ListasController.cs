using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Negocio.entidades;
using Newtonsoft.Json;

namespace DistribuidoraAPI.Controllers
{
    [Authorize(Roles = "Desarrollo")]
    [SessionExpire]
    public class ListasController : Controller
    {
        private DistribuidoraDBEntities db = new DistribuidoraDBEntities();

        // GET: Listas
        public ActionResult Index()
        {
            return View(db.Lista.ToList());
        }

        // GET: Listas/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Lista lista = db.Lista.Find(id);
            if (lista == null)
            {
                return HttpNotFound();
            }
            return PartialView(lista);
        }

        [HttpGet]
        public JsonResult GetListas()
        {
            List<Lista> listas = db.Lista.Where(x => x.Activo == true).ToList();
            //var list = listas.Select(x => new { x.ListaId.ToString() = x.Nombre, Valor = x.Nombre }).ToList();
            //string str = "{";
            //for(int i = 0; i<listas.Count; i++)
            //{
            //    str += "'" + listas[i].ListaId.ToString() + "' : '" + listas[i].Nombre + "'";
            //    if(i < listas.Count - 1) { str += ","; }
            //}
            //str += "}";

            var dict = new Dictionary<int, string>();
            for (int i = 0; i < listas.Count; i++)
            {
                dict.Add(listas[i].ListaId, listas[i].Nombre);
               
            }

            var json = JsonConvert.SerializeObject(dict);
            return Json(json, JsonRequestBehavior.AllowGet);
        }

        // GET: Listas/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Listas/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ListaId,Nombre,Descripcion,RecargoGeneral,RecargoImporte")] Lista lista)
        {
            if (ModelState.IsValid)
            {
                lista.Activo = true;
                lista.Nombre = lista.Nombre.ToUpper();
                lista.Fecha = DateTime.Now;
                db.Lista.Add(lista);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(lista);
        }

        // GET: Listas/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Lista lista = db.Lista.Find(id);
            
            if (lista == null)
            {
                return HttpNotFound();
            }
            return View(lista);
        }

        // POST: Listas/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ListaId,Nombre,Descripcion,RecargoGeneral,RecargoImporte")] Lista lista)
        {
            if (ModelState.IsValid)
            {
                lista.Fecha = DateTime.Now;
                lista.Activo = true;
                lista.Nombre = lista.Nombre.ToUpper();
                db.Entry(lista).State = EntityState.Modified;

                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(lista);
        }

        // GET: Listas/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Lista lista = db.Lista.Find(id);
            if (lista == null)
            {
                return HttpNotFound();
            }
            return View(lista);
        }

        // POST: Listas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Lista lista = db.Lista.Find(id);
            lista.Activo = false;
            db.Entry(lista).State = EntityState.Modified;
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
