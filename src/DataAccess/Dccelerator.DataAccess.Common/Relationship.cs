namespace Dccelerator.DataAccess {
    public enum Relationship {

        /// <summary>
        /// Relates many entities to one secondary key.
        /// The secondary index will have non-unique keys; in other words, duplicates will be allowed.
        /// The secondary key field is singular, in other words, it may not be an array or collection type.
        /// </summary>
        ManyToOne,

        /// <summary>
        /// Relates one entity to many secondary keys.
        /// The secondary index will have unique keys, in other words, duplicates will not be allowed.
        /// The secondary key field must be an array or collection type.
        /// </summary>
        OneToMany,

        /// <summary>
        /// Relates many entities to many secondary keys.
        /// The secondary index will have non-unique keys, in other words, duplicates will be allowed.
        /// The secondary key field must be an array or collection type.
        /// </summary>
        ManyToMany,

        /// <summary>
        /// Relates one entity to one secondary key.
        /// The secondary index will have unique keys, in other words, duplicates will not be allowed.
        /// The secondary key field is singular, in other words, it may not be an array or collection type.
        /// </summary>
        OneToOne

    }
}