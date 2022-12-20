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
    [Authorize(Roles = "Administracion, Desarrollo")]
    [SessionExpire]
    public class ClientesController : Controller
    {
        private DistribuidoraDBEntities db = new DistribuidoraDBEntities();

        // GET: Clientes
        public ActionResult Index()
        {
            List<Cliente> listCliente = db.Cliente.Include(c => c.CondicionIVA).Include(c => c.PersonaTipo).Where(x => x.Activo == true).ToList();
            return View(listCliente);
        }

        // GET: Clientes/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Cliente cliente = db.Cliente.Find(id);
            if (cliente == null)
            {
                return HttpNotFound();
            }
            return PartialView("_Details", cliente);
        }

        [Authorize(Roles = "Caja, Preventista, Administracion, Desarrollo")]
        // GET: Clientes/Create
        public ActionResult Create(int? esTemporal)
        {
            ViewBag.CondicionIVAId = new SelectList(db.CondicionIVA, "CondicionIVAId", "Nombre");
            ViewBag.PersonaTipoId = new SelectList(db.PersonaTipo, "PersonaTipoId", "Nombre");
            
            if (esTemporal == null)
            {
                ViewBag.esTemporal = 0;
                return View();
            }
            else
            {
                ViewBag.esTemporal = 1;
                return PartialView("_Create");
            }

        }

        [Authorize(Roles = "Caja, Preventista, Administracion, Desarrollo")]
        // POST: Clientes/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "RazonSocial,Email,CUIT,Direccion,CondicionIVAId,Telefono,PersonaTipoId")] Cliente cliente, int? esTemporal)
        {
            if(esTemporal == null)
            {
                //return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                esTemporal = 0;
                ViewBag.esTemporal = 0;
            }
            bool temporal = (esTemporal == 1) ? true : false;
            if (ModelState.IsValid)
            {
                cliente.FechaAlta = DateTime.Now;
                cliente.Activo = true;
                cliente.RazonSocial = cliente.RazonSocial.ToUpper();
                db.Cliente.Add(cliente);
                db.SaveChanges();
                if (temporal)
                {
                    ViewBag.esTemporal = 1;
                    System.Text.StringBuilder sbb = new System.Text.StringBuilder();
                    sbb.Append("<h4>Cliente Registrado!</h4>");

                    return Content(sbb.ToString());
                }
                else
                {
                    return RedirectToAction("Index");
                }
            }


            if (temporal)
            {
                ViewBag.esTemporal = 1;
                System.Text.StringBuilder sbb = new System.Text.StringBuilder();
                sbb.Append("<h4>Cliente No Registrado!</h4>");

                return Content(sbb.ToString());
            }
            else
            {
                ViewBag.esTemporal = 0;
                ViewBag.CondicionIVAId = new SelectList(db.CondicionIVA, "CondicionIVAId", "Nombre", cliente.CondicionIVAId);
                ViewBag.PersonaTipoId = new SelectList(db.PersonaTipo, "PersonaTipoId", "Nombre", cliente.PersonaTipoId);
                
                return View(cliente);
            }

           
        }

        [Authorize(Roles = "Caja, Preventista, Administracion, Desarrollo")]
        public JsonResult GetClientes()
        {
            List<SelectListItem> listItems = new List<SelectListItem>();
            List<Cliente> listClientes = db.Cliente.Where(x => x.Activo == true).ToList();
            listClientes.ForEach(x =>
            {
                listItems.Add(new SelectListItem { Text = x.RazonSocial, Value = x.ClienteId.ToString() });
            });

            return Json(listItems, JsonRequestBehavior.AllowGet);
        }

        [Authorize(Roles = "Caja, Preventista, Administracion, Desarrollo")]
        public JsonResult GetClienteUnico(int? id)
        {
            if(id == null)
            {
                id=0;
            }
            Cliente oCliente = db.Cliente.Find((int)id);
            if(oCliente != null)
            {
                var result = new { Success = "True", Direccion = oCliente.Direccion };
                return Json(result);
            } else
            {
                var result = new { Success = "False", Direccion = "" };
                return Json(result);
            }
           
           
        }

        [Authorize(Roles = "Caja, Preventista, Administracion, Desarrollo")]
        [HttpGet]
        public JsonResult GetClientesCombo()
        {
            List<Cliente> clientes = db.Cliente.Where(x => x.Activo == true).ToList();

            var dict = new Dictionary<int, string>();
            for (int i = 0; i < clientes.Count; i++)
            {
                dict.Add(clientes[i].ClienteId, clientes[i].RazonSocial);
            }

            var json = JsonConvert.SerializeObject(dict);
            return Json(json, JsonRequestBehavior.AllowGet);
        }

        // GET: Clientes/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Cliente cliente = db.Cliente.Find(id);
            if (cliente == null)
            {
                return HttpNotFound();
            }
            ViewBag.CondicionIVAId = new SelectList(db.CondicionIVA, "CondicionIVAId", "Nombre", cliente.CondicionIVAId);
            ViewBag.PersonaTipoId = new SelectList(db.PersonaTipo, "PersonaTipoId", "Nombre", cliente.PersonaTipoId);
            return View(cliente);
        }

        // POST: Clientes/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ClienteId,RazonSocial,Email,CUIT,Direccion,CondicionIVAId,Telefono,PersonaTipoId,FechaAlta")] Cliente cliente)
        {
            if (ModelState.IsValid)
            {
                cliente.Activo = true;
                cliente.RazonSocial = cliente.RazonSocial.ToUpper();
                db.Entry(cliente).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.CondicionIVAId = new SelectList(db.CondicionIVA, "CondicionIVAId", "Nombre", cliente.CondicionIVAId);
            ViewBag.PersonaTipoId = new SelectList(db.PersonaTipo, "PersonaTipoId", "Nombre", cliente.PersonaTipoId);
            return View(cliente);
        }

        [HttpPost]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Cliente cliente = db.Cliente.Find(id);
            if (cliente == null)
            {
                return HttpNotFound();
            }
            try
            {
                cliente.Activo = false;
                db.Entry(cliente).State = EntityState.Modified;
               
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
