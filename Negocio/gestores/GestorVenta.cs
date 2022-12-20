using Negocio.entidades;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Negocio.gestores
{
    public class GestorVenta
    {
        public static Venta Insertar(Venta oVenta)
        {
            oVenta.Activo = true;
            oVenta.Fecha = DateTime.Now;
            if (!GestorEntidadesConexion.getConexionState())
            {
                using (var ctx = new DistribuidoraDBEntities())
                {
                    ctx.Venta.Add(oVenta);
                    ctx.SaveChanges();
                }
            }
            else
            {
                GestorEntidadesConexion._contexto.Venta.Add(oVenta);
                GestorEntidadesConexion._contexto.SaveChanges();
            }
            return oVenta;
        }

        public static Venta Actualizar(Venta oVenta)
        {
            oVenta.Activo = true;
            oVenta.Fecha = DateTime.Now;
            if (!GestorEntidadesConexion.getConexionState())
            {
                using (var ctx = new DistribuidoraDBEntities())
                {
                    ctx.Entry(oVenta).State = EntityState.Modified;
                    ctx.SaveChanges();
                }
            }
            else
            {
                GestorEntidadesConexion._contexto.Entry(oVenta).State = EntityState.Modified;
                GestorEntidadesConexion._contexto.SaveChanges();
            }
            return oVenta;
        }

        public static Venta getById(int VentaId)
        {
            Venta oVenta;
            if (!GestorEntidadesConexion.getConexionState())
            {
                using (var ctx = new DistribuidoraDBEntities())
                {
                    oVenta = ctx.Venta.Find(VentaId);
                }
            }
            else
            {
                oVenta = GestorEntidadesConexion._contexto.Venta.Find(VentaId);
            }
            return oVenta;
        }

    }
}
