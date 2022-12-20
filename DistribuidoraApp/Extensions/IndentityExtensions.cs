using Negocio.entidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Web;

namespace DistribuidoraAPI.Extensions
{
    public static class IndentityExtensions
    {
        public static string GetPerfilId(this IIdentity identity)
        {
            var claim = ((ClaimsIdentity)identity).FindFirst("PerfilId");
            // Test for null to avoid issues during local testing
            return (claim != null) ? claim.Value : string.Empty;
        }

        public static string GetPerfilName(this IIdentity identity)
        {
            var claim = ((ClaimsIdentity)identity).FindFirst("PerfilNombre");
            // Test for null to avoid issues during local testing
            return (claim != null) ? claim.Value : string.Empty;
        }

        public static string GetUsuarioId(this IIdentity identity)
        {
            var claim = ((ClaimsIdentity)identity).FindFirst("UsuarioId");
            // Test for null to avoid issues during local testing
            return (claim != null) ? claim.Value : string.Empty;
        }
        public static string GetSucursalId(this IIdentity identity)
        {
            var claim = ((ClaimsIdentity)identity).FindFirst("SucursalId");
            // Test for null to avoid issues during local testing
            return (claim != null) ? claim.Value : string.Empty;
        }
    }
}