using System;
using System.Collections.Generic;


namespace Dccelerator.DataAccess.BerkeleyDb {
    class BDbEntityInfo {
        public Type Type { get; set; }

        public Dictionary<string, BDbMapping> Mappings { get; set; } 


        public IBDbRepository Repository { get; set; }
    }
}