using Negocio.entidades;
using Negocio.enumeradores;
using Negocio.gestores;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DistribuidoraAPI.Helpers
{
    public class ProductoHelper
    {
        public static bool guardarStockPorTipo(int ProductoId, int ComboId, TipoStockE TipoStockId, int Cantidad, int[] DepositosList, TipoAccionE tipoAccionE)
        {
            bool bandEjecuto = false;
            foreach (int DepositoId in DepositosList)
            {
                StockDeposito oStockDeposito;
                if (TipoStockId == TipoStockE.Producto)
                {
                    oStockDeposito = GestorStock.GetStockDeposito(DepositoId, ProductoId, 0);
                }
                else
                {
                    oStockDeposito = GestorStock.GetStockDeposito(DepositoId, 0, ComboId);
                }

                if (oStockDeposito != null)
                {//Ya existe ese Tipo de Stock (Producto o combo)
                    if(tipoAccionE == TipoAccionE.Guardar)
                    {
                        oStockDeposito.Cantidad += Cantidad;
                        oStockDeposito.CantidadUsada += Cantidad;
                    }// else oStockDeposito.Cantidad = Cantidad;

                    GestorStock.actualizarStockDeposito(oStockDeposito);
                }
                else
                {//Insertar
                    oStockDeposito = new StockDeposito();
                    oStockDeposito.DepositoId = DepositoId;
                    oStockDeposito.TipoStockId = (int)TipoStockId;
                    if (TipoStockId == TipoStockE.Producto) { oStockDeposito.ProductoId = ProductoId; }
                    if (TipoStockId == TipoStockE.Combo) { oStockDeposito.ComboId = ComboId; }
                    oStockDeposito.Cantidad = Cantidad;
                    oStockDeposito.CantidadUsada = Cantidad;
                    GestorStock.insertarStockDeposito(oStockDeposito);
                }
            }
            bandEjecuto = true;
            return bandEjecuto;
        }

        public static List<int> getProductoIdPorCodigo(List<int> productos)
        {
            List<int> productosConId = new List<int>();
            foreach (int item in productos)
            {
                Producto producto = GestorProducto.getByCodigo(item.ToString());
                productosConId.Add(producto.ProductoId);
            }
            return productosConId;
        }
    }
}