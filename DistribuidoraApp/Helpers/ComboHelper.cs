using DistribuidoraAPI.Models;
using Negocio.entidades;
using Negocio.enumeradores;
using Negocio.gestores;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DistribuidoraAPI.Helpers
{
    public class ComboHelper
    {
        public static Combo guardarCombo(ComboViewModel oComboViewModel, int[] MultiProductos)
        {
            try
            {
                GestorEntidadesConexion.inicializarContexto();
                GestorEntidadesConexion.beginTransaccion();

                Combo oCombo = new Combo();
                oCombo.Nombre = oComboViewModel.Nombre;
                oCombo.Descripcion = oComboViewModel.Descripcion;
                oCombo.ComboId = oComboViewModel.ComboId;
                oCombo.PrecioCosto = oComboViewModel.PrecioCosto;
                oCombo.Minimo = oComboViewModel.Minimo;
                oCombo.Codigo = oComboViewModel.Codigo;
                
                oCombo = GestorCombo.Insertar(oCombo);
                PrecioHelper.verificarPrecioLista(0, oCombo.ComboId);

                foreach (int i in MultiProductos)
                {
                    ComboProducto oComboProducto = new ComboProducto
                    {
                        ComboId = oCombo.ComboId,
                        ProductoId = i
                    };
                    GestorComboProducto.Insertar(oComboProducto);
                }
                
                ProductoHelper.guardarStockPorTipo(0, oCombo.ComboId, TipoStockE.Combo, 0, oComboViewModel.DepositosList, TipoAccionE.Guardar);

                GestorEntidadesConexion.commitTransaccion();
                return oCombo;
            }
            catch (Exception ex)
            {
                GestorEntidadesConexion.rollbackTransaccion();
                return null;
                //throw ex;
            }
            finally
            {
                GestorEntidadesConexion.disposeTransaccion();
            }
        }

        public static bool editarCombo(ComboViewModel oComboViewModel, int[] MultiProductos)
        {
            try
            {
                GestorEntidadesConexion.inicializarContexto();
                GestorEntidadesConexion.beginTransaccion();

                int[] MultiProductosCustom = MultiProductos;

                Combo oCombo = GestorCombo.getById(oComboViewModel.ComboId);
                oCombo.Nombre = oComboViewModel.Nombre;
                oCombo.Descripcion = oComboViewModel.Descripcion;
                oCombo.ComboId = oComboViewModel.ComboId;
                oCombo.PrecioCosto = oComboViewModel.PrecioCosto;
                oCombo.Codigo = oComboViewModel.Codigo;
                
                oCombo.Minimo = oComboViewModel.Minimo;
                

                oCombo = GestorCombo.Actualizar(oCombo);

                List<ComboProducto> listComboProductosDB = oCombo.ComboProducto.Where(x => x.ComboId == oCombo.ComboId && x.Activo == true).ToList();
                int[] ComboProductosId = new int[listComboProductosDB.Count()];
                List<int> array = new List<int>();
                int k = 0;

                foreach (int i in MultiProductos)
                {
                    bool esProducto = false;
                    foreach (ComboProducto item in listComboProductosDB)
                    {
                        if (item.ProductoId == i && !array.Contains(item.ComboProductoId))
                        {
                            esProducto = true;
                            array.Insert(k, item.ComboProductoId);
                            k += 1;
                        }
                    }

                    if (!esProducto)
                    {
                        ComboProducto oComboProducto = new ComboProducto
                        {
                            ComboId = oCombo.ComboId,
                            ProductoId = i
                        };
                        GestorComboProducto.Insertar(oComboProducto);
                    }
                }

                foreach (ComboProducto item in listComboProductosDB)
                {
                    bool esProducto = false;
                    int j = 0;
                    foreach (int i in MultiProductos)
                    {
                        if (item.ProductoId == i)
                        {
                            esProducto = true;
                            MultiProductos[j] = 0;
                        }
                        j += 1;
                    }
                    if (!esProducto)
                    {
                        GestorComboProducto.Eliminar(item.ComboProductoId);
                    }
                }

                GestorEntidadesConexion.commitTransaccion();
                return true;
            }
            catch (Exception ex)
            {
                GestorEntidadesConexion.rollbackTransaccion();
                return false;
                throw ex;
            }
            finally
            {
                GestorEntidadesConexion.disposeTransaccion();
            }
        }

        public static string getNombreProductosPorCombo(Combo combo)
        {
            string productos = "";
            List<ComboProducto> listComboProductos = combo.ComboProducto.Where(x => x.Activo == true).ToList();
            List<NombreProductoCustom> lista = listComboProductos.GroupBy(x => x.Producto).Select(g => new NombreProductoCustom { oProducto = g.Key, count = g.Count() }).ToList();


            foreach (NombreProductoCustom item in lista.ToList())
            {
                if (productos != "") { productos += " ; "; }
                productos += item.oProducto.Nombre + "(" + item.count + ")";
            }
            return productos;
        }
    }
}