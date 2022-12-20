using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using DistribuidoraAPI.Extensions;
using DistribuidoraAPI.Helpers;
using DistribuidoraAPI.Models;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.AspNet.Identity;
using Microsoft.Reporting.WebForms;
using Negocio.entidades;
using Negocio.enumeradores;
using Newtonsoft.Json;
using DistribuidoraAPI.Util;
using Negocio.gestores;

namespace DistribuidoraAPI.Controllers
{
    [Authorize]
    [SessionExpire]
    public class VentasController : Controller
    {
        private DistribuidoraDBEntities db = new DistribuidoraDBEntities();

        // GET: Ventas
        public ActionResult Index(string mensaje, string tipoMensaje, string facturaAPI, string mensajeFactura, bool? conFactura)
        {

            ViewBag.cantidadRegistros = 10;
            ViewBag.mensaje = mensaje;
            ViewBag.tipoMensaje = tipoMensaje;

            ViewBag.conFactura = (conFactura == true) ? "true" : "false";
            ViewBag.facturaAPI = facturaAPI;
            ViewBag.mensajeFactura = mensajeFactura;

            ViewBag.ClienteId = new SelectList(db.Cliente.Where(x => x.Activo == true), "ClienteId", "RazonSocial");

            var preventistas = db.AspNetUsers.Where(x => x.PerfilId == (int)PerfilE.Preventista).Select(x => new
            {
                Text = x.Nombre + ", " + x.Apellido,
                Value = x.UsuarioId
            }).ToList();

            ViewBag.PreventistaId = new SelectList(preventistas, "Value", "Text");
            ViewBag.CajeroId = new SelectList(db.AspNetUsers.Where(x => x.PerfilId == (int)PerfilE.Caja), "UsuarioId", "Apellido");
            ViewBag.EstadoId = new SelectList(db.Estado.Where(x => x.Activo == true), "EstadoId", "Nombre");
            List<Venta> listVentas;
            DateTime FechaDesde = DateTime.Now.AddDays(-1); TimeSpan ts = new TimeSpan(0, 0, 0);
            DateTime FechaHasta = DateTime.Now; TimeSpan ts1 = new TimeSpan(23, 59, 59);

            int PerfilId = Convert.ToInt32(User.Identity.GetPerfilId());
            int UsuarioId = Convert.ToInt32(User.Identity.GetUsuarioId());
            //int TipoVentaId = (PerfilId == (int)PerfilE.Preventista) ? (int)TipoVentaE.Preventista : (int)TipoVentaE.Caja;
            int TipoVentaId = (int)TipoVentaE.Administracion;
            if (PerfilId == (int)PerfilE.Preventista)
            {
                TipoVentaId = (int)TipoVentaE.Preventista;
            }
            else if (PerfilId == (int)PerfilE.Caja)
            {
                TipoVentaId = (int)TipoVentaE.Caja;
            }
            if (PerfilId == (int)PerfilE.Administracion || PerfilId == (int)PerfilE.Desarrollo)
            {
                listVentas = db.Venta
                             .Include(v => v.Cliente)
                             .Include(v => v.Sucursal)
                             .Include(v => v.TipoCobro)
                             .Include(v => v.TipoVenta)
                             .Where(x => x.Fecha >= FechaDesde && x.Fecha <= FechaHasta && x.Activo == true)
                             .OrderByDescending(x => x.VentaId)
                             .ToList();
            }
            else
            {
                listVentas = db.Venta
                             .Include(v => v.Cliente)
                             .Include(v => v.Sucursal)
                             .Include(v => v.TipoCobro)
                             .Include(v => v.TipoVenta)
                             .Where(x => x.UsuarioId == UsuarioId && x.Fecha >= FechaDesde && x.Fecha <= FechaHasta && x.Activo == true && x.TipoVentaId == TipoVentaId)
                             .OrderByDescending(x => x.VentaId)
                             .ToList();
            }
            return View("Index", listVentas);
        }

        [HttpPost]
        public ActionResult IndexBusqueda(int? ClienteId, int? PreventistaId, string fechas, int? EstadoId, int? CajeroId)
        {
            List<Venta> listVentas = new List<Venta>();

            string[] arrayFechas = fechas.Split('-');
            string desde = arrayFechas[0].Trim();
            string hasta = arrayFechas[1].Trim();
            DateTime FechaDesde = Convert.ToDateTime(desde);
            TimeSpan ts = new TimeSpan(0, 0, 0);
            //FechaDesde = FechaDesde.Date + ts;
            DateTime FechaHasta = Convert.ToDateTime(hasta);
            TimeSpan ts1 = new TimeSpan(23, 59, 59);
            FechaHasta = FechaHasta.Date + ts1;
            int PerfilId = Convert.ToInt32(User.Identity.GetPerfilId());
            int UsuarioId = Convert.ToInt32(User.Identity.GetUsuarioId());

            int TipoVentaId = (int)TipoVentaE.Administracion;
            if (PerfilId == (int)PerfilE.Preventista)
            {
                TipoVentaId = (int)TipoVentaE.Preventista;
            }
            else if (PerfilId == (int)PerfilE.Caja)
            {
                TipoVentaId = (int)TipoVentaE.Caja;
            }

            if (PerfilId == (int)PerfilE.Administracion || PerfilId == (int)PerfilE.Desarrollo)
            {
                listVentas = db.Venta.Include(v => v.Cliente)
                             .Include(v => v.Sucursal)
                             .Include(v => v.TipoCobro)
                             .Include(v => v.TipoVenta)
                             .Where(x => x.Activo == true)
                             .OrderByDescending(x => x.VentaId)
                             .Where(x => x.Fecha >= FechaDesde && x.Fecha <= FechaHasta).ToList();
            }
            else
            {
                listVentas = db.Venta
                             .Include(v => v.Cliente)
                             .Include(v => v.Sucursal)
                             .Include(v => v.TipoCobro)
                             .Include(v => v.TipoVenta)
                             .Where(x => x.UsuarioId == UsuarioId && x.Fecha >= FechaDesde && x.Fecha <= FechaHasta && x.Activo == true && x.TipoVentaId == TipoVentaId)
                             .OrderByDescending(x => x.VentaId)
                             .ToList();
            }


            if (PreventistaId != null)
            {
                listVentas = listVentas.Where(x => x.UsuarioId == PreventistaId).ToList();
            }
            if (ClienteId != null)
            {
                listVentas = listVentas.Where(x => x.ClienteId == ClienteId).ToList();
            }
            if (EstadoId != null)
            {
                listVentas = listVentas.Where(x => x.EstadoId == EstadoId).ToList();
            }
            if (CajeroId != null)
            {
                listVentas = listVentas.Where(x => x.UsuarioId == CajeroId).ToList();
            }

            if (PreventistaId != null)
            {
                decimal total = 0;
                listVentas.ForEach(x =>
                {
                    total += (decimal)x.Final;
                });
                ViewBag.TotalPreventista = total;
            }


            return PartialView("_Index", listVentas);
        }

        // GET: Ventas/Create
        public ActionResult Create(int? ArqueoId)
        {
            if (ArqueoId == null)
            {
                string usuarioId = User.Identity.GetUserId();
                AspNetUsers aspNetUsers = db.AspNetUsers.Where(x => x.Id == usuarioId).FirstOrDefault();
                Arqueo oArqueo = db.Arqueo.Where(x => x.Activo == true && x.Abierto == true && x.UsuarioInicioId == aspNetUsers.UsuarioId).FirstOrDefault();
                if (oArqueo == null)
                {
                    ArqueoId = 0;
                    //return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                else
                {
                    ArqueoId = oArqueo.ArqueoId;
                }
            }
            ViewBag.ArqueoId = (int)ArqueoId;

            ViewBag.ClienteId = new SelectList(db.Cliente.Where(x => x.Activo == true), "ClienteId", "RazonSocial");
            ViewBag.SucursalId = new SelectList(db.Sucursal.Where(x => x.Activo == true), "SucursalId", "Nombre");
            ViewBag.TipoCobroId = new SelectList(db.TipoCobro.Where(x => x.Activo == true), "TipoCobroId", "Nombre", 1);
            ViewBag.TipoVentaId = new SelectList(db.TipoVenta.Where(x => x.Activo == true), "TipoVentaId", "Nombre");
            ViewBag.ListaId = new SelectList(db.Lista.Where(x => x.Activo == true), "ListaId", "Nombre");
            ViewBag.EstadoId = new SelectList(db.Estado.Where(x => x.Activo == true && x.EstadoId != (int)EstadoE.Anulada), "EstadoId", "Descripcion");
            var preventistas = db.AspNetUsers.Where(x => x.PerfilId == (int)PerfilE.Preventista).Select(x => new
            {
                Text = x.Nombre + ", " + x.Apellido,
                Value = x.UsuarioId
            }).ToList();
            ViewBag.PreventistaId = new SelectList(preventistas, "Value", "Text");
            return View(new VentaViewModel());
        }

        // POST: Ventas/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "EstadoId,TipoVentaId,SucursalId,ClienteId,TipoCobroId,Total,Descuento,Final,Observaciones,DireccionEnvio,CostoEnvio,Pendiente,ArqueoId,PreventistaId")] VentaViewModel oVentaViewModel, IEnumerable<ProductoListViewModel> productoListViewModel, IEnumerable<PromosViewModel> promosViewModels, bool ConFactura = false)
        {
            productoListViewModel = productoListViewModel.Where(x => x.ProductoId != 0).ToList();
            if (ModelState.IsValid)
            {
                if (oVentaViewModel.TipoCobroId == (int)TipoCobroE.TARJETA_DEBITO ||
                    oVentaViewModel.TipoCobroId == (int)TipoCobroE.TARJETA_CREDITO)
                {
                    ConFactura = true;
                }

                if (
                ((ConFactura) && oVentaViewModel.ClienteId == null) ||
                ((oVentaViewModel.TipoCobroId == (int)TipoCobroE.TARJETA_DEBITO ||
                oVentaViewModel.TipoCobroId == (int)TipoCobroE.TARJETA_CREDITO) &&
                oVentaViewModel.ClienteId == null)
                )
                {
                    ModelState.AddModelError("ClienteId", "Para generar factura, sí o sí se necesita cargar el cliente");
                }
                else
                {
                    string UsuarioId = User.Identity.GetUsuarioId();
                    oVentaViewModel.UsuarioId = Convert.ToInt32(UsuarioId);

                    decimal DescuentoPromo = 0;
                    string observacionesPromo = "";
                    foreach (PromosViewModel item in promosViewModels)
                    {
                        if (item.ComboId != 0)
                        {
                            if (observacionesPromo != "") observacionesPromo += " ; ";
                            observacionesPromo += item.Descripcion;
                            DescuentoPromo += Convert.ToDecimal(item.MontoDescontado);
                        }
                    }

                    if (observacionesPromo != "")
                    {
                        if (oVentaViewModel.Observaciones == null || oVentaViewModel.Observaciones == "")
                        {
                            oVentaViewModel.Observaciones += "Promociones: " + observacionesPromo;
                        }
                        else oVentaViewModel.Observaciones += " - Promociones: " + observacionesPromo;
                    }
                    if (oVentaViewModel.Descuento == null) oVentaViewModel.Descuento = 0;
                    oVentaViewModel.Descuento += DescuentoPromo;
                    //oVentaViewModel.Final -= DescuentoPromo;
                    oVentaViewModel.Promos = DescuentoPromo;

                    Venta oVentaRegistrada = VentaHelper.GuardarVenta(oVentaViewModel, productoListViewModel);
                    if (oVentaRegistrada != null)
                    {
                        if (oVentaRegistrada.TipoVentaId == (int)TipoVentaE.Caja)
                        {
                            imprimirComanda(oVentaRegistrada.VentaId, true);
                        }
                        string idFacturaAPI = "";
                        string mensajeFactura = "";
                        if (ConFactura)
                        {
                            FacturaResponse facturaResponse = FacturacionHelper.EmitirFactura(oVentaRegistrada);
                            if (facturaResponse.Exito)
                            {
                                idFacturaAPI = facturaResponse.IdFactura;
                                mensajeFactura = "Factura generada con el id " + facturaResponse.IdFactura;

                            }
                            else
                            {
                                mensajeFactura = facturaResponse.Mensaje;
                            }
                        }

                        //actualizarEstado(oVentaRegistrada.VentaId, (int)oVentaRegistrada.EstadoId);
                        string mensaje = "";
                        if (oVentaRegistrada.EstadoId == (int)EstadoE.Pedido)
                        {
                            mensaje = "Pedido registrado con éxito";
                        }
                        else { mensaje = "Venta registrada con éxito"; }
                        return RedirectToAction("Index", new
                        {
                            mensaje = mensaje,
                            tipoMensaje = "success",
                            facturaAPI = idFacturaAPI,
                            mensajeFactura = mensajeFactura,
                            conFactura = ConFactura
                        });
                    }
                    else
                    {
                        return RedirectToAction("Error");
                    }
                }
            }

            List<ModelState> listModelStates = ViewData.ModelState.Values.ToList();
            foreach (ModelState modelState in listModelStates)
            {
                foreach (ModelError error in modelState.Errors)
                {
                    ModelState.AddModelError("", error.ErrorMessage);
                }
            }

            ViewBag.ClienteId = new SelectList(db.Cliente.Where(x => x.Activo == true), "ClienteId", "RazonSocial", oVentaViewModel.ClienteId);
            ViewBag.SucursalId = new SelectList(db.Sucursal.Where(x => x.Activo == true), "SucursalId", "Nombre", oVentaViewModel.SucursalId);
            ViewBag.TipoCobroId = new SelectList(db.TipoCobro.Where(x => x.Activo == true), "TipoCobroId", "Nombre", oVentaViewModel.TipoCobroId);
            ViewBag.TipoVentaId = new SelectList(db.TipoVenta.Where(x => x.Activo == true), "TipoVentaId", "Nombre", oVentaViewModel.TipoVentaId);
            ViewBag.ListaId = new SelectList(db.Lista.Where(x => x.Activo == true), "ListaId", "Nombre");
            ViewBag.EstadoId = new SelectList(db.Estado.Where(x => x.Activo == true && x.EstadoId != (int)EstadoE.Anulada), "EstadoId", "Descripcion", oVentaViewModel.EstadoId);
            ViewBag.ArqueoId = oVentaViewModel.ArqueoId;
            var preventistas = db.AspNetUsers.Where(x => x.PerfilId == (int)PerfilE.Preventista).Select(x => new
            {
                Text = x.Nombre + ", " + x.Apellido,
                Value = x.UsuarioId
            }).ToList();
            ViewBag.PreventistaId = new SelectList(preventistas, "Value", "Text");
            if (productoListViewModel.Count() > 0)
            {
                ViewBag.esVuelta = "esVuelta";
                //productoListViewModel = productoListViewModel.Where(x => x.)
                ViewBag.productoListViewModel = productoListViewModel;
            }
            return View(oVentaViewModel);
        }

        // GET: Ventas/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Venta venta = db.Venta.Find(id);
            if (venta == null)
            {
                return HttpNotFound();
            }
            int? ArqueoId = venta.ArqueoId;
            if (ArqueoId == null)
            {
                string usuarioId = User.Identity.GetUserId();
                AspNetUsers aspNetUsers = db.AspNetUsers.Where(x => x.Id == usuarioId).FirstOrDefault();
                Arqueo oArqueo = db.Arqueo.Where(x => x.Activo == true && x.Abierto == true && x.UsuarioInicioId == aspNetUsers.UsuarioId).FirstOrDefault();
                if (oArqueo == null)
                {
                    ArqueoId = 0;
                    //return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                else
                {
                    ArqueoId = oArqueo.ArqueoId;
                }
            }

            ViewBag.ArqueoId = (int)ArqueoId;

            ViewBag.ClienteId = new SelectList(db.Cliente.Where(x => x.Activo == true), "ClienteId", "RazonSocial", venta.ClienteId);
            ViewBag.SucursalId = new SelectList(db.Sucursal.Where(x => x.Activo == true), "SucursalId", "Nombre");
            ViewBag.TipoCobroId = new SelectList(db.TipoCobro.Where(x => x.Activo == true), "TipoCobroId", "Nombre", venta.TipoCobroId);
            ViewBag.TipoVentaId = new SelectList(db.TipoVenta.Where(x => x.Activo == true), "TipoVentaId", "Nombre", venta.TipoVentaId);
            ViewBag.ListaId = new SelectList(db.Lista.Where(x => x.Activo == true), "ListaId", "Nombre");
            ViewBag.EstadoId = new SelectList(db.Estado.Where(x => x.Activo == true && x.EstadoId != (int)EstadoE.Anulada), "EstadoId", "Descripcion", venta.EstadoId);
            var preventistas = db.AspNetUsers.Where(x => x.PerfilId == (int)PerfilE.Preventista).Select(x => new
            {
                Text = x.Nombre + ", " + x.Apellido,
                Value = x.UsuarioId
            }).ToList();
            ViewBag.PreventistaId = new SelectList(preventistas, "Value", "Text");

            List<VentaDetalle> listVentaDetalles = venta.VentaDetalle.Where(x => x.Activo == true).ToList();

            List<ProductoListViewModel> productoListViewModel = listVentaDetalles.Select(x => new ProductoListViewModel
            {
                ProductoId = (x.ProductoId != null) ? (int)x.ProductoId : (int)x.ComboId,
                Nombre = (x.ProductoId != null) ? x.Producto.Nombre : x.Combo.Nombre,
                Precio = Convert.ToDecimal(x.Precio.Replace(".", ",")),
                Cambio = (int)x.MonedaId,
                CambioStr = x.Moneda.Nombre,
                Marca = (x.ProductoId != null) ? x.Producto.Marca.Nombre : "Combo",
                Codigo = (x.ProductoId != null) ? x.Producto.Codigo : x.Combo.Codigo,
                Cantidad = Math.Round((decimal)x.Cantidad)
            }).ToList();

            ViewBag.productoListViewModel = productoListViewModel;

            VentaViewModel ventaViewModel = new VentaViewModel();
            ventaViewModel = VentaHelper.convertVentaToViewModel(ventaViewModel, venta);
            ViewBag.esEdicion = true;
            return View("Create", ventaViewModel);
        }

        // POST: Ventas/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "VentaId,EstadoId,TipoVentaId,SucursalId,ClienteId,TipoCobroId,Total,Descuento,Final,Observaciones,DireccionEnvio,CostoEnvio,Pendiente,ArqueoId")] VentaViewModel oVentaViewModel, IEnumerable<ProductoListViewModel> productoListViewModel, IEnumerable<PromosViewModel> promosViewModels, bool ConFactura = false)
        {
            productoListViewModel = productoListViewModel.Where(x => x.ProductoId != 0).ToList();
            if (ModelState.IsValid)
            {
                if (oVentaViewModel.TipoCobroId == (int)TipoCobroE.TARJETA_DEBITO ||
                    oVentaViewModel.TipoCobroId == (int)TipoCobroE.TARJETA_CREDITO)
                {
                    ConFactura = true;
                }

                if (
                ((ConFactura) && oVentaViewModel.ClienteId == null) ||
                ((oVentaViewModel.TipoCobroId == (int)TipoCobroE.TARJETA_DEBITO ||
                oVentaViewModel.TipoCobroId == (int)TipoCobroE.TARJETA_CREDITO) &&
                oVentaViewModel.ClienteId == null)
                )
                {
                    ModelState.AddModelError("ClienteId", "Para generar factura, sí o sí se necesita cargar el cliente");
                }
                else
                {
                    string UsuarioId = User.Identity.GetUsuarioId();
                    oVentaViewModel.UsuarioId = Convert.ToInt32(UsuarioId);

                    decimal DescuentoPromo = 0;
                    string observacionesPromo = "";
                    foreach (PromosViewModel item in promosViewModels)
                    {
                        if (item.ComboId != 0)
                        {
                            if (observacionesPromo != "") observacionesPromo += " ; ";
                            observacionesPromo += item.Descripcion;
                            DescuentoPromo += Convert.ToDecimal(item.MontoDescontado);
                        }
                    }

                    if (observacionesPromo != "")
                    {
                        if (oVentaViewModel.Observaciones == null || oVentaViewModel.Observaciones == "")
                        {
                            oVentaViewModel.Observaciones += "Promociones: " + observacionesPromo;
                        }
                        else oVentaViewModel.Observaciones += " - Promociones: " + observacionesPromo;
                    }
                    if (oVentaViewModel.Descuento == null) oVentaViewModel.Descuento = 0;
                    oVentaViewModel.Descuento += DescuentoPromo;
                    //oVentaViewModel.Final -= DescuentoPromo;
                    oVentaViewModel.Promos = DescuentoPromo;



                    Venta oVentaRegistrada = VentaHelper.EditarVenta(oVentaViewModel, productoListViewModel);
                    if (oVentaRegistrada != null)
                    {
                        string idFacturaAPI = "";
                        string mensajeFactura = "";
                        if (ConFactura)
                        {
                            FacturaResponse facturaResponse = FacturacionHelper.EmitirFactura(oVentaRegistrada);
                            if (facturaResponse.Exito)
                            {
                                idFacturaAPI = facturaResponse.IdFactura;
                                mensajeFactura = "Factura generada con el id " + facturaResponse.IdFactura;

                            }
                            else
                            {
                                mensajeFactura = facturaResponse.Mensaje;
                            }
                            //generar factura, devolver el Id de la factura y guardarlo en la BD
                            //Task.Run(() =>
                            //{
                            //    FacturacionHelper.EmitirFactura(oVentaRegistrada);
                            //});


                        }

                        //actualizarEstado(oVentaRegistrada.VentaId, (int)oVentaRegistrada.EstadoId);
                        string mensaje = "";
                        if (oVentaRegistrada.EstadoId == (int)EstadoE.Pedido)
                        {
                            mensaje = "Pedido editado con éxito";
                        }
                        else { mensaje = "Venta editada con éxito"; }
                        return RedirectToAction("Index", new
                        {
                            mensaje = mensaje,
                            tipoMensaje = "success",
                            facturaAPI = idFacturaAPI,
                            mensajeFactura = mensajeFactura,
                            conFactura = ConFactura
                        });
                    }
                    else
                    {
                        return RedirectToAction("Error");
                    }
                }
            }

            List<ModelState> listModelStates = ViewData.ModelState.Values.ToList();
            foreach (ModelState modelState in listModelStates)
            {
                foreach (ModelError error in modelState.Errors)
                {
                    ModelState.AddModelError("", error.ErrorMessage);
                }
            }

            ViewBag.ClienteId = new SelectList(db.Cliente.Where(x => x.Activo == true), "ClienteId", "RazonSocial", oVentaViewModel.ClienteId);
            ViewBag.SucursalId = new SelectList(db.Sucursal.Where(x => x.Activo == true), "SucursalId", "Nombre", oVentaViewModel.SucursalId);
            ViewBag.TipoCobroId = new SelectList(db.TipoCobro.Where(x => x.Activo == true), "TipoCobroId", "Nombre", oVentaViewModel.TipoCobroId);
            ViewBag.TipoVentaId = new SelectList(db.TipoVenta.Where(x => x.Activo == true), "TipoVentaId", "Nombre", oVentaViewModel.TipoVentaId);
            ViewBag.ListaId = new SelectList(db.Lista.Where(x => x.Activo == true), "ListaId", "Nombre");
            ViewBag.EstadoId = new SelectList(db.Estado.Where(x => x.Activo == true && x.EstadoId != (int)EstadoE.Anulada), "EstadoId", "Descripcion", oVentaViewModel.EstadoId);
            ViewBag.ArqueoId = oVentaViewModel.ArqueoId;
            var preventistas = db.AspNetUsers.Where(x => x.PerfilId == (int)PerfilE.Preventista).Select(x => new
            {
                Text = x.Nombre + ", " + x.Apellido,
                Value = x.UsuarioId
            }).ToList();
            ViewBag.PreventistaId = new SelectList(preventistas, "Value", "Text");
            if (productoListViewModel.Count() > 0)
            {
                ViewBag.esVuelta = "esVuelta";
                //productoListViewModel = productoListViewModel.Where(x => x.)
                ViewBag.productoListViewModel = productoListViewModel;
            }
            return View(oVentaViewModel);
        }

        public ActionResult GetVentasFiltro(int? cant)
        {
            List<Venta> listVentas;

            int PerfilId = Convert.ToInt32(User.Identity.GetPerfilId());
            int TipoVentaId = (int)TipoVentaE.Administracion;

            if (PerfilId == (int)PerfilE.Preventista)
            {
                TipoVentaId = (int)TipoVentaE.Preventista;
            }
            else if (PerfilId == (int)PerfilE.Caja)
            {
                TipoVentaId = (int)TipoVentaE.Caja;
            }
            if (PerfilId == (int)PerfilE.Administracion || PerfilId == (int)PerfilE.Desarrollo)
            {
                listVentas = db.Venta
                             .Include(v => v.Cliente)
                             .Include(v => v.Sucursal)
                             .Include(v => v.TipoCobro)
                             .Include(v => v.TipoVenta)
                             .Where(x => x.Activo == true)
                             .OrderByDescending(x => x.VentaId)
                             .Take((int)cant)
                             .ToList();
            }
            else
            {
                listVentas = db.Venta
                             .Include(v => v.Cliente)
                             .Include(v => v.Sucursal)
                             .Include(v => v.TipoCobro)
                             .Include(v => v.TipoVenta)
                             .Where(x => x.Activo == true && x.TipoVentaId == TipoVentaId)
                             .OrderByDescending(x => x.VentaId)
                             .Take((int)cant)
                             .ToList();
            }


            ViewBag.cantidadRegistros = cant;
            return PartialView("_Index", listVentas);

        }

        [HttpGet]
        public JsonResult GetFormasPago()
        {
            List<TipoCobro> tipoCobros = db.TipoCobro.Where(x => x.Activo == true).ToList();

            var dict = new Dictionary<int, string>();
            for (int i = 0; i < tipoCobros.Count; i++)
            {
                dict.Add(tipoCobros[i].TipoCobroId, tipoCobros[i].Nombre);
            }

            var json = JsonConvert.SerializeObject(dict);
            return Json(json, JsonRequestBehavior.AllowGet);
        }

        // GET: Ventas/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Venta venta = db.Venta.Find(id);
            if (venta == null)
            {
                return HttpNotFound();
            }
            return PartialView("_Details", venta);
        }

        // GET: Ventas/Details/5
        public ActionResult Estado(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Venta venta = db.Venta.Find(id);
            if (venta == null)
            {
                return HttpNotFound();
            }
            return PartialView("_Estado", venta);
        }

        public byte[] imprimirComanda(int VentaId, bool autoPrint)
        {
            Venta oVenta = db.Venta.Find(VentaId);

            List<VentaDetalle> listVentaDetalle = oVenta.VentaDetalle.Where(x => x.Activo == true).ToList();

            AspNetUsers inicio = db.AspNetUsers.Where(x => x.UsuarioId == oVenta.UsuarioId).FirstOrDefault();

            string UsuarioVenta = inicio.Nombre + ", " + inicio.Apellido;

            List<ProductoListViewModel> list = listVentaDetalle.Select(x => new ProductoListViewModel
            {
                ProductoId = (x.ProductoId != null) ? (int)x.ProductoId : (int)x.ComboId,
                Nombre = (x.ProductoId != null) ? x.Producto.Nombre : x.Combo.Nombre,
                Precio = Convert.ToDecimal(x.Precio.Replace(".", ",")),
                Cambio = (int)x.MonedaId,
                CambioStr = x.Moneda.Nombre,
                Marca = (x.ProductoId != null) ? x.Producto.Marca.Nombre : "Combo",
                Codigo = (x.ProductoId != null) ? x.Producto.Codigo : x.Combo.Codigo,
                Cantidad = (decimal)x.Cantidad,
                SubTotal = (decimal)Math.Round(((decimal)x.Cantidad * Convert.ToDecimal(x.Precio.Replace(".", ","))), 2)
            }).ToList();


            LocalReport localReport = new LocalReport();
            localReport.ReportPath = @"Reports/ReportResumen.rdlc";
            localReport.DataSources.Add(new ReportDataSource("DataSet1", list));


            string lbltipo, lblgenerador, lblusuario;
            if (oVenta.EstadoId == (int)EstadoE.Pedido || oVenta.EstadoId == (int)EstadoE.Preparacion || oVenta.EstadoId == (int)EstadoE.Despachado)
            {
                lbltipo = "Detalle de Pedido";
                lblgenerador = "Prev.:";
            }
            else
            {
                lbltipo = "Detalle de Venta";
                lblgenerador = "Cajero/a:";
            }
            AspNetUsers aspNetUsers = oVenta.UsuarioReferencia.AspNetUsers.FirstOrDefault();
            lblusuario = aspNetUsers.Nombre + ", " + aspNetUsers.Apellido;


            localReport.SetParameters(new ReportParameter("tipo", lbltipo));
            localReport.SetParameters(new ReportParameter("generador", lblgenerador));
            localReport.SetParameters(new ReportParameter("usuario", lblusuario));
            localReport.SetParameters(new ReportParameter("numero", oVenta.VentaId.ToString()));
            localReport.SetParameters(new ReportParameter("formaPago", oVenta.TipoCobro.Nombre));

            string reportType = "PDF";
            string mimeType;
            string encoding;
            string fileNameExtension = "pdf";

            string deviceInfo =
             @"<DeviceInfo>
                <OutputFormat>PDF</OutputFormat>
                </DeviceInfo>";
            Warning[] warnings;

            string[] streams;

            byte[] renderedBytes;
            //Render the report

            renderedBytes = localReport.Render(
                            reportType,
                            deviceInfo,
                            out mimeType,
                            out encoding,
                            out fileNameExtension,
                            out streams,
                            out warnings);

            var doc = new Document();
            var reader = new PdfReader(renderedBytes);

            using (FileStream fs = new FileStream(Server.MapPath("~/Reports/Summary.pdf"), FileMode.Create))
            {
                PdfStamper stamper = new PdfStamper(reader, fs);

                string Printer = "FK58 Printer";
                //string Printer = "";
                if (Printer == null || Printer == "")
                {
                    stamper.JavaScript = "var pp = getPrintParams();pp.interactive = pp.constants.interactionLevel.automatic;pp.printerName = getPrintParams().printerName;print(pp);\r";
                    stamper.Close();
                }
                else
                {
                    stamper.JavaScript = "var pp = getPrintParams();pp.interactive = pp.constants.interactionLevel.automatic;pp.printerName = " + Printer + ";print(pp);\r";
                    stamper.Close();
                }
            }
            reader.Close();

            FileStream fss = new FileStream(Server.MapPath("~/Reports/Summary.pdf"), FileMode.Open);
            byte[] bytes = new byte[fss.Length];
            fss.Read(bytes, 0, Convert.ToInt32(fss.Length));
            fss.Close();

            if (autoPrint)
            {
                SendToDirectPrint(Server.MapPath("~/Reports/Summary.pdf"));
            }
            System.IO.File.Delete(Server.MapPath("~/Reports/Summary.pdf"));
            return bytes;
        }

        [AllowAnonymous]
        public ActionResult ReporteResumen(int? VentaId)
        {
            if (VentaId == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            byte[] bytes = imprimirComanda((int)VentaId, false);
            return File(bytes, "application/pdf");
        }

        private void SendToDirectPrint(string filename)
        {
            ProcessStartInfo info = new ProcessStartInfo();
            info.Verb = "print";
            info.FileName = filename;
            info.CreateNoWindow = true;

            info.WindowStyle = ProcessWindowStyle.Normal;
            // info.UseShellExecute = false;

            Process p = new Process();

            p.StartInfo = info;
            p.Start();

            if (p.HasExited == false)
            {
                p.WaitForExit(10000);
            }

            p.WaitForInputIdle();
            if (false == p.CloseMainWindow())
            {
                p.Kill();
            }
        }

        public ActionResult ReportePedido(int? id)
        {

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Venta oVenta = db.Venta.Find(id);
            if (oVenta != null)
            {
                List<VentaDetalle> listVentaDetalle = oVenta.VentaDetalle.Where(x => x.Activo == true).ToList();

                AspNetUsers inicio = db.AspNetUsers.Where(x => x.UsuarioId == oVenta.UsuarioId).FirstOrDefault();

                string UsuarioVenta = inicio.Nombre + ", " + inicio.Apellido;

                List<ProductoListViewModel> list = listVentaDetalle.Select(x => new ProductoListViewModel
                {
                    ProductoId = (x.ProductoId != null) ? (int)x.ProductoId : (int)x.ComboId,
                    Nombre = (x.ProductoId != null) ? x.Producto.Nombre : x.Combo.Nombre,
                    Precio = Convert.ToDecimal(x.Precio),
                    Cambio = (int)x.MonedaId,
                    CambioStr = x.Moneda.Nombre,
                    Marca = (x.ProductoId != null) ? x.Producto.Marca.Nombre : "Combo",
                    Codigo = (x.ProductoId != null) ? x.Producto.Codigo : x.Combo.Codigo,
                    Cantidad = (decimal)x.Cantidad
                }).ToList();

                LocalReport localReport = new LocalReport();
                localReport.ReportPath = @"Reports/ReportPedido.rdlc";
                localReport.DisplayName = "Detalle " + DateTime.Now.ToShortDateString();
                localReport.DataSources.Add(new ReportDataSource("DataSet1", list));

                string lblTitulo, lblVenta, RazonSocial, CUIT;
                string descripcion = "";
                if (oVenta.EstadoId == (int)EstadoE.Pedido || oVenta.EstadoId == (int)EstadoE.Preparacion)
                {
                    lblTitulo = "Detalle de Pedido";
                    lblVenta = "Pedido ";
                }
                else
                {
                    lblTitulo = "Detalle de Venta";
                    lblVenta = "Venta ";
                }
                descripcion = "Monto: $" + oVenta.Final + " - Fecha: " + oVenta.Fecha;

                localReport.SetParameters(new ReportParameter("lblTitulo", lblTitulo));
                localReport.SetParameters(new ReportParameter("lblVenta", lblVenta));
                localReport.SetParameters(new ReportParameter("VentaId", oVenta.VentaId.ToString()));
                localReport.SetParameters(new ReportParameter("Fecha", oVenta.Fecha.ToString()));
                localReport.SetParameters(new ReportParameter("Descripcion", descripcion));
                if (oVenta.Cliente != null)
                {
                    RazonSocial = oVenta.Cliente.RazonSocial;
                    CUIT = oVenta.Cliente.CUIT;
                }
                else { RazonSocial = " "; CUIT = " "; }

                string Descuento, Recargo, Observaciones;
                Descuento = (oVenta.Descuento != null) ? oVenta.Descuento.ToString() : "0";
                Recargo = (oVenta.Recargo != null) ? oVenta.Recargo.ToString() : " ";
                Observaciones = (oVenta.Observaciones != null) ? oVenta.Observaciones.ToString() : " ";
                localReport.SetParameters(new ReportParameter("RazonSocial", RazonSocial));
                localReport.SetParameters(new ReportParameter("CUIT", CUIT));
                localReport.SetParameters(new ReportParameter("Total", "$" + oVenta.Total.ToString()));
                localReport.SetParameters(new ReportParameter("Descuento", "$" + Descuento));
                localReport.SetParameters(new ReportParameter("Recargo", "%" + Recargo));
                localReport.SetParameters(new ReportParameter("Final", "$" + oVenta.Final.ToString()));
                localReport.SetParameters(new ReportParameter("Observaciones", Observaciones));
                localReport.SetParameters(new ReportParameter("Usuario", UsuarioVenta));


                string reportType = "PDF";
                string mimeType;
                string encoding;
                string fileNameExtension = "pdf";

                string deviceInfo =
                 @"<DeviceInfo>
                <OutputFormat>PDF</OutputFormat>
               <PageWidth>9.2in</PageWidth>
                <PageHeight>12in</PageHeight>
                <MarginTop>0.25in</MarginTop>
                <MarginLeft>0.45in</MarginLeft>
                <MarginRight>0.45in</MarginRight>
                <MarginBottom>0.25in</MarginBottom>
                </DeviceInfo>";
                Warning[] warnings;

                string[] streams;

                byte[] renderedBytes;
                //Render the report

                renderedBytes = localReport.Render(
                                reportType,
                                deviceInfo,
                                out mimeType,
                                out encoding,
                                out fileNameExtension,
                                out streams,
                                out warnings);

                var doc = new Document();
                var reader = new PdfReader(renderedBytes);
                //reader.Info["Title"] = "Detalle de " + lblVenta + " " + DateTime.Now.ToShortDateString();
                //doc.AddTitle("Detalle de " + lblVenta + " " + DateTime.Now.ToShortDateString());

                reader.Info["Title"] = "Detalle " + DateTime.Now.ToShortDateString();
                doc.AddTitle("Detalle  " + DateTime.Now.ToShortDateString());

                using (FileStream fs = new FileStream(Server.MapPath("~/Reports/Summary.pdf"), FileMode.Create))
                {
                    PdfStamper stamper = new PdfStamper(reader, fs);

                    string Printer = "";
                    if (Printer == null || Printer == "")
                    {
                        stamper.JavaScript = "var pp = getPrintParams();pp.interactive = pp.constants.interactionLevel.automatic;pp.printerName = getPrintParams().printerName;print(pp);\r";
                        stamper.Close();
                    }
                    else
                    {
                        stamper.JavaScript = "var pp = getPrintParams();pp.interactive = pp.constants.interactionLevel.automatic;pp.printerName = " + Printer + ";print(pp);\r";
                        stamper.Close();
                    }
                }
                reader.Close();

                FileStream fss = new FileStream(Server.MapPath("~/Reports/Summary.pdf"), FileMode.Open);
                byte[] bytes = new byte[fss.Length];
                fss.Read(bytes, 0, Convert.ToInt32(fss.Length));
                fss.Close();
                System.IO.File.Delete(Server.MapPath("~/Reports/Summary.pdf"));
                return File(bytes, "application/pdf");
            }
            else
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

        }

        [HttpPost]
        public ActionResult EstadoConfirm(int? id, int? tipo, int? formaPago, int? cliente)
        {
            try
            {


                if (id == null || tipo == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }


                string[] respuesta = actualizarEstado((int)id, (int)tipo, formaPago, cliente);
                string tipoEstado = "";
                string mensaje = "";
                if (respuesta != null)
                {
                    tipoEstado = respuesta[0];
                    mensaje = respuesta[1];
                }

                var result = new { Success = "True", Message = mensaje, icon = "success", tipo = tipoEstado };
                return Json(result);
            }
            catch (Exception)
            {
                var result = new { Success = "False", Message = "Ocurrió un Error", icon = "error", tipo = "" };
                return Json(result);
            }

        }

        private string[] actualizarEstado(int id, int tipo, int? formaPago, int? cliente)
        {
            Venta venta = db.Venta.Find(id);
            if (cliente != null)
            {
                venta.ClienteId = (int)cliente;
            }
            int estadoVentaAnterior = (int)venta.EstadoId;
            if (venta == null)
            {
                return null;
            }
            venta.EstadoId = (int)tipo;
            Deposito oDeposito = db.Deposito.Where(x => x.Activo == true && x.SucursalId == venta.SucursalId).FirstOrDefault();
            List<VentaDetalle> listVentaDetalles = venta.VentaDetalle.Where(x => x.Activo == true).ToList();

            string[] respuesta = new string[2];

            if (venta.EstadoId == (int)EstadoE.Completada)
            {//buscar todas las ventasdetalle y descontar el stock
                respuesta[0] = EstadoE.Completada.ToString();
                //tipoEstado = EstadoE.Completada.ToString();
                respuesta[1] = "Finalización Completada";
                //mensaje = "Finalización Completada";
                foreach (VentaDetalle item in listVentaDetalles)
                {
                    StockDeposito oStockDeposito;
                    if (item.ProductoId != null)
                    {
                        oStockDeposito = db.StockDeposito.Where(x => x.Activo == true && x.DepositoId == oDeposito.DepositoId && x.ProductoId == item.ProductoId).FirstOrDefault();
                        oStockDeposito.Cantidad -= (decimal)item.Cantidad;
                        oStockDeposito.FechaActualizacion = DateTime.Now;
                        db.Entry(oStockDeposito).State = EntityState.Modified;
                        db.SaveChanges();
                    }
                    else
                    {
                        List<ComboProducto> listComboProductos = GestorComboProducto.getComboProductoByComboId((int)item.ComboId);
                        foreach (ComboProducto comboProducto in listComboProductos)
                        {
                            StockDeposito oStockDepositoCombo = GestorStockDeposito.getByProductoAndDeposito(comboProducto.ProductoId, oDeposito.DepositoId);
                            if (oStockDepositoCombo != null)
                            {
                                //if (venta.EstadoId == (int)EstadoE.Completada)
                                //{
                                //    oStockDepositoCombo.Cantidad -= (decimal)item.Cantidad;
                                //    oStockDepositoCombo.CantidadUsada -= item.Cantidad;
                                //}
                                //else
                                //{
                                oStockDepositoCombo.Cantidad -= (decimal)item.Cantidad;
                                //}
                                oStockDepositoCombo.FechaActualizacion = DateTime.Now;
                                GestorStockDeposito.Actualizar(oStockDepositoCombo);

                            }
                        }
                        //oStockDeposito = db.StockDeposito.Where(x => x.Activo == true && x.DepositoId == oDeposito.DepositoId && x.ComboId == item.ComboId).FirstOrDefault();
                    }


                    //venta.Pendiente = false;

                }
                venta.Pendiente = false;
                venta.TipoCobroId = (int)formaPago;
            }
            else if (venta.EstadoId == (int)EstadoE.Anulada)
            {//buscar todas las ventas detalle y aumentarle el stock.
                respuesta[0] = EstadoE.Anulada.ToString();
                respuesta[1] = "Anulación Registrada";
                foreach (VentaDetalle item in listVentaDetalles)
                {
                    StockDeposito oStockDeposito;
                    if (item.ProductoId != null)
                    {
                        oStockDeposito = db.StockDeposito.Where(x => x.Activo == true && x.DepositoId == oDeposito.DepositoId && x.ProductoId == item.ProductoId).FirstOrDefault();
                        if (estadoVentaAnterior == (int)EstadoE.Completada)
                        {
                            oStockDeposito.Cantidad += (decimal)item.Cantidad;
                        }
                        oStockDeposito.CantidadUsada += (decimal)item.Cantidad;
                        oStockDeposito.FechaActualizacion = DateTime.Now;
                        db.Entry(oStockDeposito).State = EntityState.Modified;
                        db.SaveChanges();
                    }
                    else
                    {
                        List<ComboProducto> listComboProductos = GestorComboProducto.getComboProductoByComboId((int)item.ComboId);
                        foreach (ComboProducto comboProducto in listComboProductos)
                        {
                            StockDeposito oStockDepositoCombo = GestorStockDeposito.getByProductoAndDeposito(comboProducto.ProductoId, oDeposito.DepositoId);
                            if (oStockDepositoCombo != null)
                            {
                                if (estadoVentaAnterior == (int)EstadoE.Completada)
                                {
                                    oStockDepositoCombo.Cantidad += (decimal)item.Cantidad;
                                }
                                oStockDepositoCombo.CantidadUsada += (decimal)item.Cantidad;
                                oStockDepositoCombo.FechaActualizacion = DateTime.Now;
                                GestorStockDeposito.Actualizar(oStockDepositoCombo);

                            }
                        }
                        //oStockDeposito = db.StockDeposito.Where(x => x.Activo == true && x.DepositoId == oDeposito.DepositoId && x.ComboId == item.ComboId).FirstOrDefault();
                    }

                }
                venta.Pendiente = false;
            }
            else if (venta.EstadoId == (int)EstadoE.Pedido)
            {
                if (estadoVentaAnterior == (int)EstadoE.Completada)
                {//Si estaba completada y hay que ponerla en pedido
                 //Hay que sumar el stock, solo a Cantidad
                    foreach (VentaDetalle item in listVentaDetalles)
                    {
                        StockDeposito oStockDeposito;
                        if (item.ProductoId != null)
                        {
                            oStockDeposito = db.StockDeposito.Where(x => x.Activo == true && x.DepositoId == oDeposito.DepositoId && x.ProductoId == item.ProductoId).FirstOrDefault();
                            oStockDeposito.Cantidad += (decimal)item.Cantidad;
                            oStockDeposito.FechaActualizacion = DateTime.Now;
                            db.Entry(oStockDeposito).State = EntityState.Modified;
                            db.SaveChanges();
                        }
                        else
                        {
                            List<ComboProducto> listComboProductos = GestorComboProducto.getComboProductoByComboId((int)item.ComboId);
                            foreach (ComboProducto comboProducto in listComboProductos)
                            {
                                StockDeposito oStockDepositoCombo = GestorStockDeposito.getByProductoAndDeposito(comboProducto.ProductoId, oDeposito.DepositoId);
                                if (oStockDepositoCombo != null)
                                {
                                    oStockDepositoCombo.Cantidad += (decimal)item.Cantidad;
                                    oStockDepositoCombo.FechaActualizacion = DateTime.Now;
                                    GestorStockDeposito.Actualizar(oStockDepositoCombo);

                                }
                            }
                            //oStockDeposito = db.StockDeposito.Where(x => x.Activo == true && x.DepositoId == oDeposito.DepositoId && x.ComboId == item.ComboId).FirstOrDefault();
                        }

                    }
                }
                else if (estadoVentaAnterior == (int)EstadoE.Anulada)
                {//Si estaba anulada y hay que ponerla en pedido
                 //Hay que restar el stock, solo a CantidadUsada
                    foreach (VentaDetalle item in listVentaDetalles)
                    {
                        StockDeposito oStockDeposito;
                        if (item.ProductoId != null)
                        {
                            oStockDeposito = db.StockDeposito.Where(x => x.Activo == true && x.DepositoId == oDeposito.DepositoId && x.ProductoId == item.ProductoId).FirstOrDefault();
                            oStockDeposito.CantidadUsada -= (decimal)item.Cantidad;
                            oStockDeposito.FechaActualizacion = DateTime.Now;
                            db.Entry(oStockDeposito).State = EntityState.Modified;
                            db.SaveChanges();
                        }
                        else
                        {
                            List<ComboProducto> listComboProductos = GestorComboProducto.getComboProductoByComboId((int)item.ComboId);
                            foreach (ComboProducto comboProducto in listComboProductos)
                            {
                                StockDeposito oStockDepositoCombo = GestorStockDeposito.getByProductoAndDeposito(comboProducto.ProductoId, oDeposito.DepositoId);
                                if (oStockDepositoCombo != null)
                                {
                                    oStockDepositoCombo.CantidadUsada -= (decimal)item.Cantidad;
                                    oStockDepositoCombo.FechaActualizacion = DateTime.Now;
                                    GestorStockDeposito.Actualizar(oStockDepositoCombo);

                                }
                            }
                            //oStockDeposito = db.StockDeposito.Where(x => x.Activo == true && x.DepositoId == oDeposito.DepositoId && x.ComboId == item.ComboId).FirstOrDefault();
                        }


                    }
                }

                venta.Pendiente = true;
                respuesta[0] = EstadoE.Pedido.ToString();
                respuesta[1] = "Pedido Registrado";
            }
            else if (venta.EstadoId == (int)EstadoE.Preparacion)
            {   //imprimir detalle 
                respuesta[0] = EstadoE.Preparacion.ToString();
                respuesta[1] = "Preparación Registrada";
                if (estadoVentaAnterior == (int)EstadoE.Completada)
                {//Si estaba completada y se vuelve a poner en preparacion, sumarle la cantidad.
                    foreach (VentaDetalle item in listVentaDetalles)
                    {
                        StockDeposito oStockDeposito;
                        if (item.ProductoId != null)
                        {
                            oStockDeposito = db.StockDeposito.Where(x => x.Activo == true && x.DepositoId == oDeposito.DepositoId && x.ProductoId == item.ProductoId).FirstOrDefault();
                            oStockDeposito.Cantidad += (decimal)item.Cantidad;

                            oStockDeposito.FechaActualizacion = DateTime.Now;
                            db.Entry(oStockDeposito).State = EntityState.Modified;
                            db.SaveChanges();
                        }
                        else
                        {
                            List<ComboProducto> listComboProductos = GestorComboProducto.getComboProductoByComboId((int)item.ComboId);
                            foreach (ComboProducto comboProducto in listComboProductos)
                            {
                                StockDeposito oStockDepositoCombo = GestorStockDeposito.getByProductoAndDeposito(comboProducto.ProductoId, oDeposito.DepositoId);
                                if (oStockDepositoCombo != null)
                                {
                                    oStockDepositoCombo.Cantidad += (decimal)item.Cantidad;
                                    oStockDepositoCombo.FechaActualizacion = DateTime.Now;
                                    GestorStockDeposito.Actualizar(oStockDepositoCombo);

                                }
                            }
                            //oStockDeposito = db.StockDeposito.Where(x => x.Activo == true && x.DepositoId == oDeposito.DepositoId && x.ComboId == item.ComboId).FirstOrDefault();
                        }


                    }
                }
                venta.Pendiente = true;
            }
            else if (venta.EstadoId == (int)EstadoE.Despachado)
            {
                respuesta[0] = EstadoE.Despachado.ToString();
                respuesta[1] = "Pedido Despachado";
            }
            venta.FechaModificacion = DateTime.Now;
            db.Entry(venta).State = EntityState.Modified;
            db.SaveChanges();

            return respuesta;
        }

        [HttpPost]
        public PartialViewResult GetProductosVenta(string id, int ListaId, int[] listas, string[] array, decimal[] cantidad)
        {
            int DepositoId = 1;
            // string SucursalId = User.Identity.GetSucursalId();
            // Deposito deposito = db.Deposito.Where(x => x.Activo == true && x.SucursalId == Convert.ToInt32(SucursalId)).FirstOrDefault();
            //if(deposito != null)
            //{
            //    DepositoId = deposito.DepositoId;
            //}

            if (array == null)
            {
                array = new string[0];
            }

            bool esIncluido = false;//Si es True, indica que el id esta dentro del array, osea ya estaba cargado.
            bool esAgregado = false; //Si es True, indica que no es el primer registro, ya hay algo cargado.
            if (cantidad != null)
            {
                esAgregado = true;
                //OP 1: el id que pasa esta dentro del array
                //OP 2: el id que pasa no esta dentro del array

                foreach (string i in array)
                {
                    if (id == i)
                    {//EL ID esta dentro del array
                        esIncluido = true;
                        break;
                    }
                }

            }

            //List<Root> listDolar = PrecioHelper.GetPrecioDolar();
            //decimal precioUSD = Convert.ToDecimal(listDolar.FirstOrDefault().casa.venta.ToString().Replace(",", "."));
            decimal precioUSD;
            if (Session["Dolar"] != null)
            {
                precioUSD = Convert.ToDecimal(Session["Dolar"].ToString());
            }
            else
            {
                precioUSD = PrecioHelper.GetPrecioDolarStatico();
                Session.Add("Dolar", precioUSD);
            }



            List<ProductoListViewModel> lista = new List<ProductoListViewModel>();

            Producto oProducto = db.Producto.Where(x => x.Codigo == id.ToString()).FirstOrDefault();
            Combo oCombo = db.Combo.Where(x => x.Codigo == id.ToString()).FirstOrDefault();

            List<Producto> listProductos = db.Producto.Where(x => array.Contains(x.Codigo)).ToList();
            List<Combo> listCombo = db.Combo.Where(x => array.Contains(x.Codigo)).ToList();



            if (oProducto != null)
            {//estoy buscando un producto
                if (!esIncluido)
                {
                    listProductos.Add(oProducto);
                }
            }
            else
            {//estoy buscando un combo
                if (!esIncluido)
                {
                    listCombo.Add(oCombo);
                }
            }

            List<ProductoListViewModel> list = new List<ProductoListViewModel>();

            if (listProductos.Count > 0)
            {
                foreach (Producto item in listProductos)
                {
                    int intListaId = 0;
                    int contador = 0;
                    string[] arrayTemp = array;
                    foreach (string str in arrayTemp)
                    {
                        if (str == item.Codigo)
                        {
                            intListaId = listas[contador];
                            break;
                        }
                        contador += 1;
                    }
                    if ((listas == null || !esIncluido) && intListaId == 0)
                    {
                        intListaId = ListaId;
                    }
                    else
                    {
                        intListaId = listas[contador];
                        string[] arrayDelete = new string[arrayTemp.Length - 1];
                    }
                    PrecioLista precioLista = db.PrecioLista.Where(z => z.ListaId == intListaId && z.Activo == true && z.ProductoId == item.ProductoId).FirstOrDefault();
                    ProductoListViewModel productoListViewModel = new ProductoListViewModel()
                    {
                        ProductoId = item.ProductoId,
                        Nombre = item.Nombre,
                        Precio = (decimal)precioLista.Precio,
                        Cambio = precioLista.MonedaId,
                        CambioStr = precioLista.Moneda.Nombre,
                        Marca = item.Marca.Nombre,
                        Codigo = item.Codigo,
                        TipoStock = 1,
                        ListaId = intListaId
                    };

                    list.Add(productoListViewModel);
                }

            }

            if (listCombo.Count > 0)
            {
                foreach (Combo item in listCombo)
                {
                    int intListaId = 0;
                    int contador = 0;
                    foreach (string str in array)
                    {
                        if (str == item.Codigo)
                        {
                            break;
                        }
                        contador += 1;
                    }
                    if (listas == null || !esIncluido)
                    {
                        intListaId = ListaId;
                    }
                    else
                    {
                        intListaId = listas[contador];
                    }
                    PrecioLista precioLista = db.PrecioLista.Where(z => z.ListaId == intListaId && z.Activo == true && z.ComboId == item.ComboId).FirstOrDefault();
                    ProductoListViewModel productoListViewModel = new ProductoListViewModel()
                    {
                        ProductoId = item.ComboId,
                        Nombre = item.Nombre,
                        Precio = (decimal)precioLista.Precio,
                        Cambio = precioLista.MonedaId,
                        CambioStr = precioLista.Moneda.Nombre,
                        Marca = "",
                        Codigo = item.Codigo,
                        TipoStock = 2,
                        ListaId = intListaId
                    };

                    list.Add(productoListViewModel);

                }

            }

            //Opciones:
            //OP 1: el id que pasa esta dentro del array
            //OP 2: el id que pasa no esta dentro del array
            //OP 3: el array es nulo

            if (esAgregado)
            {//Ya tiene datos cargados
                foreach (ProductoListViewModel item in list)
                {
                    if (item.Codigo == id.ToString())
                    {
                        if (esIncluido)
                        {//Id dentro del array
                            int index = Array.IndexOf(array, item.Codigo);
                            decimal cant = cantidad[index] + 1;
                            //Sumar un valor a la cantidad que tenia
                            item.Cantidad += cant;
                        }
                        else
                        {//Id esta fuera del array pero dentro de la list (primera vez)

                            item.Cantidad = 1;
                            //NO HAGO NADA
                        }
                    }
                    else
                    {
                        int index = Array.IndexOf(array, item.Codigo);
                        decimal cant = cantidad[index];
                        item.Cantidad = cant;
                    }
                }
            }
            else
            {//Id esta fuera del array pero dentro de la list (primera vez)
                list.FirstOrDefault().Cantidad = 1;
            }

            try
            {
                foreach (ProductoListViewModel item in list)
                {
                    item.Alerta = false;
                    StockDeposito oStockDeposito;
                    if (item.TipoStock == (int)TipoStockE.Producto)
                    {
                        oStockDeposito = db.StockDeposito
                            .Where(x => x.Activo == true && x.ProductoId == item.ProductoId && x.DepositoId == DepositoId).FirstOrDefault();
                        //Aca viene el stock actual de ese producto
                        Producto producto = db.Producto.Find(item.ProductoId);
                        if (((decimal)oStockDeposito.CantidadUsada - item.Cantidad) <= producto.Minimo)
                        {
                            //poner alerta
                            item.Alerta = true;
                            item.AlertStr = "Min: " + producto.Minimo.ToString() + ". Actual: " + (oStockDeposito.CantidadUsada - item.Cantidad).ToString();
                        }
                    }
                    else
                    {
                        oStockDeposito = db.StockDeposito
                            .Where(x => x.Activo == true && x.ComboId == item.ProductoId && x.DepositoId == DepositoId).FirstOrDefault();
                        //Aca viene el stock actual de ese producto
                        Combo combo = db.Combo.Find(item.ProductoId);
                        if (((decimal)oStockDeposito.CantidadUsada - item.Cantidad) <= combo.Minimo)
                        {
                            //poner alerta
                            item.Alerta = true;
                            item.AlertStr = "Min: " + combo.Minimo.ToString() + ". Actual: " + (oStockDeposito.CantidadUsada - item.Cantidad).ToString();
                        }
                    }
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }

            ViewBag.PrecioUSD = precioUSD;
            return PartialView("_ProductosVenta", list);
        }

        [HttpPost]
        public string GetProductosFiltro(string filtro, int? listaId)
        {

            List<ProductoListViewModel> listProductos = new List<ProductoListViewModel>();

            char[] separator = { ' ' };
            string[] texto = filtro.Split(separator, StringSplitOptions.RemoveEmptyEntries);

            var lista = from p in db.Producto
                        where texto.All(x => (p.Nombre + " " + p.Codigo + " " + p.Marca.Nombre + " " + p.TipoProducto.Nombre).Contains(x))
                        && p.Activo == true

                        select new ProductoListViewModel
                        {
                            ProductoId = p.ProductoId,
                            Nombre = p.Nombre,
                            Marca = p.Marca.Nombre,
                            TipoProducto = p.TipoProducto.Nombre,
                            Codigo = p.Codigo,
                            Cargado = p.PrecioLista.Where(x => x.ListaId == listaId).FirstOrDefault().Cargado


                        };

            if (lista != null)
            {
                if (lista.Count() > 0)
                {
                    listProductos = lista.ToList();
                }
            }

            var listaCom = from c in db.Combo
                           where texto.All(x => (c.Nombre + " " + c.Codigo).Contains(x))
                           && c.Activo == true
                           select new ProductoListViewModel
                           {
                               ProductoId = c.ComboId,
                               Nombre = c.Nombre,
                               Marca = "Combo",
                               TipoProducto = "-",
                               Codigo = c.Codigo,
                               Cargado = c.PrecioLista.Where(x => x.ListaId == listaId).FirstOrDefault().Cargado

                           };
            if (listaCom != null)
            {
                if (listaCom.Count() > 0)
                {
                    if (lista != null)
                    {
                        listProductos.AddRange(listaCom.ToList());
                    }
                    else
                    {
                        listProductos = listaCom.ToList();
                    }
                }
            }

            string list;
            list = JsonConvert.SerializeObject(listProductos, Formatting.None, new JsonSerializerSettings() { ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore });
            return list;
        }

        [HttpPost]
        public ActionResult CalcularPromociones(string[] productos, string[] cantidades, string[] cantidadesPromo, int listaPromo)
        {
            if (productos == null || cantidades == null)
            {
                var result = new { Success = "False", Message = "Valores Nulos", icon = "danger" };
                return Json(result);
            }

            int[] productosInt = CustomMethods.convertStringArrayToIntArray(productos);
            int[] cantidadesInt = CustomMethods.convertStringArrayToIntArray(cantidades);
            int[] cantidadesPromoInt = CustomMethods.convertStringArrayToIntArray(cantidadesPromo);

            List<int> listProductos = new List<int>();
            List<int> listCantidades = new List<int>();
            List<int> listCantidadesPromo = new List<int>();
            listProductos = CustomMethods.convertIntArrayToIntList(productosInt);
            listProductos = ProductoHelper.getProductoIdPorCodigo(listProductos);
            listCantidades = CustomMethods.convertIntArrayToIntList(cantidadesInt);
            listCantidadesPromo = CustomMethods.convertIntArrayToIntList(cantidadesPromoInt);

            List<Combo> listCombos = buscadorPromociones(listProductos, listCantidades, listCantidadesPromo);
            List<PromosViewModel> listPromosViewModel = new List<PromosViewModel>();
            foreach (Combo item in listCombos)
            {
                decimal PrecioCombo, TotalProductos, Diferencia;
                PrecioCombo = PrecioHelper.getPrecioCombo(item, listaPromo, Session["Dolar"].ToString());
                TotalProductos = PrecioHelper.getPrecioComboProductosSumatoria(item, listaPromo, Session["Dolar"].ToString());
                Diferencia = TotalProductos - PrecioCombo;

                listPromosViewModel.Add(new PromosViewModel()
                {
                    Descripcion = item.Descripcion,
                    MontoDescontado = Diferencia.ToString(),
                    ProductosIncluidos = ComboHelper.getNombreProductosPorCombo(item),
                    ComboId = item.ComboId
                });

            }


            return PartialView("_PromosEncontradas", listPromosViewModel);
            // return Json(new { Success = "True", Message = descriCombos, icon = "success" }, JsonRequestBehavior.AllowGet);
        }

        private List<Combo> buscadorPromociones(List<int> productos, List<int> cantidades, List<int> cantidadesOtro)
        {
            List<int> combos = new List<int>();
            List<int> productosExcluidos = new List<int>();
            List<int> combosExcluidos = new List<int>();
            List<Combo> listCombosEncontrados = new List<Combo>();
            bool band = false;
            for (int valor = 0; valor < productos.Count(); valor++)
            {
                if (valor < productos.Count())
                {
                    int ProductoId = productos[valor];
                    int cantidad = cantidades[valor];
                    List<ComboProducto> list = db.ComboProducto.Where(x => x.Activo == true && x.ProductoId == ProductoId).ToList(); //&& !combos.Contains(x.ComboId)
                    if (list.Count() > 0 && cantidad > 0)
                    {
                        Combo oComboEncontrado = examinarCombos(productos, cantidades, list, valor, 0, productosExcluidos, cantidadesOtro, listCombosEncontrados);
                        if (oComboEncontrado != null)
                        {
                            listCombosEncontrados.Add(oComboEncontrado);
                            band = true;
                        }
                        else
                        {//Si el combo es null, quiere decir que no encontro ningun combo con ese producto. IR al siguiente
                            for (int j = valor; j < cantidadesOtro.Count; j++)
                            {
                                if (cantidadesOtro[j] != 0) { cantidades[j] = cantidadesOtro[j]; cantidadesOtro[j] = 0; }
                            }
                            continue;
                        }
                        if (cantidades[valor] == 0)
                        {
                            if (band == true)
                            {
                                int valorBack = valor;
                                if (cantidadesOtro[valor] > 0)
                                {
                                    cantidades[valor] = cantidadesOtro[valor];
                                    cantidadesOtro[valor] = 0;
                                    valor -= 1;
                                }
                                //else
                                //{
                                for (int j = valorBack + 1; j < cantidadesOtro.Count; j++)
                                {
                                    if (cantidadesOtro[j] != 0) { cantidades[j] = cantidadesOtro[j]; cantidadesOtro[j] = 0; }
                                }

                                //}
                                band = false;
                            }
                            else
                            {
                                productosExcluidos.Add(productos[valor]);
                                var listaCP = db.ComboProducto.Where(x => x.ProductoId == ProductoId && x.Activo == true).ToList();
                                var listaCPGroup = listaCP.GroupBy(x => x.Combo).Select(g => new { valor = g.Key.ComboId }).ToList();
                                if (listaCPGroup.Count() > 0)
                                {
                                    combos.AddRange(listaCPGroup.Select(x => x.valor).ToList());
                                }
                            }
                        }
                        else
                        {
                            valor -= 1;
                        }
                    }
                }
            }
            return listCombosEncontrados;
        }

        private Combo examinarCombos(List<int> productos, List<int> cantidades, List<ComboProducto> list, int i, int suma, List<int> productosExcluidos, List<int> cantidadesOtros, List<Combo> combosExcluidos)
        {
            List<ComboProducto> listNuevo = new List<ComboProducto>();
            int ProductoId = productos[i];
            list = list.Where(x => x.ProductoId == ProductoId && x.Activo == true && !productosExcluidos.Contains(x.ProductoId)).ToList(); // && !combosExcluidos.Contains(x.Combo)
            if (list.Count() > 0)
            {
                var lista = list.GroupBy(x => x.Combo).Select(g => new { valor = g.Key, count = g.Count() }).ToList();
                if (lista != null)
                {
                    int cant = cantidades[i];
                    var otraLista = lista.Where(x => x.count == cant).ToList();
                    if (otraLista.Count() > 0)
                    {
                        foreach (Combo item in otraLista.Select(x => x.valor).ToList())
                        {
                            int cantidadCP = item.ComboProducto.Count(x => x.Activo == true);
                            int cantidadCPProducto = item.ComboProducto.Count(x => x.Activo == true && x.ProductoId == ProductoId);
                            if ((cantidadCP - suma) == cant)
                            {//hubo coinicidencia. la cantidad buscada es igual a la cantidad total de ese combo.
                                cantidades[i] -= cant;
                                return item;
                            }
                            else if (cantidadCPProducto == cant && ((cantidadCP - suma) > cant))
                            {//la cantidad buscada es igual a la cantidad de CombosProductos del producto buscado, pero el combo tiene mas productos
                                if (i < productos.Count - 1)
                                {
                                    listNuevo.AddRange(item.ComboProducto.Where(x => x.Activo == true).ToArray());
                                    cantidades[i] -= cant;

                                    Combo oCombo1 = examinarCombos(productos, cantidades, listNuevo, i + 1, suma + cant, productosExcluidos, cantidadesOtros, combosExcluidos);
                                    if (oCombo1 == null)
                                    {
                                        cantidadesOtros[i] += 1;
                                    }
                                    return oCombo1;
                                }
                                else
                                {
                                    cantidades[i] -= cant;
                                    cantidadesOtros[i] += cant;
                                    return null;
                                }
                            }
                        }
                    }
                    else
                    {
                        if (cantidades[i] > 0)
                        {
                            //descontar uno y volver a buscar.
                            cantidades[i] -= 1;
                            cantidadesOtros[i] += 1;
                            Combo oCombo1 = examinarCombos(productos, cantidades, list, i, suma, productosExcluidos, cantidadesOtros, combosExcluidos);
                            return oCombo1;
                        }
                        else return null;
                    }

                }
                return null;
            }
            else return null;
        }

        public JsonResult GenerarFactura(int? id)
        {
            try
            {
                Venta oVenta = db.Venta.Find((int)id);
                FacturaResponse facturaResponse = FacturacionHelper.EmitirFactura(oVenta);
                string mensajeFactura = "";
                string success = "";
                string iconStr = "";
                if (facturaResponse.Exito)
                {
                    mensajeFactura = "Factura generada con el id " + facturaResponse.IdFactura;
                    success = "True";
                    iconStr = "success";
                }
                else
                {
                    mensajeFactura = facturaResponse.Mensaje;
                    success = "False";
                    iconStr = "error";
                }

                var result = new { Success = success, Message = mensajeFactura, facturaAPI = facturaResponse.IdFactura, icon = iconStr };
                return Json(result);
            }
            catch (Exception)
            {
                var result = new { Success = "False", Message = "Ocurrió un Error", icon = "error" };
                return Json(result);
            }

        }

        public JsonResult GetVentasChart()
        {
            try
            {
                string[] meses = { "Enero", "Febrero", "Marzo", "Abril", "Mayo", "Junio", "Julio", "Agosto", "Septiembre", "Octubre", "Noviembre", "Diciembre" };
                
                
                List<Venta> listVentas = db.Venta
                    .Where(x => x.Activo == true && x.EstadoId == (int)EstadoE.Completada).ToList();
                var list = listVentas.GroupBy(x => x.Fecha.Month).Select(f => new
                {
                    valor = meses[f.Key-1],
                    sum = f.Sum(s => s.Final)
                }).ToList();

                //return Json(list, JsonRequestBehavior.AllowGet);
                var listX = list.Select(x => x.valor).ToArray();
                var listY = list.Select(x => x.sum).ToArray();
                string descri = "Gráfico de dinero ingresado por ventas y pedidos finalizados, mensual";
                var result = new { Success = "True", x = listX, y = listY, descri = descri };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                var result = new { Success = "False", Message = "Ocurrió un Error", icon = "error" };
                return Json(result, JsonRequestBehavior.AllowGet);
            }

        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        public ActionResult Reporte(int? id)
        {

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Arqueo oArqueo = db.Arqueo.Find(id);
            if (oArqueo.FechaFin != null)
            {
                List<Venta> listVentas = db.Venta.Where(x => x.Activo == true && x.ArqueoId == oArqueo.ArqueoId).ToList();
                //List<Venta> listCobrosReport = listVenta.Select(x => new CobroReportViewModel
                //{
                //    CobroId = x.CobroId,
                //    Fecha = x.Fecha,
                //    DatosPersona = (x.Persona.PersonaTipoId == (int)Negocio.enumeradores.PersonaTipoE.Fisica) ? x.Persona.Nombre + ", " + x.Persona.Apellido : x.Persona.RazonSocial,
                //    Final = (decimal)x.Final
                //}).ToList();
                if (oArqueo == null)
                {
                    return HttpNotFound();
                }
                int CantVentas = listVentas.Count();
                AspNetUsers inicio = db.AspNetUsers.Where(x => x.UsuarioId == oArqueo.UsuarioInicioId).FirstOrDefault();
                AspNetUsers finalizo = db.AspNetUsers.Where(x => x.UsuarioId == oArqueo.UsuarioFinalizoId).FirstOrDefault();
                string UsuarioInicio = inicio.Nombre + ", " + inicio.Apellido;
                string UsuarioFinalizo = finalizo.Nombre + ", " + finalizo.Apellido;
                decimal dinero = 0;

                foreach (Venta item in listVentas)
                {
                    dinero += (decimal)item.Final;

                }


                string Dinero = dinero.ToString();

                LocalReport localReport = new LocalReport();
                localReport.ReportPath = @"Reports/ReportArqueo.rdlc";
                localReport.DisplayName = "Arqueo de Caja " + DateTime.Now.ToShortDateString();
                localReport.DataSources.Add(new ReportDataSource("DataSet1", listVentas));


                localReport.SetParameters(new ReportParameter("FechaInicio", oArqueo.FechaInicio.ToShortDateString()));
                localReport.SetParameters(new ReportParameter("HoraInicio", oArqueo.HoraInicio));
                string fechaFin = ""; string horaFin = "";
                if (oArqueo.FechaFin != null)
                {
                    fechaFin = oArqueo.FechaFin.Value.ToShortDateString();
                    horaFin = oArqueo.HoraFin.ToString();
                }
                localReport.SetParameters(new ReportParameter("FechaFin", fechaFin));
                localReport.SetParameters(new ReportParameter("HoraFin", horaFin));
                localReport.SetParameters(new ReportParameter("Total", "$" + oArqueo.Total.ToString()));
                localReport.SetParameters(new ReportParameter("CantidadVentas", CantVentas.ToString()));
                localReport.SetParameters(new ReportParameter("TotalEfectivo", "$" + oArqueo.TotalEfectivo.ToString()));
                localReport.SetParameters(new ReportParameter("TotalOtrosMedios", "$" + (oArqueo.Total - oArqueo.TotalEfectivo).ToString()));
                localReport.SetParameters(new ReportParameter("IniciadoCon", "$" + oArqueo.Iniciado.ToString()));
                localReport.SetParameters(new ReportParameter("Sobrante", "$" + oArqueo.Sobrante.ToString()));
                localReport.SetParameters(new ReportParameter("Faltante", "$" + oArqueo.Faltante.ToString()));
                localReport.SetParameters(new ReportParameter("FinalizadoCon", "$" + oArqueo.Finalizado.ToString()));
                localReport.SetParameters(new ReportParameter("DineroIngresado", "$" + Dinero));
                localReport.SetParameters(new ReportParameter("UsuarioInicio", UsuarioInicio));
                localReport.SetParameters(new ReportParameter("UsuarioFinalizo", UsuarioFinalizo));


                string reportType = "PDF";
                string mimeType;
                string encoding;
                string fileNameExtension = "pdf";

                string deviceInfo =
                 @"<DeviceInfo>
                <OutputFormat>PDF</OutputFormat>
               <PageWidth>9.2in</PageWidth>
                <PageHeight>12in</PageHeight>
                <MarginTop>0.25in</MarginTop>
                <MarginLeft>0.45in</MarginLeft>
                <MarginRight>0.45in</MarginRight>
                <MarginBottom>0.25in</MarginBottom>
                </DeviceInfo>";
                Warning[] warnings;

                string[] streams;

                byte[] renderedBytes;
                //Render the report

                renderedBytes = localReport.Render(
                                reportType,
                                deviceInfo,
                                out mimeType,
                                out encoding,
                                out fileNameExtension,
                                out streams,
                                out warnings);

                var doc = new Document();
                var reader = new PdfReader(renderedBytes);
                reader.Info["Title"] = "Arqueo de Caja " + DateTime.Now.ToShortDateString();
                doc.AddTitle("Arqueo de Caja " + DateTime.Now.ToShortDateString());

                using (FileStream fs = new FileStream(Server.MapPath("~/Reports/Summary.pdf"), FileMode.Create))
                {
                    PdfStamper stamper = new PdfStamper(reader, fs);

                    string Printer = "";
                    if (Printer == null || Printer == "")
                    {
                        stamper.JavaScript = "var pp = getPrintParams();pp.interactive = pp.constants.interactionLevel.automatic;pp.printerName = getPrintParams().printerName;print(pp);\r";
                        stamper.Close();
                    }
                    else
                    {
                        stamper.JavaScript = "var pp = getPrintParams();pp.interactive = pp.constants.interactionLevel.automatic;pp.printerName = " + Printer + ";print(pp);\r";
                        stamper.Close();
                    }
                }
                reader.Close();

                FileStream fss = new FileStream(Server.MapPath("~/Reports/Summary.pdf"), FileMode.Open);
                byte[] bytes = new byte[fss.Length];
                fss.Read(bytes, 0, Convert.ToInt32(fss.Length));
                fss.Close();
                System.IO.File.Delete(Server.MapPath("~/Reports/Summary.pdf"));
                return File(bytes, "application/pdf");
            }
            else
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

        }

        // GET: Ventas/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Venta venta = db.Venta.Find(id);
            if (venta == null)
            {
                return HttpNotFound();
            }
            return View(venta);
        }

        // POST: Ventas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Venta venta = db.Venta.Find(id);
            db.Venta.Remove(venta);
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
