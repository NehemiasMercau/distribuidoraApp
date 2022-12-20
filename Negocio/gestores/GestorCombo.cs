using Negocio.entidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Negocio.gestores
{
    public class GestorCombo
    {
        public static Combo Insertar(Combo oCombo)
        {
            oCombo.Activo = true;
            oCombo.FechaAlta = DateTime.Now;
            if (!GestorEntidadesConexion.getConexionState())
            {
                using (var ctx = new DistribuidoraDBEntities())
                {
                    ctx.Combo.Add(oCombo);
                    ctx.SaveChanges();
                }
            }
            else
            {
                GestorEntidadesConexion._contexto.Combo.Add(oCombo);
                GestorEntidadesConexion.SaveChanges();
            }
            return oCombo;
        }

        public static Combo Actualizar(Combo oCombo)
        {
            oCombo.Activo = true;
            oCombo.FechaModificacion = DateTime.Now;
            if (!GestorEntidadesConexion.getConexionState())
            {
                using (var ctx = new DistribuidoraDBEntities())
                {
                    ctx.Entry(oCombo).State = System.Data.Entity.EntityState.Modified;
                    ctx.SaveChanges();
                }
            }
            else
            {
                GestorEntidadesConexion._contexto.Entry(oCombo).State = System.Data.Entity.EntityState.Modified;
                GestorEntidadesConexion._contexto.SaveChanges();
            }
            return oCombo;
        }

        public static Combo getById(int ComboId)
        {
            Combo oCombo;
            if (!GestorEntidadesConexion.getConexionState())
            {
                using (var ctx = new DistribuidoraDBEntities())
                {
                    oCombo = ctx.Combo.Find(ComboId);
                }
            }
            else
            {
                oCombo = GestorEntidadesConexion._contexto.Combo.Find(ComboId);
            }
            return oCombo;
        }

        public static Combo getByCodigo(string Codigo)
        {
            Combo oCombo;
            if (!GestorEntidadesConexion.getConexionState())
            {
                using (var ctx = new DistribuidoraDBEntities())
                {
                    oCombo = ctx.Combo.FirstOrDefault(x => x.Codigo == Codigo && x.Activo == true);
                }
            }
            else
            {
                oCombo = GestorEntidadesConexion._contexto.Combo.FirstOrDefault(x => x.Codigo == Codigo && x.Activo == true);
            }
            return oCombo;
        }

    }
}
