using System;

namespace Dccelerator.DataAccess {
    public class DbTypeAttribute : Attribute {

        public object DbTypeName { get; set; }

        public Type RepositoryType { get; set; }

        public DbTypeAttribute(object dbTypeName, Type repositoryType) {
            DbTypeName = dbTypeName;
            RepositoryType = repositoryType;
        }
    }
}