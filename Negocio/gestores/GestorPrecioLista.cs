using Negocio.entidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Negocio.gestores
{
    public class GestorPrecioLista
    {

        public static List<PrecioLista> getPrecioLista(int ProductoId, int ComboId)
        {
            List<PrecioLista> listPrecioLista;
            if (!GestorEntidadesConexion.getConexionState())
            {
                using (var ctx = new DistribuidoraDBEntities())
                {
                    if (ComboId == 0)
                    {
                        listPrecioLista = ctx.PrecioLista.Include("Producto").Include("Lista").Where(x => x.ProductoId == ProductoId && x.Activo == true).ToList();
                    }
                    else
                    {
                        listPrecioLista = ctx.PrecioLista.Include("Combo").Include("Lista").Where(x => x.ComboId == ComboId && x.Activo == true).ToList();
                    }
                }
            }
            else
            {
                if (ComboId == 0)
                {
                    listPrecioLista = GestorEntidadesConexion._contexto.PrecioLista.Include("Producto").Include("Lista").Where(x => x.ProductoId == ProductoId && x.Activo == true).ToList();
                }
                else
                {
                    listPrecioLista = GestorEntidadesConexion._contexto.PrecioLista.Include("Combo").Include("Lista").Where(x => x.ComboId == ComboId && x.Activo == true).ToList();
                }
            }
            return listPrecioLista;
        }

        public static PrecioLista Insertar(PrecioLista oPrecioLista, bool Cargado)
        {
            oPrecioLista.Fecha = DateTime.Now;
            oPrecioLista.Activo = true;
            oPrecioLista.Cargado = Cargado;
            if (!GestorEntidadesConexion.getConexionState())
            {
                using (var ctx = new DistribuidoraDBEntities())
                {
                    ctx.PrecioLista.Add(oPrecioLista);
                    ctx.SaveChanges();
                }
            }
            else
            {
                GestorEntidadesConexion._contexto.PrecioLista.Add(oPrecioLista);
                GestorEntidadesConexion.SaveChanges();
            }
            return oPrecioLista;
        }

    }
}
