using System;
using BerkeleyDB;


namespace Dccelerator.DataAccess.BerkeleyDb {

    /// <summary>
    /// Means what marked property will be used as for searching and maps current entity to another.
    /// For that property should(and will) be generated an secondary index and constraint.
    /// </summary>
    public class ForeignKeyAttribute : Attribute {

        public static readonly Type Type = typeof (ForeignKeyAttribute);


        /// <summary>
        /// Reletionship of key and entity.
        /// </summary>
        public Relationship Relationship { get; set; }


        /// <summary>
        /// Duplication policy between current entity and marked secondary key
        /// </summary>
        public DuplicatesPolicy DuplicatesPolicy { get; set; }


        /// <summary>
        /// Name of the other entity, that is referenced by the current
        /// </summary>
        public string ForeignEntityName { get; set; }

        /// <summary>
        /// Name of navigation property, that navigates to the other entity, from the current.
        /// </summary>
        public string ForeignEntityNavigationPath { get; set; }


        /// <summary>
        /// Name of the foreign key. It may be just marked property name.
        /// </summary>
        public string ForeignKeyPath { get; set; }
    }
}