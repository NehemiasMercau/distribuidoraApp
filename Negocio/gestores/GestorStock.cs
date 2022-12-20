using Negocio.entidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Negocio.gestores
{
    public class GestorStock
    {
        public static StockDeposito GetStockDeposito(int DepositoId, int ProductoId, int ComboId)
        {
            StockDeposito oStockDeposito;
            if (!GestorEntidadesConexion.getConexionState())
            {
                using (var ctx = new DistribuidoraDBEntities())
                {
                    if (ComboId == 0)
                    {
                        oStockDeposito = ctx.StockDeposito.Where(x => x.DepositoId == DepositoId && x.ProductoId == ProductoId && x.Activo == true).FirstOrDefault();
                    }
                    else
                    {
                        oStockDeposito = ctx.StockDeposito.Where(x => x.DepositoId == DepositoId && x.ComboId == ComboId && x.Activo == true).FirstOrDefault();
                    }
                }
            }
            else
            {
                if (ComboId == 0)
                {
                    oStockDeposito = GestorEntidadesConexion._contexto.StockDeposito.Where(x => x.DepositoId == DepositoId && x.ProductoId == ProductoId && x.Activo == true).FirstOrDefault();
                }
                else
                {
                    oStockDeposito = GestorEntidadesConexion._contexto.StockDeposito.Where(x => x.DepositoId == DepositoId && x.ComboId == ComboId && x.Activo == true).FirstOrDefault();
                }
            }
            return oStockDeposito;
        }

        public static List<StockDeposito> GetStockDepositoProductos(int DepositoId)
        {
            List<StockDeposito> listStockDeposito;
            if (!GestorEntidadesConexion.getConexionState())
            {
                using (var ctx = new DistribuidoraDBEntities())
                {
                    listStockDeposito = ctx.StockDeposito.Include("Producto").Where(x => x.DepositoId == DepositoId && x.ProductoId != null && x.Activo == true).ToList();
                }
            }
            else
            {
                listStockDeposito = GestorEntidadesConexion._contexto.StockDeposito.Include("Producto").Where(x => x.DepositoId == DepositoId && x.ProductoId != null && x.Activo == true).ToList();
            }
            return listStockDeposito;
        }

        public static List<StockDeposito> getDepositosConStock(int ProductoId, int ComboId)
        {
            List<StockDeposito> listStockDeposito;
            if (!GestorEntidadesConexion.getConexionState())
            {
                using (var ctx = new DistribuidoraDBEntities())
                {
                    if (ComboId == 0)
                    {
                        listStockDeposito = ctx.StockDeposito.Include("Deposito").Include("Producto").Where(x => x.ProductoId == ProductoId && x.Activo == true).ToList();
                    }
                    else
                    {
                        listStockDeposito = ctx.StockDeposito.Include("Deposito").Include("Combo").Where(x => x.ComboId == ComboId && x.Activo == true).ToList();
                    }
                }
            }
            else
            {
                if (ComboId == 0)
                {
                    listStockDeposito = GestorEntidadesConexion._contexto.StockDeposito.Include("Deposito").Include("Producto").Where(x => x.ProductoId == ProductoId && x.Activo == true).ToList();
                }
                else
                {
                    listStockDeposito = GestorEntidadesConexion._contexto.StockDeposito.Include("Deposito").Include("Combo").Where(x => x.ComboId == ComboId && x.Activo == true).ToList();
                }
            }
            return listStockDeposito;
        }

        public static StockDeposito insertarStockDeposito(StockDeposito oStockDeposito)
        {
            oStockDeposito.Activo = true;
            oStockDeposito.FechaActualizacion = DateTime.Now;
            if (!GestorEntidadesConexion.getConexionState())
            {
                using (var ctx = new DistribuidoraDBEntities())
                {
                    ctx.StockDeposito.Add(oStockDeposito);
                    ctx.SaveChanges();
                }
            }
            else
            {
                GestorEntidadesConexion._contexto.StockDeposito.Add(oStockDeposito);
                GestorEntidadesConexion.SaveChanges();
            }
            return oStockDeposito;
        }

        public static void actualizarStockDeposito(StockDeposito oStockDeposito)
        {
            oStockDeposito.FechaActualizacion = DateTime.Now;
            if (!GestorEntidadesConexion.getConexionState())
            {
                using (var ctx = new DistribuidoraDBEntities())
                {
                    ctx.Entry(oStockDeposito).State = System.Data.Entity.EntityState.Modified;
                    ctx.SaveChanges();
                }
            }
            else
            {
                GestorEntidadesConexion._contexto.Entry(oStockDeposito).State = System.Data.Entity.EntityState.Modified;
                GestorEntidadesConexion.SaveChanges();
            }
        }

        public static void agregarActualizacionStock(string Descripcion, int UsuarioId)
        {
            ActualizacionStock oActualizacionStock = new ActualizacionStock();
            oActualizacionStock.Fecha = DateTime.Now;
            oActualizacionStock.Hora = DateTime.Now.Minute.ToString() + ":" + DateTime.Now.Second.ToString();
            oActualizacionStock.UsuarioReferenciaId = UsuarioId;
            oActualizacionStock.Activo = true;
            oActualizacionStock.Descripcion = Descripcion;
            if (!GestorEntidadesConexion.getConexionState())
            {
                using (var ctx = new DistribuidoraDBEntities())
                {
                    ctx.ActualizacionStock.Add(oActualizacionStock);
                    ctx.SaveChanges();
                }
            }
            else
            {
                GestorEntidadesConexion._contexto.ActualizacionStock.Add(oActualizacionStock);
                GestorEntidadesConexion.SaveChanges();
            }
        }
    }
}
