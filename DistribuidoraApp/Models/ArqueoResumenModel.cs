using Negocio.entidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DistribuidoraAPI.Models
{
    public class ArqueoResumenModel
    {
        public TipoCobro Tipo { get; set; }
        public string Text { get; set; }
        public decimal Valor { get; set; }
        public int Cant { get; set; }
    }
}