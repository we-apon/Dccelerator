namespace Dccelerator.DataAccess.Entities {

    /// <summary>
    /// Context of loading of entity.
    /// Can be used to manage parameters of lazy loading or caching.
    /// </summary>
    public class LoadingContext {

        internal LoadingContext(IInternalReadingRepository repository) {
            ReadingRepository = repository;
        }


        public LoadingContext() { }


        /// <summary>
        /// Reading repository what was used to get current entity
        /// </summary>
        
        internal IInternalReadingRepository ReadingRepository { get; }


        /// <summary>
        /// Tells, is current entity and all entities that was lazy loaded from it, can use lazy loading. 
        /// You can set it to <see langword="false" /> before using recursive reflection algorithms (like automatic logging or  serialization), 
        /// to avoid infinite loops. 
        /// Also, you can manyally set it to <see langword="true"/> to allow automatic loading of references entities.
        /// </summary>
        public bool IsLazyLoadingAllowed { get; set; }


        /// <summary>
        /// Tells, is current entity lazy loading can use cache. 
        /// Default is <see langword="true"/>. 
        /// Once you set it to <see langword="false"/>, all lazy loading queries will not be use cache, even it <see cref="CachedEntityAttribute"/> was used.
        /// </summary>
        public bool DontUseCache { get; set; }
    }
}