using Negocio.entidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Negocio.gestores
{
    public class GestorComboProducto
    {
        public static ComboProducto Insertar(ComboProducto oComboProducto)
        {
            oComboProducto.Activo = true;
            oComboProducto.Fecha = DateTime.Now;
            if (!GestorEntidadesConexion.getConexionState())
            {
                using (var ctx = new DistribuidoraDBEntities())
                {
                    ctx.ComboProducto.Add(oComboProducto);
                    ctx.SaveChanges();
                }
            }
            else
            {
                GestorEntidadesConexion._contexto.ComboProducto.Add(oComboProducto);
                GestorEntidadesConexion.SaveChanges();
            }
            return oComboProducto;
        }

        public static void eliminarPorProducto(int oProductoId, int ComboId)
        {
            ComboProducto oComboProducto;
            if (!GestorEntidadesConexion.getConexionState())
            {
                using (var ctx = new DistribuidoraDBEntities())
                {
                    oComboProducto = ctx.ComboProducto.Where(x => x.ProductoId == oProductoId && x.ComboId == ComboId && x.Activo == true).FirstOrDefault();
                    oComboProducto.Activo = false;
                    ctx.SaveChanges();
                }
            }
            else
            {
                oComboProducto = GestorEntidadesConexion._contexto.ComboProducto.Where(x => x.ProductoId == oProductoId && x.ComboId == ComboId && x.Activo == true).FirstOrDefault();
                oComboProducto.Activo = false;
                GestorEntidadesConexion.SaveChanges();
            }
            
        }

        public static void Eliminar(int ComboProductoId)
        {
            ComboProducto oComboProducto;
            if (!GestorEntidadesConexion.getConexionState())
            {
                using (var ctx = new DistribuidoraDBEntities())
                {
                    oComboProducto = ctx.ComboProducto.Find(ComboProductoId);
                    oComboProducto.Activo = false;
                    ctx.SaveChanges();
                }
            }
            else
            {
                oComboProducto = GestorEntidadesConexion._contexto.ComboProducto.Find(ComboProductoId);
                oComboProducto.Activo = false;
                GestorEntidadesConexion.SaveChanges();
            }

        }

        public static List<ComboProducto> getComboProductoByComboId(int ComboId)
        {
            List<ComboProducto> listComboProductos;
            if (!GestorEntidadesConexion.getConexionState())
            {
                using (var ctx = new DistribuidoraDBEntities())
                {

                    listComboProductos = ctx.ComboProducto.Where(x => x.ComboId == ComboId && x.Activo == true).ToList() ;
                    
                }
            }
            else
            {
                listComboProductos = GestorEntidadesConexion._contexto.ComboProducto.Where(x => x.ComboId == ComboId && x.Activo == true).ToList();
     
            }
            return listComboProductos;
        }
      

    }
}
