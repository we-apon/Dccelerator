using System;

namespace Dccelerator.DataAccess {

    /// <summary>
    /// Attribute, usable to append data from additional result sets of an query as hierarchical chield of entity, that was queried.
    /// See documentation of <see cref="ResultSetIndex"/>, <see cref="TargetPath"/> and <see cref="KeyIdName"/> properties to get some information about this feature.
    /// </summary> //TODO: detailed documentation with examples
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public class IncludeChildrenAttribute : Attribute {

        /// <summary>
        /// Index of includeon in <see cref="DbDataReader"/> resulting set.
        /// So.. First includeon should has index 1 (because 0 is index of main entities table), second - 2 and so on..
        /// </summary>
        public int ResultSetIndex { get; set; }

        /// <summary>
        /// Path to property in that included children will be storred.
        /// </summary>
        public string TargetPath { get; set; }

        /// <summary>
        /// Name of property, that should be determined as key of children entity.
        /// <para> If <see cref="TargetPath"/> property is collection - <see cref="KeyIdName"/> should equals to name of foreign key property to the main entity, from the child.</para>
        /// <para>If <see cref="TargetPath"/> property is NOT collection - <see cref="KeyIdName"/> should equals to name of foreing key property to the child entity, from the main</para>
        /// </summary>
        public string KeyIdName { get; set; }


        /// <summary>
        /// Initializes an instance of <see cref="IncludeChildrenAttribute"/>
        /// </summary>
        /// <param name="resultSetIndex">
        /// Index of includeon in <see cref="DbDataReader"/> resulting set.
        /// So.. First includeon should has index 1 (because 0 is index of main entities table), second - 2 and so on..
        /// </param>
        /// <param name="targetPath">
        /// Path to property in that included children will be storred.
        /// </param>
        /// <seealso cref="KeyIdName"/>
        public IncludeChildrenAttribute(int resultSetIndex, string targetPath) {
            ResultSetIndex = resultSetIndex;
            TargetPath = targetPath;
        }
    }
}