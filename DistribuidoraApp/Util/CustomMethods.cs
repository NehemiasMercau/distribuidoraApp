using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DistribuidoraAPI.Util
{
    public class CustomMethods
    {
        public static List<int> convertIntArrayToIntList(int[] arr)
        {
            List<int> lista = new List<int>();
            for (int i = 0; i < arr.Length; i++)
            {
                lista.Add(arr[i]);
            }
            return lista;
        }

        public static int[] convertStringArrayToIntArray(string[] arr)
        {
            int[] myInts = Array.ConvertAll(arr, s => int.Parse(s));
            return myInts;
        }
    }
}