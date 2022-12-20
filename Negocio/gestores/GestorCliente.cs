using Negocio.entidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Negocio.gestores
{
    public class GestorCliente
    {
        public static Cliente getById(int ClienteId)
        {
            Cliente oCliente;
            if (!GestorEntidadesConexion.getConexionState())
            {
                using (var ctx = new DistribuidoraDBEntities())
                {
                    oCliente = ctx.Cliente.Find(ClienteId);
                }
            }
            else
            {
                oCliente = GestorEntidadesConexion._contexto.Cliente.Find(ClienteId);
            }
            return oCliente;
        }
    }
}
