using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DistribuidoraAPI.Models
{
    public class HomeViewModel
    {
        
            public string DeudasAsignadasPorcentaje { get; set; }
            public decimal DeudasAsignadasTotal { get; set; }
            public int Personas { get; set; }
            public int Entidades { get; set; }
            public int EntidadesParticular { get; set; }
            public int Domicilios { get; set; }
            public int Deudas { get; set; }
            public int Arqueos { get; set; }
            public int Cobros { get; set; }
            public int Rubros { get; set; }
            public int Usuarios { get; set; }
        
    }
}