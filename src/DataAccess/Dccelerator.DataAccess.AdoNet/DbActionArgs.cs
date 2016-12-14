using System.Data.Common;
using JetBrains.Annotations;


namespace Dccelerator.DataAccess.Ado {
    public class DbActionArgs {


        public DbActionArgs([NotNull] DbConnection connection) {
            Connection = connection;
        }

        public DbActionArgs([NotNull] DbConnection connection, [NotNull] DbTransaction transaction) : this(connection) {
            Transaction = transaction;
        }


        public DbConnection Connection { get; }

        [CanBeNull]
        public DbTransaction Transaction { get; }
    }
}