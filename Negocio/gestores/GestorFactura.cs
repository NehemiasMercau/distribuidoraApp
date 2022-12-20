using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Negocio.entidades;

namespace Negocio.gestores
{
    public class GestorFactura
    {
        public static int getLast()
        {
            int FacturaId = 0;
            Factura oFactura;
            if (!GestorEntidadesConexion.getConexionState())
            {
                using (var ctx = new DistribuidoraDBEntities())
                {
                    oFactura = ctx.Factura.LastOrDefault();
                }
            }
            else
            {
                oFactura = GestorEntidadesConexion._contexto.Factura.LastOrDefault();
            }
           
            if (oFactura != null) { FacturaId = oFactura.FacturaId; }
            return FacturaId;
        }


        public static Factura Insertar(Factura oFactura)
        {
           if (!GestorEntidadesConexion.getConexionState())
            {
                using (var ctx = new DistribuidoraDBEntities())
                {
                    ctx.Factura.Add(oFactura);
                    ctx.SaveChanges();
                }
            }
            else
            {
                oFactura = GestorEntidadesConexion._contexto.Factura.Add(oFactura);
                GestorEntidadesConexion._contexto.SaveChanges();
            }
           
            return oFactura;
        }
    }
}
