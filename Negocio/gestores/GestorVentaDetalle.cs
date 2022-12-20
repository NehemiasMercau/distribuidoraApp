using Negocio.entidades;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Negocio.gestores
{
    public class GestorVentaDetalle
    {
        public static void Eliminar(VentaDetalle oVentaDetalle)
        {
            oVentaDetalle.Activo = false;
            if (!GestorEntidadesConexion.getConexionState())
            {
                using (var ctx = new DistribuidoraDBEntities())
                {
                    ctx.Entry(oVentaDetalle).State = EntityState.Modified;
                    ctx.SaveChanges();
                }
            }
            else
            {
                GestorEntidadesConexion._contexto.Entry(oVentaDetalle).State = EntityState.Modified;
                GestorEntidadesConexion._contexto.SaveChanges();
            }
        }
    }
}
