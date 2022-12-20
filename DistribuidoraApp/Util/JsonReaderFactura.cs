using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace DistribuidoraAPI.Util
{
    public static class JsonReaderFactura
    {
        public static Root getFromJson()
        {
            string json = System.IO.File.ReadAllText("D:/Drive/FreeLancer/RepoGit/distribuidoraAPI/DistribuidoraAPI/Util/ejemploPOST.json");
            Root root = Newtonsoft.Json.JsonConvert.DeserializeObject<Root>(json);

            return root;
        }

    }
}