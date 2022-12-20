using Negocio.entidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Negocio.gestores
{
   public class GestorDolar
    {
        public static Dolar GetLastDolar()
        {
            Dolar oDolar;
            if (!GestorEntidadesConexion.getConexionState())
            {
                using (var ctx = new DistribuidoraDBEntities())
                {
                    oDolar = ctx.Dolar.Where(x => x.DolarId > 0).ToList().LastOrDefault();
                }
            }
            else
            {
                oDolar = GestorEntidadesConexion._contexto.Dolar.Where(x => x.DolarId > 0).ToList().LastOrDefault();
            }
            return oDolar;
        }

        public static Dolar Actualizar(decimal precioUSD)
        {
            Dolar oDolar = new Dolar()
            {
                Fecha = DateTime.Now,
                Precio = precioUSD
            };
            if (!GestorEntidadesConexion.getConexionState())
            {
                using (var ctx = new DistribuidoraDBEntities())
                {
                    ctx.Dolar.Add(oDolar);
                    ctx.SaveChanges();
                }
            }
            else
            {
                GestorEntidadesConexion._contexto.Dolar.Add(oDolar);
                GestorEntidadesConexion._contexto.SaveChanges();
            }
            return oDolar;
        }
         
    }
}
