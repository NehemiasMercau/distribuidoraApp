using DistribuidoraAPI.Models;
using Negocio.entidades;
using Negocio.enumeradores;
using Negocio.gestores;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace DistribuidoraAPI.Helpers
{
    public class VentaHelper
    {
        public static Venta GuardarVenta(VentaViewModel oVentaViewModel, IEnumerable<ProductoListViewModel> productoListViewModel)
        {
            try
            {
                GestorEntidadesConexion.inicializarContexto();
                GestorEntidadesConexion.beginTransaccion();

                oVentaViewModel.Fecha = DateTime.Now;
                if (oVentaViewModel.EstadoId == (int)EstadoE.Completada) { oVentaViewModel.Pendiente = false; } else oVentaViewModel.Pendiente = true;
                //Deposito oDeposito = db.Deposito.Where(x => x.Activo == true && x.SucursalId == oVentaViewModel.SucursalId).FirstOrDefault();

                if (oVentaViewModel.PreventistaId != null)
                {
                    if (oVentaViewModel.PreventistaId != 0)
                    {
                        oVentaViewModel.TipoVentaId = (int)TipoVentaE.Preventista;
                        oVentaViewModel.PreventistaId = oVentaViewModel.PreventistaId;
                    }
                }

                Venta oVenta = new Venta();
                oVenta = convertViewModelToVenta(oVentaViewModel, oVenta);



                Deposito oDeposito = GestorDeposito.getBySucursalId(oVentaViewModel.SucursalId);

                foreach (ProductoListViewModel item in productoListViewModel)
                {
                    Producto oProducto = GestorProducto.getByCodigo(item.Codigo);
                    Combo oCombo = GestorCombo.getByCodigo(item.Codigo);

                    if (oProducto != null)
                    {
                        item.ProductoId = oProducto.ProductoId;
                    }
                    else { item.ProductoId = oCombo.ComboId; }

                    StockDeposito oStockDeposito;
                    if (item.TipoStock == (int)TipoStockE.Producto)
                    {
                        oStockDeposito = GestorStockDeposito.getByProductoAndDeposito(item.ProductoId, oDeposito.DepositoId);
                        //oStockDeposito = db.StockDeposito.Where(x => x.Activo == true && x.DepositoId == oDeposito.DepositoId && x.ProductoId == item.ProductoId).FirstOrDefault();
                        if (oStockDeposito != null)
                        {
                            if (oVentaViewModel.EstadoId == (int)EstadoE.Completada)
                            {
                                oStockDeposito.Cantidad -= item.Cantidad;
                                oStockDeposito.CantidadUsada -= item.Cantidad;
                            }
                            else
                            {
                                oStockDeposito.CantidadUsada -= item.Cantidad;
                            }
                            oStockDeposito.FechaActualizacion = DateTime.Now;
                            GestorStockDeposito.Actualizar(oStockDeposito);

                        }
                    }
                    else
                    {
                        List<ComboProducto> listComboProductos = GestorComboProducto.getComboProductoByComboId(item.ProductoId);
                        foreach (ComboProducto comboProducto in listComboProductos)
                        {
                            StockDeposito oStockDepositoCombo = GestorStockDeposito.getByProductoAndDeposito(comboProducto.ProductoId, oDeposito.DepositoId);
                            if (oStockDepositoCombo != null)
                            {
                                if (oVentaViewModel.EstadoId == (int)EstadoE.Completada)
                                {
                                    oStockDepositoCombo.Cantidad -= item.Cantidad;
                                    oStockDepositoCombo.CantidadUsada -= item.Cantidad;
                                }
                                else
                                {
                                    oStockDepositoCombo.CantidadUsada -= item.Cantidad;
                                }
                                oStockDepositoCombo.FechaActualizacion = DateTime.Now;
                                GestorStockDeposito.Actualizar(oStockDepositoCombo);

                            }
                        }
                        //oStockDeposito = GestorStockDeposito.getByComboAndDeposito(item.ProductoId, oDeposito.DepositoId);
                        //oStockDeposito = db.StockDeposito.Where(x => x.Activo == true && x.DepositoId == oDeposito.DepositoId && x.ComboId == item.ProductoId).FirstOrDefault();

                    }

                    Dolar oDolar = GestorDolar.GetLastDolar();
                    decimal precioFinalUSD = oDolar.Precio;
                    VentaDetalle oVentaDetalle = new VentaDetalle();
                    oVentaDetalle.Activo = true;
                    oVentaDetalle.Cantidad = item.Cantidad;
                    if (item.Cambio == (int)MonedaE.USD)
                    {
                        oVentaDetalle.Precio = (item.Precio * precioFinalUSD).ToString("#.##");
                    }
                    else
                    {
                        oVentaDetalle.Precio = item.Precio.ToString("#.##");
                    }

                    oVentaDetalle.VentaId = oVenta.VentaId;
                    oVentaDetalle.MonedaId = item.Cambio;
                    oVentaDetalle.ListaId = item.ListaId;
                    if (oProducto != null) { oVentaDetalle.ProductoId = oProducto.ProductoId; }
                    if (oCombo != null) { oVentaDetalle.ComboId = oCombo.ComboId; }
                    oVenta.VentaDetalle.Add(oVentaDetalle);
                }
                oVenta.UsuarioId = oVentaViewModel.UsuarioId;
                oVenta.Promos = oVentaViewModel.Promos;
                if (oVenta.ClienteId != null)
                {
                    oVenta.Cliente = GestorCliente.getById((int)oVenta.ClienteId);
                }
                oVenta = GestorVenta.Insertar(oVenta);
                GestorEntidadesConexion.commitTransaccion();
                return oVenta;
            }
            catch (Exception ex)
            {
                // throw ex;
                GestorEntidadesConexion.rollbackTransaccion();
                return null;
            }
            finally
            {
                GestorEntidadesConexion.disposeTransaccion();
            }
        }

        public static Venta convertViewModelToVenta(VentaViewModel oVentaViewModel, Venta oVenta)
        {
            oVenta.TipoVentaId = oVentaViewModel.TipoVentaId;
            oVenta.SucursalId = oVentaViewModel.SucursalId;
            oVenta.ClienteId = oVentaViewModel.ClienteId;
            oVenta.TipoCobroId = oVentaViewModel.TipoCobroId;
            oVenta.Total = oVentaViewModel.Total;
            oVenta.Descuento = oVentaViewModel.Descuento;
            oVenta.Final = oVentaViewModel.Final;
            oVenta.Observaciones = oVentaViewModel.Observaciones;
            oVenta.DireccionEnvio = oVentaViewModel.DireccionEnvio;
            oVenta.CostoEnvio = oVentaViewModel.CostoEnvio;
            oVenta.Pendiente = oVentaViewModel.Pendiente;
            oVenta.EstadoId = oVentaViewModel.EstadoId;
            oVenta.Recargo = oVentaViewModel.Recargo;
            oVenta.PreventistaId = oVentaViewModel.PreventistaId;
            if (oVentaViewModel.ArqueoId != 0)
            {
                oVenta.ArqueoId = oVentaViewModel.ArqueoId;
            }
            oVenta.UsuarioId = oVentaViewModel.UsuarioId;
            return oVenta;
        }
        public static VentaViewModel convertVentaToViewModel(VentaViewModel oVentaViewModel, Venta oVenta)
        {
            oVentaViewModel.TipoVentaId = oVenta.TipoVentaId;
            oVentaViewModel.SucursalId = oVenta.SucursalId;
            oVentaViewModel.ClienteId = oVenta.ClienteId;
            oVentaViewModel.TipoCobroId = oVenta.TipoCobroId;
            oVentaViewModel.Total = oVenta.Total;
            oVentaViewModel.Descuento = oVenta.Descuento;
            oVentaViewModel.Final = oVenta.Final;
            oVentaViewModel.Observaciones = oVenta.Observaciones;
            oVentaViewModel.DireccionEnvio = oVenta.DireccionEnvio;
            oVentaViewModel.CostoEnvio = oVenta.CostoEnvio;
            oVentaViewModel.Pendiente = oVenta.Pendiente;
            oVentaViewModel.EstadoId = (int)oVenta.EstadoId;
            oVentaViewModel.Recargo = oVenta.Recargo;
            if (oVenta.ArqueoId != null)
            {
                oVentaViewModel.ArqueoId = (int)oVenta.ArqueoId;
            }
            oVentaViewModel.UsuarioId = (int)oVenta.UsuarioId;
            return oVentaViewModel;
        }

        public static Venta EditarVenta(VentaViewModel oVentaViewModel, IEnumerable<ProductoListViewModel> productoListViewModel)
        {
            try
            {
                GestorEntidadesConexion.inicializarContexto();
                GestorEntidadesConexion.beginTransaccion();

                oVentaViewModel.Fecha = DateTime.Now;
                if (oVentaViewModel.EstadoId == (int)EstadoE.Completada) { oVentaViewModel.Pendiente = false; } else oVentaViewModel.Pendiente = true;
                //Deposito oDeposito = db.Deposito.Where(x => x.Activo == true && x.SucursalId == oVentaViewModel.SucursalId).FirstOrDefault();

                Venta oVenta = GestorVenta.getById(oVentaViewModel.VentaId);

                limpiarStockVenta(oVenta);
                eliminarDetalles(oVenta);

                oVenta = convertViewModelToVenta(oVentaViewModel, oVenta);

                Deposito oDeposito = GestorDeposito.getBySucursalId(oVentaViewModel.SucursalId);

                foreach (ProductoListViewModel item in productoListViewModel)
                {
                    Producto oProducto = GestorProducto.getByCodigo(item.Codigo);
                    Combo oCombo = GestorCombo.getByCodigo(item.Codigo);

                    if (oProducto != null)
                    {
                        item.ProductoId = oProducto.ProductoId;
                    }
                    else { item.ProductoId = oCombo.ComboId; }

                    StockDeposito oStockDeposito;
                    if (item.TipoStock == (int)TipoStockE.Producto)
                    {
                        oStockDeposito = GestorStockDeposito.getByProductoAndDeposito(item.ProductoId, oDeposito.DepositoId);
                        //oStockDeposito = db.StockDeposito.Where(x => x.Activo == true && x.DepositoId == oDeposito.DepositoId && x.ProductoId == item.ProductoId).FirstOrDefault();
                        if (oStockDeposito != null)
                        {
                            if (oVentaViewModel.EstadoId == (int)EstadoE.Completada)
                            {
                                oStockDeposito.Cantidad -= item.Cantidad;
                                oStockDeposito.CantidadUsada -= item.Cantidad;
                            }
                            else
                            {
                                oStockDeposito.CantidadUsada -= item.Cantidad;
                            }
                            oStockDeposito.FechaActualizacion = DateTime.Now;
                            GestorStockDeposito.Actualizar(oStockDeposito);

                        }
                    }
                    else
                    {
                        List<ComboProducto> listComboProductos = GestorComboProducto.getComboProductoByComboId(item.ProductoId);
                        foreach (ComboProducto comboProducto in listComboProductos)
                        {
                            StockDeposito oStockDepositoCombo = GestorStockDeposito.getByProductoAndDeposito(comboProducto.ProductoId, oDeposito.DepositoId);
                            if (oStockDepositoCombo != null)
                            {
                                if (oVentaViewModel.EstadoId == (int)EstadoE.Completada)
                                {
                                    oStockDepositoCombo.Cantidad -= item.Cantidad;
                                    oStockDepositoCombo.CantidadUsada -= item.Cantidad;
                                }
                                else
                                {
                                    oStockDepositoCombo.CantidadUsada -= item.Cantidad;
                                }
                                oStockDepositoCombo.FechaActualizacion = DateTime.Now;
                                GestorStockDeposito.Actualizar(oStockDepositoCombo);

                            }
                        }
                        //oStockDeposito = GestorStockDeposito.getByComboAndDeposito(item.ProductoId, oDeposito.DepositoId);
                        //oStockDeposito = db.StockDeposito.Where(x => x.Activo == true && x.DepositoId == oDeposito.DepositoId && x.ComboId == item.ProductoId).FirstOrDefault();

                    }

                    Dolar oDolar = GestorDolar.GetLastDolar();
                    decimal precioFinalUSD = oDolar.Precio;


                    VentaDetalle oVentaDetalle = new VentaDetalle();
                    oVentaDetalle.Activo = true;
                    oVentaDetalle.Cantidad = item.Cantidad;

                    if (item.Cambio == (int)MonedaE.USD)
                    {
                        oVentaDetalle.Precio = (item.Precio * precioFinalUSD).ToString("#.##");
                    }
                    else
                    {
                        oVentaDetalle.Precio = item.Precio.ToString("#.##");
                    }

                    oVentaDetalle.VentaId = oVenta.VentaId;
                    oVentaDetalle.MonedaId = item.Cambio;
                    oVentaDetalle.ListaId = item.ListaId;
                    if (oProducto != null) { oVentaDetalle.ProductoId = oProducto.ProductoId; }
                    if (oCombo != null) { oVentaDetalle.ComboId = oCombo.ComboId; }
                    oVenta.VentaDetalle.Add(oVentaDetalle);
                }
                oVenta.UsuarioId = oVentaViewModel.UsuarioId;
                oVenta.Promos = oVentaViewModel.Promos;

                if (oVenta.ClienteId != null)
                {
                    oVenta.Cliente = GestorCliente.getById((int)oVenta.ClienteId);
                }
                oVenta = GestorVenta.Actualizar(oVenta);
                GestorEntidadesConexion.commitTransaccion();
                return oVenta;
            }
            catch (Exception ex)
            {
                GestorEntidadesConexion.rollbackTransaccion();
                return null;
            }
            finally
            {
                GestorEntidadesConexion.disposeTransaccion();
            }
        }

        public static void limpiarStockVenta(Venta oVenta)
        {

            Deposito oDeposito = GestorDeposito.getBySucursalId(oVenta.SucursalId);

            List<VentaDetalle> listVentaDetalles = oVenta.VentaDetalle.Where(x => x.Activo == true).ToList();
            foreach (VentaDetalle item in listVentaDetalles)
            {
                StockDeposito oStockDeposito;
                if (item.ProductoId != null)
                {
                    oStockDeposito = GestorStockDeposito.getByProductoAndDeposito((int)item.ProductoId, oDeposito.DepositoId);

                    if (oVenta.EstadoId == (int)EstadoE.Completada)
                    {
                        oStockDeposito.Cantidad += (decimal)item.Cantidad;
                    }
                    oStockDeposito.CantidadUsada += (decimal)item.Cantidad;
                    oStockDeposito.FechaActualizacion = DateTime.Now;
                    GestorStockDeposito.Actualizar(oStockDeposito);

                }
                else
                {
                    List<ComboProducto> listComboProductos = GestorComboProducto.getComboProductoByComboId((int)item.ComboId);
                    foreach (ComboProducto comboProducto in listComboProductos)
                    {
                        StockDeposito oStockDepositoCombo = GestorStockDeposito.getByProductoAndDeposito(comboProducto.ProductoId, oDeposito.DepositoId);
                        if (oStockDepositoCombo != null)
                        {
                            if (oVenta.EstadoId == (int)EstadoE.Completada)
                            {
                                oStockDepositoCombo.Cantidad += (decimal)item.Cantidad;
                            }
                            oStockDepositoCombo.CantidadUsada += (decimal)item.Cantidad;
                            oStockDepositoCombo.FechaActualizacion = DateTime.Now;
                            GestorStockDeposito.Actualizar(oStockDepositoCombo);

                        }
                    }

                }

            }
        }

        public static void eliminarDetalles(Venta oVenta)
        {
            List<VentaDetalle> listVentaDetalles = oVenta.VentaDetalle.Where(x => x.Activo == true).ToList();
            foreach (VentaDetalle item in listVentaDetalles)
            {
                GestorVentaDetalle.Eliminar(item);
            }
        }
    }
}