using DistribuidoraAPI.Models;
using Negocio.entidades;
using Negocio.enumeradores;
using Negocio.gestores;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

namespace DistribuidoraAPI.Helpers
{
    public class PrecioHelper
    {
        public static void verificarPrecioLista(int ProductoId, int ComboId)
        {
            List<Lista> listListas = GestorLista.getListas();
            List<Lista> listListasCreadas = new List<Lista>();
            if (listListas.Count > 0)
            {
                List<PrecioLista> listPrecioLista = GestorPrecioLista.getPrecioLista(ProductoId, ComboId);
                foreach (PrecioLista item in listPrecioLista)
                {
                    listListasCreadas.Add(item.Lista);
                }

                if (listListasCreadas.Count == 0)
                {//No tiene ninguna creada, agregar todas
                    agregarListas(listListas, ProductoId, ComboId);
                }
                else
                {
                    List<Lista> listasNoCreadas = new List<Lista>();
                    List<int> listasInt = listListas.Select(x => x.ListaId).ToList();
                    for (int i = 0; i < listasInt.Count; i++)
                    {
                        var ListaCreada = listListasCreadas.Where(x => x.ListaId == listasInt[i]).FirstOrDefault();
                        if (ListaCreada == null)
                        {
                            var ListaAgregarNoCreada = listListas.Where(x => x.ListaId == listasInt[i]).FirstOrDefault();
                            listasNoCreadas.Add(ListaAgregarNoCreada);
                        }
                    }
                    //foreach(Lista item in listListas)
                    //{
                    //    if (!listListasCreadas.Contains(item))
                    //    {
                    //        listasNoCreadas.Add(item);
                    //    }
                    //}
                    if (listasNoCreadas.Count > 0) { agregarListas(listasNoCreadas, ProductoId, ComboId); }
                }

            }

        }

        public static void agregarListas(List<Lista> listas, int ProductoId, int ComboId)
        {
            foreach (Lista item in listas)
            {
                PrecioLista oPrecioLista = new PrecioLista();
                if (ProductoId != 0) { oPrecioLista.ProductoId = ProductoId; }
                if (ComboId != 0) { oPrecioLista.ComboId = ComboId; }
                oPrecioLista.Precio = 0;
                oPrecioLista.ListaId = item.ListaId;
                oPrecioLista.MonedaId = (int)MonedaE.ARS;
                GestorPrecioLista.Insertar(oPrecioLista, false);
            }
        }

        public static List<Root> GetPrecioDolar()
        {
            List<Root> myDeserializedClass;
            using (WebClient wc = new WebClient())
            {
                var json = wc.DownloadString("https://www.dolarsi.com/api/api.php?type=valoresprincipales");
                myDeserializedClass = JsonConvert.DeserializeObject<List<Root>>(json);
            }

            return myDeserializedClass;
        }

        public static decimal GetPrecioDolarStatico()
        {
            List<Root> listDolar = PrecioHelper.GetPrecioDolar();
            decimal precioUSD = Convert.ToDecimal(listDolar.FirstOrDefault().casa.venta.ToString());

            if (precioUSD != 0)
            {
                GestorDolar.Actualizar(precioUSD);
            }

            return precioUSD;
        }

        public static decimal getPrecioCombo(Combo combo, int listaPromo, string sessionDolar)
        {
            PrecioLista precioLista;
            if (listaPromo != 0)
            {
                precioLista = combo.PrecioLista.Where(x => x.Activo == true && x.ListaId == listaPromo).FirstOrDefault();
            }
            else
            { //Lista minorista.
                precioLista = combo.PrecioLista.Where(x => x.Activo == true && x.ListaId == (int)ListaE.Minorista).FirstOrDefault();
            }


            if (precioLista.MonedaId == (int)MonedaE.ARS)
            {
                return (decimal)precioLista.Precio;
            }
            else
            {
                decimal precioUSD = Convert.ToDecimal(sessionDolar);
                return ((decimal)precioLista.Precio * precioUSD);
            }
        }

        public static decimal getPrecioComboProductosSumatoria(Combo combo, int listaPromo, string sessionDolar)
        {
            decimal Total = 0;
            List<ComboProducto> listComboProductos = combo.ComboProducto.Where(x => x.Activo == true).ToList();
            foreach (ComboProducto item in listComboProductos)
            {
                PrecioLista precioLista;
                if (listaPromo != 0)
                {
                    precioLista = item.Producto.PrecioLista.Where(x => x.Activo == true && x.ListaId == listaPromo).FirstOrDefault();
                }
                else
                { //Lista minorista.
                    precioLista = item.Producto.PrecioLista.Where(x => x.Activo == true && x.ListaId == (int)ListaE.Minorista).FirstOrDefault();
                }
                //PrecioLista precioLista = item.Producto.PrecioLista.Where(x => x.Activo == true && x.ListaId == (int)ListaE.Minorista).FirstOrDefault();
                if (precioLista.MonedaId == (int)MonedaE.ARS)
                {
                    Total += (decimal)precioLista.Precio;
                }
                else
                {
                    decimal precioUSD = Convert.ToDecimal(sessionDolar);
                    Total += ((decimal)precioLista.Precio * precioUSD);
                }
            }
            return Total;
        }
    }
}