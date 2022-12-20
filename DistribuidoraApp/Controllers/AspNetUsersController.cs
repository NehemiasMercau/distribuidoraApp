using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using DistribuidoraAPI.Extensions;
using DistribuidoraAPI.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Negocio.entidades;
using Negocio.enumeradores;
using Negocio.gestores;

namespace DistribuidoraAPI.Controllers
{
    [Authorize(Roles = "Administracion, Desarrollo")]
    [SessionExpire]
    public class AspNetUsersController : Controller
    {
        private DistribuidoraDBEntities db = new DistribuidoraDBEntities();
        private ApplicationUserManager _userManager;
        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }
        // GET: AspNetUsers
        public ActionResult Index(string mensaje, string tipoMensaje)
        {
            var aspNetUsers = db.AspNetUsers.Include(a => a.Perfil).Include(a => a.UsuarioReferencia);
            if(User.Identity.GetPerfilId() != "4")
            {
                aspNetUsers = aspNetUsers.Where(x => x.PerfilId != 4);
            }
            ViewBag.mensaje = mensaje;
            ViewBag.tipoMensaje = tipoMensaje;
            return View(aspNetUsers.ToList());
        }

        // GET: AspNetUsers/Details/5
        public ActionResult Details(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AspNetUsers aspNetUsers = db.AspNetUsers.Find(id);
            if (aspNetUsers == null)
            {
                return HttpNotFound();
            }
            return View(aspNetUsers);
        }

        // GET: AspNetUsers/Create
        public ActionResult Create()
        {
            ViewBag.PerfilId = new SelectList(db.Perfil, "PerfilId", "Nombre");
            ViewBag.SucursalId = new SelectList(db.Sucursal.Where(d => d.Activo == true), "SucursalId", "Nombre");
            return View();
        }

        // POST: AspNetUsers/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que quiere enlazarse. Para obtener 
        // más detalles, vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "Email,SucursalId,Nombre,Apellido,PerfilId,PhoneNumber,Dni,Password,ConfirmPassword")] UserViewModel model)
        {
            if (ModelState.IsValid)
            {
                model.Nombre = model.Nombre.ToUpper();
                model.Apellido = model.Apellido.ToUpper();
                var user = new ApplicationUser { UserName = model.Email, Email = model.Email, PhoneNumber = model.PhoneNumber, Dni = model.Dni, Nombre = model.Nombre, Apellido = model.Apellido, PerfilId = model.PerfilId, SucursalId = model.SucursalId };
                //Consultar si ese cuit/cuil ya existe en la tabla pesonas. Si no existe, guardarlo.
                AspNetUsers oAspNetUsers = GestorUsuario.getUserByDni(model.Dni);
                if (oAspNetUsers != null) //El DNI existe, no se puede registrar
                {
                    ModelState.AddModelError("", "El DNI ingresado ya se encuentra registrado.");
                }
                else //No existe el DNI, dar de alta el Usuario.
                {
                    UsuarioReferencia oUsuarioReferencia = GestorUsuario.insertarUsuarioRef(user.Id);
                    user.UsuarioId = oUsuarioReferencia.UsuarioRefId;
                    user.EmailConfirmed = true;
                    var result = await UserManager.CreateAsync(user, model.Password);
                    if (result.Succeeded)//Ya se guardo el usuario
                    {
                        // Enviar correo electrónico con este vínculo
                       // string code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
                        //var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
                       // await UserManager.SendEmailAsync(user.Id, "Confirmar cuenta", "Para confirmar la cuenta, haga clic <a href=\"" + callbackUrl + "\">aquí</a>");

                        return RedirectToAction("Index", new { mensaje = "Usuario insertado correctamente", tipoMensaje= "success" });
                        //return RedirectToAction("EmailSent", "Account");
                    }
                    AddErrors(result);
                }
                //return RedirectToAction("Index");
            }

            ViewBag.PerfilId = new SelectList(db.Perfil.Where(d => d.Activo == true), "PerfilId", "Nombre");
            ViewBag.SucursalId = new SelectList(db.Sucursal.Where(d => d.Activo == true), "SucursalId", "Nombre");
            return View("Create", model);

        }

        // GET: AspNetUsers/Edit/5
        public ActionResult Edit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AspNetUsers aspNetUsers = db.AspNetUsers.Find(id);
            if (aspNetUsers == null)
            {
                return HttpNotFound();
            }
            UserViewModel userViewModel = new UserViewModel();
            userViewModel.Apellido = aspNetUsers.Apellido;
            userViewModel.Nombre = aspNetUsers.Nombre;
            userViewModel.PerfilId = (int)aspNetUsers.PerfilId;
            userViewModel.SucursalId = (aspNetUsers.SucursalId != null) ? (int)aspNetUsers.SucursalId : 0;
            userViewModel.PhoneNumber = aspNetUsers.PhoneNumber;
            userViewModel.Email = aspNetUsers.Email;
            userViewModel.Dni = aspNetUsers.Dni;

            ViewBag.PerfilId = new SelectList(db.Perfil, "PerfilId", "Nombre", aspNetUsers.PerfilId);
            ViewBag.SucursalId = new SelectList(db.Sucursal.Where(d => d.Activo == true), "SucursalId", "Nombre", aspNetUsers.SucursalId);
            return View(userViewModel);
        }

        // POST: AspNetUsers/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que quiere enlazarse. Para obtener 
        // más detalles, vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Email,SucursalId,Nombre,Apellido,PerfilId,PhoneNumber,Dni")] UserEditViewModel userViewModel)
        {
            if (ModelState.IsValid)
            {
                AspNetUsers aspNetUsers = db.AspNetUsers.Where(x => x.Email == userViewModel.Email).FirstOrDefault();
              //  AspNetUsers aspNetUsers = new AspNetUsers();

                aspNetUsers.Apellido = userViewModel.Apellido.ToUpper();
                aspNetUsers.Nombre = userViewModel.Nombre.ToUpper();
                aspNetUsers.PerfilId = userViewModel.PerfilId;
                aspNetUsers.SucursalId = userViewModel.SucursalId;
                aspNetUsers.PhoneNumber = userViewModel.PhoneNumber;
                aspNetUsers.Email = userViewModel.Email;
                aspNetUsers.Dni = userViewModel.Dni;
                
                db.Entry(aspNetUsers).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index", new
                {
                    mensaje = "Usuario registrado con éxito",
                    tipoMensaje = "success"
                });
            }
            ViewBag.PerfilId = new SelectList(db.Perfil, "PerfilId", "Nombre", userViewModel.PerfilId);
            ViewBag.SucursalId = new SelectList(db.Sucursal.Where(d => d.Activo == true), "SucursalId", "Nombre", userViewModel.SucursalId);
            return View(userViewModel);
        }

        [HttpPost]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AspNetUsers aspNetUsers = db.AspNetUsers.Where( x=> x.UsuarioId == id).FirstOrDefault();
            if (aspNetUsers == null)
            {
                return HttpNotFound();
            }
            try
            {

                aspNetUsers.EmailConfirmed = false;
                db.Entry(aspNetUsers).State = EntityState.Modified;

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
