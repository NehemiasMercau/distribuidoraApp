using Negocio.entidades;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Negocio.gestores
{
    public class GestorStockDeposito
    {
        public static StockDeposito getByProductoAndDeposito(int ProductoId, int DepositoId)
        {
            StockDeposito oStockDeposito;
            if (!GestorEntidadesConexion.getConexionState())
            {
                using (var ctx = new DistribuidoraDBEntities())
                {
                    oStockDeposito = ctx.StockDeposito
                        .FirstOrDefault(x => x.Activo == true && x.DepositoId == DepositoId && x.ProductoId == ProductoId);
                }
            }
            else
            {
                oStockDeposito = GestorEntidadesConexion._contexto.StockDeposito
                    .FirstOrDefault(x => x.Activo == true && x.DepositoId == DepositoId && x.ProductoId == ProductoId);

            }
            return oStockDeposito;
        }

        public static StockDeposito getById(int StockDepositoId)
        {
            StockDeposito oStockDeposito;
            if (!GestorEntidadesConexion.getConexionState())
            {
                using (var ctx = new DistribuidoraDBEntities())
                {
                    oStockDeposito = ctx.StockDeposito.Find(StockDepositoId);
                }
            }
            else
            {
                oStockDeposito = GestorEntidadesConexion._contexto.StockDeposito.Find(StockDepositoId);
            }
            return oStockDeposito;
        }

        public static StockDeposito getByComboAndDeposito(int ComboId, int DepositoId)
        {
            StockDeposito oStockDeposito;
            if (!GestorEntidadesConexion.getConexionState())
            {
                using (var ctx = new DistribuidoraDBEntities())
                {
                    oStockDeposito = ctx.StockDeposito
                        .FirstOrDefault(x => x.Activo == true && x.DepositoId == DepositoId && x.ComboId == ComboId);
                }
            }
            else
            {
                oStockDeposito = GestorEntidadesConexion._contexto.StockDeposito
                    .FirstOrDefault(x => x.Activo == true && x.DepositoId == DepositoId && x.ComboId == ComboId);

            }
            return oStockDeposito;
        }

        public static void Actualizar(StockDeposito oStockDeposito)
        {
            if (!GestorEntidadesConexion.getConexionState())
            {
                using (var ctx = new DistribuidoraDBEntities())
                {
                    ctx.Entry(oStockDeposito).State = EntityState.Modified;
                    ctx.SaveChanges();
                }
            }
            else
            {
                GestorEntidadesConexion._contexto.Entry(oStockDeposito).State = EntityState.Modified;
                GestorEntidadesConexion._contexto.SaveChanges();
            }
        }
    }
}
