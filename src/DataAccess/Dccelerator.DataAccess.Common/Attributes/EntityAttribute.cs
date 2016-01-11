using System;


namespace Dccelerator.DataAccess
{
    /// <summary>
    /// Changes name, that will be used by <see cref="Get{TEntity}"/> for getting marked class from database
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class EntityAttribute : Attribute
    {
        public string Name { get; private set; }

        /// <summary>
        /// Path to property that represents identifier of entity.
        /// </summary>
        public string IdProperty { get; set; }

        /// <summary>
        /// <see cref="Type"/> of repository. That type SHOULD implement <see cref="IDataAccessRepository"/> interface.
        /// </summary>
        
        public Type Repository { get; set; }
        



        /// <summary>
        /// Specifies <c>repository</c>, that can be used to get marked entity.
        /// </summary>
        /// <param name="repository"><see cref="Type"/> of class, that implements <see cref="IDataAccessRepository"/> interface</param>
        public EntityAttribute(Type repository) {
            Repository = repository;

            if (!(Repository is IDataAccessRepository))
                throw new InvalidOperationException($"{nameof(Repository)} should implement {nameof(IDataAccessRepository)} interface.");
        }


        /// <summary>
        /// Specifies <c>repository</c>, that can be used to get marked entity.
        /// </summary>
        /// <param name="name"><see cref="Name"/> of entity, used to get it from specified <see cref="Repository"/></param>
        /// <param name="repository"><see cref="Type"/> of class, that implements <see cref="IDataAccessRepository"/> interface</param>
        public EntityAttribute( string name,  Type repository) : this(repository) {
            Name = name;
        }
    }
}