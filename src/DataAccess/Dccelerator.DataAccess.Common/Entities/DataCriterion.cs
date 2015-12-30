using System;


namespace Dccelerator.DataAccess.Entities {
    public class DataCriterion : IDataCriterion {
        public string Name { get; set; }
        public object Value { get; set; }
        public Type Type { get; set; }
    }
}