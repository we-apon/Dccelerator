using System;
using JetBrains.Annotations;


namespace Dccelerator.DataAccess
{
    /// <summary>
    /// Overrides <see cref="Name"/> of entity globally, or in concrete <see cref="Repository"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class EntityAttribute : Attribute
    {
        /// <summary>
        /// Overiden name of the entity. 
        /// If null - name of class used as name of entity.
        /// </summary>
        [CanBeNull]
        public string Name { get; private set; }


        /// <summary>
        /// If null - attriute <see cref="Name"/> overides <see cref="Name"/> in all repositories.
        /// If not null - attribute overides <see cref="Name"/> of entity in concrete repository.
        /// </summary>
        [CanBeNull]
        public Type Repository { get; set; }

        
        /// <summary>
        /// Overrides <see cref="Name"/> of entity globally, or in concrete <see cref="Repository"/>.
        /// </summary>
        /// <param name="name"><see cref="Name"/> of entity, used to get it from specified <see cref="Repository"/>.</param>
        public EntityAttribute(string name) : this(name, null) { }


        /// <summary>
        /// Overrides <see cref="Name"/> of entity globally, or in concrete <see cref="Repository"/>.
        /// </summary>
        /// <param name="name"><see cref="Name"/> of entity, used to get it from specified <see cref="Repository"/>.</param>
        /// <param name="repositoryType"><see cref="Type"/> of repository.</param>
        public EntityAttribute(string name, [CanBeNull] Type repositoryType) {
            Name = name;
            Repository = repositoryType;
        }
    }
}