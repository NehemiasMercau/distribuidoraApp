using Negocio.entidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Negocio.gestores
{
    public class GestorDeposito
    {
        public static Deposito getBySucursalId(int SucursalId)
        {
            Deposito oDeposito;
            if (!GestorEntidadesConexion.getConexionState())
            {
                using (var ctx = new DistribuidoraDBEntities())
                {
                    oDeposito = ctx.Deposito.Where(x => x.Activo == true && x.SucursalId == SucursalId).FirstOrDefault();
                }
            }
            else
            {
                oDeposito = GestorEntidadesConexion._contexto.Deposito.Where(x => x.Activo == true && x.SucursalId == SucursalId).FirstOrDefault();
            }
            return oDeposito;
        }
    }
}
