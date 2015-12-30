using System;


namespace Dccelerator.DataAccess
{

    /// <summary>
    /// Allows to cache marked entity.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class CachedEntityAttribute : EntityAttribute
    {
        /// <summary>
        /// Timeout of caching.
        /// </summary>
        public TimeSpan Timeout { get; }


        /// <summary>
        /// Allows DataAccess utilities to cache marked entity.
        /// </summary>
        /// <param name="repository"><see cref="Type"/> of class, that implements <see cref="IDataAccessRepository"/> interface</param>
        /// <param name="hoursTimeout">Cache timeout in hours. Default is 1 hour.</param>
        public CachedEntityAttribute( Type repository, double hoursTimeout = 1.0) : base(repository) {
            Timeout = TimeSpan.FromHours(hoursTimeout);
        }


        /// <summary>
        /// Allows DataAccess utilities to cache marked entity.
        /// </summary>
        /// <param name="name">Name of entity, used to get it from specified <see cref="IInternalReadingRepository"/></param>
        /// <param name="repository"><see cref="Type"/> of class, that implements <see cref="IDataAccessRepository"/> interface</param>
        /// <param name="hoursTimeout">Cache timeout in hours. Default is 1 hour.</param>
        public CachedEntityAttribute( string name,  Type repository, double hoursTimeout = 1.0) : base(name, repository) {
            Timeout = TimeSpan.FromHours(hoursTimeout);
        }
    }
}