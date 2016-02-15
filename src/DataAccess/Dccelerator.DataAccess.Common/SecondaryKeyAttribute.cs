using System;


namespace Dccelerator.DataAccess {
    /// <summary>
    /// Means what marked property will be used as for searching, and for that property should be generated an secondary index.
    /// </summary>
    public class SecondaryKeyAttribute : Attribute {

        /// <summary>
        /// Reletionship of key and entity.
        /// </summary>
        public Relationship Relationship { get; set; }

        /// <summary>
        /// Duplication policy between current entity and marked secondary key
        /// </summary>
        public DuplicatesPolicy DuplicatesPolicy { get; set; }


        /// <summary>
        /// Name of secondary key. 
        /// By default, it's name of marged property.
        /// </summary>
        public string Name { get; set; }



        /// <param name="relationship">Reletionship of key and entity.</param>
        /// <param name="duplicatesPolicy">Duplication policy between current entity and marked secondary key.</param>
        public SecondaryKeyAttribute(Relationship relationship = Relationship.ManyToOne, DuplicatesPolicy duplicatesPolicy = DuplicatesPolicy.NONE) {
            Relationship = relationship;
            DuplicatesPolicy = duplicatesPolicy;
            //todo: check relationship and duplicatesPolicy for compatibility
        }
    }
}