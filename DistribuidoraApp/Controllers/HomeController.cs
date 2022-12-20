using DistribuidoraAPI.Extensions;
using DistribuidoraAPI.Helpers;
using DistribuidoraAPI.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Negocio.entidades;
using Negocio.enumeradores;
using Negocio.gestores;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web;
using System.Web.Mvc;

namespace DistribuidoraAPI.Controllers
{
    [Authorize]
    [PaymentExpireFilter]
    //[SessionExpire]
    public class HomeController : Controller
    {
        private ApplicationSignInManager _signInManager;
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

        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set
            {
                _signInManager = value;
            }
        }

        public ActionResult Index()
        {
            

            if (User.Identity.IsAuthenticated)
            {
                using (ApplicationDbContext db = new ApplicationDbContext())
                {
                    var idUsuarioActual = User.Identity.GetUserId();
                    var PerfilId = User.Identity.GetPerfilId();
                    var SucursalId = User.Identity.GetSucursalId();
                    string NombrePerfil = "";
                    switch (Convert.ToInt32(PerfilId))
                    {
                        case (int)PerfilE.Administracion:
                            NombrePerfil = PerfilE.Administracion.ToString();
                            break;
                        case (int)PerfilE.Caja:
                            NombrePerfil = PerfilE.Caja.ToString();
                            break;
                        case (int)PerfilE.Preventista:
                            NombrePerfil = PerfilE.Preventista.ToString();
                            break;
                        case (int)PerfilE.Desarrollo:
                            NombrePerfil = PerfilE.Desarrollo.ToString();
                            break;

                    }

                    var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(db));
                    //usuario esta en rol?
                    var usuarioEstaEnRol = userManager.IsInRole(idUsuarioActual, NombrePerfil);
                    if (!usuarioEstaEnRol)
                    {//El usuario no tiene ese rol, y deberia tenerlo.
                        //Agregar usuario a rol
                        UserManager.AddToRole(idUsuarioActual, NombrePerfil);
                    }


                    //roles del usuario
                    var roles = userManager.GetRoles(idUsuarioActual);
                    if (roles.Count > 1)
                    {
                        for (int i = 0; i < roles.Count; i++)
                        {
                            if (roles[i] != NombrePerfil)
                            {
                                userManager.RemoveFromRole(idUsuarioActual, roles[i]);
                            }
                        }

                    }
                    //remover a usuario de rol
                    // resultado = userManager.RemoveFromRole(idUsuarioActual, "ApruebaPrestamos");
                    //Borrar rol
                    // var rolVendedor = roleManager.FindByName("ApruebaPrestamos");
                    // roleManager.Delete(rolVendedor);

                    ApplicationUser oApplicationUser = userManager.FindById(User.Identity.GetUserId());
                    SignInManager.SignIn(oApplicationUser, true, true);



                }


                using (var ctx = new DistribuidoraDBEntities())
                {
                    string usuarioId = User.Identity.GetUserId();
                    AspNetUsers aspNetUsers = ctx.AspNetUsers.Where(x => x.Id == usuarioId).FirstOrDefault();
                    Arqueo oArqueo = ctx.Arqueo.Where(x => x.Activo == true && x.Abierto == true && x.UsuarioInicioId == aspNetUsers.UsuarioId).FirstOrDefault();
                    if (oArqueo != null)
                    {
                        ViewBag.Abierto = true;
                        ViewBag.oArqueo = oArqueo;
                        List<Venta> listVentas = ctx.Venta.Where(x => x.Activo == true && x.ArqueoId == oArqueo.ArqueoId).ToList();
                        if (listVentas != null)
                        {
                            ViewBag.CantVentas = listVentas.Count();
                            decimal dinero = 0;
                            foreach (Venta item in listVentas)
                            {
                                decimal desc = (item.Descuento != null) ? (decimal)item.Descuento : 0;
                                decimal recar = (item.Recargo != null) ? (decimal)item.Recargo / 100 : 0;
                                dinero += (decimal)item.Final;
                            }
                            ViewBag.Dinero = dinero;
                            ViewBag.listCobros = listVentas;
                        }

                    }
                    else ViewBag.Abierto = false;
                }
            }


            return View();
        }


    }
}