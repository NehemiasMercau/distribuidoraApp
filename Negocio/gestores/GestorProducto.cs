using Negocio.entidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Negocio.gestores
{
    public class GestorProducto
    {
        public static Producto Insertar(Producto oProducto)
        {
            oProducto.FechaAlta = DateTime.Now;
            oProducto.Activo = true;
            oProducto.FechaModificacion = DateTime.Now;
            if (!GestorEntidadesConexion.getConexionState())
            {
                using (var ctx = new DistribuidoraDBEntities())
                {
                    ctx.Producto.Add(oProducto);
                    ctx.SaveChanges();
                }
            }
            else
            {
                GestorEntidadesConexion._contexto.Producto.Add(oProducto);
                GestorEntidadesConexion.SaveChanges();
            }
            return oProducto;
        }

        public static void Actualizar(Producto oProducto)
        {
            oProducto.FechaModificacion = DateTime.Now;
            if (!GestorEntidadesConexion.getConexionState())
            {
                using (var ctx = new DistribuidoraDBEntities())
                {
                    ctx.Entry(oProducto).State = System.Data.Entity.EntityState.Modified;
                    ctx.SaveChanges();
                }
            }
            else
            {
                GestorEntidadesConexion._contexto.Entry(oProducto).State = System.Data.Entity.EntityState.Modified;
                GestorEntidadesConexion.SaveChanges();
            }
            
        }

        public static Producto getById(int ProductoId)
        {
            Producto oProducto;
            if (!GestorEntidadesConexion.getConexionState())
            {
                using (var ctx = new DistribuidoraDBEntities())
                {
                    oProducto = ctx.Producto.Include("Marca").FirstOrDefault(x => x.ProductoId == ProductoId);
                }
            }
            else
            {
                oProducto = GestorEntidadesConexion._contexto.Producto.Include("Marca").FirstOrDefault(x => x.ProductoId == ProductoId);
            }
            return oProducto;
        }

        public static Producto getByCodigo(string Codigo)
        {
            Producto oProducto;
            if (!GestorEntidadesConexion.getConexionState())
            {
                using (var ctx = new DistribuidoraDBEntities())
                {
                    oProducto = ctx.Producto.FirstOrDefault(x => x.Codigo == Codigo && x.Activo == true);
                }
            }
            else
            {
                oProducto = GestorEntidadesConexion._contexto.Producto.FirstOrDefault(x => x.Codigo == Codigo && x.Activo == true);
            }
            return oProducto;
        }
    }
}
