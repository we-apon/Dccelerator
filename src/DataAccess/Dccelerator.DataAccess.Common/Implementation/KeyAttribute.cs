using System;

namespace Dccelerator.DataAccess.Implementation {
    public abstract class KeyAttribute : Attribute {
        public string Name { get; set; }
    }
}