using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DistribuidoraAPI.Models
{
    public class PromosViewModel
    {
        public string Descripcion { get; set; }
        public string MontoDescontado { get; set; }
        public string ProductosIncluidos { get; set; }
        public int ComboId { get; set; }
    }
}