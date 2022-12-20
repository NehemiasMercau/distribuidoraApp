using Negocio.entidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Negocio.gestores
{
    public class GestorUsuario
    {
        public static AspNetUsers getUserByDni(string Dni)
        {
            AspNetUsers oAspNetUsers;
            using (var ctx = new DistribuidoraDBEntities())
            {
                oAspNetUsers = ctx.AspNetUsers.Where(x => x.Dni == Dni).FirstOrDefault();
            }
            return oAspNetUsers;
        }

        public static AspNetUsers getUserByUsuarioId(int UsuarioId)
        {
            AspNetUsers oAspNetUsers;
            using (var ctx = new DistribuidoraDBEntities())
            {
                oAspNetUsers = ctx.AspNetUsers.Where(x => x.UsuarioId == UsuarioId).FirstOrDefault();
            }
            return oAspNetUsers;
        }

        public static UsuarioReferencia insertarUsuarioRef(string AspNetUsersId)
        {
            UsuarioReferencia oUsuarioReferencia = new UsuarioReferencia();
            oUsuarioReferencia.Activo = true;
            oUsuarioReferencia.AspNetUsersId = AspNetUsersId;
            using (var ctx = new DistribuidoraDBEntities())
            {
                ctx.UsuarioReferencia.Add(oUsuarioReferencia);
                ctx.SaveChanges();
            }
            return oUsuarioReferencia;
        }

        public static UsuarioReferencia getUsuarioRefByAspNetId(string AspNetUsersId)
        {
            UsuarioReferencia oUsuarioReferencia;
            if (!GestorEntidadesConexion.getConexionState())
            {
                using (var ctx = new DistribuidoraDBEntities())
                {
                    oUsuarioReferencia = ctx.UsuarioReferencia.Where(x => x.AspNetUsersId == AspNetUsersId).FirstOrDefault();
                }
            }
            else
            {
                oUsuarioReferencia = GestorEntidadesConexion._contexto.UsuarioReferencia.Where(x => x.AspNetUsersId == AspNetUsersId).FirstOrDefault();
            }
            return oUsuarioReferencia;
        }

        public static UsuarioReferencia getUsuarioRefByUsuarioId(int UsuarioId)
        {
            UsuarioReferencia oUsuarioReferencia;
            if (!GestorEntidadesConexion.getConexionState())
            {
                using (var ctx = new DistribuidoraDBEntities())
                {
                    oUsuarioReferencia = ctx.UsuarioReferencia.Where(x => x.UsuarioRefId == UsuarioId).FirstOrDefault();
                }
            }
            else
            {
                oUsuarioReferencia = GestorEntidadesConexion._contexto.UsuarioReferencia.Where(x => x.UsuarioRefId == UsuarioId).FirstOrDefault();
            }

            return oUsuarioReferencia;
        }

        public static int getUsuariosCant()
        {
            int Cantidad;
            if (!GestorEntidadesConexion.getConexionState())
            {
                using (var ctx = new DistribuidoraDBEntities())
                {
                    Cantidad = ctx.AspNetUsers.ToList().Count();
                }
            }
            else
            {
                Cantidad = GestorEntidadesConexion._contexto.AspNetUsers.ToList().Count();
            }
            return Cantidad;
        }

        public static string getPerfilNombreByPerfilId(int PerfilId)
        {
            string Nombre;
            if (!GestorEntidadesConexion.getConexionState())
            {
                using (var ctx = new DistribuidoraDBEntities())
                {
                    Nombre = ctx.Perfil.Where(x => x.PerfilId == PerfilId).FirstOrDefault().Nombre;
                }
            }
            else
            {
                Nombre = GestorEntidadesConexion._contexto.Perfil.Where(x => x.PerfilId == PerfilId).FirstOrDefault().Nombre;
            }

            return Nombre;
        }
    }
}
