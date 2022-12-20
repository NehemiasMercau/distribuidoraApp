using ExpressiveAnnotations.Attributes;
using Negocio.entidades;
using Negocio.enumeradores;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace DistribuidoraAPI.Models
{
    public class PrecioListaViewModel
    {
        public int PrecioListaId { get; set; }
        public int ListaId { get; set; }
        [Required]
        public decimal Precio { get; set; }
        public Nullable<decimal> PrecioAnterior { get; set; }
        public bool Cargado { get; set; }
        //public int MonedaId { get; set; }
        //[RequiredIf("MonedaUSD == null")]
        //public MonedaE MonedaARS { get; set; }
        //[RequiredIf("MonedaARS == null")]
        //public MonedaE MonedaUSD { get; set; }

        public MonedaE MonedaId { get; set; }

        public Nullable<int> ProductoId { get; set; }
        public Nullable<int> ComboId { get; set; }

        public virtual Combo Combo { get; set; }
        public virtual Lista Lista { get; set; }
        public virtual Producto Producto { get; set; }

        public List<PrecioLista> listPrecioLista { get; set; }
    }
}