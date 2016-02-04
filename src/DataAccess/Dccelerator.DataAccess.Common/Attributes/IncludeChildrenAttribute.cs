using System;

namespace Dccelerator.DataAccess {

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public class IncludeChildrenAttribute : Attribute {

        /// <summary>
        /// Index of includeon in <see cref="DbDataReader"/> resulting set. It starts from 1
        /// </summary>
        public int ResultSetIndex { get; set; }


        public string PropertyName { get; set; }

        public string KeyIdName { get; set; }


        /// <param name="resultSetIndex">Starts from 1.</param>
        public IncludeChildrenAttribute(int resultSetIndex, string propertyName) {
            ResultSetIndex = resultSetIndex;
            PropertyName = propertyName;
        }
    }
}