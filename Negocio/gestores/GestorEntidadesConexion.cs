using Negocio.entidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Negocio.gestores
{
    public class GestorEntidadesConexion
    {
        public static DistribuidoraDBEntities _contexto;

        public static void inicializarContexto()
        {
            if (_contexto != null)
            {
                _contexto = null;
            }
            _contexto = new DistribuidoraDBEntities();
        }

        public static void finalizarContexto()
        {
            _contexto.Dispose();
        }

        public static void beginTransaccion()
        {
            _contexto.Database.BeginTransaction();
        }

        public static void commitTransaccion()
        {
            _contexto.Database.CurrentTransaction.Commit();
        }

        public static void rollbackTransaccion()
        {
            _contexto.Database.CurrentTransaction.Rollback();
        }

        public static void disposeTransaccion()
        {
            if (_contexto != null)
            {
                _contexto.Dispose();
                _contexto = null;
            }
        }

        public static bool getConexionState()
        {
            if (_contexto != null)
            {
                return true;
            }
            else return false;
        }

        public static void SaveChanges()
        {
            _contexto.SaveChanges();
        }
    }
}
