using System;


namespace Dccelerator.DataAccess
{

    /// <summary>
    /// Allows to cache marked entity.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class GloballyCachedEntityAttribute : EntityAttribute
    {
        /// <summary>
        /// Timeout of caching.
        /// </summary>
        public TimeSpan Timeout { get; }




        /// <summary>
        /// Allows DataAccess utilities to cache marked entity.
        /// </summary>
        /// <param name="hoursTimeout">Cache timeout in hours. Default is 1 hour.</param>
        public GloballyCachedEntityAttribute(double hoursTimeout = 1.0) : this(null, null, hoursTimeout) { }


        /// <summary>
        /// Allows DataAccess utilities to cache marked entity.
        /// </summary>
        /// <param name="name"><see cref="EntityAttribute.Name"/> of entity, used to get it from specified <see cref="EntityAttribute.Repository"/>.</param>
        /// <param name="hoursTimeout">Cache timeout in hours. Default is 1 hour.</param>
        public GloballyCachedEntityAttribute(string name, double hoursTimeout = 1.0) : this(name, null, hoursTimeout) { }



        /// <summary>
        /// Allows DataAccess utilities to cache marked entity.
        /// </summary>
        /// <param name="name"><see cref="EntityAttribute.Name"/> of entity, used to get it from specified <see cref="EntityAttribute.Repository"/>.</param>
        /// <param name="repositoryType"><see cref="Type"/> of repository.</param>
        /// <param name="hoursTimeout">Cache timeout in hours. Default is 1 hour.</param>
        public GloballyCachedEntityAttribute(string name, Type repositoryType, double hoursTimeout = 1.0) : base(name, repositoryType) {
            Timeout = TimeSpan.FromHours(hoursTimeout);
        }

    }
}