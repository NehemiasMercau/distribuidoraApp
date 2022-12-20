using Negocio.entidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Negocio.gestores
{
    public class GestorLista
    {

        

        public static List<Lista> getListas()
        {
            List<Lista> listLista;
            if (!GestorEntidadesConexion.getConexionState())
            {
                using (var ctx = new DistribuidoraDBEntities())
                {
                    listLista = ctx.Lista.Where(x => x.Activo == true).ToList();
                }
            }
            else
            {
                listLista = GestorEntidadesConexion._contexto.Lista.Where(x => x.Activo == true).ToList();
            }
            return listLista;
        }

    }
}
