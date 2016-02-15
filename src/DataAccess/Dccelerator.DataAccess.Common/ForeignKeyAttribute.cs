namespace Dccelerator.DataAccess {

    /// <summary>
    /// Means what marked property will be used as for searching and maps current entity to another.
    /// For that property should(and will) be generated an secondary index and constraint.
    /// </summary>
    public class ForeignKeyAttribute : SecondaryKeyAttribute {

        /// <summary>
        /// Name of the other entity, that is referenced by the current.
        /// Be sure - it's the name of the entity, not the name of class that represents entity.
        /// </summary>
        /// <seealso cref="IEntityInfo.EntityName"/>
        public string ForeignEntityName { get; set; }


        /// <summary>
        /// Name of navigation property, that navigates to the other entity, from the current.
        /// </summary>
        public string NavigationPropertyPath { get; set; }


        /// <param name="navigationPropertyPath">Name of navigation property, that navigates to the other entity, from the current.</param>
        /// <param name="foreignEntityName">
        /// Name of the other entity, that is referenced by the current.
        /// Be sure - it's the name of the entity, not the name of class that represents entity.
        /// </param>
        /// <param name="relationship">Reletionship of key and entity.</param>
        /// <param name="duplicatesPolicy">Duplication policy between current entity and marked secondary key.</param>
        public ForeignKeyAttribute(string navigationPropertyPath, string foreignEntityName, Relationship relationship = Relationship.ManyToOne, DuplicatesPolicy duplicatesPolicy = DuplicatesPolicy.UNSORTED)
            : base(relationship, duplicatesPolicy) {
            NavigationPropertyPath = navigationPropertyPath;
            ForeignEntityName = foreignEntityName;
        }
    }
}